using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO.Ports;
using System.Threading;
using System.Data.SqlTypes;
using System.Net.Sockets;
using System.Net;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Bcpg;
using System.Text;
using MySqlX.XDevAPI.Common;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using rfiddbapp;
using Org.BouncyCastle.Asn1.Cmp;
namespace rfiddbapp
{
    //Lecture de la BD
    public partial class Form1 : Form
    {
        static MySqlConnection db = new MySqlConnection("server=localhost;user=root;database=vigichantier;port=3308;password=Makson2004belka!");
        static string scannedId;
        static string scannedMarqueT = "";

        static string id_Badge;
        static string MarqueT = "";

        public Form1()
        {
            InitializeComponent();
            SocketReader.Socket();
            readAsyncData();
        }

        private async void readAsyncData()
        {
            //btnStart.Enabled = false; // Disable button to prevent multiple clicks
            await Task.Run(() =>
            {
                SocketReader.ReadData();
            });
            //btnStart.Enabled = true;  // Re-enable button after task finishes+
        }


        void readDB(string name, string id)
        {
            
            string query = "SELECT * FROM Badge;";
            /*if (idInput.Text != "" && nameInput.Text != "")
            {
                scannedId = idInput.Text;
                scannedMarqueT = nameInput.Text;
            }
            else
            {
                MessageBox.Show("Le champ ne peut pas etre vide!");
            }*/
            if (db.State == ConnectionState.Open)
            {
                using (MySqlCommand cmd = new MySqlCommand(query, db))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.IsClosed.Equals(false))
                        {
                            while (reader.Read()) // Loop through results
                            {
                                id_Badge = reader.GetString("id_Badge");
                                MarqueT = reader.GetString("MarqueT");
                                Console.WriteLine($"ID: {id_Badge}, Marque Tag: {MarqueT}");
                                //if (id_Badge == scannedId && MarqueT == scannedMarqueT)
                                if (id_Badge == SocketReader.toOutput)
                                {
                                    messageLbl.Text = "Access granted";
                                    MessageBox.Show(SocketReader.toOutput);
                                    break;
                                }
                            }
                            //if (id_Badge != scannedId || MarqueT != scannedMarqueT)
                            if (id_Badge != SocketReader.toOutput)
                            {
                                messageLbl.Text = "Access denied";
                                MessageBox.Show(SocketReader.toOutput);
                            }
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                db.Open();
                if (db.State == ConnectionState.Open)
                {
                    MessageBox.Show("Connection succesful");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed! ", ex.Message);
            }
        }

        private void checkBtn_Click(object sender, EventArgs e)
        {
            readDB(MarqueT, id_Badge);
        }
    }
}

//Lecture de tag RFID
public class SocketReader
{
    //static Thread TlectureRFID = new Thread(ReadData);
    //static TcpClient client = new TcpClient();
    static IPAddress address = IPAddress.Parse("192.168.30.10");
    static IPEndPoint endpoint = new IPEndPoint(address, 10001);
    static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    static byte[] responseBytes = new byte[256];
    static char[] responseChars = new char[256];
    public static string result = "";
    public static string toOutput = "";

    public static void Socket()
    {
        try
        {
            //client.Connect(endpoint);
            socket.Connect(endpoint);
            MessageBox.Show("Connected to socket!");
            // Add code to read from the socket
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error connecting to socket: {ex.Message}");
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0

    public static void ReadData()
    {
        try
        {
            while (true)
            {
                //socket.Blocking = false;
                Thread.Sleep(2000);
                int bytesReceived = socket.Receive(responseBytes);

                // Receiving 0 bytes means EOF has been reached
                if (bytesReceived == 0) break;

                // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
                int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);

                // Print the contents of the 'responseChars' buffer to Console.Out
                result = new string(responseChars);

                //MessageBox.Show(result[3..9]);
                MessageBox.Show(ParseId(result));

                toOutput = ParseId(result);

                //socket.Blocking = true;


                //break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }
    //[A60611FD01] or [60611FD01]
    public static string ParseId(string result)
    {
        toOutput = "";
        if (result[0] == '[')
        {
            int count = 1;
            while (result[count] != ']')
            {
                toOutput += result[count];
                count++;
            }
        }
        if (toOutput.Length == 10)
        {
            return toOutput[2..8];
        }
        else if (toOutput.Length == 9)
        {
            return toOutput[1..7];
        }
        return toOutput;
    }

    public static void CloseSocket()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
}

   
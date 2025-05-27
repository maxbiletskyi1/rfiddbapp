using MySql.Data.MySqlClient;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rfiddbapp
{
    public partial class Form1 : Form
    {
        private readonly AccessChecker accessChecker;

        public Form1()
        {
            InitializeComponent();
            accessChecker = new AccessChecker(messageLbl); // Pass the label to update it directly

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await accessChecker.InitializeAsync();
            await Task.Run(() => accessChecker.StartReadingRFID());
        }
    }

    public class AccessChecker
    {
        private static readonly string connectionString = "server=localhost;user=root;database=vigichantier;port=3308;password=Makson2004belka!";
        private readonly MySqlConnection db = new MySqlConnection(connectionString);
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("192.168.30.10"), 10001);
        private readonly Label messageLabel;

        public AccessChecker(Label label)
        {
            messageLabel = label;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await db.OpenAsync();
                socket.Connect(endpoint);
                UpdateLabel("Connected - Waiting for RFID");
            }
            catch (Exception ex)
            {
                UpdateLabel($"Connection failed: {ex.Message}");
            }
        }

        public void StartReadingRFID()
        {
            try
            {
                byte[] buffer = new byte[256];
                while (true)
                {
                    int bytesReceived = socket.Receive(buffer);
                    if (bytesReceived > 0)
                    {
                        string result = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                        string id = ParseId(result);
                        if (!string.IsNullOrEmpty(id))
                        {
                            string accessResult = CheckAccess(id);
                            UpdateLabel(accessResult);
                        }
                    }
                    Task.Delay(2000).Wait(); // Delay to prevent overwhelming the system
                }
            }
            catch (Exception ex)
            {
                UpdateLabel($"RFID reading error: {ex.Message}");
            }
        }

        private string CheckAccess(string id)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Badge WHERE id_Badge = @Id";
                using (var cmd = new MySqlCommand(query, db))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0 ? "Access Allowed " + id : "Access Denied " + id;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string ParseId(string input)
        {
            if (string.IsNullOrEmpty(input) || input[0] != '[') return "";

            int start = 1;
            int length = input.IndexOf(']') - 1;
            if (length <= 0) return "";

            string id = input.Substring(start, length);
            return id.Length switch
            {
                10 => id.Substring(2, 6),
                9 => id.Substring(1, 6),
                _ => id
            };
        }

        private void UpdateLabel(string text)
        {
            if (messageLabel.InvokeRequired)
            {
                messageLabel.Invoke(new Action(() => messageLabel.Text = text));
            }
            else
            {
                messageLabel.Text = text;
            }
        }
    }
}
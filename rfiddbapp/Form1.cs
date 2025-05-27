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

        //Initialiser avec l'IHM
        public Form1()
        {
            InitializeComponent();
            accessChecker = new AccessChecker(messageLbl); // Pass the label to update it directly

        }

        //Executer quand l'IHM est chargée
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Execution assyncrone de la methode InitializeAsync
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

        //Fonction assyncrone pour initialiser la connexion a la base de données et au lecteur RFID
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

        // Fonction pour lire les données RFID
        //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0
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
                         
                        (string id, int receptionLevel) = ParseId(result);
                        if (receptionLevel <= 160 && !string.IsNullOrEmpty(id))
                        {
                            string accessResult = CheckAccess(id);
                            //Debug
                            //UpdateLabel(result);
                            UpdateLabel(accessResult);
                        }
                        else if (receptionLevel > 160)
                        {
                            UpdateLabel("Pas de tag a proximite");
                        }
                        Task.Delay(1000).Wait();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                UpdateLabel($"RFID reading error: {ex.Message}");
            }
        }

        // Fonction pour vérifier l'accès avec la BDD
        private string CheckAccess(string id)
        {
            try
            {
                // Requete avec espace réservé pour l'ID avec prevention des injections SQL
                string query = "SELECT COUNT(*) FROM Badge WHERE id_Badge = @Id";

                //Declarer la commande SQL et la disposer automatiquement
                using (var cmd = new MySqlCommand(query, db))
                {
                    // Remplacer le paramètre ID avec la valeur de l'ID la maniere securisee 
                    cmd.Parameters.AddWithValue("@Id", id);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    // If court: ? = true, : = false 
                    return count > 0 ? "Access Allowed " + id : "Access Denied " + id;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Fonction pour accepter la plage de réception
        /*private string checkReception(string input)
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
                _ => ""
            };
        }*/

        // Fonction pour parser l'ID du badge
        private (string id, int receptionLevel) ParseId(string input)
        {
            if (string.IsNullOrEmpty(input) || input[0] != '[') return ("", 0);

            int start = 1;
            int length = input.IndexOf(']') - 1;
            if (length <= 0) return ("", 0);

            string rawId = input.Substring(start, length);
            if (rawId.Length < 2) return ("", 0);

            // Extract first two characters as reception level (in hex)
            string receptionHex = rawId.Substring(0, 2);
            int receptionLevel;
            try
            {
                receptionLevel = Convert.ToInt32(receptionHex, 16); // Convert hex to decimal
            }
            catch
            {
                return ("", 0); // Invalid hex
            }

            // Extract the rest as ID
            string id = rawId.Length switch
            {
                10 => rawId.Substring(2, 6),
                9 => rawId.Substring(1, 6),
                _ => rawId
            };

            return (id, receptionLevel);
        }

        // Mettre a jour le label
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
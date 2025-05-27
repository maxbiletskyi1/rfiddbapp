using MySql.Data.MySqlClient;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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
                            UpdateLabel(accessResult);
                        }
                        else
                        {
                            UpdateLabel("Pas de tag a proximite");
                        }
                        //Debug
                        Debug.WriteLine("Raw: " + result);
                        Debug.WriteLine("Processed: " + id);
                        Task.Delay(8000).Wait();
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
            if (string.IsNullOrEmpty(input)) return ("", 0);

            // Separate tags based on '[' and put inside the array
            string[] tags = input.Split(new[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
            if (tags.Length < 1) return ("", 0); //If 0 tags return 0

            // Filter each tag with reception level <= 160
            List<string> validTags = new List<string>();
            foreach (string tag in tags)
            {
                if (!tag.EndsWith("]")) continue; // Skip incomplete tags
                string rawId = tag.TrimEnd(']'); //remove ]
                if (rawId.Length < 2) continue;

                // Check reception level (first two characters)
                string receptionHex = rawId.Substring(0, 2);
                try
                {
                    int receptionLevel = Convert.ToInt32(receptionHex, 16);
                    if (receptionLevel <= 160)
                    {
                        validTags.Add(rawId); // Keep tag without brackets
                    }
                }
                catch
                {
                    continue; // Skip tags with invalid hex
                }
            }

            // Get the last tag from valid tags
            if (validTags.Count < 1) return ("", 0); // No valid tags
            string lastTag = validTags[validTags.Count - 1];
            if (lastTag.Length < 2) return ("", 0);

            // Extract reception level
            string receptionHexFinal = lastTag.Substring(0, 2);
            int receptionLevelFinal;
            try
            {
                receptionLevelFinal = Convert.ToInt32(receptionHexFinal, 16);
            }
            catch
            {
                return ("", 0);
            }

            // Extract ID (e.g., 0611FD from A00611FD01)
            string id = "";
            if (lastTag.Length == 10) // e.g., A00611FD01
            {
                id = lastTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }
            else if (lastTag.Length == 9) // e.g., 90611FD01
            {
                id = lastTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }
            else if (lastTag.Length == 8) // e.g., 060611FD
            {
                id = lastTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }

            return (id, receptionLevelFinal);
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
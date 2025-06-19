using MySql.Data.MySqlClient;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using Mysqlx.Crud;
using static Mysqlx.Crud.Order.Types;

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

        private async void btnHistorique_Click(object sender, EventArgs e)
        {
            await AdminWindow();
        }
        private async Task AdminWindow()
        {
            Form2 adminWindow = new Form2(accessChecker);
            adminWindow.Show();
        }

        private async void btnVehicule_Click(object sender, EventArgs e)
        {
            Form3 vehicleWindow = new Form3(accessChecker);
            vehicleWindow.Show();
        }
    }

    public class AccessChecker
    {
        //private static readonly string connectionString = "server=localhost;user=root;database=vigichantier;port=3308;password=Makson2004belka!";
        private static readonly string connectionString = "server=192.168.60.10;user=LD;database=vigichantier;port=3306;password=Azerty77";
        private readonly MySqlConnection db = new MySqlConnection(connectionString);
        public MySqlConnection DbConnection => db;
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("192.168.30.10"), 10001);
        private readonly Label messageLabel;
        private byte[] buffer = new byte[256];
        string id = "";
        private int? idTravailleur;
        public int? travailleurIdentifiant => idTravailleur;

        public AccessChecker(Label label)
        {
            messageLabel = label;
        }

        ~AccessChecker()
        {
            
        }

        //Fonction assyncrone pour initialiser la connexion a la base de données et au lecteur RFID
        public async Task InitializeAsync()
        {
            try
            {
                UpdateLabel("Waiting for connection...");
                await db.OpenAsync();
                Debug.WriteLine("BD Connecte!");
                socket.Connect(endpoint);
                Debug.WriteLine("Lecteur Connecte!");
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
                
                while (true)
                {
                    int bytesReceived = socket.Receive(buffer);
                    //Debug
                    //int bytesReceived = 5;
                    if (bytesReceived > 0)
                    {
                        string result = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                        //Debug
                        //string result = "[6FAAAAAA01][7F0611FA01][4A0611FD01][930611FD01][5A06BBFD01][908ED19A01][910611FD01][900611FD01][900611FD01][8F0611FD01]";
                        //string result = "[BC070F7401]";

                        (string id, int receptionLevel) = ParseId(result);
                        //if (receptionLevel <= 160 && !string.IsNullOrEmpty(id)) 
                        if (!string.IsNullOrEmpty(id))
                        {
                            string accessResult = CheckAccess(id);
                            UpdateLabel(accessResult);

                            //Sauvegarder 
                            idTravailleur = GetTravailleur(id);
                            sauvegarderPassage();

                            //Clear buffer 
                            Array.Clear(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            UpdateLabel("No tags nearby...");
                        }
                        //Debug
                        Debug.WriteLine("Raw: " + result);
                        Debug.WriteLine("Processed: " + id);
                        Task.Delay(5000).Wait();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                UpdateLabel($"RFID reading error: {ex.Message}");
            }
        }

        public int? GetTravailleur(string badgeId)
        {
            try
            {
                string query = "SELECT idTravailleur FROM Badge WHERE idBadge = @Id";
                using (var cmd = new MySqlCommand(query, db))
                {
                    cmd.Parameters.AddWithValue("@Id", badgeId);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result is int)
                    {
                        int travailleurId = Convert.ToInt32(result);
                        // Verify idTravailleur exists in travailleur table
                        string verifyQuery = "SELECT COUNT(*) FROM travailleur WHERE idTravailleur = @IdTravailleur";
                        using (var verifyCmd = new MySqlCommand(verifyQuery, db))
                        {
                            verifyCmd.Parameters.AddWithValue("@IdTravailleur", travailleurId);
                            int count = Convert.ToInt32(verifyCmd.ExecuteScalar());
                            if (count > 0)
                            {
                                return travailleurId;
                            }
                        }
                    }
                    else
                    {
                        UpdateLabel($"Access Denied: {badgeId}");
                        return null;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                UpdateLabel($"Erreur GetTravailleur: {ex.Message}");
                return null;
            }
        }

        private int GetNumberPassage(string badgeId)
        {
            string query = "SELECT COUNT(*) FROM historiquepassage WHERE idTravailleur = @idTravailleur";
            using (var cmd = new MySqlCommand(query, db))
            {
                cmd.Parameters.AddWithValue("@idTravailleur", idTravailleur);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count;
            }
        }

        private void sauvegarderPassage()
        {
            try
            {
                string query = "INSERT INTO historiquepassage (idTravailleur, datePassage, Direction) VALUES (@IdTravailleur, @DatePassage, @Direction)";
                //string query = "INSERT INTO historiquepassage (idTravailleur, datePassage) VALUES (@IdTravailleur, @DatePassage)";
                int count = GetNumberPassage(id);
                using (var cmd = new MySqlCommand(query, db))
                {
                    cmd.Parameters.AddWithValue("@IdTravailleur", idTravailleur);
                    cmd.Parameters.AddWithValue("@DatePassage", DateTime.Now);
                    if (idTravailleur != null)
                    {
                        if (count % 2 == 0)
                        {
                            cmd.Parameters.AddWithValue("@Direction", "Entree");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Direction", "Sortie");
                        }
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Direction", "Entree");
                    }

                        cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                UpdateLabel($"Error writing to historiquepassage: {ex.Message}");
            }
        }

        // Fonction pour vérifier l'accès
        private string CheckAccess(string id)
        {
            try
            {
                // Requete avec espace réservé pour l'ID avec prevention des injections SQL
                string query = "SELECT COUNT(*) FROM Badge WHERE idBadge = @Id";

                //Declarer la commande SQL et la disposer automatiquement
                using (var cmd = new MySqlCommand(query, db))
                {
                    // Remplacer le paramètre ID avec la valeur de l'ID la maniere securisee 
                    cmd.Parameters.AddWithValue("@Id", id);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    // If count: ? = true, : = false 
                    return count > 0 ? "Access Allowed " + id : "Access Denied " + id;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Fonction pour parser l'ID du badge
        private (string id, int receptionLevel) ParseId(string input)
        {
            int receptionLevelMin = 150;
            string validTag = "";
            if (string.IsNullOrEmpty(input)) return ("", 0);

            // Separate tags based on '[' and put inside the array
            string[] tags = input.Split(new[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
            if (tags.Length < 1) return ("", 0); //If 0 tags return 0

            foreach (string tag in tags)
            {
                if (!tag.EndsWith("]")) continue; // Skip incomplete tags
                string rawId = tag.TrimEnd(']'); //remove ]
                //if (rawId.Length < 2) continue;

                // RECEPTION FILTER
                string receptionHex = rawId.Substring(0, 2);
                try
                {
                    int receptionLevel = Convert.ToInt32(receptionHex, 16);
                    if (receptionLevel <= 150)
                    {
                        //validTags.Add(rawId);

                        if (receptionLevel < receptionLevelMin)
                        {
                            receptionLevelMin = receptionLevel;
                            validTag = rawId; // Keep the tag with the lowest reception level
                        }
                    }
                }
                catch
                {
                    continue; // Skip tags with invalid hex
                }
            }
            
            if (validTag.Length == 10) // e.g., A00611FD01
            {
                id = validTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }
            else if (validTag.Length == 9) // e.g., 90611FD01
            {
                id = validTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }
            else if (validTag.Length == 8) // e.g., 060611FD
            {
                id = validTag.Substring(2, 6); // Skip first 2 chars (reception), take 6 chars
            }
            

            return (id, receptionLevelMin);
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
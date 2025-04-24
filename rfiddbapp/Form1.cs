using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlTypes;
namespace rfiddbapp
{
    public partial class Form1 : Form
    {
        static MySqlConnection db = new MySqlConnection("server=localhost;user=root;database=vigichantier;port=3308;password=Makson2004belka!");
        static int scannedId;
        static string scannedMarqueT = "";

        static int id_Badge;
        static string MarqueT = "";
        public Form1()
        {
            InitializeComponent();
        }

        void readDB(string name, int id)
        {
            string query = "SELECT * FROM Badge;";
            if (idInput.Text != "" && nameInput.Text != "")
            {
                scannedId = Int32.Parse(idInput.Text);
                scannedMarqueT = nameInput.Text;  
            }
            else
            {
                MessageBox.Show("Le champ ne peut pas etre vide!");
            }
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
                                id_Badge = reader.GetInt32("id_Badge");
                                MarqueT = reader.GetString("MarqueT");
                                Console.WriteLine($"ID: {id_Badge}, Marque Tag: {MarqueT}");
                                if (id_Badge == scannedId && MarqueT == scannedMarqueT)
                                {
                                    MessageBox.Show("Access granted");
                                    break;
                                }
                            }
                            if (id_Badge != scannedId || MarqueT != scannedMarqueT)
                            {
                                MessageBox.Show("Access denied");
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

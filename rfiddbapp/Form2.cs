using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace rfiddbapp
{
    public partial class Form2 : Form
    {
        private readonly AccessChecker accessChecker;

        public Form2(AccessChecker checker)
        {
            InitializeComponent();
            accessChecker = checker;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            ConfigureDataGridView();
            LoadData();
        }

        private void ConfigureDataGridView()
        {
            // Set DataGridView properties
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void LoadData()
        {
            try
            {
                if (accessChecker.DbConnection.State != ConnectionState.Open)
                {
                    accessChecker.DbConnection.Open();
                }

                string testQuery = "SELECT COUNT(*) FROM historiquepassage";
                using (MySqlCommand testCmd = new MySqlCommand(testQuery, accessChecker.DbConnection))
                {
                    int rowCount = Convert.ToInt32(testCmd.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        MessageBox.Show("No data found in historiquepassage table.");
                        return;
                    }
                }

                string query = "SELECT idPassage, idTravailleur, datePassage, Direction FROM historiquepassage";
                using (MySqlCommand cmd = new MySqlCommand(query, accessChecker.DbConnection))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show("Query returned no rows.");
                            return;
                        }

                        // Bind DataTable to DataGridView
                        dataGridView1.DataSource = dataTable;

                        // Log column names for verification
                        //string columns = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                        //MessageBox.Show($"Columns in DataTable: {dataGridView1.Columns.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}

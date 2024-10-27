using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class historyLoginForm : Form
    {
        public historyLoginForm()
        {
            InitializeComponent();
            button1.Click += button1_Click;
            button2.Click += button2_Click; // Убедитесь, что обработчик события добавлен
        }

        private void LoadLoginHistory(string filter = "")
        {
            string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";
            string query = "SELECT Login, LoginTime, Success FROM LoginHistory";

            if (!string.IsNullOrEmpty(filter))
            {
                query += " WHERE Login LIKE @Filter";
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    cmd.Parameters.AddWithValue("@Filter", "%" + filter + "%");
                }

                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка при загрузке истории входов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filter = textBox1.Text.Trim();
            LoadLoginHistory(filter);
        }

        private void button2_Click(object sender, EventArgs e) // Измените имя метода, если нужно
        {
            textBox1.Clear();
            LoadLoginHistory();
        }

        private void historyLoginForm_Load(object sender, EventArgs e)
        {
            LoadLoginHistory();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

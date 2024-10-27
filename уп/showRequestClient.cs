using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class showRequestClient : Form
    {
        private int clientID; // Идентификатор текущего авторизованного клиента

        public showRequestClient(int clientID)
        {
            InitializeComponent();
            this.clientID = clientID;
            RefreshDataGridView(); // Загружаем заявки клиента
        }

        private void showRequestClient_Load(object sender, EventArgs e)
        {
            RefreshDataGridView(); // Загружаем заявки при загрузке формы
        }

        // Метод для открытия формы добавления заявки
        public void OpenAddRequestForm()
        {
            addRequest addRequestForm = new addRequest(clientID);
            addRequestForm.FormClosed += (s, args) => RefreshDataGridView(); // Обновляем заявки после закрытия формы
            addRequestForm.ShowDialog();
        }

        // Метод для загрузки заявок клиента в DataGridView
        public void RefreshDataGridView()
        {
            string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";
            string query = "SELECT r.requestID, r.startDate, c.carType, c.carModel, r.problemDescription, s.statusName " +
                           "FROM Requests r " +
                           "JOIN Cars c ON r.carID = c.carID " +
                           "JOIN Statuses s ON r.statusID = s.statusID " +
                           "WHERE r.clientID = @clientID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@clientID", clientID);

                    try
                    {
                        conn.Open();
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable; // Заполняем DataGridView
                        ResizeColumns(); // Настроить ширину столбцов
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при загрузке заявок: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Метод для настройки ширины столбцов по содержимому
        private void ResizeColumns()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        // Кнопка для открытия формы редактирования заявки
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                refreshRequest refreshForm = new refreshRequest(selectedRequestID, clientID, this); // Передаем ID заявки и клиента
                refreshForm.FormClosed += (s, args) =>
                {
                    RefreshDataGridView(); // Обновляем данные после редактирования
                    MessageBox.Show("Заявка успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                refreshForm.ShowDialog(); // Показываем форму редактирования
            }
            else
            {
                MessageBox.Show("Выберите заявку для редактирования.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Кнопка для удаления заявки
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);

                // Уведомление с вопросом о подтверждении удаления
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    // Если пользователь подтвердил, вызываем метод удаления
                    DeleteRequest(selectedRequestID);
                    RefreshDataGridView(); // Обновить данные после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Метод для удаления заявки из базы данных
        private void DeleteRequest(int requestID)
        {
            string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";
            string query = "DELETE FROM Requests WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestID", requestID);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Заявка успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при удалении заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Кнопка для возврата к форме клиента
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            clientForm clientForm = new clientForm(clientID);
            clientForm.Show();
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class changeStatus : Form
    {
        private SqlConnection connection;
        private int requestId; // ID заявки, которую мы редактируем

        public event EventHandler StatusChanged; // Событие для уведомления об успешном изменении статуса

        public changeStatus(int requestId)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=true;"); // Замените на свою строку подключения
            this.requestId = requestId;
            LoadStatuses(); // Загружаем статусы в comboBox1
        }

        private void LoadStatuses()
        {
            try
            {
                connection.Open();
                string query = "SELECT statusID, statusName FROM Statuses";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable statusesTable = new DataTable();
                adapter.Fill(statusesTable);
                comboBox1.DataSource = statusesTable;
                comboBox1.DisplayMember = "statusName"; // Отображаем название статуса
                comboBox1.ValueMember = "statusID"; // Сохраняем statusID статусов
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                int selectedStatusId = (int)comboBox1.SelectedValue;

                try
                {
                    connection.Open();
                    // Обновляем заявку с новым статусом
                    string query = "UPDATE Requests SET statusID = @statusID WHERE requestID = @requestID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@statusID", selectedStatusId);
                        command.Parameters.AddWithValue("@requestID", requestId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Статус заявки успешно изменен!");
                    StatusChanged?.Invoke(this, EventArgs.Empty); // Уведомляем об успешном изменении статуса
                    this.Close(); // Закрываем форму изменения статуса
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите статус.");
            }
        }
    }
}

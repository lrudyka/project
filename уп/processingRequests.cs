using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class processingRequests : Form
    {
        private SqlConnection connection;
        private int requestId; // ID заявки, которую мы редактируем

        public event EventHandler RequestAssigned; // Событие для уведомления об успешном сохранении

        public processingRequests(int requestId)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=true;"); // Замените на свою строку подключения
            this.requestId = requestId;
            LoadMechanics(); // Загружаем механиков в comboBox1
        }

        private void LoadMechanics()
        {
            try
            {
                connection.Open();
                string query = "SELECT userID, fio FROM Users WHERE roleID = (SELECT roleID FROM Roles WHERE roleName = 'Автомеханик')";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable mechanicsTable = new DataTable();
                adapter.Fill(mechanicsTable);
                comboBox1.DataSource = mechanicsTable;
                comboBox1.DisplayMember = "fio"; // Отображаем ФИО механиков
                comboBox1.ValueMember = "userID"; // Сохраняем userID механиков
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке механиков: {ex.Message}");
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
                int selectedMechanicId = (int)comboBox1.SelectedValue;

                try
                {
                    connection.Open();
                    // Обновляем заявку с назначением механика
                    string query = "UPDATE Requests SET masterID = @masterID WHERE requestID = @requestID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@masterID", selectedMechanicId);
                        command.Parameters.AddWithValue("@requestID", requestId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Механик успешно назначен!");
                    RequestAssigned?.Invoke(this, EventArgs.Empty); // Уведомляем об успешном сохранении
                    this.Close(); // Закрываем форму редактирования
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
                MessageBox.Show("Пожалуйста, выберите механика.");
            }
        }
    }
}

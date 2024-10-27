using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class refreshRequest : Form
    {
        private int requestID; // Идентификатор заявки
        private int clientID;  // Идентификатор клиента
        private showRequestClient parentForm; // Экземпляр родительской формы

        public refreshRequest(int requestID, int clientID, showRequestClient parentForm)
        {
            InitializeComponent();
            this.requestID = requestID;
            this.clientID = clientID;
            this.parentForm = parentForm; // Сохраняем ссылку на родительскую форму для обновления данных
            LoadProblems();     // Загружаем список проблем в ComboBox
            LoadRequestData();  // Загружаем данные заявки
        }

        // Метод для загрузки списка проблем в ComboBox
        private void LoadProblems()
        {
            comboBox1.Items.AddRange(new object[]
            {
                "Отказали тормоза", "В салоне пахнет бензином", "Руль плохо крутится",
                "Двигатель стучит", "Не заводится", "Проблемы с электроникой",
                "Проблемы с трансмиссией", "Шум в колесах", "Течет масло",
                "Проблемы с подвеской"
            });
        }

        // Метод для загрузки старых данных заявки
        private void LoadRequestData()
        {
            string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";
            string query = @"SELECT Cars.carType, Cars.carModel, Requests.problemDescription, Users.fio, Users.phone 
                             FROM Requests 
                             JOIN Cars ON Requests.carID = Cars.carID
                             JOIN Users ON Requests.clientID = Users.userID  -- Обратите внимание на правильное имя столбца
                             WHERE Requests.requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@requestID", requestID);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполняем поля формы данными из базы данных
                            textBox1.Text = reader["carType"].ToString();
                            textBox2.Text = reader["carModel"].ToString();
                            comboBox1.SelectedItem = reader["problemDescription"].ToString();
                            textBox4.Text = reader["fio"].ToString();  // Изменили на fio
                            textBox5.Text = reader["phone"].ToString(); // Изменили на phone
                        }
                    }
                }
            }
        }

        // Метод для валидации введённых данных
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите вид автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Введите модель автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите проблему автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Введите ФИО клиента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox5.Text) || textBox5.Text.Length < 10)
            {
                MessageBox.Show("Введите корректный телефон клиента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // Метод для обновления данных заявки
        private void UpdateRequest()
        {
            if (ValidateForm())
            {
                string carType = textBox1.Text.Trim();
                string carModel = textBox2.Text.Trim();
                string problem = comboBox1.SelectedItem.ToString();
                string clientFIO = textBox4.Text.Trim();
                string clientPhone = textBox5.Text.Trim();

                string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Обновляем автомобиль
                    string updateCarQuery = "UPDATE Cars SET carType = @carType, carModel = @carModel WHERE carID = (SELECT carID FROM Requests WHERE requestID = @requestID)";
                    using (SqlCommand updateCarCmd = new SqlCommand(updateCarQuery, conn))
                    {
                        updateCarCmd.Parameters.AddWithValue("@carType", carType);
                        updateCarCmd.Parameters.AddWithValue("@carModel", carModel);
                        updateCarCmd.Parameters.AddWithValue("@requestID", requestID);
                        updateCarCmd.ExecuteNonQuery();
                    }

                    // Обновляем данные пользователя
                    string updateUserQuery = "UPDATE Users SET fio = @FIO, phone = @Phone WHERE userID = @clientID"; // Заменяем на правильное имя
                    using (SqlCommand updateUserCmd = new SqlCommand(updateUserQuery, conn))
                    {
                        updateUserCmd.Parameters.AddWithValue("@FIO", clientFIO);
                        updateUserCmd.Parameters.AddWithValue("@Phone", clientPhone);
                        updateUserCmd.Parameters.AddWithValue("@clientID", clientID); // Используем clientID
                        updateUserCmd.ExecuteNonQuery();
                    }

                    // Обновляем заявку
                    string updateRequestQuery = "UPDATE Requests SET problemDescription = @problemDescription WHERE requestID = @requestID";
                    using (SqlCommand updateRequestCmd = new SqlCommand(updateRequestQuery, conn))
                    {
                        updateRequestCmd.Parameters.AddWithValue("@problemDescription", problem);
                        updateRequestCmd.Parameters.AddWithValue("@requestID", requestID);
                        updateRequestCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Заявка успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновляем запись в DataGridView в форме просмотра
                    parentForm.RefreshDataGridView(); // Обновляем данные в родительской форме

                    // Закрываем форму после успешного обновления
                    this.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateRequest();
        }
    }
}

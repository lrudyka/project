using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class addRequest : Form
    {
        private int clientID;

        public addRequest(int clientID)
        {
            InitializeComponent();
            LoadProblems();
            this.clientID = clientID;
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

        // Метод для валидации введённых данных
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(textBoxCarType.Text))
            {
                MessageBox.Show("Введите вид автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxCarModel.Text))
            {
                MessageBox.Show("Введите модель автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите проблему автомобиля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxClientFIO.Text))
            {
                MessageBox.Show("Введите ФИО клиента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxClientPhone.Text) || textBoxClientPhone.Text.Length < 10)
            {
                MessageBox.Show("Введите корректный телефон клиента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрываем текущую форму
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                string carType = textBoxCarType.Text.Trim();
                string carModel = textBoxCarModel.Text.Trim();
                string problem = comboBox1.SelectedItem.ToString();
                DateTime startDate = DateTime.Now; // Дата создания заявки

                string connectionString = "Server=LAPTOP-O0E8Q1IU\\LIZA;Database=avtoservis;Integrated Security=True;";

                // Подключаемся к базе данных и сохраняем заявку
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Сначала проверим, существует ли уже автомобиль
                    int carID;
                    string carCheckQuery = "SELECT carID FROM Cars WHERE carType = @carType AND carModel = @carModel";

                    using (SqlCommand carCheckCmd = new SqlCommand(carCheckQuery, conn))
                    {
                        carCheckCmd.Parameters.AddWithValue("@carType", carType);
                        carCheckCmd.Parameters.AddWithValue("@carModel", carModel);

                        object result = carCheckCmd.ExecuteScalar();
                        if (result != null) // Если автомобиль уже существует
                        {
                            carID = (int)result;
                        }
                        else // Если нет, добавим новый автомобиль
                        {
                            string insertCarQuery = "INSERT INTO Cars (carType, carModel) OUTPUT INSERTED.carID VALUES (@carType, @carModel)";
                            using (SqlCommand insertCarCmd = new SqlCommand(insertCarQuery, conn))
                            {
                                insertCarCmd.Parameters.AddWithValue("@carType", carType);
                                insertCarCmd.Parameters.AddWithValue("@carModel", carModel);
                                carID = (int)insertCarCmd.ExecuteScalar();
                            }
                        }
                    }

                    // Теперь добавим заявку
                    string requestQuery = "INSERT INTO Requests (startDate, carID, problemDescription, statusID, clientID) " +
                                          "VALUES (@startDate, @carID, @problemDescription, 1, @clientID)";

                    using (SqlCommand requestCmd = new SqlCommand(requestQuery, conn))
                    {
                        requestCmd.Parameters.AddWithValue("@startDate", startDate);
                        requestCmd.Parameters.AddWithValue("@carID", carID);
                        requestCmd.Parameters.AddWithValue("@problemDescription", problem);
                        requestCmd.Parameters.AddWithValue("@clientID", clientID); // Используем переданный clientID

                        try
                        {
                            requestCmd.ExecuteNonQuery();
                            MessageBox.Show("Заявка успешно создана и передана оператору.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close(); // Закрываем форму после успешного создания заявки
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Ошибка при создании заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Метод для открытия формы просмотра заявок
        public void OpenViewingForm()
        {
            // Открываем форму для просмотра, передавая clientID
            showRequestClient viewingForm = new showRequestClient(clientID); // Передаем clientID
            viewingForm.Show(); // Используем Show() вместо ShowDialog()
        }
    }
}

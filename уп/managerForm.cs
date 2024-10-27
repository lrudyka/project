using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace уп
{
    public partial class managerForm : Form
    {
        private string connectionString = @"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=True";
        private const int InProgressStatusId = 2; // ID статуса "В процессе ремонта"

        public managerForm()
        {
            InitializeComponent();
            LoadRequests();
            LoadMechanics();
        }

        private void LoadRequests()
        {
            // Загрузка всех заявок в DataGridView
            string query = @"SELECT R.requestID, R.startDate, C.carModel, R.problemDescription, S.statusName, U.fio AS Master, R.completionDate 
                             FROM Requests R
                             LEFT JOIN Cars C ON R.carID = C.carID
                             LEFT JOIN Statuses S ON R.statusID = S.statusID
                             LEFT JOIN Users U ON R.masterID = U.userID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Columns["requestID"].Visible = false; // Скрываем колонку с ID заявки
            }
        }

        private void LoadMechanics()
        {
            // Загрузка всех автомехаников в ComboBox
            string query = @"SELECT userID, fio 
                             FROM Users 
                             WHERE roleID = (SELECT roleID FROM Roles WHERE roleName = 'Автомеханик')";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox1.DisplayMember = "fio";   // Отображаем ФИО автомехаников
                comboBox1.ValueMember = "userID";  // Используем userID как значение
                comboBox1.DataSource = dt;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Назначить выбранного автомеханика на выбранную заявку
            if (dataGridView1.SelectedRows.Count > 0 && comboBox1.SelectedValue != null)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                int selectedMechanicId = Convert.ToInt32(comboBox1.SelectedValue);

                AssignMechanicToRequest(selectedRequestId, selectedMechanicId);
                MessageBox.Show("Механик успешно назначен на заявку!");

                // Обновляем данные заявок после изменения
                LoadRequests();
            }
            else
            {
                MessageBox.Show("Выберите заявку и автомеханика.");
            }
        }

        private void AssignMechanicToRequest(int requestId, int mechanicId)
        {
            // SQL-запрос для назначения автомеханика на заявку
            string query = @"UPDATE Requests
                             SET masterID = @masterID
                             WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masterID", mechanicId);
                    cmd.Parameters.AddWithValue("@requestID", requestId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Изменить дату завершения заявки и статус на "В процессе"
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                DateTime selectedDate = dateTimePicker1.Value;

                UpdateRequestCompletionDateAndStatus(selectedRequestId, selectedDate);
                MessageBox.Show("Дата завершения заявки успешно обновлена и статус изменен на 'В процессе'!");

                // Обновляем данные заявок после изменения
                LoadRequests();
            }
            else
            {
                MessageBox.Show("Выберите заявку.");
            }
        }

        private void UpdateRequestCompletionDateAndStatus(int requestId, DateTime completionDate)
        {
            // SQL-запрос для обновления даты завершения и статуса заявки
            string query = @"UPDATE Requests
                             SET completionDate = @completionDate, statusID = @statusID
                             WHERE requestID = @requestID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@completionDate", completionDate);
                    cmd.Parameters.AddWithValue("@statusID", InProgressStatusId); // Статус "В процессе"
                    cmd.Parameters.AddWithValue("@requestID", requestId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

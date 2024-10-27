using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace уп
{
    public partial class requestFormMaster : Form
    {
        private SqlConnection connection;
        private DataTable requestsTable; // Таблица для хранения заявок
        private int mechanicID; // ID авторизованного автомеханика
        private int totalRecords; // Общее количество записей

        // Добавляем параметр mechanicID в конструктор
        public requestFormMaster(int loggedInMechanicID)
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=true;"); // Строка подключения к БД
            mechanicID = loggedInMechanicID; // Присваиваем переданный ID автомеханика
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                connection.Open();
                // Запрос для получения заявок только для текущего автомеханика
                string query = @"
                SELECT 
                    r.requestID,
                    r.startDate,
                    c.carID,
                    r.problemDescription,
                    s.statusName,
                    r.completionDate,
                    u.fio AS clientName
                FROM Requests r
                LEFT JOIN Cars c ON r.carID = c.carID
                LEFT JOIN Statuses s ON r.statusID = s.statusID
                LEFT JOIN Users u ON r.clientID = u.userID
                WHERE r.masterID = @mechanicID";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@mechanicID", mechanicID);

                requestsTable = new DataTable();
                adapter.Fill(requestsTable);
                dataGridView1.DataSource = requestsTable;

                totalRecords = requestsTable.Rows.Count; // Сохраняем общее количество записей
                UpdateAverageCompletionTime();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void UpdateAverageCompletionTime()
        {
            // Вычисляем среднее время выполнения заказов
            var completedRows = requestsTable.Select("completionDate IS NOT NULL");
            if (completedRows.Length > 0)
            {
                TimeSpan totalDuration = TimeSpan.Zero;
                foreach (DataRow row in completedRows)
                {
                    DateTime startDate = Convert.ToDateTime(row["startDate"]);
                    DateTime completionDate = Convert.ToDateTime(row["completionDate"]);
                    totalDuration += (completionDate - startDate);
                }
                double avgDuration = totalDuration.TotalDays / completedRows.Length;
                label2.Text = $"Среднее время выполнения: {avgDuration:F1} дней";
            }
            else
            {
                label2.Text = "Среднее время выполнения: нет завершенных заявок";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Фильтрация данных по статусу, ФИО и другим текстовым полям (без даты)
            string filter = $"clientName LIKE '%{textBox1.Text}%' OR " +
                            $"problemDescription LIKE '%{textBox1.Text}%' OR " +
                            $"statusName LIKE '%{textBox1.Text}%'";

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Сбрасываем фильтр, очищаем текстовое поле и обновляем данные
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            textBox1.Clear();
            LoadRequests(); // Перезагружаем заявки автомеханика
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Сортировка по столбцу, используем System.Windows.Forms.SortOrder
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (dataGridView1.SortOrder == System.Windows.Forms.SortOrder.Ascending)
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], ListSortDirection.Descending);
            }
            else
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], ListSortDirection.Ascending);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли заявка
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                CompleteRequest(selectedRequestId); // Завершение заявки
                LoadRequests(); // Обновляем список заявок
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для завершения.");
            }
        }

        private void CompleteRequest(int requestId)
        {
            try
            {
                connection.Open();
                // Запрос на изменение статуса заявки и добавление даты завершения
                string query = @"
                UPDATE Requests
                SET statusID = (SELECT statusID FROM Statuses WHERE statusName = 'Готова к выдаче'), 
                    completionDate = @completionDate
                WHERE requestID = @requestID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@completionDate", DateTime.Now);
                    command.Parameters.AddWithValue("@requestID", requestId);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("Заявка успешно завершена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении заявки: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                var orderData = GetOrderData(selectedRequestId);
                string report = GenerateReport(selectedRequestId, orderData);
                SaveReportToFile(report);
                MessageBox.Show("Отчет успешно создан!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для создания отчета.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private (string ClientName, string OrderDescription, DateTime StartTime, DateTime EndTime) GetOrderData(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(@"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=True"))
            {
                conn.Open();
                string query = @"
                    SELECT c.fio AS ClientName, r.problemDescription, r.startDate, r.completionDate
                    FROM Requests r
                    INNER JOIN Users c ON r.clientID = c.userID
                    WHERE r.requestID = @orderId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string clientName = reader.GetString(0);
                    string description = reader.GetString(1);
                    DateTime startTime = reader.GetDateTime(2);
                    DateTime endTime = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3);

                    return (clientName, description, startTime, endTime);
                }
                else
                {
                    throw new Exception("Заявка не найдена.");
                }
            }
        }

        // Обновленный метод для генерации отчета
        private string GenerateReport(int requestId, (string ClientName, string OrderDescription, DateTime StartTime, DateTime EndTime) orderData)
        {
            TimeSpan duration = orderData.EndTime - orderData.StartTime;
            double durationInDays = duration.TotalDays;

            string report = $"Отчет по заявке №{requestId}\n\n" +
                            $"Клиент: {orderData.ClientName}\n" +
                            $"Описание заказа: {orderData.OrderDescription}\n" +
                            $"Время начала выполнения: {orderData.StartTime}\n" +
                            $"Время завершения выполнения: {orderData.EndTime}\n" +
                            $"Общее время выполнения: {durationInDays:F1} дней\n";

            return report;
        }

        private void SaveReportToFile(string report)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, report);
                }
            }
        }
    }
}
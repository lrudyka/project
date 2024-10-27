using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace уп
{
    public partial class showRequestAdmin : Form
    {
        private SqlConnection connection;
        private DataTable requestsTable; // Таблица для хранения заявок
        private int totalRecords; // Общее количество записей

        public showRequestAdmin()
        {
            InitializeComponent();
            connection = new SqlConnection(@"Server=LAPTOP-O0E8Q1IU\LIZA;Database=avtoservis;Integrated Security=true;"); // Замените на свою строку подключения
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                connection.Open();
                // Запрос для получения всех заявок с присоединением к другим таблицам для отображения имен клиентов и механиков
                string query = @"
                SELECT 
                    r.requestID,
                    r.startDate,
                    c.carID,
                    r.problemDescription,
                    s.statusName,
                    r.completionDate,
                    u.userID AS masterID,
                    u.fio AS masterName,
                    uc.fio AS clientName
                FROM Requests r
                LEFT JOIN Cars c ON r.carID = c.carID
                LEFT JOIN Statuses s ON r.statusID = s.statusID
                LEFT JOIN Users u ON r.masterID = u.userID
                LEFT JOIN Users uc ON r.clientID = uc.userID";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                requestsTable = new DataTable();
                adapter.Fill(requestsTable);
                dataGridView1.DataSource = requestsTable;

                totalRecords = requestsTable.Rows.Count; // Сохраняем общее количество записей
                UpdateRecordCount();
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

        private void UpdateRecordCount()
        {
            int filteredRecords = (dataGridView1.DataSource as DataTable)?.DefaultView.Count ?? 0;
            label2.Text = $"{filteredRecords} из {totalRecords}"; // Обновляем текст метки с количеством записей
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Открываем форму для назначения механика
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                processingRequests processingRequestsForm = new processingRequests(selectedRequestId); // Открытие формы назначения механика
                processingRequestsForm.RequestAssigned += (s, args) =>
                {
                    UpdateRequestStatusToInProgress(selectedRequestId);
                    LoadRequests(); // Обновляем список заявок после редактирования
                };
                processingRequestsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для редактирования.");
            }
        }

        private void UpdateRequestStatusToInProgress(int requestId)
        {
            try
            {
                connection.Open();
                string query = "UPDATE Requests SET statusID = (SELECT statusID FROM Statuses WHERE statusName = 'В процессе ремонта') WHERE requestID = @requestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@requestID", requestId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса заявки: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Открываем форму для изменения статуса заявки
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRequestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["requestID"].Value);
                changeStatus changeStatusForm = new changeStatus(selectedRequestId); // Открытие формы изменения статуса
                changeStatusForm.ShowDialog();
                LoadRequests(); // Обновляем список заявок после изменения статуса
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для изменения статуса.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Фильтрация данных
            string filter = $"masterName LIKE '%{textBox1.Text}%' OR " +
                            $"clientName LIKE '%{textBox1.Text}%' OR " +
                            $"problemDescription LIKE '%{textBox1.Text}%' OR " +
                            $"statusName LIKE '%{textBox1.Text}%'"; // Добавлено условие для статуса

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
            UpdateRecordCount(); // Обновляем количество записей после фильтрации
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Сбрасываем фильтр и обновляем данные
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            LoadRequests(); // Перезагружаем все заявки
            textBox1.Clear(); // Очищаем текстовое поле
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Сортировка по заголовку столбца
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (dataGridView1.SortOrder == System.Windows.Forms.SortOrder.Ascending) // Указали полное имя класса
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], System.ComponentModel.ListSortDirection.Descending);
            }
            else
            {
                dataGridView1.Sort(dataGridView1.Columns[columnName], System.ComponentModel.ListSortDirection.Ascending);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            avtorizatia avtorizatiaForm = new avtorizatia();
            avtorizatiaForm.Show();
            this.Close(); // Закрываем текущую форму
        }
    }
}

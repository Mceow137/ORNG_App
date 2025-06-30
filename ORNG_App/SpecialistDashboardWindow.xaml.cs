using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ORNG_App
{
    /// <summary>
    /// Логика взаимодействия для SpecialistDashboardWindow.xaml
    /// </summary>
    public partial class SpecialistDashboardWindow : Window
    {
        private User _currentUser;
        private string _selectedFilePath;

        public SpecialistDashboardWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            txtWelcome.Text = $"Добро пожаловать, {user.FullName}";
            LoadCustomers();
            LoadMyDocuments();
        }

        private void LoadCustomers()
        {
            string query = "SELECT UserId, FullName FROM Users WHERE Role = 'User' AND IsActive = 1";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            cbCustomers.ItemsSource = dt.DefaultView;
        }

        private void LoadMyDocuments()
        {
            string query = @"SELECT d.*, c.FullName AS CustomerName 
                            FROM GasActivationDocuments d
                            JOIN Users c ON d.CustomerId = c.UserId
                            WHERE d.SpecialistId = @SpecialistId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SpecialistId", _currentUser.UserId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            dgMyDocuments.ItemsSource = dt.DefaultView;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                txtFileName.Text = System.IO.Path.GetFileName(_selectedFilePath);
            }
        }

        private void btnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDocNumber.Text) ||
                cbCustomers.SelectedItem == null ||
                string.IsNullOrEmpty(txtAddress.Text) ||
                string.IsNullOrEmpty(txtMeterNumber.Text) ||
                string.IsNullOrEmpty(txtInitialReading.Text) ||
                dpActivationDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtInitialReading.Text, out decimal initialReading))
            {
                MessageBox.Show("Введите корректные начальные показания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedCustomer = (DataRowView)cbCustomers.SelectedItem;
            int customerId = (int)selectedCustomer["UserId"];

            byte[] documentScan = DatabaseHelper.ImageToByteArray(_selectedFilePath);

            string query = @"INSERT INTO GasActivationDocuments 
                            (DocumentNumber, SpecialistId, CustomerId, Address, MeterNumber, InitialMeterReading, ActivationDate, DocumentScan)
                            VALUES 
                            (@DocumentNumber, @SpecialistId, @CustomerId, @Address, @MeterNumber, @InitialMeterReading, @ActivationDate, @DocumentScan)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@DocumentNumber", txtDocNumber.Text),
                new SqlParameter("@SpecialistId", _currentUser.UserId),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@Address", txtAddress.Text),
                new SqlParameter("@MeterNumber", txtMeterNumber.Text),
                new SqlParameter("@InitialMeterReading", initialReading),
                new SqlParameter("@ActivationDate", dpActivationDate.SelectedDate),
                new SqlParameter("@DocumentScan", documentScan ?? (object)DBNull.Value)
            };

            int result = DatabaseHelper.ExecuteNonQuery(query, parameters);

            if (result > 0)
            {
                MessageBox.Show("Акт на пуск газа успешно создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Отправка уведомления пользователю
                string notificationQuery = @"INSERT INTO Notifications (UserId, DocumentId, Message)
                                           VALUES (@UserId, 
                                           (SELECT TOP 1 DocumentId FROM GasActivationDocuments WHERE DocumentNumber = @DocumentNumber),
                                           @Message)";

                SqlParameter[] notificationParams = new SqlParameter[]
                {
                    new SqlParameter("@UserId", customerId),
                    new SqlParameter("@DocumentNumber", txtDocNumber.Text),
                    new SqlParameter("@Message", $"Для вас создан новый акт на пуск газа №{txtDocNumber.Text}")
                };

                DatabaseHelper.ExecuteNonQuery(notificationQuery, notificationParams);

                ClearForm();
                LoadMyDocuments();
            }
            else
            {
                MessageBox.Show("Не удалось создать акт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtDocNumber.Text = "";
            cbCustomers.SelectedIndex = -1;
            txtAddress.Text = "";
            txtMeterNumber.Text = "";
            txtInitialReading.Text = "";
            dpActivationDate.SelectedDate = null;
            _selectedFilePath = null;
            txtFileName.Text = "Файл не выбран";
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

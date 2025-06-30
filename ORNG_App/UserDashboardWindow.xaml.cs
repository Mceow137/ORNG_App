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
    /// Логика взаимодействия для UserDashboardWindow.xaml
    /// </summary>
    public partial class UserDashboardWindow : Window
    {
        private User _currentUser;

        public UserDashboardWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            txtWelcome.Text = $"Добро пожаловать, {user.FullName}";
            LoadNotifications();
            LoadMyDocuments();
        }

        private void LoadNotifications()
        {
            string query = @"SELECT n.* FROM Notifications n
                            WHERE n.UserId = @UserId
                            ORDER BY n.CreatedDate DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", _currentUser.UserId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            dgNotifications.ItemsSource = dt.DefaultView;
        }

        private void LoadMyDocuments()
        {
            string query = @"SELECT d.*, s.FullName AS SpecialistName 
                            FROM GasActivationDocuments d
                            JOIN Users s ON d.SpecialistId = s.UserId
                            WHERE d.CustomerId = @CustomerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", _currentUser.UserId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            dgMyDocuments.ItemsSource = dt.DefaultView;
        }

        private void btnMarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            if (dgNotifications.SelectedItem == null)
            {
                MessageBox.Show("Выберите уведомление", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgNotifications.SelectedItem;
            int notificationId = (int)row["NotificationId"];

            string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@NotificationId", notificationId)
            };

            int result = DatabaseHelper.ExecuteNonQuery(query, parameters);

            if (result > 0)
            {
                MessageBox.Show("Уведомление отмечено как прочитанное", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadNotifications();
            }
            else
            {
                MessageBox.Show("Не удалось обновить статус уведомления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOpenDocument_Click(object sender, RoutedEventArgs e)
        {
            if (dgNotifications.SelectedItem == null)
            {
                MessageBox.Show("Выберите уведомление", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgNotifications.SelectedItem;
            if (row["DocumentId"] == DBNull.Value)
            {
                MessageBox.Show("Это уведомление не связано с документом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int documentId = (int)row["DocumentId"];

            string query = "SELECT * FROM GasActivationDocuments WHERE DocumentId = @DocumentId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@DocumentId", documentId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 1)
            {
                GasActivationDocument document = new GasActivationDocument
                {
                    DocumentId = (int)dt.Rows[0]["DocumentId"],
                    DocumentNumber = dt.Rows[0]["DocumentNumber"].ToString(),
                    Address = dt.Rows[0]["Address"].ToString(),
                    MeterNumber = dt.Rows[0]["MeterNumber"].ToString(),
                    InitialMeterReading = (decimal)dt.Rows[0]["InitialMeterReading"],
                    ActivationDate = (DateTime)dt.Rows[0]["ActivationDate"],
                    DocumentScan = dt.Rows[0]["DocumentScan"] as byte[]
                };

                ViewDocumentWindow viewWindow = new ViewDocumentWindow(document);
                viewWindow.ShowDialog();
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

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
    /// Логика взаимодействия для AdminDashboardWindow.xaml
    /// </summary>
    public partial class AdminDashboardWindow : Window
    {
        private User _currentUser;

        public AdminDashboardWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            txtWelcome.Text = $"Добро пожаловать, {user.FullName}";
            LoadUsers();
            LoadDocuments();
        }

        private void LoadUsers()
        {
            string query = "SELECT * FROM Users";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            dgUsers.ItemsSource = dt.DefaultView;
        }

        private void LoadDocuments()
        {
            string query = @"SELECT d.*, s.FullName AS SpecialistName, c.FullName AS CustomerName 
                            FROM GasActivationDocuments d
                            JOIN Users s ON d.SpecialistId = s.UserId
                            JOIN Users c ON d.CustomerId = c.UserId";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            dgDocuments.ItemsSource = dt.DefaultView;
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            AddEditUserWindow addWindow = new AddEditUserWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgUsers.SelectedItem;
            User user = new User
            {
                UserId = (int)row["UserId"],
                Username = row["Username"].ToString(),
                Password = row["Password"].ToString(),
                FullName = row["FullName"].ToString(),
                Role = row["Role"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"].ToString(),
                IsActive = (bool)row["IsActive"]
            };

            AddEditUserWindow editWindow = new AddEditUserWindow(user);
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgUsers.SelectedItem;
            int userId = (int)row["UserId"];

            if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя {row["Username"]}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string query = "UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    MessageBox.Show("Пользователь успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Не удалось удалить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

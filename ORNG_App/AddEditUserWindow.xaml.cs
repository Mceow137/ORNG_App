using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AddEditUserWindow.xaml
    /// </summary>
    public partial class AddEditUserWindow : Window
    {
        private User _user;

        public AddEditUserWindow(User user = null)
        {
            InitializeComponent();
            _user = user;

            if (_user != null)
            {
                txtUsername.Text = _user.Username;
                txtFullName.Text = _user.FullName;
                txtEmail.Text = _user.Email;
                txtPhone.Text = _user.Phone;

                foreach (ComboBoxItem item in cbRole.Items)
                {
                    if (item.Content.ToString() == _user.Role)
                    {
                        cbRole.SelectedItem = item;
                        break;
                    }
                }

                if (_user.UserId == 1) // Запрет редактирования администратора по умолчанию
                {
                    txtUsername.IsEnabled = false;
                    cbRole.IsEnabled = false;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) ||
                string.IsNullOrEmpty(txtFullName.Text) ||
                cbRole.SelectedItem == null)
            {
                MessageBox.Show("Заполните обязательные поля (Логин, ФИО, Роль)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = _user?.Password;
            if (_user == null || !string.IsNullOrEmpty(txtPassword.Password))
            {
                if (string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                password = txtPassword.Password;
            }

            string role = (cbRole.SelectedItem as ComboBoxItem).Content.ToString();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (_user == null)
            {
                // Добавление нового пользователя
                string query = @"INSERT INTO Users (Username, Password, FullName, Role, Email, Phone)
                                VALUES (@Username, @Password, @FullName, @Role, @Email, @Phone)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtUsername.Text),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@FullName", txtFullName.Text),
                    new SqlParameter("@Role", role),
                    new SqlParameter("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    MessageBox.Show("Пользователь успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось добавить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Редактирование существующего пользователя
                string query = @"UPDATE Users SET 
                                Username = @Username,
                                Password = @Password,
                                FullName = @FullName,
                                Role = @Role,
                                Email = @Email,
                                Phone = @Phone
                                WHERE UserId = @UserId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtUsername.Text),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@FullName", txtFullName.Text),
                    new SqlParameter("@Role", role),
                    new SqlParameter("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                    new SqlParameter("@UserId", _user.UserId)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    MessageBox.Show("Пользователь успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось обновить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

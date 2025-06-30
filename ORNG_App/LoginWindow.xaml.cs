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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Логин и пароль обязательны для заполнения";
                return;
            }

            string query = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password AND IsActive = 1";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };

            DataTable result = DatabaseHelper.ExecuteQuery(query, parameters);

            if (result.Rows.Count == 1)
            {
                User currentUser = new User
                {
                    UserId = (int)result.Rows[0]["UserId"],
                    Username = result.Rows[0]["Username"].ToString(),
                    FullName = result.Rows[0]["FullName"].ToString(),
                    Role = result.Rows[0]["Role"].ToString(),
                    Email = result.Rows[0]["Email"].ToString(),
                    Phone = result.Rows[0]["Phone"].ToString()
                };

                OpenDashboard(currentUser);
            }
            else
            {
                lblError.Text = "Неверный логин или пароль";
            }
        }

        private void OpenDashboard(User user)
        {
            Window dashboard = null;

            switch (user.Role)
            {
                case "Admin":
                    dashboard = new AdminDashboardWindow(user);
                    break;
                case "Specialist":
                    dashboard = new SpecialistDashboardWindow(user);
                    break;
                case "User":
                    dashboard = new UserDashboardWindow(user);
                    break;
            }

            if (dashboard != null)
            {
                dashboard.Show();
                this.Close();
            }
        }
    }
}

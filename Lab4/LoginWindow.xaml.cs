using System;
using System.Collections.Generic;
using System.Data;
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

namespace Lab4
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public Role Role { get; set; } = Role.None;
        public string? ActiveUser { get; set; }
        public int UserId { get; set; }
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }
            var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Text;
            var users = await MainWindow.GetTable("LOAD_TABLE|Users");
            DataRow? user = users.AsEnumerable().FirstOrDefault(row => row["Username"].ToString() == username);
            if (user != null)
            {
                if (user["PasswordHash"].ToString() == password)
                {
                    var role = user["Role"];
                    if (Enum.TryParse(role.ToString(), out Role role_))
                    {
                        Role = role_;
                        ActiveUser = user["Username"].ToString();
                        UserId = int.Parse(user["Id"].ToString());
                    }
                    else
                    {
                        Role = Role.None;
                    }
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Пароль не вірний.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Користувача не знайдено.");
                return;
            }
            
        }
    }
}

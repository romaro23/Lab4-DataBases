using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for DataInputWindow.xaml
    /// </summary>
    public partial class UsersInputWindow : Window
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public UsersInputWindow()
        {
            InitializeComponent();
        }

        public UsersInputWindow(string username, string password, string role) : this()
        {
            UsernameTextBox.Text = username;
            PasswordTextBox.Text = password;
            foreach (var item in RoleComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == role)
                {
                    RoleComboBox.SelectedItem = comboBoxItem;
                    break;
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordTextBox.Text) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            Username = UsernameTextBox.Text;
            Password = PasswordTextBox.Text;
            Role = RoleComboBox.SelectedValue.ToString();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

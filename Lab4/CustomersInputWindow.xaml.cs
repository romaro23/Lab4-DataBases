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
    /// Interaction logic for CustomersInputWindow.xaml
    /// </summary>
    public partial class CustomersInputWindow : Window
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public CustomersInputWindow()
        {
            InitializeComponent();
        }
        public CustomersInputWindow(string name, string phone, string email) : this()
        {
            NameTextBox.Text = name;
            PhoneTextBox.Text = phone;
            EmailTextBox.Text = email;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            Name = NameTextBox.Text;
            Phone = PhoneTextBox.Text;
            Email = EmailTextBox.Text;
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

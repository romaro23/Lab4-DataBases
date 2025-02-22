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
    /// Interaction logic for OrdersInputWindow.xaml
    /// </summary>
    public partial class OrdersInputWindow : Window
    {
        public int CustomerId { get; set; }
        public string Item { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public double TotalPrice { get; set; }
        public OrdersInputWindow()
        {
            InitializeComponent();
        }

        public void FeelCustomersId(DataTable ids)
        {
            foreach (DataRow row in ids.Rows)
            {
                if (!CustomerIdComboBox.Items.Contains(row["CustomerId"].ToString()))
                {
                    CustomerIdComboBox.Items.Add(row["CustomerId"].ToString());
                }
            }
        }
        public OrdersInputWindow(int id, string item, DateTime orderDate, string status, double totalPrice, DataTable ids) : this()
        {
            FeelCustomersId(ids);
            foreach (var item_ in CustomerIdComboBox.Items)
            {
                if (item_.ToString() == id.ToString())
                {
                    CustomerIdComboBox.SelectedItem = item_;
                    break;
                }
            }
            ItemTextBox.Text = item;
            OrderDatePicker.SelectedDate = orderDate;
            foreach (var item_ in StatusComboBox.Items)
            {
                if (item_ is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == status)
                {
                    StatusComboBox.SelectedItem = comboBoxItem;
                    break;
                }
            }

            TotalPriceTextBox.Text = totalPrice.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemTextBox.Text) ||
                string.IsNullOrWhiteSpace(TotalPriceTextBox.Text) ||
                CustomerIdComboBox.SelectedItem == null || OrderDatePicker.SelectedDate == null || StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            CustomerId = int.Parse(CustomerIdComboBox.SelectedItem.ToString());
            Item = ItemTextBox.Text;
            OrderDate = OrderDatePicker.SelectedDate;
            Status = StatusComboBox.SelectedValue.ToString();
            TotalPrice = Double.Parse(TotalPriceTextBox.Text);
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

using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab4
{

    public enum Role
    {
        Admin,
        Manager,
        None
    }
    public partial class MainWindow : Window
    {
        DataBase db = new DataBase();
        private Role Role = Lab4.Role.None;
        public int UserId { get; set; }
        public delegate void LogEventHandler(object sender, int userId, string action);
        public event LogEventHandler LogEvent;

        public MainWindow()
        {
            InitializeComponent();
            LogEvent += (sender, userId, action) => AddLog(userId, action);
            var login = new LoginWindow();
            login.ShowDialog();
            Role = login.Role;
            if (login.IsActive == false && Role == Role.None)
            {
                this.Close();
            }

            this.Title = login.ActiveUser;
            UserId = login.UserId;
            if (Role == Role.Manager)
            {
                UsersTab.IsEnabled = false;
                LogsTab.IsEnabled = false;
                ClientsTab.IsSelected = true;
                CustomersGrid.SelectionChanged -= CustomersGrid_SelectionChanged;
                OrdersGrid.SelectionChanged -= OrdersGrid_SelectionChanged;

            }
        }

        private void OnLogEvent(int userId, string action)
        {
            LogEvent?.Invoke(this, userId, action);
        }

        private void AddLog(int userId, string action)
        {
            db.AddLog(userId, action);
        }
        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUserButton.IsEnabled = true;
            DeleteUserButton.IsEnabled = true;
        }
        private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomerButton.IsEnabled = true;
            DeleteCustomerButton.IsEnabled = true;
        }
        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrderButton.IsEnabled = true;
            DeleteOrderButton.IsEnabled = true;
        }

        private void LoadData()
        {
            var table = db.LoadTable("Users");
            UsersGrid.ItemsSource = table.DefaultView;
            table = db.LoadTable("Customers");
            CustomersGrid.ItemsSource = table.DefaultView;
            table = db.LoadTable("Orders");
            OrdersGrid.ItemsSource = table.DefaultView;
            table = db.LoadTable("Logs");
            LogsGrid.ItemsSource = table.DefaultView;
        }
        private void RefreshData(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        
        private void AddUser(object sender, RoutedEventArgs e)
        {
            var dialog = new UsersInputWindow();
            if (dialog.ShowDialog() == true)
            {
                string username = dialog.Username;
                string password = dialog.Password;
                string role = dialog.Role;
                db.AddUser(username, password, role);
                var table = db.LoadTable("Users");
                UsersGrid.ItemsSource = table.DefaultView;
                OnLogEvent(UserId, "Added user");
            }
        }
        private void UpdateUser(object sender, RoutedEventArgs e)
        {
            var dialog = SelectUser();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((UsersGrid.SelectedItem as DataRowView)["Id"].ToString());
                var username = dialog.Username;
                var password = dialog.Password;
                var role = dialog.Role;
                db.UpdateUser(id, username, password, role);
                var table = db.LoadTable("Users");
                UsersGrid.ItemsSource = table.DefaultView;
                UnselectUser();
                OnLogEvent(UserId, "Updated user");
            }
        }
        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            var dialog = SelectUser();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((UsersGrid.SelectedItem as DataRowView)["Id"].ToString());
                db.DeleteUser(id);
                var table = db.LoadTable("Users");
                UsersGrid.ItemsSource = table.DefaultView;
                UnselectUser();
                OnLogEvent(UserId, "Deleted user");
            }
        }

        private UsersInputWindow SelectUser()
        {
            var selectedUser = UsersGrid.SelectedItem as DataRowView;
            var username = selectedUser["Username"].ToString();
            var password = selectedUser["PasswordHash"].ToString();
            var role = selectedUser["Role"].ToString();
            return new UsersInputWindow(username, password, role);
        }

        private void UnselectUser()
        {
            UpdateUserButton.IsEnabled = false;
            DeleteUserButton.IsEnabled = false;
        }
        private void AddCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomersInputWindow();
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.Name;
                string phone = dialog.Phone;
                string email = dialog.Email;
                db.AddCustomer(name, phone, email);
                var table = db.LoadTable("Customers");
                CustomersGrid.ItemsSource = table.DefaultView;
                OnLogEvent(UserId, "Added customer");
            }
        }
        private void UpdateCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = SelectCustomer();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((CustomersGrid.SelectedItem as DataRowView)["Id"].ToString());
                var name = dialog.Name;
                var phone = dialog.Phone;
                var email = dialog.Email;
                db.UpdateCustomer(id, name, phone, email);
                var table = db.LoadTable("Customers");
                CustomersGrid.ItemsSource = table.DefaultView;
                UnselectCustomer();
                OnLogEvent(UserId, "Updated customer");
            }
        }
        private void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = SelectCustomer();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((CustomersGrid.SelectedItem as DataRowView)["Id"].ToString());
                db.DeleteCustomer(id);
                var table = db.LoadTable("Customers");
                CustomersGrid.ItemsSource = table.DefaultView;
                UnselectCustomer();
                OnLogEvent(UserId, "Deleted customer");
            }
        }
        private CustomersInputWindow SelectCustomer()
        {
            var selectedCustomer = CustomersGrid.SelectedItem as DataRowView;
            var name = selectedCustomer["FullName"].ToString();
            var phone = selectedCustomer["Phone"].ToString();
            var email = selectedCustomer["Email"].ToString();
            return new CustomersInputWindow(name, phone, email);
        }
        private void UnselectCustomer()
        {
            UpdateCustomerButton.IsEnabled = false;
            DeleteCustomerButton.IsEnabled = false;
        }
        private void AddOrder(object sender, RoutedEventArgs e)
        {
            var ids = db.GetCustomersIds();
            var dialog = new OrdersInputWindow();
            dialog.FeelCustomersId(ids);
            if (dialog.ShowDialog() == true)
            {
                int customerId = dialog.CustomerId;
                string item = dialog.Item;
                DateTime? orderDate = dialog.OrderDate;
                string status = dialog.Status;
                double totalPrice = dialog.TotalPrice;
                db.AddOrder(customerId, item, orderDate, status, totalPrice);
                var table = db.LoadTable("Orders");
                OrdersGrid.ItemsSource = table.DefaultView;
                OnLogEvent(UserId, "Added order");
            }
        }
        
        private void UpdateOrder(object sender, RoutedEventArgs e)
        {
            var dialog = SelectOrder();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((OrdersGrid.SelectedItem as DataRowView)["Id"].ToString());
                int customerId = dialog.CustomerId;
                string item = dialog.Item;
                DateTime? orderDate = dialog.OrderDate;
                string status = dialog.Status;
                double totalPrice = dialog.TotalPrice;
                db.UpdateOrder(id, customerId, item, orderDate, status, totalPrice);
                var table = db.LoadTable("Orders");
                OrdersGrid.ItemsSource = table.DefaultView;
                UnselectOrder();
                OnLogEvent(UserId, "Updated order");
            }
        }
        private void DeleteOrder(object sender, RoutedEventArgs e)
        {
            var dialog = SelectOrder();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((OrdersGrid.SelectedItem as DataRowView)["Id"].ToString());
                db.DeleteOrder(id);
                var table = db.LoadTable("Orders");
                OrdersGrid.ItemsSource = table.DefaultView;
                UnselectOrder();
                OnLogEvent(UserId, "Deleted order");
            }
        }
        private OrdersInputWindow SelectOrder()
        {
            var selectedOrder = OrdersGrid.SelectedItem as DataRowView;
            var customerId = int.Parse(selectedOrder["CustomerId"].ToString());
            var item = selectedOrder["Item"].ToString();
            var orderDate = DateTime.Parse(selectedOrder["OrderDate"].ToString());
            var status = selectedOrder["Status"].ToString();
            var totalPrice = double.Parse(selectedOrder["TotalPrice"].ToString());
            var ids = db.GetCustomersIds();
            return new OrdersInputWindow(customerId, item, orderDate, status, totalPrice, ids);
        }
        private void UnselectOrder()
        {
            UpdateCustomerButton.IsEnabled = false;
            DeleteCustomerButton.IsEnabled = false;
        }
    }
}
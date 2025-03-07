using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
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
using Azure;
using Newtonsoft.Json;
using ScottPlot;

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
        private Role Role = Lab4.Role.None;
        public int UserId { get; set; }
        public delegate void LogEventHandler(object sender, int userId, string action);
        public event LogEventHandler LogEvent;
        public Dictionary<string, string> Queries = new Dictionary<string, string>
        {
            {"Customers with 'gmail.com'", "SELECT * FROM Customers WHERE Email LIKE '%gmail.com'"},
            {"Count of orders for a customer", "SELECT c.FullName AS Name, COUNT(o.Item) AS OrdersCount From Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId GROUP BY c.FullName"},
            {"Added orders actions", "SELECT * FROM Logs WHERE Action LIKE '%Added order%'"},
            {"Completed orders for a customer", "SELECT c.FullName AS Name, Count(o.Item) AS OrdersCount, o.Status AS Status From Customers c JOIN Orders o ON c.ID = o.CustomerId WHERE o.Status = 'Completed' GROUP By c.FullName, o.Status"},
            {"Pending orders", "SELECT * From Orders WHERE Status = 'Pending'"},
            {"Items with price > 3000", "SELECT DISTINCT o.Item AS Item, o.TotalPrice AS Price From Orders o WHERE o.TotalPrice > 3000"},
            {"Total price for a customer", "SELECT c.FullName AS Name, SUM(o.TotalPrice) AS TotalPrice From Customers c JOIN Orders o ON c.Id = o.CustomerId GROUP BY c.FullName"},
            {"Orders in date range", "SELECT * FROM Orders WHERE OrderDate BETWEEN '2025.01.01' AND '2025.12.12'"},
            {"Cancelled orders > 5000", "SELECT * FROM Orders WHERE Status = 'Cancelled' AND TotalPrice > 5000"},
            {"Total price for completed orders", "SELECT o.Status AS Status, SUM(o.TotalPrice) AS TotalPrice FROM Orders o WHERE Status = 'Completed' GROUP BY o.Status"}
        };
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

        private async void AddLog(int userId, string action)
        {
            string request = $"ADD_LOG|{userId}|{action}";
            await SendRequest(request);
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
            PrintReport.IsEnabled = true;
            var selectedCustomer = CustomersGrid.SelectedItem as DataRowView;
            if (selectedCustomer != null && int.TryParse(selectedCustomer["Id"].ToString(), out var id))
            {
                string query = $"SELECT o.OrderDate AS OrderDate, o.TotalPrice AS TotalPrice FROM Orders o WHERE o.CustomerId = {id}";
                CreateOrdersPlot(query);
            }
        }
        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrderButton.IsEnabled = true;
            DeleteOrderButton.IsEnabled = true;
        }

        private async void CreateOrdersPlot(string query)
        {
            OrdersPlot.Plot.Clear();
            var table = await GetTable($"QUERY|{query}");
            var orderDates = new List<DateTime>();
            var orderPrices = new List<double>();
            foreach (DataRow row in table.Rows)
            {
                orderDates.Add(Convert.ToDateTime(row["OrderDate"]));
                orderPrices.Add(Convert.ToDouble(row["TotalPrice"]));
            }
            OrdersPlot.Plot.Add.Scatter(orderDates.Select(d => d.ToOADate()).ToArray(), orderPrices.ToArray());
            OrdersPlot.Plot.Axes.DateTimeTicksBottom();
            OrdersPlot.Plot.YLabel("Total Price");
            OrdersPlot.Plot.XLabel("Order Date");
            OrdersPlot.Plot.Axes.Margins(0, 0);
            OrdersPlot.Refresh();
            GenerateReport(table);
        }

        private void GenerateReport(DataTable table)
        {
            string directoryPath = AppContext.BaseDirectory;
            string baseFileName = "Report.csv";
            string filePath = System.IO.Path.Combine(directoryPath, baseFileName);

            int counter = 1;

            while (File.Exists(filePath))
            {
                string newFileName = $"Report ({counter}).csv";
                filePath = System.IO.Path.Combine(directoryPath, newFileName);
                counter++;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(",", table.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));
            foreach (DataRow row in table.Rows)
            {
                sb.AppendLine(string.Join(",", row.ItemArray.Select(field => field.ToString())));
            }

            File.WriteAllText(filePath, sb.ToString());
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();
                Table docTable = new Table();
                doc.Blocks.Add(docTable);

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    docTable.Columns.Add(new TableColumn());
                }

                TableRowGroup headerGroup = new TableRowGroup();
                docTable.RowGroups.Add(headerGroup);
                TableRow headerRow = new TableRow();
                headerGroup.Rows.Add(headerRow);

                foreach (DataColumn column in table.Columns)
                {
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run(column.ColumnName)))
                        { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, Padding = new Thickness(5) });
                }

                TableRowGroup dataGroup = new TableRowGroup();
                docTable.RowGroups.Add(dataGroup);

                foreach (DataRow row in table.Rows)
                {
                    TableRow dataRow = new TableRow();
                    dataGroup.Rows.Add(dataRow);

                    foreach (var cell in row.ItemArray)
                    {
                        dataRow.Cells.Add(new TableCell(new Paragraph(new Run(cell.ToString())))
                            { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, Padding = new Thickness(5) });
                    }
                }

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Друк таблиці");
            }
        }
        private async void LoadData()
        {
            var table = await GetTable("LOAD_TABLE|Users");
            UsersGrid.ItemsSource = table.DefaultView;
            table = await GetTable("LOAD_TABLE|Customers");
            CustomersGrid.ItemsSource = table.DefaultView;
            table = await GetTable("LOAD_TABLE|Orders");
            OrdersGrid.ItemsSource = table.DefaultView;
            table = await GetTable("LOAD_TABLE|Logs");
            LogsGrid.ItemsSource = table.DefaultView;
        }
        private void RefreshData(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private async Task<string> SendRequest(string request)
        {
            using (TcpClient client = new TcpClient("192.168.0.103", 1433))
            {
                NetworkStream stream = client.GetStream();
                byte[] requestData = Encoding.UTF8.GetBytes(request);
                await stream.WriteAsync(requestData, 0, requestData.Length);

                byte[] responseBuffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                if (response != "")
                {
                    MessageBox.Show(response);
                }

                return response;
            }
        }
        public static async Task<DataTable> GetTable(string request)
        {
            using (TcpClient client = new TcpClient("192.168.0.103", 1433))
            {
                NetworkStream stream = client.GetStream();
                byte[] requestData = Encoding.UTF8.GetBytes(request);
                await stream.WriteAsync(requestData, 0, requestData.Length);
                byte[] responseBuffer = new byte[4096];
                StringBuilder fullResponse = new StringBuilder();

                int bytesRead;
                do
                {
                    bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                    fullResponse.Append(Encoding.UTF8.GetString(responseBuffer, 0, bytesRead));
                }
                while (bytesRead == responseBuffer.Length);

                string response = fullResponse.ToString();
                var responseParts = response.Split('|');
                DataTable table = JsonConvert.DeserializeObject<DataTable>(responseParts[0]);

                if (responseParts.Length > 1 && !string.IsNullOrEmpty(responseParts[1]))
                {
                    MessageBox.Show(responseParts[1]);
                }

                return table;
            }
        }
        private async void AddUser(object sender, RoutedEventArgs e)
        {
            var dialog = new UsersInputWindow();
            if (dialog.ShowDialog() == true)
            {
                string username = dialog.Username;
                string password = dialog.Password;
                string role = dialog.Role;
                string request = $"ADD_USER|{username}|{password}|{role}";
                string response = await SendRequest(request);
                if (response.Contains("додано"))
                {
                    var table = await GetTable("LOAD_TABLE|Users");
                    UsersGrid.ItemsSource = table.DefaultView;
                    OnLogEvent(UserId, "Added user");
                }
            }
        }
        private async void UpdateUser(object sender, RoutedEventArgs e)
        {
            var dialog = SelectUser();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((UsersGrid.SelectedItem as DataRowView)["Id"].ToString());
                var username = dialog.Username;
                var password = dialog.Password;
                var role = dialog.Role;
                string request = $"UPDATE_USER|{id}|{username}|{password}|{role}";
                string response = await SendRequest(request);
                if (response.Contains("оновлено"))
                {
                    var table = await GetTable("LOAD_TABLE|Users");
                    UsersGrid.ItemsSource = table.DefaultView;
                    UnselectUser();
                    OnLogEvent(UserId, "Updated user");
                }
                
            }
        }
        private async void DeleteUser(object sender, RoutedEventArgs e)
        {
            var dialog = SelectUser();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((UsersGrid.SelectedItem as DataRowView)["Id"].ToString());
                string request = $"DELETE_USER|{id}";
                string response = await SendRequest(request);
                if (response.Contains("видалено"))
                {
                    var table = await GetTable("LOAD_TABLE|Users");
                    UsersGrid.ItemsSource = table.DefaultView;
                    UnselectUser();
                    OnLogEvent(UserId, "Deleted user");
                }
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
        private async void AddCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomersInputWindow();
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.Name;
                string phone = dialog.Phone;
                string email = dialog.Email;
                string request = $"ADD_CUSTOMER|{name}|{phone}|{email}";
                string response = await SendRequest(request);
                if (response.Contains("додано"))
                {
                    var table = await GetTable("LOAD_TABLE|Customers");
                    CustomersGrid.ItemsSource = table.DefaultView;
                    OnLogEvent(UserId, "Added customer");
                }
                
            }
        }
        private async void UpdateCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = SelectCustomer();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((CustomersGrid.SelectedItem as DataRowView)["Id"].ToString());
                var name = dialog.Name;
                var phone = dialog.Phone;
                var email = dialog.Email;
                string request = $"UPDATE_CUSTOMER|{id}|{name}|{phone}|{email}";
                string response = await SendRequest(request);
                if (response.Contains("оновлено"))
                {
                    var table = await GetTable("LOAD_TABLE|Customers");
                    CustomersGrid.ItemsSource = table.DefaultView;
                    UnselectCustomer();
                    OnLogEvent(UserId, "Updated customer");
                }
            }
        }
        private async void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            var dialog = SelectCustomer();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((CustomersGrid.SelectedItem as DataRowView)["Id"].ToString());
                string request = $"DELETE_CUSTOMER|{id}";
                string response = await SendRequest(request);
                if (response.Contains("видалено"))
                {
                    var table = await GetTable("LOAD_TABLE|Customers");
                    CustomersGrid.ItemsSource = table.DefaultView;
                    UnselectCustomer();
                    OnLogEvent(UserId, "Deleted customer");
                }
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
        private async void AddOrder(object sender, RoutedEventArgs e)
        {
            var customers = await GetTable("LOAD_TABLE|Orders");
            DataTable ids = customers.DefaultView.ToTable(false, "CustomerId");
            var dialog = new OrdersInputWindow();
            dialog.FeelCustomersId(ids);
            if (dialog.ShowDialog() == true)
            {
                int customerId = dialog.CustomerId;
                string item = dialog.Item;
                DateTime? orderDate = dialog.OrderDate;
                string status = dialog.Status;
                double totalPrice = dialog.TotalPrice;
                string request = $"ADD_ORDER|{customerId}|{item}|{orderDate}|{status}|{totalPrice}";
                string response = await SendRequest(request);
                if (response.Contains("додано"))
                {
                    var table = await GetTable("LOAD_TABLE|Orders");
                    OrdersGrid.ItemsSource = table.DefaultView;
                    OnLogEvent(UserId, "Added order");
                }
            }
        }
        
        private async void UpdateOrder(object sender, RoutedEventArgs e)
        {
            var dialog = await SelectOrder();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((OrdersGrid.SelectedItem as DataRowView)["Id"].ToString());
                int customerId = dialog.CustomerId;
                string item = dialog.Item;
                DateTime? orderDate = dialog.OrderDate;
                string status = dialog.Status;
                double totalPrice = dialog.TotalPrice;
                string request = $"UPDATE_ORDER|{id}|{customerId}|{item}|{orderDate}|{status}|{totalPrice}";
                string response = await SendRequest(request);
                if (response.Contains("оновлено"))
                {
                    var table = await GetTable("LOAD_TABLE|Orders");
                    OrdersGrid.ItemsSource = table.DefaultView;
                    UnselectOrder();
                    OnLogEvent(UserId, "Updated order");
                }
            }
        }
        private async void DeleteOrder(object sender, RoutedEventArgs e)
        {
            var dialog = await SelectOrder();
            if (dialog.ShowDialog() == true)
            {
                var id = int.Parse((OrdersGrid.SelectedItem as DataRowView)["Id"].ToString());
                string request = $"DELETE_ORDER|{id}";
                string response = await SendRequest(request);
                if (response.Contains("видалено"))
                {
                    var table = await GetTable("LOAD_TABLE|Orders");
                    OrdersGrid.ItemsSource = table.DefaultView;
                    UnselectOrder();
                    OnLogEvent(UserId, "Deleted order");
                }
            }
        }
        private async Task<OrdersInputWindow> SelectOrder()
        {
            var selectedOrder = OrdersGrid.SelectedItem as DataRowView;
            var customerId = int.Parse(selectedOrder["CustomerId"].ToString());
            var item = selectedOrder["Item"].ToString();
            var orderDate = DateTime.Parse(selectedOrder["OrderDate"].ToString());
            var status = selectedOrder["Status"].ToString();
            var totalPrice = double.Parse(selectedOrder["TotalPrice"].ToString());
            var customers = await GetTable("LOAD_TABLE|Orders");
            DataTable ids = customers.DefaultView.ToTable(false, "CustomerId");
            return new OrdersInputWindow(customerId, item, orderDate, status, totalPrice, ids);
        }
        private void UnselectOrder()
        {
            UpdateCustomerButton.IsEnabled = false;
            DeleteCustomerButton.IsEnabled = false;
        }

        private async void DoQuery(object sender, RoutedEventArgs e)
        {
            var dialog = new QueriesWindow(Queries.Keys.ToList());
            if (dialog.ShowDialog() == true)
            {
                var query = dialog.Query;
                var table = await GetTable($"QUERY|{Queries[query]}");
                QueriesGrid.ItemsSource = table.DefaultView;
                if (query.Contains("range"))
                {
                    GenerateReport(table);
                }
            }
        }

        private void PrintReport_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
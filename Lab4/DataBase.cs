using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Lab4
{
    public class DataBase
    {
        SqlConnection _sqlConnection = new SqlConnection(@"Data Source=Romaro-PC\SQLEXPRESS;Initial Catalog=Lab4;Integrated Security=True;TrustServerCertificate=True");
        public void LoadTable(string tableName, DataGrid grid)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"SELECT * FROM {tableName}", _sqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                grid.ItemsSource = dataTable.DefaultView;
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження {tableName}: {ex.Message}");
            }
        }

        public DataTable GetCustomersIds()
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"SELECT CustomerId FROM Orders", _sqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                CloseConnection();
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження Orders: {ex.Message}");
                return null;
            }
        }
        public void AddUser(string username, string password, string role)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("INSERT INTO Users (Username, PasswordHash, Role) VALUES (@username, @password, @role)", _sqlConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@role", role);
                command.ExecuteNonQuery();
                MessageBox.Show("Користувача додано.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання користувача: {ex.Message}");
            }
        }
        public void UpdateUser(int id, string username, string password, string role)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("Update Users SET Username = @username, PasswordHash = @password, Role = @role WHERE Id = @id", _sqlConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@role", role);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                MessageBox.Show("Користувача оновлено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення користувача: {ex.Message}");
            }
        }
        public void DeleteUser(int id)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"DELETE from Users WHERE Id = {id}", _sqlConnection);
                command.ExecuteNonQuery();
                MessageBox.Show("Користувача видалено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення користувача: {ex.Message}");
            }
        }

        public void AddCustomer(string name, string phone, string email)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("INSERT INTO Customers (FullName, Phone, Email) VALUES (@name, @phone, @email)", _sqlConnection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@phone", phone);
                command.Parameters.AddWithValue("@email", email);
                command.ExecuteNonQuery();
                MessageBox.Show("Клієнта додано.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання клієнта: {ex.Message}");
            }
        }

        public void UpdateCustomer(int id, string name, string phone, string email)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("Update Customers SET FullName = @name, Phone = @phone, Email = @email WHERE Id = @id", _sqlConnection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@phone", phone);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                MessageBox.Show("Клієнта оновлено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення клієнта: {ex.Message}");
            }
        }

        public void DeleteCustomer(int id)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"DELETE from Customers WHERE Id = {id}", _sqlConnection);
                command.ExecuteNonQuery();
                MessageBox.Show("Клієнта видалено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення клієнта: {ex.Message}");
            }
        }

        public void AddOrder(int customerId, string item, DateTime? orderDate, string status, double totalPrice)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("INSERT INTO Orders (CustomerId, Item, OrderDate, Status, TotalPrice) VALUES (@customerId, @item, @orderDate, @status, @totalPrice)", _sqlConnection);
                command.Parameters.AddWithValue("@customerId", customerId);
                command.Parameters.AddWithValue("@item", item);
                command.Parameters.AddWithValue("@orderDate", orderDate);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@totalPrice", totalPrice);
                command.ExecuteNonQuery();
                MessageBox.Show("Замовлення додано.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання замовлення: {ex.Message}");
            }
        }

        public void UpdateOrder(int id, int customerId, string item, DateTime? orderDate, string status,
            double totalPrice)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("Update Orders SET CustomerId = @customerId, Item = @item, OrderDate = @orderDate, Status = @status, TotalPrice = @totalPrice WHERE Id = @id", _sqlConnection);
                command.Parameters.AddWithValue("@customerId", customerId);
                command.Parameters.AddWithValue("@item", item);
                command.Parameters.AddWithValue("@orderDate", orderDate);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@totalPrice", totalPrice);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                MessageBox.Show("Замовлення оновлено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення замовлення: {ex.Message}");
            }
        }

        public void DeleteOrder(int id)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"DELETE from Orders WHERE Id = {id}", _sqlConnection);
                command.ExecuteNonQuery();
                MessageBox.Show("Замовлення видалено.");
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення замовлення: {ex.Message}");
            }
        }
        public void OpenConnection()
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }
        }
        public void CloseConnection()
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Open)
            {
                _sqlConnection.Close();
            }
        }
        public SqlConnection GetConnection()
        {
            return _sqlConnection;
        }
    }
}

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


        public DataTable ProceedQuery(string query)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand(query, _sqlConnection);
                command.CommandTimeout = 3;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                try
                {
                    adapter.Fill(dataTable);
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("timeout"))
                    {
                        MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                    }
                    return dataTable;
                }

                CloseConnection();
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
                return null;
            }
        }
        public DataTable LoadTable(string tableName)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"SELECT * FROM {tableName} WITH (NOLOCK)", _sqlConnection);
                command.CommandTimeout = 3;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                try
                {
                    adapter.Fill(dataTable);
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("timeout"))
                    {
                        MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                    }
                    return dataTable;
                }

                CloseConnection();
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження {tableName}: {ex.Message}");
                return null;
            }
        }

        public DataTable GetCustomersIds()
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand($"SELECT CustomerId FROM Orders WITH (NOLOCK)", _sqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                try
                {
                    adapter.Fill(dataTable);
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("timeout"))
                    {
                        MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        return dataTable;
                    }
                    MessageBox.Show($"Помилка завантаження Orders: {ex.Message}");
                    return dataTable;
                }
                CloseConnection();
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження Orders: {ex.Message}");
                return null;
            }
        }

        public void AddLog(int userId, string action)
        {
            try
            {
                OpenConnection();
                SqlCommand command = new SqlCommand("INSERT INTO Logs (UserId, Action, Timestamp) VALUES (@userId, @action, @timestamp)", _sqlConnection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@action", action);
                command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                command.ExecuteNonQuery();
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання: {ex.Message}");
            }
        }
        public void AddUser(string username, string password, string role)
        {
            try
            {
                OpenConnection();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO Users WITH (TABLOCKX) (Username, PasswordHash, Role) VALUES (@username, @password, @role)", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", role);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Користувача додано.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");

                        }
                        transaction.Rollback();
                    }
                }

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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command =
                            new SqlCommand(
                                "Update Users WITH(ROWLOCK) SET Username = @username, PasswordHash = @password, Role = @role WHERE Id = @id",
                                _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", role);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Користувача оновлено.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Цей рядок заблокований іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Users WITH (TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Користувача видалено.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO Customers WITH (TABLOCKX) (FullName, Phone, Email) VALUES (@name, @phone, @email)", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@phone", phone);
                        command.Parameters.AddWithValue("@email", email);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Клієнта додано.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("Update Customers WITH (ROWLOCK) SET FullName = @name, Phone = @phone, Email = @email WHERE Id = @id", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@phone", phone);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Клієнта оновлено.");
                    }

                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Цей рядок заблокований іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }

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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Customers WITH(TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Клієнта видалено.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO Orders WITH (TABLOCKX) (CustomerId, Item, OrderDate, Status, TotalPrice) VALUES (@customerId, @item, @orderDate, @status, @totalPrice)", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@customerId", customerId);
                        command.Parameters.AddWithValue("@item", item);
                        command.Parameters.AddWithValue("@orderDate", orderDate);
                        command.Parameters.AddWithValue("@status", status);
                        command.Parameters.AddWithValue("@totalPrice", totalPrice);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Замовлення додано.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
                
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("Update Orders WITH (ROWLOCK) SET CustomerId = @customerId, Item = @item, OrderDate = @orderDate, Status = @status, TotalPrice = @totalPrice WHERE Id = @id", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@customerId", customerId);
                        command.Parameters.AddWithValue("@item", item);
                        command.Parameters.AddWithValue("@orderDate", orderDate);
                        command.Parameters.AddWithValue("@status", status);
                        command.Parameters.AddWithValue("@totalPrice", totalPrice);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Замовлення оновлено.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Цей рядок заблокований іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
                
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
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Orders WITH(TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(5000);
                        transaction.Commit();
                        MessageBox.Show("Замовлення видалено.");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            MessageBox.Show("Таблиця заблокована іншим користувачем, спробуйте пізніше.");
                        }
                        transaction.Rollback();
                    }
                }
                
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

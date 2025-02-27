using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;

namespace Server
{
    internal class DataBase
    {
        private string _connectionString =
            @"Data Source=Romaro-PC\SQLEXPRESS;Initial Catalog=Lab4;Integrated Security=True;TrustServerCertificate=True";

        public Dictionary<string, string> Queries = new Dictionary<string, string>
        {
            { "Customers with 'gmail.com'", "SELECT * FROM Customers WHERE Email LIKE '%gmail.com'" },
            {
                "Count of orders for a customer",
                "SELECT c.FullName AS Name, COUNT(o.Item) AS OrdersCount From Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId GROUP BY c.FullName"
            },
            { "Added orders actions", "SELECT * FROM Logs WHERE Action LIKE '%Added order%'" },
            {
                "Completed orders for a customer",
                "SELECT c.FullName AS Name, Count(o.Item) AS OrdersCount, o.Status AS Status From Customers c JOIN Orders o ON c.ID = o.CustomerId WHERE o.Status = 'Completed' GROUP By c.FullName, o.Status"
            }
        };

        public async Task<(DataTable, string)> ProceedQuery(string query)
        {
            try
            {
                using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
                {
                    await _sqlConnection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, _sqlConnection);
                    command.CommandTimeout = 3;
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    string result = "";
                    try
                    {
                        adapter.Fill(dataTable);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            result = "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }
                    }
                    await _sqlConnection.CloseAsync();
                    return (dataTable, result);
                }
                
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }
        public async Task<(DataTable, string)> LoadTable(string tableName)
        {
            try
            {
                using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
                {
                    await _sqlConnection.OpenAsync();
                    SqlCommand command = new SqlCommand($"SELECT * FROM {tableName} WITH (NOLOCK)", _sqlConnection);
                    command.CommandTimeout = 3;
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    string result = "";
                    try
                    {
                        adapter.Fill(dataTable);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("timeout"))
                        {
                            result = "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }
                    }
                    await _sqlConnection.CloseAsync();
                    return (dataTable, result);
                }
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }

        public async Task<string> AddLog(int userId, string action)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                try
                {
                    await _sqlConnection.OpenAsync();
                    SqlCommand command = new SqlCommand("INSERT INTO Logs (UserId, Action, Timestamp) VALUES (@userId, @action, @timestamp)", _sqlConnection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@action", action);
                    command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    await command.ExecuteNonQueryAsync();
                    await _sqlConnection.CloseAsync();
                    return "";
                }
                catch (Exception ex)
                {
                    await _sqlConnection.CloseAsync();
                    return $"Помилка додавання: {ex.Message}";
                }
            }
                
        }
        public async Task<string> AddUser(string username, string password, string role)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command =
                            new SqlCommand(
                                "INSERT INTO Users WITH (TABLOCKX) (Username, PasswordHash, Role) VALUES (@username, @password, @role)",
                                _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", role);
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Користувача додано.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка додавання користувача: {ex.Message}";
                    }
                }
            }
                
        }
        public async Task<string> UpdateUser(int id, string username, string password, string role)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
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
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Користувача оновлено.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Цей рядок заблокований іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка оновлення користувача: {ex.Message}";

                    }
                }
            }
                
        }
        public async Task<string> DeleteUser(int id)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Users WITH (TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Користувача видалено.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }

                        return $"Помилка видалення користувача: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> AddCustomer(string name, string phone, string email)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO Customers WITH (TABLOCKX) (FullName, Phone, Email) VALUES (@name, @phone, @email)", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@phone", phone);
                        command.Parameters.AddWithValue("@email", email);
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Клієнта додано.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }

                        return $"Помилка додавання клієнта: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> UpdateCustomer(int id, string name, string phone, string email)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
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
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Клієнта оновлено.";
                    }

                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Цей рядок заблокований іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка оновлення клієнта: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> DeleteCustomer(int id)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Customers WITH(TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Клієнта видалено.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка видалення клієнта: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> AddOrder(int customerId, string item, DateTime? orderDate, string status, double totalPrice)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
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
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Замовлення додано.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка додавання замовлення: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> UpdateOrder(int id, int customerId, string item, DateTime? orderDate, string status,
            double totalPrice)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
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
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Замовлення оновлено.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Цей рядок заблокований іншим користувачем, спробуйте пізніше.";
                        }
                        return $"Помилка оновлення замовлення: {ex.Message}";
                    }
                }
            }
                
        }

        public async Task<string> DeleteOrder(int id)
        {
            using (SqlConnection _sqlConnection = new SqlConnection(_connectionString))
            {
                await _sqlConnection.OpenAsync();
                using (SqlTransaction transaction = _sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand($"DELETE from Orders WITH(TABLOCKX) WHERE Id = {id}", _sqlConnection, transaction);
                        command.CommandTimeout = 3;
                        await command.ExecuteNonQueryAsync();
                        await Task.Delay(5000);
                        await transaction.CommitAsync();
                        await _sqlConnection.CloseAsync();
                        return "Замовлення видалено.";
                    }
                    catch (SqlException ex)
                    {
                        await transaction.RollbackAsync();
                        await _sqlConnection.CloseAsync();
                        if (ex.Message.Contains("timeout"))
                        {
                            return "Таблиця заблокована іншим користувачем, спробуйте пізніше.";
                        }

                        return $"Помилка видалення замовлення: {ex.Message}";
                    }
                }
            }
                

        }


    }
}


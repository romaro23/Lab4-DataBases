using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server
{
    internal class Server
    {
        private static DataBase db = new DataBase();
        static async Task Main()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClient(client);
            }
        }
        static async Task HandleClient(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Request: {request}");

            string response = await HandleRequest(request);
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseData, 0, responseData.Length);
        }
        static async Task<string> HandleRequest(string request)
        {
            var requestParts = request.Split('|');
            string command = requestParts[0];
            string? response;
            int id;
            string? username, password, role;
            string? name, phone, email;
            int customerId;
            string item, status;
            DateTime? orderDate;
            double totalPrice;
            (DataTable, string) result;
            string json;
            switch (command)
            {
                case "ADD_USER":
                    username = requestParts[1];
                    password = requestParts[2];
                    role = requestParts[3];
                    response = await db.AddUser(username, password, role);
                    return response;
                case "UPDATE_USER":
                    id = int.Parse(requestParts[1]);
                    username = requestParts[2];
                    password = requestParts[3];
                    role = requestParts[4];
                    response = await db.UpdateUser(id, username, password, role);
                    return response;
                case "DELETE_USER":
                    id = int.Parse(requestParts[1]);
                    response = await db.DeleteUser(id);
                    return response;
                case "ADD_CUSTOMER":
                    name = requestParts[1];
                    phone = requestParts[2];
                    email = requestParts[3];
                    response = await db.AddCustomer(name, phone, email);
                    return response;
                case "UPDATE_CUSTOMER":
                    id = int.Parse(requestParts[1]);
                    name = requestParts[2];
                    phone = requestParts[3];
                    email = requestParts[4];
                    response = await db.UpdateCustomer(id, name, phone, email);
                    return response;
                case "DELETE_CUSTOMER":
                    id = int.Parse(requestParts[1]);
                    response = await db.DeleteCustomer(id);
                    return response;
                case "ADD_ORDER":
                    customerId = int.Parse(requestParts[1]);
                    item = requestParts[2];
                    orderDate = DateTime.Parse(requestParts[3]);
                    status = requestParts[4];
                    totalPrice = double.Parse(requestParts[5]);
                    response = await db.AddOrder(customerId, item, orderDate, status, totalPrice);
                    return response;
                case "UPDATE_ORDER":
                    id = int.Parse(requestParts[1]);
                    customerId = int.Parse(requestParts[2]);
                    item = requestParts[3];
                    orderDate = DateTime.Parse(requestParts[4]);
                    status = requestParts[5];
                    totalPrice = double.Parse(requestParts[6]);
                    response = await db.UpdateOrder(id, customerId, item, orderDate, status, totalPrice);
                    return response;
                case "DELETE_ORDER":
                    id = int.Parse(requestParts[1]);
                    response = await db.DeleteOrder(id);
                    return response;
                case "ADD_LOG":
                    int userId = int.Parse(requestParts[1]);
                    string action = requestParts[2];
                    response = await db.AddLog(userId, action);
                    return response;
                case "LOAD_TABLE":
                    string tableName = requestParts[1];
                    result = await db.LoadTable(tableName);
                    json = JsonConvert.SerializeObject(result.Item1);
                    return $"{json}|{result.Item2}";
                case "QUERY":
                    string query = requestParts[1];
                    result = await db.ProceedQuery(query);
                    json = JsonConvert.SerializeObject(result.Item1);
                    return $"{json}|{result.Item2}";
                default:
                    return "Unknown command";
            }
        }

    }
}

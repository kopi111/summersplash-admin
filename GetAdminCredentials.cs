using System;
using System.Data;
using Microsoft.Data.SqlClient;

class GetAdminCredentials
{
    static void GetAdminUsers()
    {
        string connectionString = "Server=69.164.241.104,1433;Database=SummerSplashDB;User Id=WebAppUser;Password=WebApp2024!;TrustServerCertificate=true;MultipleActiveResultSets=true;";

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully!\n");

                // Query for admin users
                string query = @"
                    SELECT TOP 5
                        UserId,
                        FirstName,
                        LastName,
                        Email,
                        Position,
                        IsActive,
                        IsApproved
                    FROM Users
                    WHERE IsActive = 1 AND IsApproved = 1
                    ORDER BY
                        CASE WHEN Position = 'Admin' THEN 1
                             WHEN Position = 'Manager' THEN 2
                             ELSE 3 END,
                        UserId";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Available Active Users:");
                        Console.WriteLine("======================");

                        while (reader.Read())
                        {
                            Console.WriteLine($"\nEmail: {reader["Email"]}");
                            Console.WriteLine($"Name: {reader["FirstName"]} {reader["LastName"]}");
                            Console.WriteLine($"Position: {reader["Position"]}");
                            Console.WriteLine($"Active: {reader["IsActive"]}");
                            Console.WriteLine($"Approved: {reader["IsApproved"]}");
                            Console.WriteLine("---");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

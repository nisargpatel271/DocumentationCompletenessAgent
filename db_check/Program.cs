using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string[] hosts = { "localhost", "127.0.0.1" };
        foreach (var host in hosts)
        {
            try
            {
                Console.WriteLine($"Trying {host}...");
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Port = 5432,
                    Database = "doc_agent_db",
                    Username = "admin",
                    Password = "password123"
                };
                
                using (var conn = new NpgsqlConnection(builder.ToString()))
                {
                    conn.Open();
                    Console.WriteLine($"Connected to {host}!");
                    using (var cmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Table: {reader.GetString(0)}");
                        }
                    }
                    return; // Success
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to {host}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                }
            }
        }
    }
}

using AzureDBAccess.Entities;
using AzureDBAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        TutorialExample();
        Console.WriteLine("\n\n\n");
        await CreateExample();
        Console.WriteLine("\n\n\n");
        await GetExample();
        Console.WriteLine("\n\n\n");
    }

    private static string GetConnectionString()
    {
        IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        string connectionString = config["connectionString"] ?? throw new Exception("User secret 'connectionString' not found");
        Console.WriteLine($"Found connection string: {connectionString}");
        return connectionString;
    }

    private static void RunThroughConnection(Action<SqlConnection> setup, Action<SqlConnection> execution)
    {
        // Create and open the connection in a using block. This
        // ensures that all resources will be closed and disposed
        // when the code exits.
        Console.WriteLine("Creating the SQL connection...");
        using (SqlConnection connection = new(GetConnectionString()))
        {
            Console.WriteLine("Connection created.");

            // Perform any setup needed by the method
            setup(connection);

            // Execute the method, surrounding with a try/except clause
            try
            {
                execution(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("SQL connection disposed of.");
        Console.ReadLine();
    }

    private async static Task RunThroughConnectionAsync(
        Func<SqlConnection, Task> setup,
        Func<SqlConnection, Task> execution)
    {
        // Create and open the connection in a using block. This
        // ensures that all resources will be closed and disposed
        // when the code exits.
        Console.WriteLine("Creating the SQL connection...");
        using (SqlConnection connection = new(GetConnectionString()))
        {
            Console.WriteLine("Connection created.");

            // Perform any setup needed by the method
            await setup(connection);

            // Execute the method, surrounding with a try/except clause
            try
            {
                await execution(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n---ERROR---");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("---ERROR---\n");
            }
        }
        Console.WriteLine("SQL connection disposed of.");
        Console.ReadLine();
    }

    private static void TutorialExample()
    {
        // Provide the query string with a parameter placeholder.
        const string queryString =
            "SELECT ProductID, ListPrice, Name from SalesLT.Product "
                + "WHERE ListPrice > @pricePoint "
                + "ORDER BY ListPrice DESC;";

        // Specify the parameter value.
        const int paramValue = 5;

        SqlCommand command = new();

        // Define the setup action
        void setup(SqlConnection connection)
        {
            // Create the Command and Parameter objects.
            command = new(queryString, connection);
            command.Parameters.AddWithValue("@pricePoint", paramValue);
        }

        // Define the execution action
        void execution(SqlConnection connection)
        {
            // Open the connection in a try/catch block.
            // Create and execute the DataReader, writing the result
            // set to the console window.
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("\t{0}\t{1}\t{2}",
                    reader[0], reader[1], reader[2]);
            }
            reader.Close();
        }

        // Use the RunThroughConnection method
        RunThroughConnection(setup, execution);
    }

    private static async Task CreateExample()
    {
        AddressDTO addressDTO = new()
        {
            AddressLine1 = "22915 Lanark St",
            City = "West Hills",
            StateProvince = "California",
            CountryRegion = "United States",
            PostalCode = "91304"
        };
        Console.WriteLine($"Created the following Address: {addressDTO}");

        // Define the setup action
        AddressRepository? addressRepo = null;
        Task setup(SqlConnection connection)
        {
            addressRepo = new(connection);
            return Task.CompletedTask;
        }

        // Define the execution action
        async Task execution(SqlConnection connection)
        {
            if (addressRepo is null)
            {
                Console.WriteLine("The address repository could not be set up.");
            }
            else
            {
                Console.WriteLine("Attempting to add the address.");
                var rowsAffected = await addressRepo.Create(addressDTO);
                Console.WriteLine($"Create command completed; {rowsAffected} rows were affected.");
            }
        }

        // Use the RunThroughConnection method
        await RunThroughConnectionAsync(setup, execution);
    }

    private static async Task GetExample()
    {
        // Define the IDs to attempt to look up
        int[] addressIds = [9, 450, 451, 1, 452];

        // Define the setup action
        AddressRepository? addressRepo = null;
        Task setup(SqlConnection connection)
        {
            addressRepo = new(connection);
            return Task.CompletedTask;
        }

        // Define the execution action
        async Task execution(SqlConnection connection)
        {
            if (addressRepo is null)
            {
                Console.WriteLine("The address repository could not be set up.");
            }
            else
            {
                Console.WriteLine("Looking for addresses with given IDs");
                foreach (var addressId in addressIds)
                {
                    var address = await addressRepo.Get(addressId);
                    Console.WriteLine($"Result for ID {addressId}: {address}");
                }
            }
        }

        await RunThroughConnectionAsync(setup, execution);
    }
}
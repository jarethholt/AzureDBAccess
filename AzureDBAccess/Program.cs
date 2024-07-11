using AzureDBAccess.Entities;
using AzureDBAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //TutorialExample();
        //Console.WriteLine("\n\n\n");
        _ = await CombinedExample();
        //var addressId = await CreateExample();
        //Console.WriteLine("\n\n\n");
        //Address? address = await GetExample(addressId);
        //Console.WriteLine("\n\n\n");
        //var rowsAffected = await DeleteExample(addressId);
        //Console.WriteLine("\n\n\n");
    }

    private static string GetConnectionString()
    {
        IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        string connectionString = config["connectionString"] ?? throw new Exception("User secret 'connectionString' not found");
        Console.WriteLine($"Found connection string: {connectionString}");
        return connectionString;
    }

    private async static Task<T?> RunThroughConnectionAsync<T>(
        Func<AddressRepository, Task<T>> execution)
    {
        T? output = default;

        // Create and open the connection in a using block. This
        // ensures that all resources will be closed and disposed
        // when the code exits.
        Console.WriteLine("Creating the SQL connection...");
        using (SqlConnection connection = new(GetConnectionString()))
        {
            Console.WriteLine("Connection created.");

            // Create a new address repository
            AddressRepository addressRepo = new(connection);

            // Execute the method, surrounding with a try/except clause
            try
            {
                output = await execution(addressRepo);
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
        return output;
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

        // Create and open the connection in a using block. This
        // ensures that all resources will be closed and disposed
        // when the code exits.
        Console.WriteLine("Creating the SQL connection...");
        using (SqlConnection connection = new(GetConnectionString()))
        {
            Console.WriteLine("Connection created.");

            // Create the Command and Parameter objects.
            SqlCommand command = new(queryString, connection);
            command.Parameters.AddWithValue("@pricePoint", paramValue);

            // Open the connection in a try/catch block.
            try
            {
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

    private static async Task<Address?> CombinedExample()
    {
        AddressDTO addressDTO = new()
        {
            AddressLine1 = "22915 Larkan St",
            City = "West Hills",
            StateProvince = "California",
            CountryRegion = "United States",
            PostalCode = "91304"
        };
        Console.WriteLine($"Starting the following AddressDTO: {addressDTO}");

        // Define the execution action
        async Task<Address?> execution(AddressRepository addressRepo)
        {
            Address? addressNew = null;
            // Add the address to the database, recording the ID
            Console.WriteLine("Attempting to add the address.");
            var addressId = await addressRepo.Create(addressDTO);
            if (addressId == default)
            {
                Console.WriteLine("\n---ERROR---");
                Console.WriteLine("Did not retrieve the AddressId with the INSERT command...");
                return addressNew;
            }
            Console.WriteLine($"Create command completed; added AddressId = {addressId}.");

            // Retrieve the entire Address object
            Console.WriteLine($"Retrieving the full object with ID {addressId}...");
            var addressOrig = await addressRepo.Get(addressId);
            if (addressOrig is null)
            {
                Console.WriteLine("\n---ERROR---");
                Console.WriteLine("Got null when retrieving the address...");
                return addressNew;
            }
            Console.WriteLine($"Complete Address object: {addressOrig}");

            // Update the address and re-retrieve to verify
            Console.WriteLine("Fixing a typo in the address...");
            var addressDTOFixed = addressDTO with { AddressLine1 = "22915 Lanark St" };
            var rowsAffected = await addressRepo.Update(addressId, addressDTOFixed);
            if (rowsAffected != 1)
                Console.WriteLine($"Somehow the update command affected {rowsAffected} rows instead of 1...");
            addressNew = await addressRepo.Get(addressId);
            if (addressNew is null)
            {
                Console.WriteLine("\n---ERROR---");
                Console.WriteLine("Got null when retrieving the address...");
                return addressNew;
            }
            else if (!addressNew.AddressLine1.Equals(addressDTOFixed.AddressLine1))
                Console.WriteLine($"The address was not updated correctly and has AddressLine1 = {addressNew.AddressLine1}...");
            else
                Console.WriteLine($"The address was updated correctly and now has AddressLine1 = {addressNew.AddressLine1}.");

            // Delete the address and verify it has been deleted
            Console.WriteLine("Deleting this address from the database.");
            rowsAffected = await addressRepo.Delete(addressId);
            if (rowsAffected != 1)
                Console.WriteLine($"Somehow the delete command affected {rowsAffected} rows instead of 1...");
            var addressDeleted = await addressRepo.Get(addressId);
            if (addressDeleted is not null)
            {
                Console.WriteLine("\n---ERROR---");
                Console.WriteLine($"The delete command did NOT delete the address: {addressDeleted}.");
            }
            else
                Console.WriteLine("Address successfully deleted!");

            return addressNew;
        }

        return await RunThroughConnectionAsync(execution);
    }

    private static async Task<int> CreateExample()
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

        // Define the execution action
        async Task<int> execution(AddressRepository addressRepo)
        {
            Console.WriteLine("Attempting to add the address.");
            var addressId = await addressRepo.Create(addressDTO);
            Console.WriteLine($"Create command completed; added AddressId = {addressId}.");
            return addressId;
        }

        // Use the RunThroughConnection method
        return await RunThroughConnectionAsync(execution);
    }

    private static async Task<Address?> GetExample(int addressId)
    {
        // Define the execution action
        async Task<Address?> execution(AddressRepository addressRepo)
        {
            Console.WriteLine($"Looking for address with ID {addressId}...");
            var address = await addressRepo.Get(addressId);
            Console.WriteLine($"Result for ID {addressId}: {address}");
            return address;
        }

        return await RunThroughConnectionAsync(execution);
    }

    private static async Task<int> DeleteExample(int addressId)
    {
        // Define the execution action
        async Task<int> execution(AddressRepository addressRepo)
        {
            Console.WriteLine($"Deleting address with ID {addressId}");
            var rowsAffected = await addressRepo.Delete(addressId);
            Console.WriteLine($"Rows affected when deleting ID {addressId}: {rowsAffected}");
            return rowsAffected;
        }

        return await RunThroughConnectionAsync(execution);
    }
}
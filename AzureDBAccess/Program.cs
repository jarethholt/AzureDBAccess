using Microsoft.Data.SqlClient;
//using System.Configuration;
using Microsoft.Extensions.Configuration;

//const string connectionString =
//    "Data Source=(local);Initial Catalog=Northwind;"
//    + "Integrated Security=true";
//string connectionString = ConfigurationManager.ConnectionStrings["Azure"].ConnectionString;
IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string connectionString = config["connectionString"] ?? throw new Exception("User secret 'connectionString' not found");

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
using (SqlConnection connection = new(connectionString))
{
    // Create the Command and Parameter objects.
    SqlCommand command = new(queryString, connection);
    command.Parameters.AddWithValue("@pricePoint", paramValue);

    // Open the connection in a try/catch block.
    // Create and execute the DataReader, writing the result
    // set to the console window.
    try
    {
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
        Console.WriteLine(ex.Message);
    }
    Console.ReadLine();
}

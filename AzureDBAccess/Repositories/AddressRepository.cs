using AzureDBAccess.Entities;
using Microsoft.Data.SqlClient;

namespace AzureDBAccess.Repositories;

internal class AddressRepository(SqlConnection connection)
{
    private readonly SqlConnection _connection = connection;

    private static DateTime CurrentDate() =>
        DateOnly.FromDateTime(DateTime.Now).ToDateTime(new TimeOnly());

    public async Task<int> Create(AddressDTO addressDTO)
    {
        using SqlCommand createCommand = new(@"
            INSERT INTO SalesLT.Address
            (AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode, rowguid, ModifiedDate)
            VALUES(@AddressLine1, @AddressLine2, @City, @StateProvince, @CountryRegion, @PostalCode, NEWID(), @ModifiedDate)
        ", _connection);

        createCommand.Parameters.AddWithValue("AddressLine1", addressDTO.AddressLine1);
        createCommand.Parameters.AddWithValue("AddressLine2", addressDTO.AddressLine2 is null ? DBNull.Value : addressDTO.AddressLine2);
        createCommand.Parameters.AddWithValue("City", addressDTO.City);
        createCommand.Parameters.AddWithValue("StateProvince", addressDTO.StateProvince);
        createCommand.Parameters.AddWithValue("CountryRegion", addressDTO.CountryRegion);
        createCommand.Parameters.AddWithValue("PostalCode", addressDTO.PostalCode);
        createCommand.Parameters.AddWithValue("ModifiedDate", CurrentDate());

        _connection.Open();
        return await createCommand.ExecuteNonQueryAsync();
    }
}

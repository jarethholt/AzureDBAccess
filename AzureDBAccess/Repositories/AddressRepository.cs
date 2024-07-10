using System.Data;
using AzureDBAccess.Entities;
using AzureDBAccess.Extensions;
using Microsoft.Data.SqlClient;

namespace AzureDBAccess.Repositories;

internal class AddressRepository(SqlConnection connection)
{
    private readonly SqlConnection _connection = connection;

    private static DateTime CurrentDate() =>
        DateOnly.FromDateTime(DateTime.Now).ToDateTime(new TimeOnly());

    public async Task<Address?> Get(int addressId)
    {
        using SqlCommand getCommand = new(@"
            SELECT
                AddressId
                ,AddressLine1
                ,AddressLine2
                ,City
                ,StateProvince
                ,CountryRegion
                ,PostalCode
                ,rowguid
                ,ModifiedDate
            FROM SalesLT.Address
            WHERE AddressId = @AddressId
        ", _connection);
        getCommand.Parameters.AddWithValue("AddressId", addressId);

        Address? address = null;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using SqlDataReader reader = await getCommand.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            if (await reader.ReadAsync())
                address = new()
                {
                    AddressId = reader.GetInt32(0),
                    AddressLine1 = reader.GetString(1),
                    AddressLine2 = reader.GetNullableString(2),
                    City = reader.GetString(3),
                    StateProvince = reader.GetString(4),
                    CountryRegion = reader.GetString(5),
                    PostalCode = reader.GetString(6),
                    RowGuid = reader.GetGuid(7),
                    ModifiedDate = reader.GetDateTime(8),
                };
        }
        return address;
    }

    public async Task<int> Create(AddressDTO addressDTO)
    {
        using SqlCommand createCommand = new(@"
            INSERT INTO SalesLT.Address
            (
                AddressLine1
                ,AddressLine2
                ,City
                ,StateProvince
                ,CountryRegion
                ,PostalCode
                ,rowguid
                ,ModifiedDate
            )
            VALUES(
                @AddressLine1
                ,@AddressLine2
                ,@City
                ,@StateProvince
                ,@CountryRegion
                ,@PostalCode
                ,NEWID()
                ,@ModifiedDate
            )
        ", _connection);

        createCommand.Parameters.AddWithValue("AddressLine1", addressDTO.AddressLine1);
        createCommand.Parameters.AddWithValue("AddressLine2", addressDTO.AddressLine2 is null ? DBNull.Value : addressDTO.AddressLine2);
        createCommand.Parameters.AddWithValue("City", addressDTO.City);
        createCommand.Parameters.AddWithValue("StateProvince", addressDTO.StateProvince);
        createCommand.Parameters.AddWithValue("CountryRegion", addressDTO.CountryRegion);
        createCommand.Parameters.AddWithValue("PostalCode", addressDTO.PostalCode);
        createCommand.Parameters.AddWithValue("ModifiedDate", CurrentDate());

        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        return await createCommand.ExecuteNonQueryAsync();
    }
}

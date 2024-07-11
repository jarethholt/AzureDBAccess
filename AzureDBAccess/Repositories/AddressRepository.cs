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

    private void EnsureConnectionOpen()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
    }

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
        EnsureConnectionOpen();

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
            );
            SELECT SCOPE_IDENTITY()
        ", _connection);

        createCommand.Parameters.AddWithValue("AddressLine1", addressDTO.AddressLine1);
        createCommand.Parameters.AddWithValue("AddressLine2", addressDTO.AddressLine2 is null ? DBNull.Value : addressDTO.AddressLine2);
        createCommand.Parameters.AddWithValue("City", addressDTO.City);
        createCommand.Parameters.AddWithValue("StateProvince", addressDTO.StateProvince);
        createCommand.Parameters.AddWithValue("CountryRegion", addressDTO.CountryRegion);
        createCommand.Parameters.AddWithValue("PostalCode", addressDTO.PostalCode);
        createCommand.Parameters.AddWithValue("ModifiedDate", CurrentDate());

        EnsureConnectionOpen();
        int addressId = default;

        using SqlDataReader reader = await createCommand.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            if (await reader.ReadAsync())
                addressId = (int)reader.GetDecimal(0);
        }
        return addressId;
    }

    public async Task<int> Update(int addressId, AddressDTO addressDTO)
    {
        using SqlCommand updateCommand = new(@"
        UPDATE SalesLT.Address
        SET
            AddressLine1 = @AddressLine1
            ,AddressLine2 = @AddressLine2
            ,City = @City
            ,StateProvince = @StateProvince
            ,CountryRegion = @CountryRegion
            ,PostalCode = @PostalCode
            ,ModifiedDate = @ModifiedDate
        WHERE AddressId = @AddressId
        ", _connection);

        updateCommand.Parameters.AddWithValue("AddressId", addressId);
        updateCommand.Parameters.AddWithValue("AddressLine1", addressDTO.AddressLine1);
        updateCommand.Parameters.AddWithValue("AddressLine2", addressDTO.AddressLine2 is null ? DBNull.Value : addressDTO.AddressLine2);
        updateCommand.Parameters.AddWithValue("City", addressDTO.City);
        updateCommand.Parameters.AddWithValue("StateProvince", addressDTO.StateProvince);
        updateCommand.Parameters.AddWithValue("CountryRegion", addressDTO.CountryRegion);
        updateCommand.Parameters.AddWithValue("PostalCode", addressDTO.PostalCode);
        updateCommand.Parameters.AddWithValue("ModifiedDate", CurrentDate());

        EnsureConnectionOpen();
        return await updateCommand.ExecuteNonQueryAsync();
    }

    public async Task<int> Delete(int addressId)
    {
        // Create a Delete command that runs a manual cascade delete
        using SqlCommand deleteCommand = new(@"
        DELETE
            FROM SalesLT.Address
            WHERE AddressId = @AddressId
        ", _connection);
        deleteCommand.Parameters.AddWithValue("AddressId", addressId);

        EnsureConnectionOpen();
        return await deleteCommand.ExecuteNonQueryAsync();
    }
}

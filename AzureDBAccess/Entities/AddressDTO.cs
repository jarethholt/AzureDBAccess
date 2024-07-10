namespace AzureDBAccess.Entities;

internal record AddressDTO
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public required string StateProvince { get; set; }
    public required string CountryRegion { get; set; }
    public required string PostalCode { get; set; }

    public static AddressDTO FromAddress(Address address) => new()
    {
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        City = address.City,
        StateProvince = address.StateProvince,
        CountryRegion = address.CountryRegion,
        PostalCode = address.PostalCode,
    };

    public bool WouldUpdate(Address address) =>
        Equals(FromAddress(address));
}

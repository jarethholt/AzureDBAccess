namespace AzureDBAccess.Entities;

internal record Address : AddressDTO
{
    public int AddressId { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime ModifiedDate { get; set; }
}

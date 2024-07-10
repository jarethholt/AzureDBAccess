using Microsoft.Data.SqlClient;

namespace AzureDBAccess.Extensions;

internal static class SqlDataReaderExtensions
{
    public static string? GetNullableString(this SqlDataReader reader, int columnIndex) =>
        reader.IsDBNull(columnIndex) ? null : reader.GetString(columnIndex);
}

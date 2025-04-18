using Microsoft.Data.SqlClient;

namespace SageWebApi.Services;

public class SqlConnectionFactory (string connectionString)
{
    private readonly string _connectionString = connectionString;

    public SqlConnection CreateContext()
    {
        return new SqlConnection(_connectionString);
    }
}

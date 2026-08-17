using Microsoft.Data.SqlClient;

namespace Sikkhaloy.SyncApi.Services;

public sealed class EduConnectionFactory
{
    private readonly string _connectionString;

    public EduConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EduHybrid")
            ?? throw new InvalidOperationException("ConnectionStrings:EduHybrid is missing.");
    }

    public SqlConnection Create() => new SqlConnection(_connectionString);
}

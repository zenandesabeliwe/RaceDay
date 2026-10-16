using Microsoft.Data.SqlClient;

namespace RaceDay.Api.database;

// This helper follows the same connection-string structure as your class example.
public class DatabaseConnection
{
    private readonly string connection_string;

    public DatabaseConnection(IConfiguration configuration)
    {
        connection_string = configuration.GetConnectionString("RaceDayConnection")
            ?? throw new InvalidOperationException("RaceDayConnection is missing from appsettings.json.");
    }

    public SqlConnection get_connection()
    {
        return new SqlConnection(connection_string);
    }
}

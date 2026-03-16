using Microsoft.Data.SqlClient;

namespace CyberRegistration;

public class DatabaseService
{
    private const string MasterConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Trust Server Certificate=True;";
    private const string CyberDbConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CyberDB;Integrated Security=True;Trust Server Certificate=True;";

    public void CreerTable()
    {
        using (var connection = new SqlConnection(MasterConnectionString))
        {
            connection.Open();

            const string createDatabaseSql = """
                IF DB_ID(N'CyberDB') IS NULL
                BEGIN
                    CREATE DATABASE CyberDB;
                END
                """;

            using var command = new SqlCommand(createDatabaseSql, connection);
            command.ExecuteNonQuery();
        }

        using var cyberConnection = new SqlConnection(CyberDbConnectionString);
        cyberConnection.Open();

        const string createUsersTableSql = """
            IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Username NVARCHAR(50) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(256) NOT NULL,
                    Role NVARCHAR(20) NOT NULL DEFAULT 'user'
                );
            END
            ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'Role')
            BEGIN
                ALTER TABLE dbo.Users ADD Role NVARCHAR(20) NOT NULL DEFAULT 'user';
            END
            """;

        const string createLogsTableSql = """
            IF OBJECT_ID(N'dbo.Logs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Logs
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ActionType NVARCHAR(30) NOT NULL,
                    Username NVARCHAR(50) NULL,
                    Details NVARCHAR(255) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
                );
            END
            """;

        ExecuterCreationTable(cyberConnection, createUsersTableSql);
        ExecuterCreationTable(cyberConnection, createLogsTableSql);
    }

    public void InsererUser(User user)
    {
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string existsSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username;";
        using (var existsCommand = new SqlCommand(existsSql, connection))
        {
            existsCommand.Parameters.AddWithValue("@Username", user.Username);
            var exists = (int)existsCommand.ExecuteScalar()!;

            if (exists > 0)
            {
                throw new InvalidOperationException("Cet utilisateur existe déjà.");
            }
        }

        const string insertSql = "INSERT INTO dbo.Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role);";
        using var insertCommand = new SqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@Username", user.Username);
        insertCommand.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        insertCommand.Parameters.AddWithValue("@Role", user.Role);
        insertCommand.ExecuteNonQuery();
    }

    public string GetUserRole(string username)
    {
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string sql = "SELECT Role FROM dbo.Users WHERE Username = @Username;";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        var result = command.ExecuteScalar();
        return result?.ToString() ?? "user";
    }

    public bool AuthentifierUser(string username, string passwordHash)
    {
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string querySql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username AND PasswordHash = @PasswordHash;";
        using var command = new SqlCommand(querySql, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);

        var matchCount = (int)command.ExecuteScalar()!;
        return matchCount > 0;
    }

    public void EnregistrerTentativeAttaque(string actionType, string username, string details)
    {
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string insertLogSql = "INSERT INTO dbo.Logs (ActionType, Username, Details) VALUES (@ActionType, @Username, @Details);";
        using var command = new SqlCommand(insertLogSql, connection);
        command.Parameters.AddWithValue("@ActionType", actionType);
        command.Parameters.AddWithValue("@Username", string.IsNullOrWhiteSpace(username) ? DBNull.Value : username);
        command.Parameters.AddWithValue("@Details", details);
        command.ExecuteNonQuery();
    }

    public System.Data.DataTable GetAllUsers()
    {
        var table = new System.Data.DataTable();
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string sql = "SELECT Id, Username, SUBSTRING(PasswordHash, 1, 16) + '...' AS PasswordHash FROM dbo.Users ORDER BY Id;";
        using var adapter = new SqlDataAdapter(sql, connection);
        adapter.Fill(table);
        return table;
    }

    public System.Data.DataTable GetLogs()
    {
        var table = new System.Data.DataTable();
        using var connection = new SqlConnection(CyberDbConnectionString);
        connection.Open();

        const string sql = "SELECT Id, ActionType, Username, Details, CreatedAt FROM dbo.Logs ORDER BY Id DESC;";
        using var adapter = new SqlDataAdapter(sql, connection);
        adapter.Fill(table);
        return table;
    }

    private static void ExecuterCreationTable(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);

        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqlException exception) when (exception.Number == 2714)
        {
        }
    }
}
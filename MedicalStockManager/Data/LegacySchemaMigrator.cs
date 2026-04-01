using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Data;

public static class LegacySchemaMigrator
{
    private static readonly (int LegacyDepartment, string ServiceName)[] DepartmentMappings =
    [
        (1, "Imagerie"),
        (2, "Laboratoire Analyses"),
        (3, "Consultations"),
        (4, "Hopital du Jour")
    ];

    public static void Apply(ApplicationDbContext context)
    {
        // Ensure old databases are aligned with the current Service-based model.
        EnsureServicesTable(context);
        EnsureStockItemsServiceIdColumn(context);
        BackfillStockItemServices(context);
    }

    private static void EnsureServicesTable(ApplicationDbContext context)
    {
        if (!TableExists(context, "Services"))
        {
            context.Database.ExecuteSqlRaw(
                """
                CREATE TABLE IF NOT EXISTS Services (
                    Id INTEGER NOT NULL CONSTRAINT PK_Services PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL
                );
                """);
            context.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Services_Name ON Services(Name);");
        }

        foreach (var (_, serviceName) in DepartmentMappings)
        {
            context.Database.ExecuteSqlRaw(
                "INSERT OR IGNORE INTO Services (Name) VALUES ({0});",
                serviceName);
        }
    }

    private static void EnsureStockItemsServiceIdColumn(ApplicationDbContext context)
    {
        if (!ColumnExists(context, "StockItems", "ServiceId"))
        {
            context.Database.ExecuteSqlRaw(
                "ALTER TABLE StockItems ADD COLUMN ServiceId INTEGER NOT NULL DEFAULT 0;");
        }
    }

    private static void BackfillStockItemServices(ApplicationDbContext context)
    {
        var hasLegacyDepartment = ColumnExists(context, "StockItems", "Department");

        if (hasLegacyDepartment)
        {
            foreach (var (legacyDepartment, serviceName) in DepartmentMappings)
            {
                context.Database.ExecuteSqlRaw(
                    """
                    UPDATE StockItems
                    SET ServiceId = (SELECT Id FROM Services WHERE Name = {0} LIMIT 1)
                    WHERE (ServiceId IS NULL OR ServiceId = 0) AND Department = {1};
                    """,
                    serviceName,
                    legacyDepartment);
            }
        }

        // Fallback for rows still not mapped (or databases without Department).
        context.Database.ExecuteSqlRaw(
            """
            UPDATE StockItems
            SET ServiceId = (SELECT Id FROM Services ORDER BY Id LIMIT 1)
            WHERE ServiceId IS NULL OR ServiceId = 0;
            """);
    }

    private static bool TableExists(ApplicationDbContext context, string tableName)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1;";
        command.Parameters.Add(new SqliteParameter("$name", tableName));

        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            command.Connection.Open();
        }

        return command.ExecuteScalar() is not null;
    }

    private static bool ColumnExists(ApplicationDbContext context, string tableName, string columnName)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            command.Connection.Open();
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var currentColumn = reader.GetString(1);
            if (string.Equals(currentColumn, columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

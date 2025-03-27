using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250314)]
public class AddClientLogging : Migration {
    public override void Up() {
        Create.Table(DbConstants.ClientLogs)
            .WithColumn(DbConstants.Id).AsInt64().PrimaryKey().Identity().NotNullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable()
            .WithColumn("LogLevel").AsInt32().NotNullable()
            .WithColumn("Username").AsBinString(20).NotNullable()
            .WithColumn("Hostname").AsBinString().NotNullable()
            .WithColumn("DeviceId").AsBinString(64).NotNullable()
            .WithColumn("OperatingSystem").AsBinString().NotNullable()
            .WithColumn("ClientVersion").AsBinString(16).NotNullable()
            .WithColumn("InitUrl").AsString().NotNullable()
            .WithColumn("SessionId").AsInt64().NotNullable()
            .WithColumn("Message").AsString(12288).NotNullable()
            .WithColumn("ExceptionMessage").AsString(12288).NotNullable()
            .WithColumn("RawLog").AsString(65535).NotNullable();
    }

    public override void Down() =>
        Delete.Table(DbConstants.ClientLogs);
}

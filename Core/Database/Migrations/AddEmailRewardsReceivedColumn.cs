using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250526231800)]
public class AddEmailRewardsReceivedColumn : Migration {
    public override void Up() =>
        Alter.Table(DbConstants.Players)
            .AddColumn("EmailRewardsReceived").AsBoolean().NotNullable();

    public override void Down() => Delete.Column("EmailRewardsReceived").FromTable(DbConstants.Players);
}

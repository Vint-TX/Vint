using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250604014000)]
public class AddPremiumBoostBonusesReceiving : Migration {
    public override void Up() =>
        Alter.Table(DbConstants.Players)
            .AddColumn("LastPremiumBoostBonusesReceivingTime").AsDateTime().NotNullable();

    public override void Down() =>
        Delete.Column("LastPremiumBoostBonusesReceivingTime")
            .FromTable(DbConstants.Players);
}

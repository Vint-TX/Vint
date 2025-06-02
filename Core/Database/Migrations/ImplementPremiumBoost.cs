using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250602140150)]
public class ImplementPremiumBoost : Migration {
    public override void Up() {
        Alter.Table(DbConstants.Players)
            .AddColumn("PremiumBoostEndTime").AsDateTime().Nullable().WithDefaultValue(null);
    }

    public override void Down() {
        Delete.Column("PremiumBoostEndTime")
            .FromTable(DbConstants.Players);
    }
}

using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250525225330)]
public class UpdatePlayerEmailColumns : Migration {
    public override void Up() {
        Execute.Sql($"ALTER TABLE {DbConstants.Players} RENAME COLUMN `Subscribed` TO `NewsletterSubscribed`;");

        Delete.Index()
            .OnTable(DbConstants.Players)
            .OnColumn("Email");

        Alter.Table(DbConstants.Players)
            .AlterColumn("Email").AsCustom("varchar(255) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin'").Nullable()
            .AddColumn("EmailConfirmed").AsBoolean().NotNullable()
            .AddColumn("NewsletterUnsubscribeToken").AsString().Nullable();
    }

    public override void Down() {
        Delete
            .Column("NewsletterUnsubscribeToken")
            .Column("EmailConfirmed")
            .FromTable(DbConstants.Players);

        Alter.Table(DbConstants.Players)
            .AlterColumn("Email").AsCustom("varchar(255) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin'").NotNullable();

        Create.UniqueConstraint()
            .OnTable(DbConstants.Players)
            .Column("Email");

        Execute.Sql($"ALTER TABLE {DbConstants.Players} RENAME COLUMN `NewsletterSubscribed` TO `Subscribed`;");
    }
}

using FluentMigrator;

namespace Vint.Core.Database.Migrations;

[Migration(20250528172520)]
public class ConvertCountryCodeToLower : Migration {
    public override void Up() =>
        Execute.Sql($"UPDATE {DbConstants.Players} SET CountryCode = LOWER(CountryCode) WHERE CountryCode IS NOT NULL;");

    public override void Down() {
        // This migration cannot be reversed as it changes the case of country codes to lower case.
        // If you need to revert, you would need to have a backup or a different strategy.
    }
}

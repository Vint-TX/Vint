using LinqToDB;
using LinqToDB.Configuration;

namespace Vint.Core.Database;

public class DatabaseSettings : ILinqToDBSettings {
    public IEnumerable<IDataProviderSettings> DataProviders => [];

    public string DefaultConfiguration => ProviderName.MariaDB10MySqlConnector;
    public string DefaultDataProvider => ProviderName.MariaDB10MySqlConnector;

    public IEnumerable<IConnectionStringSettings> ConnectionStrings {
        get {
            yield return new ConnectionStringSettings("Vint",
                DatabaseConfig.Get()
                    .ConnectionString,
                ProviderName.MariaDB10MySqlConnector);
        }
    }
}

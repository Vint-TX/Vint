using LinqToDB;
using LinqToDB.Configuration;

namespace Vint.Core.Database;

public class DatabaseSettings : ILinqToDBSettings {
    public IEnumerable<IDataProviderSettings> DataProviders => [];

    public string DefaultConfiguration => ProviderName.MariaDB;
    public string DefaultDataProvider => ProviderName.MariaDB;

    public IEnumerable<IConnectionStringSettings> ConnectionStrings {
        get { yield return new ConnectionStringSettings("Vint", DatabaseConfig.Get().ConnectionString, ProviderName.MariaDB); }
    }
}

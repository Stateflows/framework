using MongoDB.Driver;

namespace Stateflows.Storage.MongoDB.Utils
{
    public class MongoDatabaseConfiguration
    {
        public MongoClientSettings MongoClientSettings { get; set; }
        public string DatabaseName { get; set; }

        public MongoDatabaseConfiguration(string host, int port, string databaseName)
        {
            MongoClientSettings = new MongoClientSettings()
            {
                Server = new MongoServerAddress(host, port),
            };
            DatabaseName = databaseName;
        }

        public MongoDatabaseConfiguration(MongoServerAddress mongoServerAddress, string databaseName)
        {
            MongoClientSettings = new MongoClientSettings()
            {
                Server = mongoServerAddress,
            };
            DatabaseName = databaseName;
        }

        public MongoDatabaseConfiguration(MongoClientSettings mongoClientSettings, string databaseName)
        {
            MongoClientSettings = mongoClientSettings;
            DatabaseName = databaseName;
        }
    }
}

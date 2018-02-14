using MongoDB.Bson.Serialization;
using MongoDB.Driver;
namespace OrderManagement.DbModel
{
    public class OrderManagementDbContext 
    {

        private readonly string ConnectionString = "mongodb://localhost";
        private readonly string DatabaseName = "OrderManagement";

        public OrderManagementDbContext() 
        {

            var client = new MongoClient(ConnectionString);
            Database = client.GetDatabase(DatabaseName);
            InitializeMappers.Initialize();
        }
        public IMongoDatabase Database { get; }

        public IMongoCollection<Order> Orders => Database.GetCollection<Order>("Orders");
        public IMongoCollection<Service> Services => Database.GetCollection<Service>("Services");
        
        
        public static class InitializeMappers
        {
            public static bool Initialized { get; private set; }

            public static void Initialize()
            {
                if (Initialized)
                    return;

                BsonClassMap.RegisterClassMap<Order>();
                Initialized = true;
            }            
        }
    }
}
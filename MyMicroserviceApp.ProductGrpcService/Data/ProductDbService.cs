using MongoDB.Driver;
using MyMicroserviceApp.ProductGrpcService.Models;

namespace MyMicroserviceApp.ProductGrpcService.Data
{
    public class ProductDbService
    {
        private readonly IMongoCollection<Product> _productsCollection;

        public ProductDbService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("ProductDb"); // Tên database
            _productsCollection = database.GetCollection<Product>("Products"); // Tên collection
        }

        public async Task<List<Product>> GetAsync() =>
            await _productsCollection.Find(_ => true).ToListAsync();

        public async Task<Product?> GetAsync(string id) =>
            await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Product newProduct) =>
            await _productsCollection.InsertOneAsync(newProduct);

        public async Task CreateManyAsync(List<Product> newProducts) =>
            await _productsCollection.InsertManyAsync(newProducts);

        // --- Phương thức khởi tạo dữ liệu ---
        public async Task SeedDataAsync()
        {
            var productCount = await _productsCollection.EstimatedDocumentCountAsync();
            if (productCount == 0) // Chỉ thêm dữ liệu nếu collection trống
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Laptop HP Spectre x360",
                        Description = "Premium 2-in-1 laptop with stunning design.",
                        Price = 1499.00M,
                        Stock = 40
                    },
                    new Product
                    {
                        Name = "Gaming Headset HyperX Cloud II",
                        Description = "Comfortable gaming headset with 7.1 surround sound.",
                        Price = 89.99M,
                        Stock = 100
                    },
                    new Product
                    {
                        Name = "Monitor LG Ultrafine 4K",
                        Description = "High-resolution monitor for vivid visuals.",
                        Price = 699.00M,
                        Stock = 25
                    }
                };
                await _productsCollection.InsertManyAsync(products);
                Console.WriteLine("MongoDB Product data seeded successfully.");
            }
        }
    }
}
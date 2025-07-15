using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyMicroserviceApp.ProductGrpcService.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public int Stock { get; set; }
    }
}
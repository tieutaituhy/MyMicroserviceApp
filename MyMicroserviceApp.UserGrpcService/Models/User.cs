using System.ComponentModel.DataAnnotations;

namespace MyMicroserviceApp.UserGrpcService.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Tạo ID ngẫu nhiên

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public string? Address { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;
using MyMicroserviceApp.UserGrpcService.Models;

namespace MyMicroserviceApp.UserGrpcService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thêm dữ liệu mẫu (optional)
            modelBuilder.Entity<User>().HasData(
                new User { Id = "user001", Name = "Nguyen Van A", Email = "anva@example.com", Address = "123 Le Loi, Ha Noi" },
                new User { Id = "user002", Name = "Tran Thi B", Email = "bthit@example.com", Address = "456 Tran Hung Dao, HCM" }
            );
        }
    }
}
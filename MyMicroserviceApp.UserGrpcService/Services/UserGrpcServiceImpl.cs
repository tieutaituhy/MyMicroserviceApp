using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using MyMicroserviceApp.SharedContracts; // Sử dụng namespace từ shared contracts
using MyMicroserviceApp.UserGrpcService.Data; // Để truy cập UserDbContext

namespace MyMicroserviceApp.UserGrpcService.Services
{
    // Kế thừa từ UserService.UserServiceBase (tạo tự động từ .proto)
    public class UserGrpcServiceImpl : UserService.UserServiceBase
    {
        private readonly ILogger<UserGrpcServiceImpl> _logger;
        private readonly UserDbContext _dbContext;

        public UserGrpcServiceImpl(ILogger<UserGrpcServiceImpl> logger, UserDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all users...");
            var users = await _dbContext.Users.ToListAsync();
            var response = new GetUsersResponse();
            response.Users.AddRange(users.Select(u => new SharedContracts.User
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address ?? ""
            }));
            return response;
        }

        public override async Task<UserResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Getting user by ID: {request.UserId}");
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found."));
            }

            return new UserResponse
            {
                User = new SharedContracts.User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Address = user.Address ?? ""
                }
            };
        }
    }
}
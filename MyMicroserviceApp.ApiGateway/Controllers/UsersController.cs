using Microsoft.AspNetCore.Mvc;
using MyMicroserviceApp.SharedContracts; // Để sử dụng các models và client gRPC

namespace MyMicroserviceApp.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService.UserServiceClient _userClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService.UserServiceClient userClient, ILogger<UsersController> logger)
        {
            _userClient = userClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Gateway received request for all users.");
            try
            {
                var response = await _userClient.GetUsersAsync(new GetUsersRequest());
                return Ok(response.Users);
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, "Error calling User gRPC service.");
                return StatusCode(500, $"Internal server error: {ex.Status.Detail}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation($"Gateway received request for user with ID: {id}");
            try
            {
                var response = await _userClient.GetUserByIdAsync(new GetUserByIdRequest { UserId = id });
                return Ok(response.User);
            }
            catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(ex.Status.Detail);
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, "Error calling User gRPC service.");
                return StatusCode(500, $"Internal server error: {ex.Status.Detail}");
            }
        }
    }
}
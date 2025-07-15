using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MyMicroserviceApp.SharedContracts;
using System.Text.Json; // Để sử dụng các models và client gRPC

namespace MyMicroserviceApp.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService.ProductServiceClient _productClient;
        private readonly ILogger<ProductsController> _logger;
        private readonly IDistributedCache _cache;

        public ProductsController(ProductService.ProductServiceClient productClient, ILogger<ProductsController> logger, IDistributedCache cache)
        {
            _productClient = productClient;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            _logger.LogInformation("Gateway received request for all products.");

            // Thử lấy từ cache
            var cachedProducts = await _cache.GetStringAsync("all_products");
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                _logger.LogInformation("Returning products from cache.");
                return Ok(JsonSerializer.Deserialize<List<MyMicroserviceApp.SharedContracts.Product>>(cachedProducts));
            }

            try
            {
                var response = await _productClient.GetProductsAsync(new GetProductsRequest());

                // Lưu vào cache
                var productsJson = JsonSerializer.Serialize(response.Products);
                await _cache.SetStringAsync("all_products", productsJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache 5 phút
                });

                _logger.LogInformation("Returning products from gRPC service and caching.");
                return Ok(response.Products);
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, "Error calling Product gRPC service.");
                return StatusCode(500, $"Internal server error: {ex.Status.Detail}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(string id)
        {
            _logger.LogInformation($"Gateway received request for product with ID: {id}");
            try
            {
                var response = await _productClient.GetProductByIdAsync(new GetProductByIdRequest { ProductId = id });
                return Ok(response.Product);
            }
            catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(ex.Status.Detail);
            }
            catch (Grpc.Core.RpcException ex)
            {
                _logger.LogError(ex, "Error calling Product gRPC service.");
                return StatusCode(500, $"Internal server error: {ex.Status.Detail}");
            }
        }
    }
}
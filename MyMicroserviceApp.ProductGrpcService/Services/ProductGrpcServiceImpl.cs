using Grpc.Core;
using MyMicroserviceApp.ProductGrpcService.Data; // Để truy cập ProductDbService
using MyMicroserviceApp.SharedContracts; // Sử dụng namespace từ shared contracts

namespace MyMicroserviceApp.ProductGrpcService.Services
{
    // Kế thừa từ ProductService.ProductServiceBase (tạo tự động từ .proto)
    public class ProductGrpcServiceImpl : ProductService.ProductServiceBase
    {
        private readonly ILogger<ProductGrpcServiceImpl> _logger;
        private readonly ProductDbService _productDbService;

        public ProductGrpcServiceImpl(ILogger<ProductGrpcServiceImpl> logger, ProductDbService productDbService)
        {
            _logger = logger;
            _productDbService = productDbService;
        }

        public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all products...");
            var products = await _productDbService.GetAsync();
            var response = new GetProductsResponse();
            response.Products.AddRange(products.Select(p => new SharedContracts.Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = (double)p.Price, // Ánh xạ decimal sang double
                Stock = p.Stock
            }));
            return response;
        }

        public override async Task<ProductResponse> GetProductById(GetProductByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Getting product by ID: {request.ProductId}");
            var product = await _productDbService.GetAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found."));
            }

            return new ProductResponse
            {
                Product = new SharedContracts.Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = (double)product.Price,
                    Stock = product.Stock
                }
            };
        }
    }
}
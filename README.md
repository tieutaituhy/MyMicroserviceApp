
# MyMicroserviceApp

## Project Overview

`MyMicroserviceApp` is a sample microservices system built with .NET, including gRPC services for product and user management, an API Gateway, and shared contracts via protobuf. The project demonstrates a modern microservices architecture and integrates Docker for infrastructure deployment.

### Main Components
- **MyMicroserviceApp.ApiGateway**: API Gateway receives client requests and forwards them to gRPC services.
- **MyMicroserviceApp.ProductGrpcService**: gRPC service for product management.
- **MyMicroserviceApp.UserGrpcService**: gRPC service for user management.
- **MyMicroserviceApp.SharedContracts**: Contains protobuf files defining shared data contracts between services.
- **Docker/infrastracture.yaml**: Infrastructure definition for deployment using Docker Compose.

## How to Run the Project

### 1. Prerequisites
- .NET SDK 7.0 or higher
- Docker Desktop (if you want to run with Docker)
- Bash or PowerShell

### 2. Run with Docker
1. Open a terminal in the project root directory.
2. Run the following command to start the entire system:
   ```bash
   docker compose -f Docker/infrastracture.yaml up --build
   ```
3. All services will start and can be accessed via the API Gateway.

### 3. Run Services Manually
1. Build the entire solution:
   ```bash
   dotnet build MyMicroserviceApp.sln
   ```
2. Run each service in separate terminals:
   - Run ProductGrpcService:
     ```bash
     dotnet run --project MyMicroserviceApp.ProductGrpcService/MyMicroserviceApp.ProductGrpcService.csproj
     ```
   - Run UserGrpcService:
     ```bash
     dotnet run --project MyMicroserviceApp.UserGrpcService/MyMicroserviceApp.UserGrpcService.csproj
     ```
   - Run ApiGateway:
     ```bash
     dotnet run --project MyMicroserviceApp.ApiGateway/MyMicroserviceApp.ApiGateway.csproj
     ```

### 4. Testing
- Send requests to the API Gateway (e.g., using Postman or the `.http` file).
- Sample endpoints can be found in the `Controllers` folder of ApiGateway.

---

If you need more details about configuration, example requests, or API documentation, feel free to ask!
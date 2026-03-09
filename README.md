# IAM Service Backend

Welcome to the **IAM (Identity and Access Management) Service Backend** repository. This is a robust, scalable, and secure microservice built with **.NET 9**, implementing **Clean Architecture** and **CQRS** principles. It serves as the centralized identity provider and access control manager for the broader system ecosystem.

## 🚀 Features

- **Authentication & Authorization**: Secure user login, registration, and role-based access control (RBAC) using JWT Bearer tokens and custom security policies.
- **User Management**: Complete lifecycle management for users, roles, and privileges.
- **Token Management**: Handles access tokens, refresh tokens, and password reset tokens securely.
- **Event-Driven Architecture**: Integrates with **Kafka** for asynchronous event publishing and inter-service communication.
- **High Performance Caching**: Leverages **Redis** for distributed caching to optimize token validation and system performance.
- **gRPC Support**: Exposes high-performance gRPC endpoints (User and Privilege services) for internal microservice-to-microservice communication.
- **RESTful API**: Standardized HTTP endpoints with OpenAPI documentation provided via **Scalar**.
- **Security**: Advanced password hashing, string encryption, and audit logging for traceability.

## 🛠️ Technology Stack

- **Framework**: .NET 9.0
- **Architecture**: Clean Architecture & CQRS (Command Query Responsibility Segregation)
- **Database**: PostgreSQL (via Entity Framework Core 9)
- **Caching**: Redis (StackExchange.Redis)
- **Message Broker**: Apache Kafka
- **RPC Framework**: gRPC
- **API Documentation**: Scalar for OpenAPI
- **Libraries & Tools**: 
  - [MediatR](https://github.com/jbogard/MediatR) for CQRS implementations
  - [FluentValidation](https://docs.fluentvalidation.net/) for robust input validation
  - [AutoMapper](https://automapper.org/) for object-to-object mapping
- **Deployment & CI/CD**: Docker & Jenkins

## 📁 System Architecture

The project enforces **Clean Architecture**, separating concerns across multiple layers to achieve independence from UI, databases, and external agencies.

- `IAMService.Domain`: Core business models, entities, and domain exceptions.
- `IAMService.Application`: Use cases, MediatR handlers, DTOs, interfaces, and FluentValidation rules.
- `IAMService.Infrastructure`: External concerns including Database context (EF Core), Repositories, Redis connection, Kafka integration, Email services, and external APIs.
- `IAMService.API`: The entry point (REST + gRPC), containing Controllers, Middlewares, and dependency injection bootstrapping.
- `IAMService.*.Test`: Unit and integration testing projects.

## ⚙️ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Redis](https://redis.io/)
- [Apache Kafka](https://kafka.apache.org/)
- [Docker](https://www.docker.com/) (Optional, for containerized environments)

## 🔧 Local Environment Setup

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd IAM-Service-Backend
   ```

2. **Configure AppSettings:**
   Update the `appsettings.Development.json` in the `IAMService.API` project with your local connection strings.
   You will need to configure:
   - `ConnectionStrings:DefaultConnection` (PostgreSQL)
   - `ConnectionStrings:RedisConnection` (Redis)
   - `Jwt` settings (Issuer, Audience, SigningKey)
   - `EmailSettings`
   - `EVENT_PUBLISHING_TOPICS` (Kafka topics)

3. **Set Environment Variables / User Secrets:**
   The application requires a passphrase for encryption.
   ```bash
   dotnet user-secrets set "ENCRYPTION_PASSPHRASE" "YourSecurePassphraseHere" --project IAMService.API
   ```

4. **Apply Database Migrations:**
   The application is configured to automatically migrate the database on startup, but you can also run:
   ```bash
   dotnet ef database update --project IAMService.Infrastructure --startup-project IAMService.API
   ```

5. **Run the Application:**
   ```bash
   dotnet run --project IAMService.API
   ```
   *The API will be available at standard local host ports (e.g., `https://localhost:5001`).*

## 📚 API Documentation

Once the application is running, the interactive API documentation can be accessed via **Scalar**. Navigate to:
- `https://localhost:<port>/scalar` (or the respective documentation route configured in your environment).

## 🐳 Docker Deployment

A `Dockerfile` is provided for containerization. You can build the image using:

```bash
docker build -t iam-service-backend -f IAMService.API/Dockerfile .
```

For orchestrated deployment, refer to the `Jenkinsfile` for the CI/CD pipeline definitions.

## 🧪 Testing

The solution includes distinct test projects. You can run all tests and generate test coverage by executing the included shell script:

```bash
./generate-coverage.sh
```
Or use the standard dotnet command:
```bash
dotnet test
```

## 🤝 Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit your changes (`git commit -m 'Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

---
*Maintained by the IAM Service Team.*

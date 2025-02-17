# TestSaaSNeoLedge

## Overview

TestSaaSNeoLedge is a microservice designed to manage user authentication and secure key storage using **MinIO** for object storage. It is built using **C#** and follows clean architecture principles for better scalability and maintainability.

This microservice utilizes a **shared library** to centralize and reuse common functionalities, improving code consistency and reducing duplication. This architecture is designed to be modular and extensible, allowing easier maintenance and future enhancements.

## Project Structure

```
├── Template.SharedLibrarySolution/
│    ├── Utilities/                  # Common utilities (encryption, key generation, etc.)
│    ├── MinioClientFactory.cs       # MinIO client configuration and factory pattern
│    ├── Constants.cs                # Shared constants (e.g., bucket names)
│    └── Extensions/                 # Extension methods for improved code reusability
└── TestAuthentificationApiSolution/
     ├── AuthenticationApi.Application/    # Interfaces and DTOs
     ├── AuthenticationApi.Domain/         # Core business logic (Entities)
     ├── AuthenticationApi.Infrastructure/ # Data access, MinIO integration
     │     ├── Data/AuthenticationDbContext.cs
     │     ├── Services/
     │     │    ├── KeyService.cs          # RSA key generation and storage
     │     │    └── MinioService.cs        # MinIO interactions
     │     └── DependencyInjection/        # DI container configuration
     └── AuthenticationApi.Presentation/   # REST API controllers and endpoints
```

## How the Microservice Uses the Shared Library

The **Template.SharedLibrarySolution** is a collection of common utilities and components used across the microservice to enforce DRY (Don't Repeat Yourself) principles. Key areas where the shared library is integrated include:

1. **MinIO Client Factory**:

   - Provides a reusable factory to instantiate MinIO clients with consistent configuration.
   - Allows separation of configuration from business logic, enhancing testability and maintainability.

2. **Encryption and Key Utilities**:

   - Centralized logic for RSA key pair generation.
   - Ensures consistent PEM formatting across the service.

3. **Constants**:

   - Shared constants (e.g., bucket names like `pubkeys` and user-specific buckets).
   - Avoids hardcoding values across different layers of the application.

4. **Extension Methods**:

   - Provides helper methods to simplify complex logic and enhance code clarity.

By isolating these components in a shared library, we achieve:

- **Consistency**: Common logic is applied uniformly.
- **Reusability**: Shared code can be leveraged across multiple services if needed.
- **Maintainability**: Changes in core utilities only need to be updated once.

## Dependency Injection (DI) and Why It's Important

The microservice uses **Dependency Injection** to:

1. **Decouple Components**:

   - Business logic, infrastructure, and presentation layers are independent.
   - Allows easier testing by injecting mocks or fakes during unit tests.

2. **Service Lifetime Management**:

   - Controls the lifecycle of services (e.g., singleton for MinIO client, transient for short-lived operations).

### Example: KeyService and MinioService

```csharp
public static void AddInfrastructure(this IServiceCollection services)
{
    services.AddSingleton<IMinioService, MinioService>();
    services.AddTransient<IKeyService, KeyService>();
}
```

**Why this approach?**

- **Singleton**: MinIO client is expensive to instantiate; keeping it as a singleton improves performance.
- **Transient**: Key generation requires new instances each time to ensure unique results.

## Core Services

### 1. KeyService

Responsible for:

- Generating RSA key pairs.
- Formatting keys to PEM format.

Why it's isolated:

- Separation of concerns: Key management logic is independent of MinIO interactions.
- Easier testing and modification without affecting other services.

### 2. MinioService

Handles all MinIO-related operations:

- Creating user-specific buckets during registration.
- Uploading public and private keys to appropriate buckets.

Why it's isolated:

- Improves clarity by handling storage concerns separately.
- Ensures MinIO logic is reusable if additional storage needs emerge.

## Workflow: User Registration

1. User submits registration request.
2. **AuthenticationController** triggers the following sequence:
   - Create a new MinIO bucket with the user's name.
   - Generate RSA key pair using `KeyService`.
   - Upload the public key to `pubkeys` bucket.
   - Upload the private key to the user's specific bucket.

## Why This Design Is Beneficial

1. **Modularity**: Each responsibility is encapsulated in its own service.
2. **Testability**: DI allows easy mocking of services for unit testing.
3. **Scalability**: Clear separation of concerns allows horizontal scaling.
4. **Maintainability**: Shared library reduces code duplication and ensures consistent logic.

## How to Run the Project

1. Clone the repository:

   ```bash
   git clone https://github.com/malekzeghouf/testSaaSNeoLedge.git
   cd testSaaSNeoLedge
   ```

2. Configure MinIO settings in `appsettings.json`:

   ```json
   "Minio": {
     "Endpoint": "http://your-minio-url:9000",
     "AccessKey": "your-access-key",
     "SecretKey": "your-secret-key"
   }
   ```

3. Build and run the service:

   ```bash
   dotnet build
   dotnet run --project AuthenticationApi.Presentation
   ```

## Future Improvements

- Implement encryption for private key storage.
- Add logging for better observability.
- Enhance access control policies for sensitive data.

---

This documentation explains the architecture, reasoning behind the design decisions, and the core functionality of the TestSaaSNeoLedge microservice.


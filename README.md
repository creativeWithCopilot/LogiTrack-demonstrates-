# LogiTrack-demonstrates
LogiTrack demonstrates a robust, secure, and performant backend architecture. By addressing challenges in entity modeling, DTO separation, and role-based security, the system achieves maintainability and scalability. With caching, query optimizations, and strict validation

What does your API system do?
LogiTrack is a backend API system designed to manage inventory and orders for a logistics or warehouse environment. It allows authenticated users to view and manage stock, create and track customer orders, and enforce secure, role-based access to sensitive operations. The system ensures data consistency, performance, and security through structured business logic, caching, and JWT-based authentication.

Key Functionalities
1) Inventory Management
Provides endpoints to create, update, delete, and retrieve inventory items.
Uses in-memory caching to optimize repeated queries.
Access restricted so only Admins can modify inventory.

2) Order Processing
Supports creating orders with multiple items via a proper many-to-many join entity (OrderItem).
Validates that all items exist and quantities are positive before saving.
Returns detailed order information including item names and quantities.

3) Authentication & Authorization
Implements JWT Bearer authentication integrated with ASP.NET Core Identity.
Role-based access control ensures only authorized roles (e.g., Admin, Manager) can perform sensitive actions.
Tokens include claims for user identity and roles, enabling secure, declarative authorization.

Technologies and Tools Used
Backend Framework: ASP.NET Core Web API (.NET 8)
Database & ORM: SQLite with Entity Framework Core
Authentication & Security: ASP.NET Core Identity, JWT Bearer tokens, role-based authorization
Mapping: AutoMapper for DTO ↔ Entity conversion
Caching: IMemoryCache for performance optimization
API Documentation: Swagger / Swashbuckle
DevOps & Tooling: EF Core migrations, dependency injection, configuration via appsettings.json
###
1) Challenges Faced During Development
Entity Relationship Modeling Designing the order system was complex because each order could contain multiple inventory items. A simple one‑to‑many relationship wasn’t sufficient.
DTO vs Entity Binding Issues Early attempts to bind JSON directly to EF Core entities caused validation errors (e.g., “At least one item is required”) and exposed internal schema details to clients.
Authentication and Role Seeding Initial seeding of the admin user failed due to missing required fields (DisplayName), and tokens sometimes lacked the correct role claims, leading to 403 Forbidden errors.
Validation and Input Mismatches Client payloads didn’t always align with the expected model (e.g., sending itemId instead of inventoryItemId), which caused repeated 400 Bad Request responses.
2) How Challenges Were Solved
Entity Relationships Introduced a join entity (OrderItem) to model the many‑to‑many relationship between orders and inventory items. This allowed each order to contain multiple products with quantities.
DTO Introduction Created dedicated DTOs (OrderCreateDto, OrderReadDto, OrderLineDto) to decouple API contracts from EF Core entities. This eliminated binding errors and gave clients a clean, predictable contract.
Improved Seeding Logic Updated ApplicationUser to include required fields and ensured roles were assigned during seeding. Re‑logging in with a fresh token confirmed that role claims were present.
Validation Enhancements Added explicit validation in controllers (e.g., checking for at least one item, positive quantities, and valid inventory IDs) to provide clearer error messages and prevent invalid data from being persisted.
3) How Microsoft Copilot Helped
Debugging Errors Copilot analyzed exception traces (e.g., NOT NULL constraint failed: AspNetUsers.DisplayName) and explained the root cause, guiding fixes in the ApplicationUser model and seeding logic.
Improving API Contracts Suggested introducing DTOs and AutoMapper mappings to resolve binding issues and improve maintainability.
Security Guidance Helped configure JWT authentication, role‑based authorization, and cookie redirect suppression for APIs.
Iterative Refinement Provided corrected JSON payloads and explained why earlier payloads failed, accelerating debugging during API testing.
4) Optimizations for Performance and Scalability
Query Optimization Used Include + ThenInclude to eager‑load related entities (Orders → Items → InventoryItem) in a single query, reducing N+1 query issues. Applied AsNoTracking() for read‑only queries to reduce EF Core change tracking overhead.
Caching Implemented IMemoryCache for inventory queries with sliding expiration. Cache invalidated automatically on create, update, or delete operations.
DTO‑Based Responses Returned only necessary fields via DTOs, reducing payload size and improving client performance.
Monitoring Added response headers (X-Elapsed-ms) to measure query execution time, enabling performance tracking and future tuning.
###
Business Logic
Data Models:
InventoryItem represents stock items with properties like Name, Quantity, and Location.
Order represents customer orders, with a DatePlaced, CustomerName, and a collection of OrderItems.
OrderItem is a join entity that models the many‑to‑many relationship between orders and inventory items, including a Quantity field for each line.
Relationships:
One Order → Many OrderItems.
One InventoryItem → Many OrderItems.
This structure ensures flexibility: each order can contain multiple items, and each inventory item can appear in multiple orders.

API Design
Controllers and Routes:
InventoryController
GET /api/inventory → Retrieve all inventory items (cached).
POST /api/inventory → Create a new item (Manager only).
DELETE /api/inventory/{id} → Delete an item (Manager only).
OrderController
GET /api/order → Retrieve all orders with items.
GET /api/order/{id} → Retrieve a single order by ID.
POST /api/order → Create a new order with multiple items.
DELETE /api/order/{id} → Delete an order (Manager only).

Security
Authentication:
Implemented with JWT Bearer tokens generated by a TokenService.
Tokens include claims for user identity and roles.
Role‑Based Access Control:
[Authorize] ensures only authenticated users can access protected endpoints.
[Authorize(Roles = "Manager ")] restricts inventory post and delete to User.
[Authorize(Roles = "User")] restricts order deletion to User.
Validation:
Data annotations ([Required], [MaxLength], [Range]) enforce constraints at the model level.
Controllers add explicit checks (e.g., “At least one item is required” for orders).

Caching
In‑Memory Caching with IMemoryCache:
Applied to GET /api/inventory to reduce database load for frequently accessed data.
Cached results expire after 5 minutes of inactivity (sliding expiration).
Cache is invalidated automatically when inventory is created, updated, or deleted.
Why:
Inventory data is read far more often than it is modified, making it an ideal candidate for caching.
Improves performance and responsiveness for clients.

State Management
Persistence:
EF Core with SQLite manages application state across sessions.
Orders, inventory, and user accounts are stored in the database, ensuring durability.
Session Independence:
Since JWT tokens are stateless, authentication does not rely on server‑side session storage.
This makes the API scalable and suitable for distributed deployments.
Consistency:
EF Core migrations ensure schema consistency across environments.
AsNoTracking() is used for read‑only queries to avoid unnecessary state tracking overhead.

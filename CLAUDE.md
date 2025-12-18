# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

P4BSwagger is a standalone Swagger UI documentation server for the P4Books ERP API. It serves interactive API documentation with OpenAPI 3.0.3 specification support and custom P4Books branding.

**Live URL:** https://api.p4books.cloud

## Tech Stack

- **.NET 10.0** - Latest ASP.NET Core minimal APIs
- **Swashbuckle.AspNetCore 10.0.1** - Swagger/OpenAPI tooling with enhanced UI features
- **Azure Web Apps** - Linux runtime hosting
- **GitHub Actions** - CI/CD pipeline

## Build & Run Commands

```bash
# Build
dotnet build --configuration Release

# Run locally (HTTP)
dotnet run
# Access at http://localhost:5016

# Run locally (HTTPS)
dotnet run --launch-profile https
# Access at https://localhost:7297

# Publish for deployment
dotnet publish -c Release -o ./publish
```

## Architecture

This is a **documentation-only server** - it does NOT contain the actual P4Books API implementation.

### Key Files

- `Program.cs` - Single-file application with all configuration:
  - CORS setup (AllowAll policy for documentation access)
  - Swagger UI middleware with extensive CSS customization
  - Static OpenAPI spec serving with 5-minute caching
  - Health check (`/health`) and version (`/version`) endpoints

- `openapi.json` - Static OpenAPI 3.0.3 specification (update manually when backend API changes)

- `P4B.Api.csproj` - Project file targeting .NET 10.0

### Swagger UI Customization

The Swagger UI is heavily customized in `Program.cs:25-128`:
- P4 Software branding (Blue #2563EB, Orange #F59E0B)
- 2-column responsive header layout
- Monokai syntax highlighting theme
- Persistent authorization tokens
- Custom operation block styling by HTTP method

## ASP.NET Core 10 Minimal API Patterns

This project uses ASP.NET Core minimal APIs (no controllers). Key patterns:

```csharp
// Endpoint definition with metadata
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithTags("System")
   .WithName("HealthCheck");

// Static file serving with caching
app.MapGet("/openapi.json", async context => {
    context.Response.Headers.CacheControl = "public, max-age=300";
    await context.Response.SendFileAsync(filePath);
}).ExcludeFromDescription();
```

## Swashbuckle Configuration Patterns

Key Swashbuckle 10.x configuration used in this project:

```csharp
// Swagger UI configuration
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/openapi.json", "P4Books ERP API v1");
    options.RoutePrefix = string.Empty;  // Serve at root
    options.EnableTryItOutByDefault();
    options.EnableValidator();
    options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
});
```

## API Authentication

The P4Books API uses header-based authentication:
- **ApiKey** - Header: `ApiKey` (determines tenant access)

## Deployment

### Automatic (GitHub Actions)
Push to `main` branch triggers automatic deployment via `.github/workflows/main_p4bswagger.yml`:
1. Build with .NET 10.0
2. Publish Release configuration
3. Deploy to Azure Web App via OIDC authentication

### Manual Azure Setup
Requires GitHub secrets for Azure OIDC authentication (client-id, tenant-id, subscription-id).

## API Entities Documented

### Master Data
Customer, Vendor, Product, Warehouse, Account

### Sales Documents
SalesQuote, SalesOrder, Invoice, InventoryDelivery

### Purchase Documents
PurchaseOrder, VendorBill, InventoryReceipt

### Accounting & Reporting
JournalEntry, PrintReport, Dashboard

## Working with This Project

1. **Updating API documentation**: Edit `openapi.json` directly or regenerate from the backend API
2. **Styling changes**: Modify CSS in `Program.cs` HeadContent section (lines 64-127)
3. **Adding endpoints**: Use minimal API pattern with `.WithTags()` and `.WithName()` metadata
4. **Testing locally**: Run `dotnet run` and access http://localhost:5016

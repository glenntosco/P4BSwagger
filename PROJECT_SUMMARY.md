# P4Books API Documentation Server (v1.0.1)

**Live URL:** https://api.p4books.cloud

## Features Implemented

- **Swagger UI** with custom P4 Software branding (blue/orange theme)
- **API Proxy** forwarding `/api/*` requests to `app.p4books.cloud`
- **Custom DNS resolver** for Azure App Service (fallback IP 20.9.134.138)
- **P4 Software logo** on main page, `/health`, and `/version` pages
- **Session timeout** (10 min inactivity + logout on browser close)
- **Inventory API** endpoints (`GetAll`, `GetByProduct`, `GetByWarehouse`)

## Key Fixes

- Fixed GitHub workflow to use .NET 10.x for P4Books-Angel deployment
- Added X-Tenant-Alias header for multi-tenant support
- API key-based authentication

## Endpoints

| Page | URL |
|------|-----|
| API Docs | https://api.p4books.cloud |
| Health | https://api.p4books.cloud/health |
| Version | https://api.p4books.cloud/version |
| OpenAPI Spec | https://api.p4books.cloud/openapi.json |

## API Endpoints

### Inventory (New)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Inventory/GetAll` | List all inventory locations with stock levels |
| GET | `/api/Inventory/GetByProduct` | Get inventory for a specific product |
| GET | `/api/Inventory/GetByWarehouse` | Get all inventory in a warehouse |

### Other Available APIs
- Customer, Vendor, Product, Warehouse, Account
- SalesQuote, SalesOrder, Invoice, InventoryDelivery
- PurchaseOrder, VendorBill, InventoryReceipt
- JournalEntry, PrintReport

## Authentication

All API requests require the `ApiKey` header:
```
ApiKey: your-api-key-here
```

The tenant is automatically determined from the API key.

## Tech Stack

- .NET 10.0 / ASP.NET Core Minimal APIs
- Swashbuckle.AspNetCore 10.0.1
- Azure Web Apps (Linux runtime)
- GitHub Actions CI/CD

## Deployment

- **P4BSwagger**: Push to `main` triggers deployment via `.github/workflows/main_p4bswagger.yml`
- **P4Books Backend**: Push to `main` triggers deployment via `.github/workflows/main_p4books.yml`

## Project Completed

December 18, 2025

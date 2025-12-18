# P4Books API Documentation - Current Functionality

## Overview

**Live URL:** https://api.p4books.cloud

The P4Books API provides a RESTful interface to the P4Books ERP system. This documentation server (P4BSwagger) serves as both the Swagger UI documentation and an API proxy to the backend.

---

## Authentication

All API requests require the `ApiKey` header:

```
ApiKey: your-api-key-here
```

The tenant is automatically determined from the API key - no need to specify tenant subdomain.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    api.p4books.cloud                        │
│                      (P4BSwagger)                           │
├─────────────────────────────────────────────────────────────┤
│  - Swagger UI Documentation                                 │
│  - API Proxy (forwards /api/* requests)                     │
│  - Custom DNS resolver for p4books.cloud domains            │
│  - X-Tenant-Alias header injection                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    app.p4books.cloud                        │
│                    (P4Books Backend)                        │
├─────────────────────────────────────────────────────────────┤
│  - Multi-tenant ERP backend                                 │
│  - API key-based tenant lookup                              │
│  - SQL Server + MongoDB data stores                         │
└─────────────────────────────────────────────────────────────┘
```

---

## Available API Endpoints

### Customer
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Customer/GetById` | Get customer by ID |
| POST | `/api/Customer/CreateOrUpdate` | Create or update customer |
| POST | `/api/Customer/VerifyTaxId` | Verify customer tax ID |

### Vendor
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Vendor/GetById` | Get vendor by ID |
| POST | `/api/Vendor/CreateOrUpdate` | Create or update vendor |

### Product
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Product/GetById` | Get product by ID |
| GET | `/api/Product/GetBySku` | Get product by SKU |
| POST | `/api/Product/CreateOrUpdate` | Create or update product |
| GET | `/api/Product/ProductImages` | Get product images |

### Inventory (NEW)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Inventory/GetAll` | List all inventory locations with stock levels |
| GET | `/api/Inventory/GetByProduct` | Get inventory for a specific product |
| GET | `/api/Inventory/GetByWarehouse` | Get all inventory in a warehouse |

**Inventory Query Parameters:**
- `warehouseId` - Filter by warehouse (UUID)
- `productId` - Filter by product (UUID)
- `sku` - Filter by SKU (partial match)
- `minQuantity` - Minimum quantity filter

### Warehouse
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Warehouse/GetById` | Get warehouse by ID |
| POST | `/api/Warehouse/CreateOrUpdate` | Create or update warehouse |

### Account (Chart of Accounts)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Account/GetById` | Get account by ID |
| POST | `/api/Account/CreateOrUpdate` | Create or update account |
| POST | `/api/Account/DeleteBatch` | Delete multiple accounts |

### Sales Quote
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/SalesQuote/GetById` | Get sales quote by ID |
| POST | `/api/SalesQuote/CreateOrUpdate` | Create or update sales quote |
| POST | `/api/SalesQuote/GenerateSalesOrder` | Convert quote to sales order |

### Sales Order
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/SalesOrder/GetById` | Get sales order by ID |
| POST | `/api/SalesOrder/CreateOrUpdate` | Create or update sales order |
| POST | `/api/SalesOrder/GenerateDelivery` | Generate delivery from order |
| POST | `/api/SalesOrder/GenerateInvoice` | Generate invoice from order |

### Invoice
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Invoice/GetById` | Get invoice by ID |
| POST | `/api/Invoice/CreateOrUpdate` | Create or update invoice |

### Inventory Delivery
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/InventoryDelivery/GetById` | Get delivery by ID |
| POST | `/api/InventoryDelivery/CreateOrUpdate` | Create or update delivery |

### Purchase Order
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/PurchaseOrder/GetById` | Get purchase order by ID |
| POST | `/api/PurchaseOrder/CreateOrUpdate` | Create or update purchase order |
| POST | `/api/PurchaseOrder/GenerateReceipt` | Generate receipt from PO |
| POST | `/api/PurchaseOrder/GenerateBill` | Generate vendor bill from PO |

### Vendor Bill
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/VendorBill/GetById` | Get vendor bill by ID |
| POST | `/api/VendorBill/CreateOrUpdate` | Create or update vendor bill |

### Inventory Receipt
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/InventoryReceipt/GetById` | Get receipt by ID |
| POST | `/api/InventoryReceipt/CreateOrUpdate` | Create or update receipt |

### Journal Entry
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/JournalEntry/GetById` | Get journal entry by ID |
| POST | `/api/JournalEntry/CreateOrUpdate` | Create or update journal entry |

### Print Report
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/PrintReport/GetById` | Get report definition by ID |
| POST | `/api/PrintReport/Generate` | Generate a report |
| GET | `/api/PrintReport/GetTemplate` | Get report template |

---

## Common Query Parameters

Most `GetById` endpoints support:
- `id` (required) - Entity UUID
- `includes` - Comma-separated navigation properties to include
- `selects` - Comma-separated properties to select

**Example:**
```
/api/Customer/GetById?id=xxx&includes=Addresses,PaymentTerms&selects=*,Addresses.*
```

---

## Response Format

All responses are JSON. Successful responses return the entity or array of entities. Errors return:

```json
{
  "errors": "Error message here"
}
```

---

## System Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/version` | API version info |
| GET | `/openapi.json` | OpenAPI 3.0 specification |

---

## Technical Details

### P4BSwagger Server
- .NET 10.0 / ASP.NET Core Minimal APIs
- Swashbuckle.AspNetCore 10.0.1
- Custom API proxy with DNS fallback
- Deployed to Azure Web Apps

### P4Books Backend
- .NET 10.0 / ASP.NET Core
- Entity Framework Core
- SQL Server (tenant databases)
- MongoDB (document storage)
- Multi-tenant architecture

### DNS Resolution Fix
Azure App Service cannot resolve `*.p4books.cloud` domains. The proxy includes a custom `ConnectCallback` that falls back to hardcoded IP `20.9.134.138` when DNS resolution fails.

---

## Files Modified

### P4BSwagger
- `Program.cs` - API proxy, Swagger UI configuration, custom branding
- `openapi.json` - OpenAPI 3.0 specification
- `P4B.Api.csproj` - Project configuration

### P4Books-Angel
- `AuthenticationHandler.cs` - API key-based tenant lookup
- `WebExtensions.cs` - X-Tenant-Alias header support
- `InventoryController.cs` - Inventory API endpoints

---

## Deployment

### P4BSwagger
- GitHub Actions: `.github/workflows/main_p4bswagger.yml`
- Trigger: Push to `main` branch
- Azure Web App: `p4bswagger`

### P4Books Backend
- GitHub Actions: `.github/workflows/main_p4books.yml`
- Trigger: Push to `main` branch
- Azure Web App: `p4books`

---

## Version History

- **2025-12-18**: Added Inventory API endpoints
- **2025-12-18**: Fixed API proxy DNS resolution
- **2025-12-18**: Added API key-based tenant lookup
- **2025-12-18**: Initial Swagger UI setup with custom P4 Software branding

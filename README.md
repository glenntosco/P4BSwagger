# P4Books API Documentation

Standalone Swagger UI for the P4Books ERP API.

## Overview

This project provides an interactive API documentation interface for P4Books ERP using Swagger UI (Swashbuckle.AspNetCore 10.0.1).

**Live URLs:**
- Production: https://api.p4books.cloud
- Testing: https://api.p4books.local

## Features

- OpenAPI 3.0.3 specification
- Interactive "Try It Out" functionality
- Custom P4Books branding
- 2-column responsive header layout
- Syntax highlighting (Monokai theme)
- Persistent authorization tokens
- Response validation

## Tech Stack

- .NET 10.0
- Swashbuckle.AspNetCore 10.0.1
- IIS (OutOfProcess hosting)

## API Endpoints

The API documentation covers the following P4Books entities:

### Master Data
- **Customer** - Customer management
- **Vendor** - Vendor/supplier management
- **Product** - Product catalog
- **Warehouse** - Warehouse locations
- **Account** - Chart of accounts

### Sales Documents
- **SalesQuote** - Sales quotations
- **SalesOrder** - Sales orders
- **Invoice** - Customer invoices
- **InventoryDelivery** - Delivery notes

### Purchase Documents
- **PurchaseOrder** - Purchase orders
- **VendorBill** - Vendor bills
- **InventoryReceipt** - Goods receipts

### Accounting
- **JournalEntry** - General ledger entries

### Reporting
- **PrintReport** - Report templates
- **Dashboard** - Dashboard metrics

## Authentication

The API uses two authentication methods:
- **ApiKey** - Header: `ApiKey`
- **AuthenticationToken** - Header: `AuthenticationToken`

## Deployment

### Prerequisites
- .NET 10.0 SDK
- IIS with ASP.NET Core Hosting Bundle

### Publish

```bash
dotnet publish P4B.Api/P4B.Api.csproj -c Release -o publish
```

### IIS Configuration

1. Create a new IIS site pointing to the `publish` folder
2. Add HTTPS bindings for your domains
3. Assign SSL certificates

## Project Structure

```
P4B_Api/
├── P4B.Api/
│   ├── Program.cs          # Main application with Swagger UI config
│   ├── openapi.json        # OpenAPI specification
│   ├── web.config          # IIS configuration
│   └── P4B.Api.csproj      # Project file
├── publish/                # Deployment output (gitignored)
├── .gitignore
└── README.md
```

## Development

### Run locally

```bash
cd P4B.Api
dotnet run
```

Access Swagger UI at: http://localhost:5000

### Update OpenAPI spec

Edit `P4B.Api/openapi.json` to add or modify API endpoints.

## License

Proprietary - P4Books ERP

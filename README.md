# P4Books API Documentation

Standalone Swagger UI for the P4Books ERP API, deployed on Azure Web Apps.

## Overview

This project provides an interactive API documentation interface for P4Books ERP using Swagger UI (Swashbuckle.AspNetCore 10.0.1).

**Live URL:** https://api.p4books.cloud

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
- Azure Web Apps (Linux)
- GitHub Actions CI/CD

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

### Automatic Deployment (GitHub Actions)

Push to `main` branch triggers automatic deployment to Azure Web Apps.

### Azure Setup

1. Create Azure Web App with .NET 10.0 runtime
2. Download the **Publish Profile** from Azure Portal
3. In GitHub repo → Settings → Secrets → Actions
4. Add secret `AZURE_WEBAPP_PUBLISH_PROFILE` with the publish profile content
5. Update `AZURE_WEBAPP_NAME` in `.github/workflows/azure-deploy.yml`

### Local Development

```bash
cd P4B.Api
dotnet run
```

Access Swagger UI at: http://localhost:5000

## Project Structure

```
P4B_Api/
├── .github/
│   └── workflows/
│       └── azure-deploy.yml   # CI/CD pipeline
├── P4B.Api/
│   ├── Program.cs             # Main application
│   ├── openapi.json           # OpenAPI specification
│   ├── appsettings.json       # Configuration
│   └── P4B.Api.csproj         # Project file
├── .gitignore
└── README.md
```

## License

Proprietary - P4Books ERP

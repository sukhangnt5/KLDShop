# KLDShop - E-commerce Web Application

KLDShop is a full-featured e-commerce platform built with ASP.NET Core MVC, designed for online retail operations with integrated payment processing, newsletter management, and comprehensive product management.

## 🚀 Features

### Customer Features
- **Product Browsing**: Browse products by categories with search and filtering
- **Shopping Cart**: Add, remove, and manage items in shopping cart
- **Wishlist**: Save favorite products for later
- **User Accounts**: Registration, login, and profile management
- **Order Management**: Place orders, view order history, and track status
- **Multiple Payment Options**: VNPay and PayPal integration

### Admin Features
- **Dashboard**: Overview of sales, orders, and statistics
- **Product Management**: Create, edit, and delete products with images
- **Order Management**: View and manage customer orders
- **User Management**: Manage customer accounts
- **Newsletter Management**: Manage email subscribers and campaigns via MailChimp
- **Email Campaigns**: Create and send marketing campaigns

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 10.0 MVC
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity
- **Payment Gateways**: 
  - VNPay (Vietnam)
  - PayPal (International)
- **Email Marketing**: MailChimp API integration
- **Frontend**: Bootstrap, jQuery
- **Cloud**: Azure deployment ready

## 📋 Prerequisites

- .NET 10.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code
- Git

## ⚙️ Installation & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/YOUR_USERNAME/KLDShop.git
cd KLDShop
```

### 2. Configure Database
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=KLDShop;Trusted_Connection=true;Encrypt=false;"
  }
}
```

### 3. Run Database Migrations
```bash
dotnet ef database update
```

Or run the SQL scripts in order:
1. `KLDShop_Database.sql`
2. `Database_Newsletter_EmailCampaign.sql`
3. `Database_ProductImages.sql`

### 4. Configure Payment Gateways (Optional)

Create `appsettings.Development.json` with your API credentials:
```json
{
  "VNPay": {
    "Enabled": true,
    "TmnCode": "YOUR_TMNCODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:7012/Order/PaymentReturn"
  },
  "PayPal": {
    "Enabled": true,
    "Mode": "sandbox",
    "ClientId": "YOUR_PAYPAL_CLIENT_ID",
    "ClientSecret": "YOUR_PAYPAL_CLIENT_SECRET"
  },
  "MailChimp": {
    "ApiKey": "YOUR_MAILCHIMP_API_KEY",
    "ListId": "YOUR_LIST_ID"
  }
}
```

### 5. Run the Application
```bash
dotnet run
```

Navigate to `https://localhost:7012` in your browser.

## 📦 Database Seeding

To populate the database with sample data:
```bash
# Navigate to /Seed/SeedAll endpoint after running the application
```

## 🌐 Deployment

### Azure Deployment
See `AZURE_DEPLOYMENT_GUIDE.md` for detailed Azure deployment instructions.

Quick deployment script:
```powershell
.\deploy-to-azure.ps1
```

## 📁 Project Structure

```
KLDShop/
├── Controllers/         # MVC Controllers
├── Models/             # Data models and entities
├── Views/              # Razor views
├── Data/               # DbContext and data access
├── Services/           # Business logic and external services
├── Helpers/            # Utility classes
├── Migrations/         # EF Core migrations
├── wwwroot/            # Static files (CSS, JS, images)
└── Properties/         # Launch settings
```

## 🔐 Security Notes

- **Never commit sensitive data**: API keys, connection strings, and secrets should be stored in `appsettings.Development.json` or environment variables
- The `.gitignore` file is configured to exclude sensitive configuration files
- Use Azure Key Vault or similar services for production secrets

## 📝 API Integrations

### VNPay Payment Gateway
- Sandbox URL: https://sandbox.vnpayment.vn
- Documentation: https://sandbox.vnpayment.vn/apis/docs/

### PayPal Payment Gateway
- Sandbox URL: https://developer.paypal.com
- Documentation: https://developer.paypal.com/docs/

### MailChimp Email Marketing
- API Documentation: https://mailchimp.com/developer/

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Authors

- Your Name - Initial work

## 🙏 Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- Bootstrap Team
- All contributors and supporters

## 📞 Support

For support, email your-email@example.com or open an issue in the repository.

---

⭐ If you find this project useful, please consider giving it a star on GitHub!

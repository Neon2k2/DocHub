# DocHub - Document Management & Digital Signature Platform

DocHub is a comprehensive document management and digital signature platform built with .NET 8 and Angular. It provides secure document storage, digital signature capabilities, email management, and administrative tools for organizations.

## 🚀 Features

### Core Functionality
- **Document Management**: Upload, organize, and manage documents with version control
- **Digital Signatures**: Secure digital signature implementation with PROXKey integration
- **Email Management**: Automated email notifications and status tracking
- **Template System**: Reusable document templates for consistent formatting
- **Bulk Operations**: Efficient batch processing for multiple documents

### Administrative Features
- **User Management**: Role-based access control and user administration
- **Audit Trail**: Comprehensive logging and activity tracking
- **Settings Management**: Configurable system parameters and preferences

### Technical Features
- **Real-time Updates**: SignalR integration for live status updates
- **Excel Integration**: EPPlus-based Excel processing capabilities
- **Dynamic UI**: Responsive Angular frontend with Material Design
- **API Security**: JWT-based authentication and authorization

## 🏗️ Architecture

### Backend (.NET 8)
- **DocHub.API**: Main API project with controllers and middleware
- **DocHub.Application**: Business logic and service layer
- **DocHub.Core**: Domain entities and interfaces
- **DocHub.Infrastructure**: Data access and external service implementations

### Frontend (Angular)
- **Modern UI**: Angular 17+ with Material Design components
- **State Management**: NgRx for application state management
- **Responsive Design**: Mobile-first approach with SCSS styling
- **Component Architecture**: Modular, reusable components

## 🛠️ Technology Stack

### Backend
- .NET 8
- Entity Framework Core
- SignalR
- AutoMapper
- JWT Authentication
- EPPlus (Excel processing)
- SendGrid (Email services)

### Frontend
- Angular 17+
- Angular Material
- NgRx
- SCSS
- TypeScript

### Database
- SQL Server / SQLite
- Entity Framework Core Migrations

## 📋 Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server or SQLite
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd DocHub/backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   - Edit `DocHub.API/appsettings.json`
   - Update the connection string for your database

4. **Run migrations**
   ```bash
   cd DocHub.API
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd ../frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure environment**
   - Update `src/environments/environment.ts` with your API URL

4. **Run the application**
   ```bash
   ng serve
   ```

5. **Open browser**
   Navigate to `http://localhost:4200`

## 🔧 Configuration

### Environment Variables
- `ConnectionStrings:DefaultConnection`: Database connection string
- `JwtSettings:SecretKey`: JWT secret key for authentication
- `SendGrid:ApiKey`: SendGrid API key for email services
- `PROXKey:ApiKey`: PROXKey API key for digital signatures

### App Settings
- Email configuration
- File upload limits
- Digital signature settings
- Admin user credentials

## 📁 Project Structure

```
DocHub/
├── backend/
│   ├── DocHub.API/          # Main API project
│   ├── DocHub.Application/  # Business logic layer
│   ├── DocHub.Core/         # Domain entities
│   └── DocHub.Infrastructure/ # Data access layer
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/        # Core services and guards
│   │   │   ├── features/    # Feature modules
│   │   │   └── shared/      # Shared components
│   │   └── environments/    # Environment configuration
│   └── package.json
└── README.md
```

## 🔐 Security Features

- JWT-based authentication
- Role-based authorization
- Secure file uploads
- Input validation and sanitization
- CORS configuration
- HTTPS enforcement

## 📧 Email Integration

- SendGrid integration for reliable email delivery
- Email templates for notifications
- Status tracking and delivery confirmation
- Bulk email capabilities

## 📊 Digital Signatures

- PROXKey integration for secure signatures
- Signature verification and validation
- Audit trail for all signature operations
- Multiple signature support

## 🧪 Testing

### Backend Testing
```bash
dotnet test
```

### Frontend Testing
```bash
ng test
ng e2e
```

## 🚀 Deployment

### Backend Deployment
1. Build the project: `dotnet publish -c Release`
2. Deploy to your hosting platform (Azure, AWS, etc.)
3. Configure environment variables
4. Run database migrations

### Frontend Deployment
1. Build for production: `ng build --prod`
2. Deploy the `dist` folder to your web server
3. Configure API endpoints

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit your changes: `git commit -am 'Add feature'`
4. Push to the branch: `git push origin feature-name`
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the documentation

## 🔄 Version History

- **v1.0.0**: Initial release with core document management features
- **v1.1.0**: Added digital signature capabilities
- **v1.2.0**: Enhanced email management and bulk operations
- **v1.3.0**: Improved UI/UX and performance optimizations

---

**Built with ❤️ using .NET 8 and Angular**

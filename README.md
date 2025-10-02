# Tasky - Microservice Todo App

Simple todo application built with microservices architecture for learning purposes.

## Architecture

- **AuthService**: User authentication and JWT tokens (.NET 8)
- **TodoService**: Todo CRUD operations (.NET 8)  
- **NotificationService**: Event-driven notifications (.NET 8)
- **Frontend**: Angular 20 with Tailwind CSS
- **Message Queue**: RabbitMQ for service communication
- **Database**: SQL Server

## Quick Start

```bash
# Start all services
docker-compose up -d

# Access the app
http://localhost:4200
```

## Services

| Service | Port | Description |
|---------|------|-------------|
| Frontend | 4200 | Angular UI |
| AuthService | 5001 | Authentication API |
| TodoService | 5002 | Todo API |
| RabbitMQ | 15672 | Message queue management |
| SQL Server | 1433 | Database |

## Development

```bash
# Backend
dotnet run --project backend/services/AuthService/Presentation/Tasky.AuthService.API
dotnet run --project backend/services/TodoService/Presentation/Tasky.TodoService.API

# Frontend
cd frontend && npm start
```

This is a learning project to explore microservices patterns with .NET and Angular.
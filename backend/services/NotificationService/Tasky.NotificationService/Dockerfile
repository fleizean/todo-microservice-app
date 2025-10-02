FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Tasky.NotificationService/Tasky.NotificationService.csproj"
RUN dotnet build "Tasky.NotificationService/Tasky.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tasky.NotificationService/Tasky.NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasky.NotificationService.dll"]
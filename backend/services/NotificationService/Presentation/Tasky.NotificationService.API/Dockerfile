FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "Presentation/Tasky.NotificationService.API/Tasky.NotificationService.API.csproj"
RUN dotnet build "Presentation/Tasky.NotificationService.API/Tasky.NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Presentation/Tasky.NotificationService.API/Tasky.NotificationService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasky.NotificationService.API.dll"]


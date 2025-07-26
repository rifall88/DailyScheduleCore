# Image base ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Image untuk build .NET project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy semua file
COPY . .

# Tentukan path ke project
WORKDIR /src/DailySchedule

# Restore dependencies
RUN dotnet restore

# Build dan publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage: jalankan dari hasil publish
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DailySchedule.dll"]

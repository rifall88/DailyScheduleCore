FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj dan restore dependencies
COPY DailySchedule/DailySchedule.csproj DailySchedule/
WORKDIR /src/DailySchedule
RUN dotnet restore

# Copy semua source code
COPY DailySchedule/. .

RUN dotnet build -c ${BUILD_CONFIGURATION} -o /app/build
RUN dotnet publish -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DailySchedule.dll"]

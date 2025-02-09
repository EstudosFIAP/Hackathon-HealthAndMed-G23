# Dockerfile (HealthAndMed.API/Dockerfile)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HealthAndMed.API/HealthAndMed.API.csproj", "HealthAndMed.API/"]
COPY ["HealthAndMed.Core/HealthAndMed.Core.csproj", "HealthAndMed.Core/"]
COPY ["HealthAndMed.Infraestructure/HealthAndMed.Infraestructure.csproj", "HealthAndMed.Infraestructure/"]
RUN dotnet restore "./HealthAndMed.API/HealthAndMed.API.csproj"
COPY . .
WORKDIR "/src/HealthAndMed.API"
RUN dotnet build "./HealthAndMed.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HealthAndMed.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthAndMed.API.dll"]

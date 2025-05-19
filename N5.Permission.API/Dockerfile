# Etapa 1: build con SDK de .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los csproj
COPY N5.Permissions.Domain/N5.Permissions.Domain.csproj N5.Permissions.Domain/
COPY N5.Permissions.Application/N5.Permissions.Application.csproj N5.Permissions.Application/
COPY N5.Permissions.Infrastructure/N5.Permissions.Infrastructure.csproj N5.Permissions.Infrastructure/
COPY N5.Permission.API/N5.Permissions.API.csproj N5.Permission.API/

# Restaurar dependencias
RUN dotnet restore N5.Permission.API/N5.Permissions.API.csproj

# Copiar y compilar
COPY . .
WORKDIR /src/N5.Permission.API
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime con ASP.NET Core 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "N5.Permissions.API.dll"]

# ==============================================================================
# Build Stage (.NET 10 SDK)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY src/Orion.sln src/Orion.sln
COPY src/Orion.Domain/Orion.Domain.csproj src/Orion.Domain/Orion.Domain.csproj
COPY src/Orion.Application/Orion.Application.csproj src/Orion.Application/Orion.Application.csproj
COPY src/Orion.Infrastructure/Orion.Infrastructure.csproj src/Orion.Infrastructure/Orion.Infrastructure.csproj
COPY src/Orion.Api/Orion.Api.csproj src/Orion.Api/Orion.Api.csproj
COPY tests/Orion.UnitTests/Orion.UnitTests.csproj tests/Orion.UnitTests/Orion.UnitTests.csproj

RUN dotnet restore src/Orion.sln

# Copy remaining source code and publish the API project
COPY . .
RUN dotnet publish src/Orion.Api/Orion.Api.csproj -c Release -o /app/publish --no-restore

# ==============================================================================
# Runtime Stage (.NET 10 ASP.NET Runtime)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Listen on Render's default port 10000 (Program.cs also honors $PORT if injected by Render)
ENV ASPNETCORE_URLS=http://+:10000 \
    ASPNETCORE_HTTP_PORTS=

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Orion.Api.dll"]

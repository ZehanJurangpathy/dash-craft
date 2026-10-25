# Build context: repository root (see docker-compose.yml), so the root props files are available.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1
WORKDIR /repo

# Restore first so the layer is cached until a project file or package pin changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/api/DashCraft.Domain/DashCraft.Domain.csproj src/api/DashCraft.Domain/
COPY src/api/DashCraft.Application/DashCraft.Application.csproj src/api/DashCraft.Application/
COPY src/api/DashCraft.Infrastructure/DashCraft.Infrastructure.csproj src/api/DashCraft.Infrastructure/
COPY src/api/DashCraft.Api/DashCraft.Api.csproj src/api/DashCraft.Api/
RUN dotnet restore src/api/DashCraft.Api/DashCraft.Api.csproj

COPY src/api/ src/api/
RUN dotnet publish src/api/DashCraft.Api/DashCraft.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ENV ASPNETCORE_HTTP_PORTS=8080
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "DashCraft.Api.dll"]

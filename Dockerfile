# ============================================================================
# Simple Blocks API — multi-stage Docker image
# Build:  mcr.microsoft.com/dotnet/sdk:10.0
# Runtime: mcr.microsoft.com/dotnet/aspnet:10.0 (no SDK in the final image)
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала копируем только csproj/манифесты для кэширования слоя restore.
COPY SimpleBlocks.slnx .
COPY SimpleBlocks.Domain/SimpleBlocks.Domain.csproj              SimpleBlocks.Domain/
COPY SimpleBlocks.Application/SimpleBlocks.Application.csproj    SimpleBlocks.Application/
COPY SimpleBlocks.Infrastructure/SimpleBlocks.Infrastructure.csproj SimpleBlocks.Infrastructure/
COPY SimpleBlocks.API/SimpleBlocks.API.csproj                    SimpleBlocks.API/
COPY SimpleBlocks.API/Resources/wordlist.txt                     SimpleBlocks.API/Resources/
COPY SimpleBlocks.API/wwwroot/                                   SimpleBlocks.API/wwwroot/

RUN dotnet restore SimpleBlocks.slnx

# Остальной исходный код.
COPY . .

RUN dotnet publish SimpleBlocks.API -c Release -o /app/publish --no-restore

# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Статик-web-assets требует наличия директории wwwroot при запуске.
RUN mkdir -p /app/wwwroot

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "SimpleBlocks.API.dll"]
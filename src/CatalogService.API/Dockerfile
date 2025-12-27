FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Eventing.Abstraction/Eventing.Abstraction.csproj Eventing.Abstraction/
COPY src/Eventing.Infrastructure/Eventing.Infrastructure.csproj Eventing.Infrastructure/
COPY src/CatalogService.Events/CatalogService.Events.csproj CatalogService.Events/
COPY src/CatalogService.Domain/CatalogService.Domain.csproj CatalogService.Domain/
COPY src/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj CatalogService.Infrastructure/
COPY src/CatalogService.Application/CatalogService.Application.csproj CatalogService.Application/
COPY src/CatalogService.API/CatalogService.API.csproj CatalogService.API/
COPY Directory.*.props ./
RUN dotnet restore ./CatalogService.API/CatalogService.API.csproj
COPY ./src/ .
RUN dotnet publish ./CatalogService.API/CatalogService.API.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
EXPOSE 8080
EXPOSE 8081
# Enable ICU globalization and locale. Required for proper culture and TimeZoneInfo support in alpine. 
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
RUN apk add --no-cache icu-data-full icu-libs tzdata
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]

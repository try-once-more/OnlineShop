FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Eventing.Abstraction/Eventing.Abstraction.csproj Eventing.Abstraction/
COPY src/Eventing.Infrastructure/Eventing.Infrastructure.csproj Eventing.Infrastructure/
COPY src/CatalogService.Events/CatalogService.Events.csproj CatalogService.Events/
COPY src/CartService/CartService.csproj CartService/
COPY src/CartService.API/CartService.API.csproj CartService.API/
COPY Directory.*.props ./
RUN dotnet restore ./CartService.API/CartService.API.csproj
COPY ./src/ .
RUN dotnet publish ./CartService.API/CartService.API.csproj -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CartService.API.dll"]

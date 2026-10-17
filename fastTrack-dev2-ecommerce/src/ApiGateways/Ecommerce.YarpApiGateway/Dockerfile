# Build context: src/. Builds the React app (Web) and serves it from the gateway's wwwroot.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM node:20-alpine AS web
WORKDIR /web
COPY ["Web/package.json", "Web/yarn.lock", "./"]
RUN yarn install --frozen-lockfile
COPY Web/ .
RUN yarn build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiGateways/Ecommerce.YarpApiGateway/Ecommerce.YarpApiGateway.csproj", "ApiGateways/Ecommerce.YarpApiGateway/"]
RUN dotnet restore "./ApiGateways/Ecommerce.YarpApiGateway/Ecommerce.YarpApiGateway.csproj"
COPY ApiGateways/Ecommerce.YarpApiGateway/ ApiGateways/Ecommerce.YarpApiGateway/
WORKDIR "/src/ApiGateways/Ecommerce.YarpApiGateway"
RUN dotnet publish "./Ecommerce.YarpApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web /web/dist ./wwwroot
ENTRYPOINT ["dotnet", "Ecommerce.YarpApiGateway.dll"]

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Basket/Ecommerce.Basket.Api/Ecommerce.Basket.Api.csproj", "Services/Basket/Ecommerce.Basket.Api/"]
COPY ["Shared/Ecommerce.Shared.Messaging/Ecommerce.Shared.Messaging.csproj", "Shared/Ecommerce.Shared.Messaging/"]
COPY ["Shared/Ecommerce.Shared/Ecommerce.Shared.csproj", "Shared/Ecommerce.Shared/"]
RUN dotnet restore "./Services/Basket/Ecommerce.Basket.Api/./Ecommerce.Basket.Api.csproj"
COPY . .
WORKDIR "/src/Services/Basket/Ecommerce.Basket.Api"
RUN dotnet build "./Ecommerce.Basket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Basket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Basket.Api.dll"]
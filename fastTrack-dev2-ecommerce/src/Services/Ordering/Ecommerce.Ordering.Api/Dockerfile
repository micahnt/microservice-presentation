#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Ordering/Ecommerce.Ordering.Api/Ecommerce.Ordering.Api.csproj", "Services/Ordering/Ecommerce.Ordering.Api/"]
COPY ["Services/Ordering/Ecommerce.Ordering.Application/Ecommerce.Ordering.Application.csproj", "Services/Ordering/Ecommerce.Ordering.Application/"]
COPY ["Shared/Ecommerce.Shared.Messaging/Ecommerce.Shared.Messaging.csproj", "Shared/Ecommerce.Shared.Messaging/"]
COPY ["Shared/Ecommerce.Shared/Ecommerce.Shared.csproj", "Shared/Ecommerce.Shared/"]
COPY ["Services/Ordering/Ecommerce.Ordering.Domain/Ecommerce.Ordering.Domain.csproj", "Services/Ordering/Ecommerce.Ordering.Domain/"]
COPY ["Services/Ordering/Ecommerce.Ordering.Infrastructure/Ecommerce.Ordering.Infrastructure.csproj", "Services/Ordering/Ecommerce.Ordering.Infrastructure/"]
RUN dotnet restore "./Services/Ordering/Ecommerce.Ordering.Api/./Ecommerce.Ordering.Api.csproj"
COPY . .
WORKDIR "/src/Services/Ordering/Ecommerce.Ordering.Api"
RUN dotnet build "./Ecommerce.Ordering.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Ordering.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Ordering.Api.dll"]
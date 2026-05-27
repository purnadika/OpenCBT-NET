# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OpenCBT.Web/OpenCBT.Web.csproj", "src/OpenCBT.Web/"]
COPY ["src/OpenCBT.Application/OpenCBT.Application.csproj", "src/OpenCBT.Application/"]
COPY ["src/OpenCBT.Domain/OpenCBT.Domain.csproj", "src/OpenCBT.Domain/"]
COPY ["src/OpenCBT.Infrastructure/OpenCBT.Infrastructure.csproj", "src/OpenCBT.Infrastructure/"]
RUN dotnet restore "./src/OpenCBT.Web/OpenCBT.Web.csproj"
COPY . .
WORKDIR "/src/src/OpenCBT.Web"
RUN dotnet build "./OpenCBT.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OpenCBT.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenCBT.Web.dll"]

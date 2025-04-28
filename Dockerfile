# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["InnoClinic.Gateway.API/InnoClinic.Gateway.API.csproj", "InnoClinic.Gateway.API/"]
COPY ["InnoClinic.Gateway.Infrastructure/InnoClinic.Gateway.Infrastructure.csproj", "InnoClinic.Gateway.Infrastructure/"]

RUN dotnet restore "InnoClinic.Gateway.API/InnoClinic.Gateway.API.csproj"

COPY . .

WORKDIR "/src/InnoClinic.Gateway.API"
RUN dotnet build "InnoClinic.Gateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Gateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InnoClinic.Gateway.API.dll"]
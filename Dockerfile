# Етап 1: Збірка додатку
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копіюємо .sln та всі необхідні .csproj файли
COPY Assessment.sln .
COPY SessionMVC/SessionMVC.csproj ./SessionMVC/
COPY Session.Application/Session.Application.csproj ./Session.Application/
COPY Session.Domain/Session.Domain.csproj ./Session.Domain/
COPY Session.Persistence/Session.Persistence.csproj ./Session.Persistence/
COPY Session.Services/Session.Services.csproj ./Session.Services/
COPY Session.UnitTests/Session.UnitTests.csproj ./Session.UnitTests/

# Відновлюємо залежності
RUN dotnet restore "Assessment.sln"

# Копіюємо решту проєкту
COPY . .

# Публікуємо додаток
WORKDIR "/src/SessionMVC"
RUN dotnet publish "SessionMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Етап 2: Запуск додатку
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "SessionMVC.dll"]
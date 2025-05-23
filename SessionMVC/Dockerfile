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
COPY docker-compose.dcproj . # Цей файл має бути доступний завдяки виправленню .dockerignore

# Відновлюємо залежності для всього рішення
RUN dotnet restore "Assessment.sln"

# Копіюємо весь інший код проєкту (після restore, щоб використати кешування)
COPY . .

# Публікуємо додаток SessionMVC
WORKDIR "/src/SessionMVC"
RUN dotnet publish "SessionMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Етап 2: Запуск додатку
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Явно вказуємо ASP.NET Core слухати на порту 5000 для HTTP
ENV ASPNETCORE_HTTP_PORTS=5000

EXPOSE 5000 # Експонуємо порт, на якому додаток фактично слухає

ENTRYPOINT ["dotnet", "SessionMVC.dll"]

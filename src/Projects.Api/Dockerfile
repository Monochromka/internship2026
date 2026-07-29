# Етап збірки (build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо файл проєкту та відновлюємо залежності
COPY ["Projects.Api.csproj", "./"]
RUN dotnet restore "Projects.Api.csproj"

# Копіюємо весь інший вихідний код і публікуємо додаток
COPY . .
RUN dotnet publish "Projects.Api.csproj" -c Release -o /app/publish

# Етап виконання (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Відкриваємо порт 8080 (стандарт для ASP.NET Core в контейнерах)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Projects.Api.dll"]
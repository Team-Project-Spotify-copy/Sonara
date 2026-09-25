# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Вказуємо повний шлях до .csproj з урахуванням структури папок
COPY ["Backend/Project/WebApp.csproj", "Backend/Project/"]
RUN dotnet restore "Backend/Project/WebApp.csproj"
# Копіюємо файл рішення або всі проєкти з урахуванням структури папок Backend
COPY ["Project/WebApp.csproj", "Project/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]

RUN dotnet restore "Project/WebApp.csproj"

# Копіюємо решту файлів і переходимо в папку з проєктом для збірки
# Копіюємо решту файлів всього бекенду
COPY . .
WORKDIR "/src/Project"
RUN dotnet build "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
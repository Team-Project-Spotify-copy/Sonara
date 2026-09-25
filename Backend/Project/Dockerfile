# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копіюємо .csproj з урахуванням папки Backend/Project
COPY ["Backend/Project/WebApp.csproj", "Backend/Project/"]
RUN dotnet restore "Backend/Project/WebApp.csproj"

# Копіюємо решту файлів репозиторію
COPY . .
WORKDIR "/src/Backend/Project"
RUN dotnet build "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
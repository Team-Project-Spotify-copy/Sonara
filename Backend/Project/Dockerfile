# Stage 1: Base
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копіюємо файл проєкту в корінь збірки /src
COPY ["WebApp.csproj", "./"]
RUN dotnet restore "WebApp.csproj"

# Копіюємо решту файлів бекенду
COPY . .
RUN dotnet build "WebApp.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
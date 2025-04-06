FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID

WORKDIR /app
EXPOSE 8080
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS  mysdk
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    node -v && npm -v

FROM mysdk AS  build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BonusesApp.Server/BonusesApp.Server.csproj", "BonusesApp.Server/"]
COPY ["bonusesApp.client/bonusesApp.client.esproj", "bonusesApp.client/"]
COPY ["BonusesApp.Core/BonusesApp.Core.csproj", "BonusesApp.Core/"]
RUN dotnet restore "BonusesApp.Server/BonusesApp.Server.csproj"
COPY . .

WORKDIR "/src/BonusesApp.Server"
RUN dotnet build "BonusesApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BonusesApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY yourcert.pfx ./yourcert.pfx
COPY authcert.pfx ./authcert.pfx
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BonusesApp.Server.dll"]

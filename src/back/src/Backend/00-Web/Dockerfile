# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l’exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Ajout ICU pour la gestion des cultures (fr-FR, etc.)
RUN apk add --no-cache icu-libs icu-data-full


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Backend/00-Web/00-Web.csproj", "src/Backend/00-Web/"]
COPY ["src/Backend/01-ServicesConfiguration/01-ServicesConfiguration.csproj", "src/Backend/01-ServicesConfiguration/"]
COPY ["src/Backend/10-Application/10-Application.csproj", "src/Backend/10-Application/"]
COPY ["src/Backend/22-Infrastructure.Persistence.SQLServer/22-Infrastructure.Persistence.SQLServer.csproj", "src/Backend/22-Infrastructure.Persistence.SQLServer/"]
COPY ["src/Backend/12-Application.Common/12-Application.Common.csproj", "src/Backend/12-Application.Common/"]
COPY ["src/Backend/21-Infrastructure.Persistence/21-Infrastructure.Persistence.csproj", "src/Backend/21-Infrastructure.Persistence/"]
COPY ["src/Backend/99-Tools/99-Tools.csproj", "src/Backend/99-Tools/"]
COPY ["src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Persistence/Pcea.Core.Net.Authorization.Persistence.csproj", "src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Persistence/"]
COPY ["src/Backend/Pcea.Core/Pcea.Core.Net.Authorization/Pcea.Core.Net.Authorization.csproj", "src/Backend/Pcea.Core/Pcea.Core.Net.Authorization/"]
COPY ["src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Application/Pcea.Core.Net.Authorization.Application.csproj", "src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Application/"]
COPY ["src/Backend/11-Application.Features/11-Application.Features.csproj", "src/Backend/11-Application.Features/"]
COPY ["src/Backend/13-Application.Ressources/13-Application.Ressources.csproj", "src/Backend/13-Application.Ressources/"]
COPY ["src/Backend/20-Infrastructure/20-Infrastructure.csproj", "src/Backend/20-Infrastructure/"]
COPY ["src/Backend/23-Infrastructure.Persistence.File/23-Infrastructure.Persistence.File.csproj", "src/Backend/23-Infrastructure.Persistence.File/"]
COPY ["src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Web/Pcea.Core.Net.Authorization.Web.csproj", "src/Backend/Pcea.Core/Pcea.Core.Net.Authorization.Web/"]

RUN dotnet restore "./src/Backend/00-Web/00-Web.csproj"
COPY . .

#Build
WORKDIR "/src/src/Backend/00-Web"
RUN dotnet build "./00-Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./00-Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "00-Web.dll"]
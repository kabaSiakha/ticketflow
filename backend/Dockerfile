# Backend TicketFlow (API ASP.NET Core) : build multi-étapes, utilisateur non-root

# Étape "build" : compiler avec le SDK .NET
FROMM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Dépendances d'abord : cette couche reste en cache tant que le .csproj ne change pas
COPY src/TicketFlow.Api/TicketFlow.Api.csproj src/TicketFlow.Api/
RUN dotnet restore src/TicketFlow.Api/TicketFlow.Api.csproj

# Puis le code, et la publication
COPY src/ src/
RUN dotnet publish src/TicketFlow.Api/TicketFlow.Api.csproj -c Release -o /app --no-restore

# Étape finale : seulement le runtime ASP.NET Core et l'API compilée
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TicketFlow.Api.dll"]

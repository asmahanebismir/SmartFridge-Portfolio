FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Backend/SmartFridgeBackend.csproj
RUN dotnet publish Backend/SmartFridgeBackend.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartFridgeBackend.dll"]


# Before deployment: Store secrets in render and not a lokal json file. Secrets like apikeys. 
# Tanken om hvordan vi gør det:
# Render: Laver vi en web service til at hoste backend og en static page til at hoste frontend.
# mongodb står for databasen
# og alt authentication er gennem firebase. 
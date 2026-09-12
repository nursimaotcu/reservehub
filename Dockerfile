FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/ReserveHub.Api/ReserveHub.Api.csproj src/ReserveHub.Api/
RUN dotnet restore src/ReserveHub.Api/ReserveHub.Api.csproj
COPY src/ src/
RUN dotnet publish src/ReserveHub.Api/ReserveHub.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ReserveHub.Api.dll"]

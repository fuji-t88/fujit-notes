# Step 26: Fujit Notes ASP.NET Core API

# 1. Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/ ./src/

RUN dotnet restore src/FujitNotes.Api/FujitNotes.Api.csproj

RUN dotnet publish src/FujitNotes.Api/FujitNotes.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# 2. Production runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish ./

USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "FujitNotes.Api.dll"]
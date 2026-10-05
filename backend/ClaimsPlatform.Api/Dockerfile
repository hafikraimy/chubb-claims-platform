FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ClaimsPlatform.Api/ClaimsPlatform.Api.csproj ClaimsPlatform.Api/
RUN dotnet restore ClaimsPlatform.Api/ClaimsPlatform.Api.csproj

COPY ClaimsPlatform.Api/ ClaimsPlatform.Api/
RUN dotnet publish ClaimsPlatform.Api/ClaimsPlatform.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClaimsPlatform.Api.dll"]

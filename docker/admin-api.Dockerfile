# syntax=docker/dockerfile:1.7
# Build context: repository root

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY BFA.sln ./
COPY src/ ./src/
RUN dotnet restore "src/BFA.Admin.Api/BFA.Admin.Api.csproj"

FROM restore AS publish
RUN dotnet publish "src/BFA.Admin.Api/BFA.Admin.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    APP_DLL=BFA.Admin.Api.dll
EXPOSE 8080
COPY docker/dotnet-entrypoint.sh /app/entrypoint.sh
COPY --from=publish /app/publish .
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]


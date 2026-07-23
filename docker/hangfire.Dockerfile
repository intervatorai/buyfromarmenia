# syntax=docker/dockerfile:1.7
# Build context: repository root

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY BFA.sln ./
COPY src/ ./src/
RUN dotnet restore "src/BFA.Hangfire/BFA.Hangfire.csproj"

FROM restore AS publish
RUN dotnet publish "src/BFA.Hangfire/BFA.Hangfire.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=publish /app/publish .
CMD ["dotnet", "BFA.Hangfire.dll"]


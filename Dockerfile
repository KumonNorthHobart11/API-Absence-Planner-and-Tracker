# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore "Api-absence-planner-and-tracker.sln"
RUN dotnet publish "Api-absence-planner-and-tracker/Api-absence-planner-and-tracker.csproj" -c Release -o out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Firebase credentials must be injected at runtime via this environment variable.
# Set Firebase__CredentialJson to the full contents of the service account JSON file
# in your deployment platform's secret/environment variable settings.
# Example (Render / Railway / Docker run):
#   -e Firebase__CredentialJson='{ "type": "service_account", ... }'
ENV Firebase__CredentialJson=""

ENTRYPOINT ["dotnet", "Api-absence-planner-and-tracker.dll"]

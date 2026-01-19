# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

# NEW STEP: Restore NuGet packages (required before publish)
RUN dotnet restore 

# Original Publish Step 
RUN dotnet publish -c Release -o /app/publish --no-restore 

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The entry point to run the published DLL
ENTRYPOINT ["dotnet", "API.dll"]

EXPOSE 8080

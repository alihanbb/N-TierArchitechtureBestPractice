# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the solution file and project files
COPY ["KatmanlıMimari.sln", "./"]
COPY ["AppHost/AppHost.csproj", "AppHost/"]
COPY ["AppApis/AppApi.csproj", "AppApis/"]
COPY ["AppService/AppService.csproj", "AppService/"]
COPY ["AppRepository/AppRepository.csproj", "AppRepository/"]
COPY ["AppServiceDefaults/AppServiceDefaults.csproj", "AppServiceDefaults/"]

# Restore NuGet packages
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "KatmanlıMimari.sln" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "KatmanlıMimari.sln" -c Release -o /app/publish

# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "AppHost.dll"] 
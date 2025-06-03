FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AppApis/AppApis.csproj ./
RUN dotnet restore "./AppApi.csproj"
COPY AppApis/. ./
RUN dotnet build "AppApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppApis.dll"]
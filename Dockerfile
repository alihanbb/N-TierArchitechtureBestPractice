FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AppHost/AppHost.csproj", "AppHost/"]
COPY ["AppApis/AppApi.csproj", "AppApis/"]
COPY ["AppService/AppService.csproj", "AppService/"]
COPY ["AppRepository/AppRepository.csproj", "AppRepository/"]
RUN dotnet restore "AppApis/AppApi.csproj"
COPY . .
WORKDIR "/src/AppApis"
RUN dotnet build "AppApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "AppApi.dll"] 
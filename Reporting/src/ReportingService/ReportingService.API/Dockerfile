#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ReportingService/ReportingService.API/ReportingService.API.csproj", "src/ReportingService/ReportingService.API/"]
RUN dotnet restore "src/ReportingService/ReportingService.API/ReportingService.API.csproj"
COPY . .
WORKDIR "/src/src/ReportingService/ReportingService.API"
RUN dotnet build "ReportingService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportingService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportingService.API.dll"]
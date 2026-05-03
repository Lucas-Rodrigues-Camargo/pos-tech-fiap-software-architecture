FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OficinaMecanica.slnx ./
COPY OficinaMecanica.Domain/ OficinaMecanica.Domain/
COPY OficinaMecanica.Application/ OficinaMecanica.Application/
COPY OficinaMecanica.Infrastructure/ OficinaMecanica.Infrastructure/
COPY OficinaMecanica.Api/ OficinaMecanica.Api/

RUN dotnet restore OficinaMecanica.Api/OficinaMecanica.Api.csproj
RUN dotnet publish OficinaMecanica.Api/OficinaMecanica.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OficinaMecanica.Api.dll"]

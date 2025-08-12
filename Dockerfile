# Etapa 1: Construcción de la aplicación
# Usar la imagen base de .NET SDK
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Usar la imagen de .NET SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FORO-UTTN-API/FORO-UTTN-API.csproj", "FORO-UTTN-API/"]
RUN dotnet restore "FORO-UTTN-API/FORO-UTTN-API.csproj"
COPY . .
WORKDIR "/src/FORO-UTTN-API"
RUN dotnet build "FORO-UTTN-API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FORO-UTTN-API.csproj" -c Release -o /app/publish

# Etapa 2: Generar la imagen
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MONGO_DB_CONNECTION_STRING="mongodb://172.16.0.164:27017/foro_uttn"
ENTRYPOINT ["dotnet", "FORO-UTTN-API.dll"]

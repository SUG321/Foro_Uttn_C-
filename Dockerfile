# Etapa 1: Construcción de la aplicación
# Usar la imagen de SDK para compilar y publicar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["FORO-UTTN-API.csproj", "./"]
RUN dotnet restore "FORO-UTTN-API.csproj"

# Copiar el resto del código fuente y compilar en modo Release
COPY . .
RUN dotnet publish "FORO-UTTN-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configuración de la conexión a MongoDB (puede sobrescribirse al ejecutar el contenedor)
ENV MONGO_CONNECTION_STRING="mongodb+srv://Vero:Polla010@forouttn.ehkd7ie.mongodb.net/foro_uttn?retryWrites=true&w=majority"
EXPOSE 8080
ENTRYPOINT ["dotnet", "FORO-UTTN-API.dll"]
# Etapa 1 — Build (compila o projeto)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia o arquivo .csproj e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia todo o resto do projeto e faz o build
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2 — Runtime (imagem leve só para executar)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expõe a porta padrão do ASP.NET
EXPOSE 80

# Define o ponto de entrada 
ENTRYPOINT ["dotnet", "endpointsInterface.dll"]

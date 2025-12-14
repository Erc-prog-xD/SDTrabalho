# ============================
# STAGE 1: BUILD (MAVEN)
# ============================
FROM maven:3.9.9-eclipse-temurin-17 AS build

# Diretório de trabalho dentro do container
WORKDIR /app

# Copia o pom.xml e arquivos de configuração primeiro (melhora o cache)
COPY pom.xml .
COPY src ./src

# Faz o build do projeto (gera o JAR em target/)
RUN mvn clean package -DskipTests


# ============================
# STAGE 2: RUNTIME (JRE)
# ============================
FROM eclipse-temurin:17-jre

# Diretório de trabalho do container final
WORKDIR /app

# Copia o JAR gerado no stage de build
# Se o seu JAR tiver nome fixo, pode trocar o *.jar pelo nome exato
COPY --from=build /app/target/*.jar app.jar

# Porta do gRPC (ajusta se você mudou no application.properties/yml)
EXPOSE 9090

# Variáveis de ambiente opcionais (bom pra configurar conexão com banco via env)
# ENV SPRING_DATASOURCE_URL=jdbc:sqlserver://sqlserver:1433;databaseName=clinica_db;encrypt=false;trustServerCertificate=true
# ENV SPRING_DATASOURCE_USERNAME=sa
# ENV SPRING_DATASOURCE_PASSWORD=SuaSenhaSuperSecreta

# Comando de execução
ENTRYPOINT ["java", "-jar", "app.jar"]

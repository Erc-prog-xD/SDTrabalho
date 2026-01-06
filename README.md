# SDTrabalho

Projeto de **Sistema Distribuído** composto por múltiplos serviços que se comunicam utilizando **REST, gRPC, RMI, Socket TCP e mensageria**.  
O projeto é executado de forma integrada utilizando **Docker Compose**.

---

## Tecnologias Utilizadas

- Java 17
- Spring Boot
- .NET 9
- SQL Server
- RabbitMQ
- REST (HTTP)
- gRPC
- RMI
- Socket TCP
- Docker
- Docker Compose

---

## Serviços do Projeto

- **endpointsInterface** (.NET) – API principal
- **servicoUsuarios** (.NET) – Serviço de usuários + servidor Socket TCP
- **SchedulingService** (Java) – Serviço de agendamentos (gRPC)
- **ValidationService** (Java) – Serviço de validação (RMI)
- **ValidationInterface** (Java) – API REST que acessa o RMI
- **NotificationService** (.NET) – Consumo de mensagens (RabbitMQ)
- **RabbitMQ**
- **SQL Server**

---

## Tipos de Comunicação Utilizados

- **REST**: comunicação entre APIs
- **gRPC**: comunicação entre `endpointsInterface` e `SchedulingService`
- **RMI**: comunicação entre `ValidationInterface` e `ValidationService`
- **Socket TCP**: comunicação direta com o `servicoUsuarios`
- **Mensageria (RabbitMQ)**: envio e consumo de notificações

---

## Pré-requisitos

Para executar o projeto completo:

- Docker
- Docker Compose

> Não é necessário instalar Java, .NET, RabbitMQ ou SQL Server localmente.

---

## Como Executar o Projeto

Na **raiz do projeto**, onde está o arquivo `docker-compose.yml`, execute:

```bash
docker compose up --build

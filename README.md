# TMB Order Management System

Sistema de gestão de pedidos com CRUD, mensageria assíncrona via Azure Service Bus (emulador local), worker de processamento, frontend React/ShadCN e módulo de IA com Perplexity.

## Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                        Docker Compose                        │
│                                                             │
│  ┌──────────┐   REST    ┌──────────────────────────────┐   │
│  │ Frontend │ ◄────────► │          Orders.Api          │   │
│  │ React +  │  SignalR  │  .NET 8 · EF Core · SignalR  │   │
│  │  ShadCN  │ ◄────────► │  Health · Swagger · Outbox   │   │
│  └──────────┘           └──────────────┬─────────────┬─┘   │
│                                        │             │      │
│                              Outbox    │             │      │
│                           (transact.)  │        ┌────▼────┐ │
│                                   ┌───▼────┐   │ Perplx  │ │
│                                   │Service │   │   AI    │ │
│                                   │  Bus   │   └─────────┘ │
│                                   │Emulat. │               │
│                                   └───┬────┘               │
│                                       │                     │
│                               ┌───────▼──────┐             │
│                               │ Orders.Worker │             │
│                               │  .NET 8 BG   │             │
│                               │  Idempotent  │             │
│                               └───────┬──────┘             │
│                                       │                     │
│             ┌──────────────────────────▼──────────────┐    │
│             │            PostgreSQL 16                 │    │
│             │  orders · status_history · outbox_msgs   │    │
│             └─────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) ≥ 4.x
- [Git](https://git-scm.com/)
- Chave de API da [Perplexity](https://www.perplexity.ai/) *(opcional — apenas para o módulo de IA)*
---

## Rodando pela primeira vez

### 1. Clone o repositório

```bash
git clone <repo-url>
cd TMB
```

### 2. Configure as variáveis de ambiente

Copie o arquivo de exemplo:

```bash
cp .env.example .env
```

Abra `.env` e preencha os valores:

```env
# Banco de dados PostgreSQL
POSTGRES_DB=ordersdb
POSTGRES_USER=orders_user
POSTGRES_PASSWORD=orders_pass_change_me     

# Interface web do banco (PgAdmin)
PGADMIN_EMAIL=admin@tmb.com
PGADMIN_PASSWORD=admin123

# SQL Edge — dependência interna do emulador de Service Bus
# IMPORTANTE: deve conter maiúsculas, minúsculas, números e caractere especial
MSSQL_SA_PASSWORD=YourStrong!Passw0rd

# Service Bus Emulator — mantenha exatamente assim para uso local
SERVICEBUS_CONNECTION_STRING=Endpoint=sb://servicebus-emulator;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
QUEUE_NAME=orders-queue

# Perplexity (IA) — obtenha sua chave em https://www.perplexity.ai/
PERPLEXITY_API_KEY=pplx-your-key-here
PERPLEXITY_MODEL=sonar
```

### 3. Suba todos os serviços

```bash
docker compose up --build
```

O flag `--build` compila as imagens da API, Worker e Frontend antes de iniciar. Na primeira execução, o download das imagens pode levar alguns minutos.

### 4. Acesse os serviços

| Serviço | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| PgAdmin | http://localhost:5050 |
| Health check | http://localhost:8080/health |

## Parando o projeto

```
# Para os containers (preserva os dados do banco)
docker compose down

# Para os containers e apaga os volumes (reseta o banco)
docker compose down -v
```
## Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `POSTGRES_DB` | Nome do banco de dados |
| `POSTGRES_USER` | Usuário PostgreSQL |
| `POSTGRES_PASSWORD` | Senha PostgreSQL |
| `PGADMIN_EMAIL` | Email de acesso ao PgAdmin |
| `PGADMIN_PASSWORD` | Senha do PgAdmin |
| `MSSQL_SA_PASSWORD` | Senha SQL Edge (Service Bus Emulator) |
| `SERVICEBUS_CONNECTION_STRING` | Connection string do emulador |
| `QUEUE_NAME` | Nome da fila (padrão: `orders-queue`) |
| `PERPLEXITY_API_KEY` | Chave da API Perplexity |
| `PERPLEXITY_MODEL` | Modelo Perplexity (padrão: `sonar`) |

## Tecnologias

- **Backend:** C# .NET 8 · Entity Framework Core · Npgsql
- **Mensageria:** Azure Service Bus (emulador Docker)
- **Frontend:** React 19 · Vite · ShadCN/UI · TailwindCSS v4
- **Real-time:** SignalR (WebSocket com fallback Long Polling)
- **IA:** Perplexity API (compatible OpenAI format)
- **Infra:** Docker · Docker Compose · PostgreSQL 16 · PgAdmin
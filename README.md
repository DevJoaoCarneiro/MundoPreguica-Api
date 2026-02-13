# MundoPreguica API

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-blueviolet)](https://learn.microsoft.com/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-336791)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED)](https://www.docker.com/)

API para gestao de produtos, pedidos e categorias da MundoPreguica. Inclui autenticacao por JWT, upload de imagens via Cloudinary, dashboard de vendas e envio de email para eventos de pedido.

## Sumario

- [Visao geral](#visao-geral)
- [Principais recursos](#principais-recursos)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Requisitos](#requisitos)
- [Configuracao](#configuracao)
- [Como rodar](#como-rodar)
- [Endpoints](#endpoints)
- [Exemplos rapidos](#exemplos-rapidos)

## Visao geral

O projeto segue um estilo clean architecture com separacao entre camadas de API, aplicacao, dominio e infraestrutura. A API expoe endpoints para cadastro e consulta de produtos (com variacoes e estoque), processamento de pedidos, categorias e indicadores de dashboard.

## Principais recursos

- Autenticacao com JWT e refresh token
- Cadastro de produtos com variacoes de tamanho e estoque
- Filtros de produtos e pedidos com paginacao
- Dashboard mensal de vendas
- Email de notificacao para pedido criado (fila interna)
- Upload de imagens via Cloudinary

## Arquitetura

| Camada | Pasta | Responsabilidade |
| --- | --- | --- |
| API | Api/ | Controllers, roteamento, Swagger e pipeline HTTP |
| Aplicacao | Application/ | Casos de uso, DTOs, services |
| Dominio | Domain/ | Entidades, enums, interfaces de repositorio |
| Infra | Infrastructure/ | EF Core, repositorios, servicos externos |
| Tests | Tests/ | Testes unitarios e de servico |

## Tecnologias

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + Npgsql (PostgreSQL)
- JWT para autenticacao
- Cloudinary para upload de imagem
- SMTP (Mailtrap no dev) para notificacoes
- Docker para deploy

## Requisitos

- .NET SDK 10
- PostgreSQL
- Docker (opcional)

## Configuracao

Defina as variaveis de ambiente abaixo (ou use secrets localmente). O projeto le `appsettings.json` e sobrescreve com variaveis `__`.

| Variavel | Descricao |
| --- | --- |
| ConnectionStrings__DefaultConnection | String de conexao PostgreSQL |
| Jwt__Secret | Segredo do JWT |
| Cloudinary__CloudName | Cloudinary cloud name |
| Cloudinary__ApiKey | Cloudinary api key |
| Cloudinary__ApiSecret | Cloudinary api secret |
| EmailNotification__Enabled | true/false para envio de email |
| EmailNotification__Host | Host SMTP |
| EmailNotification__Port | Porta SMTP |
| EmailNotification__UseSsl | true/false |
| EmailNotification__Username | Usuario SMTP |
| EmailNotification__Password | Senha SMTP |
| EmailNotification__From | Email de origem |
| EmailNotification__To | Email destino padrao |

Observacao: o projeto aplica migrations automaticamente no startup.

## Como rodar

### Local

```bash
dotnet restore

dotnet run --project Api/Api.csproj
```

Swagger (ambiente Development):

- https://localhost:5001/swagger
- http://localhost:5000/swagger

### Docker

```bash
docker build -t mundopreguica-api .

docker run -p 8080:8080 -e PORT=8080 mundopreguica-api
```

## Endpoints

Base URL (padrao ASP.NET Core):

- https://localhost:5001
- http://localhost:5000

### Auth

| Metodo | Rota | Descricao |
| --- | --- | --- |
| POST | /api/auth | Login (retorna access e refresh token) |
| POST | /api/auth/refresh-token | Renova o access token |

### User

| Metodo | Rota | Descricao |
| --- | --- | --- |
| POST | /api/user | Cria usuario |

### Category

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | /api/category | Lista categorias |
| POST | /api/category | Cria categoria (body string) |
| DELETE | /api/category/{id} | Remove categoria |

### Product

| Metodo | Rota | Descricao |
| --- | --- | --- |
| POST | /api/product | Cria produto (multipart/form-data) |
| GET | /api/product | Lista produtos com filtros e paginacao |
| GET | /api/product/{productId} | Detalhe do produto |
| PUT | /api/product/{productId} | Atualiza produto (multipart/form-data) |
| PATCH | /api/product/{productId}/status | Atualiza status do produto |

Filtros de produto (query):

- name
- categoryId
- status (enum)
- size (enum)
- gender
- page
- pageSize

### Order

| Metodo | Rota | Descricao |
| --- | --- | --- |
| POST | /api/order | Cria pedido |
| GET | /api/order | Lista pedidos com filtros e paginacao |
| GET | /api/order/{orderId} | Detalhe do pedido |
| PATCH | /api/order/{orderId}/status | Alterna status (Pending/Delivered) |
| PUT | /api/order/{orderId}/return | Liquida consignado |
| PATCH | /api/order/{orderId}/cancel | Cancela pedido |

Filtros de pedido (query):

- phone
- status (enum)
- orderType (enum)
- startDate
- endDate
- page
- pageSize

### Dashboard

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | /api/dashboard | Retorna dados mensais do dashboard |

### Metadata

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | /api/metadata/sizes | Lista tamanhos com display name |
| GET | /api/metadata/status | Lista status de produto |
| GET | /api/metadata/order-status | Lista status de pedido |

## Exemplos rapidos

### Login

```bash
curl -X POST http://localhost:5000/api/auth \
  -H "Content-Type: application/json" \
  -d '{"mail":"user@email.com","password":"123456"}'
```

### Criar produto (multipart)

```bash
curl -X POST http://localhost:5000/api/product \
  -F "name=Camiseta" \
  -F "categoryId=1" \
  -F "price=59.90" \
  -F "gender=1" \
  -F "variant[0].size=1" \
  -F "variant[0].stock=10" \
  -F "image=@./camiseta.jpg"
```

### Listar pedidos

```bash
curl "http://localhost:5000/api/order?page=1&pageSize=10"
```

# Oficina Mecanica API

MVP em C# para o Tech Challenge da oficina mecanica. A solucao usa um monolito em camadas com foco em ordens de servico, clientes, veiculos, catalogo de servicos, pecas, estoque, autenticacao e metricas operacionais.

## Stack

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core com PostgreSQL
- ASP.NET Identity para usuarios e roles
- JWT Bearer para endpoints protegidos
- Swagger/OpenAPI com descricoes e tipos de response
- xUnit para testes unitarios e de integracao
- Docker Compose para API, PostgreSQL e SonarQube
- SonarQube Community para relatorios de qualidade, vulnerabilidades, security hotspots e code smells

## Features

- Login com JWT em `POST /api/auth/login`
- Cadastro de usuarios em `POST /api/auth/register`
- Seed inicial com usuario admin, roles, cliente, veiculo, servicos e peca
- CRUD de clientes
- CRUD de veiculos
- CRUD do catalogo de servicos
- CRUD de pecas com controle de estoque
- Tabelas e colunas no padrao `snake_case` com prefixo `tb_`
- Tabelas do Identity sem prefixo `asp_net`
- Criacao de ordem de servico com status inicial `Received`
- Solicitacoes do cliente salvas como lista de strings em `customer_request`
- Inicio de diagnostico com `diagnosticStartedAtUtc`
- Inclusao posterior de servicos e pecas no orcamento
- Reserva de estoque ao montar o orcamento
- Envio para aprovacao com status `WaitingApproval`
- Endpoint de aprovacao/reprovacao do orcamento
- Reprovacao com status `Reproved` e devolucao de pecas ao estoque
- Cancelamento permitido apenas em `Received`, `InDiagnosis` e `WaitingApproval`
- Status salvo no banco como string do enum
- Alteracao de status por numero do enum para `Finished` e `Delivered`
- Consulta publica da ordem por numero e documento do cliente
- Metricas gerais de tempo de execucao
- Metricas por ordem com tempo total e tempo entre etapas
- Swagger documentado com summaries, descriptions e response types
- Suite de testes com cobertura de dominio, aplicacao, controllers, auth, seed e integracao

## Credenciais iniciais

- Usuario: `admin`
- Senha: `Admin@123`

As credenciais ficam persistidas no banco via Identity, com senha armazenada em hash.

## Principais endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Clientes, veiculos, servicos e pecas

- `GET|POST|PUT|DELETE /api/customers`
- `GET|POST|PUT|DELETE /api/vehicles`
- `GET|POST|PUT|DELETE /api/services`
- `GET|POST|PUT|DELETE /api/parts`

### Ordens de servico

- `GET /api/service-orders`
- `GET /api/service-orders/{id}`
- `POST /api/service-orders`
- `POST /api/service-orders/{id}/diagnosis/start`
- `PATCH /api/service-orders/{id}/budget`
- `POST /api/service-orders/{id}/approval`
- `POST /api/service-orders/{id}/cancel`
- `POST /api/service-orders/{id}/status`
- `GET /api/service-orders/public/{number}?document=...`
- `GET /api/service-orders/metrics/execution-time`
- `GET /api/service-orders/{id}/metrics/execution-time`

## Criacao de ordem de servico

```json
{
  "customerDocument": "12345678901",
  "vehicle": {
    "plate": "BRA2E19",
    "brand": "Volkswagen",
    "model": "Gol",
    "year": 2020
  },
  "customerRequest": [
    "Customer reports engine noise.",
    "Customer reports oil leak."
  ]
}
```

Depois, o orcamento pode ser adicionado com:

```json
{
  "serviceIds": [
    "2677042f-aba1-405f-a465-68665a683169"
  ],
  "parts": [
    {
      "partId": "c9aeadbf-cb28-49c5-a47b-63328764c36c",
      "quantity": 1
    }
  ]
}
```

## Status da ordem

```csharp
Received = 1
InDiagnosis = 2
WaitingApproval = 3
InExecution = 4
Finished = 5
Delivered = 6
Reproved = 7
Canceled = 8
```

O endpoint `POST /api/service-orders/{id}/status` recebe o numero do enum e permite somente `5` (`Finished`) e `6` (`Delivered`):

```json
{
  "status": 5
}
```

## Swagger

Com a API rodando:

- Swagger UI: `http://localhost:8080/swagger`
- OpenAPI JSON: `http://localhost:8080/openapi/v1.json`

A documentacao inclui:

- Summary de cada endpoint em ingles
- Description de cada endpoint em ingles
- Tipos de response
- Status codes esperados como `200`, `201`, `204`, `400`, `401`, `404` e `409`
- Configuracao Bearer para colar o token JWT

## Como rodar localmente

```bash
dotnet restore
dotnet build
dotnet run --project OficinaMecanica.Api
```

Por padrao, a API espera PostgreSQL em `localhost:5432`.

Connection string local padrao:

```text
Host=localhost;Port=5432;Database=oficina_mecanica;Username=postgres;Password=postgres
```

## Como rodar com Docker

Subir todos os servicos:

```bash
docker compose up --build -d
```

Servicos:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- SonarQube: `http://localhost:9000`

Parar os containers:

```bash
docker compose down
```

Parar e remover os dados do banco e do SonarQube:

```bash
docker compose down -v
```

## SonarQube

O `docker-compose.yml` inclui o SonarQube Community para gerar relatorios de qualidade, vulnerabilidades, security hotspots, bugs e code smells.

Subir somente PostgreSQL e SonarQube:

```bash
docker compose up -d postgres sonarqube
```

Acessar:

```text
http://localhost:9000
```

Credenciais iniciais padrao do SonarQube:

- Usuario: `admin`
- Senha: `admin`

No primeiro login, o SonarQube solicita a troca da senha. Depois crie um token em:

```text
My Account > Security > Generate Tokens
```

Instalar o scanner .NET:

```bash
dotnet tool install --global dotnet-sonarscanner
```

Executar analise:

```bash
dotnet sonarscanner begin /k:"oficina-mecanica" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="<SEU_TOKEN>"
dotnet build
dotnet test
dotnet sonarscanner end /d:sonar.token="<SEU_TOKEN>"
```

Depois da execucao, o relatorio fica disponivel no painel do SonarQube em:

```text
http://localhost:9000/dashboard?id=oficina-mecanica
```

Observacao: em alguns ambientes Linux, o SonarQube pode exigir ajuste de `vm.max_map_count`. Para uso local via Docker Compose, foi configurado `SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true`.

## Como testar

```bash
dotnet test
```

Resultado esperado atual:

```text
Aprovado! - Com falha: 0, Aprovado: 37, Total: 37
```

## Fluxo sugerido para demonstracao

1. Subir a stack com `docker compose up --build -d`.
2. Abrir o Swagger em `http://localhost:8080/swagger`.
3. Fazer login em `POST /api/auth/login` com o admin inicial.
4. Clicar em `Authorize` no Swagger e informar `Bearer <token>`.
5. Consultar clientes, veiculos, servicos e pecas semeados.
6. Criar uma ordem de servico com `POST /api/service-orders`.
7. Iniciar diagnostico com `POST /api/service-orders/{id}/diagnosis/start`.
8. Adicionar orcamento com `PATCH /api/service-orders/{id}/budget`.
9. Aprovar ou reprovar com `POST /api/service-orders/{id}/approval`.
10. Finalizar e entregar com `POST /api/service-orders/{id}/status`.
11. Consultar metricas gerais ou por id.
12. Consultar a OS pelo endpoint publico.
13. Rodar a analise no SonarQube para verificar vulnerabilidades e qualidade.

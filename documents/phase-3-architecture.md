# Arquitetura — Fase 3

## Visão de componentes

```mermaid
flowchart LR
  Customer[Cliente] --> AuthGw[API Gateway Auth]
  AuthGw --> Auth[Lambda CPF Auth]
  Auth --> RDS[(RDS PostgreSQL)]
  Auth -->|JWT Customer| Customer

  Employee[Funcionário] --> Gateway[NGINX API Gateway]
  Customer -->|JWT| Gateway
  Gateway --> API[.NET API no EKS]
  API --> RDS
  API --> Outbox[(Outbox)]
  Outbox --> SQS[SQS + DLQ]
  SQS --> Notify[Lambda Notifications]
  Notify --> SES[Amazon SES]
```

## Sequência de autenticação do cliente

```mermaid
sequenceDiagram
  actor C as Cliente
  participant G as API Gateway
  participant L as Lambda Auth
  participant D as PostgreSQL
  participant A as API EKS
  C->>G: POST /auth/customer {cpf}
  G->>L: evento
  L->>D: cliente por CPF e is_active
  D-->>L: cliente ativo
  L-->>C: JWT (sub, cpf, role=Customer)
  C->>A: Bearer JWT
  A->>A: assinatura, issuer, audience, expiração e propriedade
  A-->>C: recurso do próprio cliente
```

## Sequência da ordem de serviço

```mermaid
sequenceDiagram
  actor F as Funcionário
  actor C as Cliente
  participant A as API
  participant D as PostgreSQL
  participant Q as SQS
  participant N as Lambda Notifications
  F->>A: abre OS / inicia diagnóstico
  A->>D: persiste OS
  F->>A: envia orçamento
  A->>D: status + evento no outbox (mesma transação)
  A->>Q: dispatcher publica evento
  Q->>N: entrega assíncrona
  N-->>C: e-mail do orçamento
  C->>A: aprova/rejeita com JWT
  A->>A: confere CPF do token contra a OS
  A->>D: persiste decisão + outbox
  F->>A: executa, finaliza e entrega
```

## Repositórios locais

| Responsabilidade | Pasta |
|---|---|
| Aplicação e manifests Kubernetes | `pos-tech-fiap-software-architecture` |
| Funções Auth e Notifications | `pos-tech-fiap-software-architecture-serverless` |
| VPC, EKS, add-ons e ECR | `pos-tech-fiap-software-architecture-kubernetes-infra` |
| RDS, segredo e migrations | `pos-tech-fiap-software-architecture-database-infra` |

As URLs Git serão adicionadas depois da criação dos quatro repositórios remotos.

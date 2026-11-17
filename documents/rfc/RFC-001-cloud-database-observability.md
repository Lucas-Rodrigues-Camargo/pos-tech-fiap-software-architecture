# RFC-001 — AWS, PostgreSQL e observabilidade

## Proposta

Adotar AWS com EKS, RDS PostgreSQL, Lambda, SQS, SES, ECR e NGINX Ingress. PostgreSQL preserva transações e integridade referencial do domínio. Logs são JSON e propagam `X-Correlation-ID`; métricas de CPU/memória vêm do metrics-server/HPA. Dashboards externos devem acompanhar volume diário de OS, duração por etapa e erros de integração.

## Alternativas consideradas

- Banco NoSQL: descartado porque orçamento, estoque e transições dependem de consistência e relacionamentos.
- E-mail síncrono: descartado porque aumenta latência e acopla disponibilidade do SES à API.
- Banco dentro do cluster: descartado por exigir operação de backup, failover e armazenamento que o RDS já fornece.

## Pendências de operação

Configurar backend Terraform remoto, roles OIDC, domínio/TLS, integração do agente Datadog ou New Relic e os três dashboards antes da demonstração.

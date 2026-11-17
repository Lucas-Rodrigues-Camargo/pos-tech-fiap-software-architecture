# Guia da entrega — Fase 4

Comece por [rastreabilidade dos requisitos](documents/phase-4-requirements.md), [arquitetura](documents/phase-4-implementation.md) e [BDD local](phase4-e2e/README.md).

Os serviços estão em três pastas/repositórios independentes dentro deste workspace:
- pos-tech-fiap-software-architecture-os-service
- pos-tech-fiap-software-architecture-billing-service
- pos-tech-fiap-software-architecture-execution-service

O contrato para usar infraestrutura AWS existente fica em docs/aws-existing-infra.md de cada serviço. O caminho de deploy da aplicação não executa Terraform apply. O provisionamento de referência é opcional e manual.

## O que demonstrar

1. Mostrar os três repositórios e suas responsabilidades/bancos.
2. Rodar testes e consultar docs/verification.json e artefatos do CI.
3. Mostrar Gherkin e executar BDD: sucesso, rejeição e cancelamento após pagamento.
4. Explicar inbox/outbox, fingerprint de idempotência, versão otimista, timeout e DLQ.
5. Mostrar compensação: só confirmar cancelamento após cancelar o trabalho e confirmar estorno.
6. Demonstrar readiness, JWT/propriedade da OS e validação de assinatura do webhook.
7. No ambiente preparado pelo grupo, executar deploy Kubernetes e demonstrar Datadog e Mercado Pago sandbox.

Os itens 1–6 podem ser reproduzidos localmente. O item 7 requer o ambiente e as contas externas. O vídeo e o PDF final devem indicar corretamente essa diferença.

## Roteiro sugerido para vídeo de até 15 minutos

- 0–2 min: arquitetura, limites e dados.
- 2–5 min: fluxo completo.
- 5–8 min: falha/rejeição e compensação financeira.
- 8–11 min: testes, BDD, cobertura e pipeline.
- 11–14 min: deploy e observabilidade reais no ambiente do grupo.
- 14–15 min: links e decisões.

Antes de enviar ao portal, preencher participantes e identificação, links dos três repositórios, vídeo e diagramas. O PDF técnico em documents/fase-4-arquitetura.pdf é uma base; não contém evidências externas que ainda não foram produzidas.

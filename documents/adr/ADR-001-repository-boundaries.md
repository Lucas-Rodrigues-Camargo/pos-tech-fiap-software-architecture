# ADR-001 — Limites dos quatro repositórios

- Status: aceito
- Contexto: a Fase 3 exige quatro repositórios com pipelines e deploys independentes.
- Decisão: manter aplicação/manifests Kubernetes, serverless, cluster e banco em quatro pastas/repositórios separados. O cluster fornece a rede usada pelo banco; a aplicação consome outputs de ambos.
- Consequências: cada unidade tem ciclo de vida e permissões próprios. A ordem de provisionamento é cluster, banco, serverless e aplicação. Essa ordem difere da sugestão não normativa da especificação porque a VPC pertence, por requisito, ao repositório do cluster.

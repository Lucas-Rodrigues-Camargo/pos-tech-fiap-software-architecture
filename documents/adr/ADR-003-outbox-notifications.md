# ADR-003 — Outbox e notificações assíncronas

- Status: aceito
- Contexto: mudanças da OS precisam disparar notificações sem tornar a transação dependente de e-mail ou SQS.
- Decisão: gravar eventos de mudança de status em `tb_outbox_messages` na mesma transação da OS. Um dispatcher publica no SQS; a Lambda usa resposta parcial de lote e DLQ e envia pelo SES.
- Consequências: há entrega pelo menos uma vez; consumidores precisam ser idempotentes. Falhas ficam registradas com tentativas e mensagem de erro.

from pathlib import Path
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "documents" / "fase-4-arquitetura.pdf"
styles = getSampleStyleSheet()
story = []
def para(text, style="BodyText"):
    story.append(Paragraph(escape(text), styles[style]))
    story.append(Spacer(1, .25*cm))
para("Oficina Mecânica — Fase 4", "Title")
para("Arquitetura implementada e evidências locais", "Heading2")
para("Documento técnico — 17/11/2026. A entrega acadêmica final precisa da identificação dos participantes, URL do vídeo e evidências reais de cloud e Mercado Pago sandbox.")
for slug, purpose, db in [
    ("os", "Clientes, OS e orquestração Saga", "PostgreSQL próprio"),
    ("billing", "Orçamento, pagamento e estorno", "PostgreSQL próprio"),
    ("execution", "Diagnóstico e execução", "DynamoDB próprio")]:
    para(slug.upper(), "Heading2")
    para(purpose + ". " + db + ".")
    para("https://github.com/Lucas-Rodrigues-Camargo/pos-tech-fiap-software-architecture-" + slug + "-service")
para("Componentes e comunicação", "Heading2")
rows = [["Entrada", "Processamento", "Persistência"],
    ["JWT / Gateway", "OS Service", "PostgreSQL OS"],
    ["JWT / Gateway", "Billing Service", "PostgreSQL Billing"],
    ["JWT Employee", "Execution Service", "DynamoDB"],
    ["Eventos SQS", "Consumidor + inbox/outbox", "Documento atômico / versão"],
    ["Webhook HMAC", "Consulta Mercado Pago", "Pagamento validado"],
    ["Traceparent / JSON", "OTLP / Datadog", "Traces, logs e métricas"]]
table = Table(rows, colWidths=[4*cm, 7*cm, 5*cm])
table.setStyle(TableStyle([("BACKGROUND",(0,0),(-1,0),colors.lightblue),("GRID",(0,0),(-1,-1),.5,colors.grey),
    ("VALIGN",(0,0),(-1,-1),"TOP"),("FONTSIZE",(0,0),(-1,-1),9)]))
story.append(table)
story.append(PageBreak())
para("Saga orquestrada", "Heading1")
for text in [
    "1. OS abre solicitação e envia CreateJob para Execution.",
    "2. Funcionário registra diagnóstico e estimativa; OS solicita orçamento ao Billing.",
    "3. Cliente aprova ou rejeita orçamento com JWT de proprietário.",
    "4. Billing cria preferência no Mercado Pago; webhook validado dispara consulta autoritativa do pagamento.",
    "5. Após pagamento confirmado, OS envia StartRepair e recebe RepairStarted.",
    "6. Funcionário finaliza; Execution publica RepairFinished; OS fica Finished."]:
    para(text)
para("Compensação e segurança", "Heading2")
para("CancelJob antecede CancelBudget. Trabalho físico iniciado exige ManualIntervention. Cobrança paga passa por RefundPending e RefundPayment; apenas estorno confirmado emite BudgetCanceled. A OS só fica Canceled após confirmação de ambas as compensações.")
para("Inbox deduplica eventId; outbox mantém mensagens até publicação; versão otimista evita sobrescrita concorrente. SQS usa entrega pelo menos uma vez e DLQ. Timeout pré-reparo inicia compensação. A implementação não promete exactly-once.")
para("JWT valida assinatura, issuer, audience, validade e CPF proprietário. A Lambda consulta cliente por API interna autenticada. Serviços não acessam bancos alheios.")
para("Decisões arquiteturais", "Heading2")
para("Três contextos independentes: atendimento/OS, financeiro e produção. Saga orquestrada torna a ordem de compensações explícita. PostgreSQL fornece durabilidade transacional; DynamoDB fornece estado documental independente. SQS desacopla serviços com inbox/outbox.")
story.append(PageBreak())
para("Verificação e operação", "Heading1")
for text in [
    "23 testes OS, 22 Billing e 19 Execution aprovados. Cobertura: 94,25%, 94,33% e 93,20%, respectivamente.",
    "Código gerado em obj é excluído; domínio, APIs, workflows e adaptadores são incluídos. Gate CI exige pelo menos 80%.",
    "BDD: dois cenários completos aprovados, com três APIs, PostgreSQL, SQS e DynamoDB em ambiente local. Apenas Mercado Pago usa simulador HTTP.",
    "Três imagens Docker construídas e três configurações Terraform validadas.",
    "CI independente: build, testes, cobertura, artefatos, Docker e Terraform. SonarQube exige secrets e servidor; deploy EKS exige configuração externa.",
    "Infra por serviço: SQS/DLQ, ECR, IAM Pod Identity e banco próprio. Backend S3 criptografado, com state separado e lockfile.",
    "Observabilidade: correlationId/orderId, traceparent no envelope, OpenTelemetry OTLP, métricas de transição, dashboard e alerta Datadog."]:
    para(text)
para("Pendências externas e limites", "Heading2")
para("Ainda dependem do grupo: provisionamento AWS, domínio/TLS, credenciais Mercado Pago sandbox, SonarQube e Datadog, proteção de main, participantes e vídeo. Esta evidência local não comprova deploy ativo ou pagamento real.")
para("Inbox/histórico embutidos precisam de arquivamento em alto volume; outbox Dynamo usa scan paginado. Cobranças extras exigem revisão operacional. Migração dos dados anteriores precisa de plano de exportação e reconciliação.")
para("Referências técnicas", "Heading2")
para("Mercado Pago: developers/pt/reference/online-payments/checkout-pro-preferences/create-preference/post e developers/pt/reference/online-payments/checkout-pro/create-refund/post.")
para("Datadog: https://docs.datadoghq.com/opentelemetry/setup/otlp_ingest_in_the_agent/")
SimpleDocTemplate(str(OUTPUT), leftMargin=1.5*cm, rightMargin=1.5*cm).build(story)
print(OUTPUT)

from pathlib import Path
import json
from reportlab.graphics.shapes import Drawing, Rect, String, Line, Polygon
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "documents" / "fase-4-arquitetura.pdf"
styles = getSampleStyleSheet()
evidence = {
    slug: json.loads((ROOT / f"pos-tech-fiap-software-architecture-{slug}-service/docs/verification.json").read_text(encoding="utf-8"))
    for slug in ("os", "billing", "execution")
}
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
diagram = Drawing(470, 230)
def box(x, y, width, title, detail, fill=colors.aliceblue):
    diagram.add(Rect(x, y, width, 42, fillColor=fill, strokeColor=colors.steelblue))
    diagram.add(String(x + width/2, y + 26, title, textAnchor="middle", fontName="Helvetica-Bold", fontSize=10))
    diagram.add(String(x + width/2, y + 11, detail, textAnchor="middle", fontSize=8))
def arrow(x1, y1, x2, y2):
    diagram.add(Line(x1, y1, x2, y2, strokeColor=colors.steelblue))
    diagram.add(Polygon([x2, y2, x2-3, y2+6, x2+3, y2+6], fillColor=colors.steelblue, strokeColor=colors.steelblue))
box(155, 180, 160, "SQS + DLQ", "comandos e eventos v1")
for x, title, database in ((0, "OS / Saga", "PostgreSQL OS"), (165, "Billing", "PostgreSQL Billing"), (330, "Execution", "DynamoDB")):
    box(x, 100, 140, title, "API JWT + inbox/outbox")
    arrow(235, 180, x+70, 142)
    box(x, 20, 140, database, "acesso exclusivo do serviço", colors.honeydew)
    arrow(x+70, 100, x+70, 62)
story.append(diagram)
para("Gateway/autenticação e Datadog são integrações transversais. O ambiente AWS é pré-existente; o deploy da aplicação não executa Terraform apply.")
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
    "5. Após pagamento confirmado, OS envia AuthorizeRepair. Execution entra em ReadyForRepair; o funcionário inicia /start e publica RepairStarted.",
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
    "Testes e cobertura de linhas: " + "; ".join(f"{slug}: {item['tests']['passed']} aprovados, {item['coverage']['lines']:.2f}%" for slug, item in evidence.items()) + ". Números gerados de TRX/Cobertura.",
    "Código gerado em obj é excluído; domínio, APIs, workflows e adaptadores são incluídos. Gate CI exige pelo menos 80%.",
    "BDD: três cenários completos aprovados (sucesso, rejeição e cancelamento pago com estorno), com três APIs, PostgreSQL, SQS e DynamoDB em ambiente local. Apenas Mercado Pago usa simulador HTTP.",
    "Três imagens Docker construídas e três configurações Terraform validadas.",
    "CI independente: build, testes, cobertura, artefatos, Docker e Terraform. SonarQube exige secrets e servidor; deploy EKS exige configuração externa.",
    "Deploy sobre infraestrutura existente: SQS/DLQ, ECR, IAM Pod Identity e banco próprio. Terraform é uma referência opcional, sem apply no pipeline da aplicação.",
    "Observabilidade: correlationId/orderId, traceparent no envelope, OpenTelemetry OTLP, métricas de transição, dashboard e alerta Datadog."]:
    para(text)
para("Pendências externas e limites", "Heading2")
para("Ainda dependem do grupo: configuração da infraestrutura AWS existente, domínio/TLS, credenciais Mercado Pago sandbox, SonarQube e Datadog, proteção de main, participantes e vídeo. Esta evidência local não comprova deploy ativo ou pagamento real.")
para("Inbox/histórico embutidos precisam de arquivamento em alto volume; outbox Dynamo usa scan paginado. Cobranças extras exigem revisão operacional. Migração dos dados anteriores precisa de plano de exportação e reconciliação.")
para("Referências técnicas", "Heading2")
para("Mercado Pago: developers/pt/reference/online-payments/checkout-pro-preferences/create-preference/post e developers/pt/reference/online-payments/checkout-pro/create-refund/post.")
para("Datadog: https://docs.datadoghq.com/opentelemetry/setup/otlp_ingest_in_the_agent/")
SimpleDocTemplate(str(OUTPUT), leftMargin=1.5*cm, rightMargin=1.5*cm).build(story)
print(OUTPUT)

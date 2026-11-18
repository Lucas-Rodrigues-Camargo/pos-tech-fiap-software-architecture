"""Generate the single PDF attachment required by the Phase 4 submission."""
import argparse
import json
from pathlib import Path
import shutil
from urllib.parse import urlparse
from html import escape

from reportlab.graphics.shapes import Drawing, Rect, String, Line, Polygon
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, KeepTogether

ROOT = Path(__file__).resolve().parents[2]
BASE = "https://github.com/Lucas-Rodrigues-Camargo/"
NAVY = colors.HexColor("#18344D")
BLUE = colors.HexColor("#087EAA")
GRAY = colors.HexColor("#526171")
LIGHT = colors.HexColor("#EAF3F8")


def valid_video(url):
    host = (urlparse(url).hostname or "").lower()
    return urlparse(url).scheme == "https" and host in {
        "youtube.com", "www.youtube.com", "m.youtube.com", "youtu.be",
        "vimeo.com", "www.vimeo.com", "player.vimeo.com"
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--video-url", default="")
    parser.add_argument("--final", action="store_true", help="Require a real video URL before exporting")
    parser.add_argument("--copy-to", type=Path)
    args = parser.parse_args()
    config = json.loads((ROOT / "documents/submission-phase4.json").read_text(encoding="utf-8"))
    video = args.video_url or config.get("video_url", "")
    if video and not valid_video(video):
        raise SystemExit("Use an HTTPS YouTube or Vimeo video URL")
    if args.final and not video:
        raise SystemExit("The submission requires the demonstration video URL")
    styles = getSampleStyleSheet()
    styles["Title"].textColor = NAVY
    styles["Heading1"].textColor = NAVY
    styles["Heading2"].textColor = BLUE
    styles["BodyText"].fontSize = 10
    styles["BodyText"].leading = 14
    styles.add(ParagraphStyle("SmallLink", parent=styles["BodyText"], fontSize=8.5, leading=12, textColor=BLUE))
    story = []

    def text(value, style="BodyText"):
        story.append(Paragraph(escape(value), styles[style]))
        story.append(Spacer(1, .22 * cm))

    def link(label, url):
        story.append(Paragraph(f'<b>{escape(label)}</b><br/><link href="{escape(url, quote=True)}" color="#087EAA">{escape(url)}</link>', styles["SmallLink"]))
        story.append(Spacer(1, .24 * cm))

    def section(title):
        text(title, "Heading2")

    def footer(canvas, doc):
        canvas.setStrokeColor(colors.HexColor("#DAE3EA"))
        canvas.line(1.6 * cm, 1.35 * cm, 19.4 * cm, 1.35 * cm)
        canvas.setFont("Helvetica", 8)
        canvas.setFillColor(GRAY)
        canvas.drawString(1.6 * cm, .95 * cm, "FIAP | Tech Challenge | Fase 4 | Oficina Mecanica")
        canvas.drawRightString(19.4 * cm, .95 * cm, f"{doc.page}")

    text("FIAP — Pós Tech", "Title")
    text("Tech Challenge — Fase 4", "Title")
    text("Sistema de gestão de oficina mecânica", "Heading2")
    text("Microsserviços, Saga e automação de entrega")
    section("Participantes")
    for member in config["participants"]:
        text(f"{member['name']} — {member['registration']}")
    section("Repositórios dos microsserviços")
    for slug, label in (("os", "Ordens de serviço e orquestração"), ("billing", "Orçamento e pagamento"), ("execution", "Diagnóstico e execução")):
        link(label, BASE + f"pos-tech-fiap-software-architecture-{slug}-service")
    section("Vídeo de demonstração — até 15 minutos")
    if video:
        link("Vídeo", video)
    else:
        box = Table([[Paragraph("<b>PENDENTE: informar o link do vídeo de demonstração.</b><br/>Este campo é obrigatório no enunciado. Incluir o link antes do envio definitivo ao portal.", styles["BodyText"])]], colWidths=[17 * cm])
        box.setStyle(TableStyle([("BACKGROUND", (0, 0), (-1, -1), colors.HexColor("#FFF2CE")), ("BOX", (0, 0), (-1, -1), .6, colors.HexColor("#B58B00")), ("TOPPADDING", (0, 0), (-1, -1), 10), ("BOTTOMPADDING", (0, 0), (-1, -1), 10)]))
        story.append(box)
    text("Versão dos códigos: branch main. Documento de entrega organizado conforme os itens do PDF do desafio.")
    story.append(PageBreak())

    text("1. Arquitetura final", "Heading1")
    text("A aplicação foi dividida em três microsserviços especializados, com repositórios, pipelines e persistências independentes. O ambiente de execução previsto é Kubernetes na AWS, aproveitando infraestrutura previamente disponível.")
    drawing = Drawing(470, 280)

    def box(x, y, title, detail, width=140, fill=LIGHT):
        drawing.add(Rect(x, y, width, 40, fillColor=fill, strokeColor=BLUE))
        drawing.add(String(x + width / 2, y + 24, title, textAnchor="middle", fontName="Helvetica-Bold", fontSize=10))
        drawing.add(String(x + width / 2, y + 10, detail, textAnchor="middle", fontSize=8))

    def line(x1, y1, x2, y2):
        drawing.add(Line(x1, y1, x2, y2, strokeColor=GRAY, strokeWidth=1))

    box(155, 235, "Gateway / JWT", "clientes e funcionarios", width=160)
    for x, title, detail, database in (
        (0, "OS Service", "OS + orquestrador Saga", "PostgreSQL OS"),
        (165, "Billing Service", "orcamento + pagamento", "PostgreSQL Billing"),
        (330, "Execution Service", "diagnostico + reparos", "DynamoDB Execution")
    ):
        box(x, 160, title, detail)
        line(235, 235, x + 70, 200)
        box(x, 90, database, "acesso exclusivo do servico", fill=colors.HexColor("#EDF7ED"))
        line(x + 70, 160, x + 70, 130)
    # Route messaging outside the database boxes, directly to each service.
    for center, side in ((70, 148), (235, 313), (400, 478)):
        line(center + 70, 180, side, 180)
        line(side, 180, side, 30)
        line(side, 30, 315, 30)
    box(155, 10, "SQS + DLQ", "comandos/eventos + inbox/outbox", width=160)
    story.append(drawing)
    text("Cada serviço publica e consome comandos/eventos via SQS, usando sua própria inbox/outbox. As conexões de persistência são exclusivas de cada serviço.", "BodyText")
    section("Integrações transversais")
    text("A Lambda autentica clientes por CPF consultando a API do OS Service. Funcionários utilizam o emissor interno de JWT já existente. Billing integra-se ao Mercado Pago. Os três serviços produzem logs JSON, propagam correlationId/causationId/traceparent e exportam traces OpenTelemetry ao Datadog.")
    section("Isolamento dos dados")
    text("OS e Billing usam bancos PostgreSQL exclusivos e credenciais distintas; Execution usa DynamoDB próprio. Nenhum microsserviço consulta diretamente o banco de outro. Referências e snapshots são transmitidos por contratos versionados ou APIs.")
    story.append(PageBreak())

    text("2. Estratégia Saga", "Heading1")
    section("Escolha: orquestração no OS Service")
    text("A orquestração concentra as decisões e o histórico transacional no serviço de OS, tornando explícita a ordem das etapas e das compensações. Os participantes permanecem independentes e se comunicam por SQS.")
    for item in (
        "1. O cliente abre uma OS autenticada. O OS Service persiste a Saga e solicita CreateJob.",
        "2. Execution recebe o trabalho; o funcionário registra diagnóstico e estimativa. DiagnosisCompleted faz o OS solicitar o orçamento ao Billing.",
        "3. Billing publica BudgetGenerated e o cliente aprova ou rejeita o orçamento usando JWT de proprietário.",
        "4. Na aprovação, Billing cria o checkout Mercado Pago. O webhook tem assinatura validada e o pagamento é consultado no provedor, conferindo referência da OS, valor e moeda BRL.",
        "5. PaymentConfirmed libera a OS; AuthorizeRepair coloca o trabalho em ReadyForRepair. O funcionário inicia o reparo explicitamente.",
        "6. Após a conclusão técnica, Execution publica RepairFinished e o OS finaliza a ordem."
    ):
        text(item)
    section("Rollback e compensação")
    text("CancelJob é solicitado antes de CancelBudget. Somente após a confirmação de cancelamento da execução começa a compensação financeira. Se o pagamento já foi confirmado, Billing persiste RefundPending e executa RefundPayment com chave idempotente. A OS só chega a Canceled após a confirmação de ambas as compensações.")
    text("Se o trabalho físico já começou, a compensação automática não simula desfazer o reparo: a Saga sinaliza ManualIntervention. Falhas no estorno mantêm a compensação pendente para retry. Timeouts pré-reparo iniciam compensação e falhas repetidas ficam na DLQ.")
    section("Consistência, idempotência e concorrência")
    text("Estado, histórico, inbox e outbox são gravados atomicamente no banco do serviço. A inbox deduplica o ID e compara o fingerprint do comando; reutilizar uma chave com dados diferentes é rejeitado. PostgreSQL usa atualização condicional por versão, e DynamoDB usa escrita condicional. A entrega é pelo menos uma vez; eventos tardios não reabrem trabalho cancelado.")
    story.append(PageBreak())

    text("3. Justificativas e verificação", "Heading1")
    section("Divisão dos microsserviços")
    text("OS concentra atendimento, clientes, histórico e coordenação. Billing concentra decisões financeiras e isolamento da integração de pagamento. Execution concentra diagnóstico, fila e trabalho físico. Essa divisão acompanha responsabilidades do negócio, reduz acoplamento e permite implantar e escalar os serviços separadamente.")
    section("Tecnologias")
    text(".NET 10 preserva o ecossistema da aplicação anterior. PostgreSQL oferece durabilidade transacional e controle de concorrência para OS e Billing. DynamoDB atende ao estado documental de execução e à persistência NoSQL exigida. SQS/DLQ desacoplam os participantes. Docker/Kubernetes padronizam execução; Terraform registra a infraestrutura como referência opcional.")
    section("Evidências locais de testes e cobertura")
    rows = [["Serviço", "Testes aprovados", "Cobertura de linhas"]]
    for slug, label in (("os", "OS"), ("billing", "Billing"), ("execution", "Execution")):
        source = ROOT / f"pos-tech-fiap-software-architecture-{slug}-service/docs/verification.json"
        evidence = json.loads(source.read_text(encoding="utf-8"))
        rows.append([label, str(evidence["tests"]["passed"]), f'{evidence["coverage"]["lines"]:.2f}%'])
    table = Table(rows, colWidths=[5 * cm, 5 * cm, 7 * cm])
    table.setStyle(TableStyle([("BACKGROUND", (0, 0), (-1, 0), LIGHT), ("GRID", (0, 0), (-1, -1), .4, colors.HexColor("#B5C6D2")), ("FONTSIZE", (0, 0), (-1, -1), 10), ("TOPPADDING", (0, 0), (-1, -1), 7), ("BOTTOMPADDING", (0, 0), (-1, -1), 7)]))
    story.append(table)
    story.append(Spacer(1, .3 * cm))
    text("Os números vêm dos relatórios TRX/Cobertura versionados como evidências por serviço. O CI exige 80% de cobertura de linhas e exclui apenas código gerado em obj. O BDD integrado validou sucesso, rejeição e cancelamento após pagamento com estorno. Os testes incluem PostgreSQL em container, SQS e DynamoDB no LocalStack.")
    section("CI/CD, segurança e observabilidade")
    text("Cada repositório possui build, testes, gate de cobertura, análise SonarQube configurável e deploy Kubernetes independente. Validação de configuração precede o acesso AWS. O deploy é habilitado somente após configuração do ambiente, e a marca [no deploy] permite publicar código sem ações cloud. JWT valida assinatura, issuer, audience, expiração e propriedade da OS.")
    text("Escopo comprovado: implementação e testes locais, builds Docker e configuração Terraform/Kubernetes. Mercado Pago é simulado nos testes; deploy real, sandbox externo e dashboards ativos dependem das contas e do ambiente do grupo. Esses resultados não são apresentados como evidência de produção.")
    story.append(PageBreak())

    text("4. Links de apoio à avaliação", "Heading1")
    root_repo = BASE + "pos-tech-fiap-software-architecture"
    for label, path in (
        ("Guia da Fase 4", "/blob/main/PHASE4.md"),
        ("Arquitetura e decisões", "/blob/main/documents/phase-4-implementation.md"),
        ("Matriz de requisitos e evidências", "/blob/main/documents/phase-4-requirements.md"),
        ("BDD e instruções de reprodução", "/blob/main/phase4-e2e/README.md"),
    ):
        link(label, root_repo + path)
    section("Documentação por microsserviço")
    for slug in ("os", "billing", "execution"):
        repo = BASE + f"pos-tech-fiap-software-architecture-{slug}-service"
        link(f"{slug.upper()} — README, contratos, configuração e testes", repo)
        link(f"{slug.upper()} — evidência de cobertura", repo + "/blob/main/docs/verification.json")
    section("Repositórios de apoio")
    for label, suffix in (
        ("Autenticação e notificações serverless", "serverless"),
        ("Referência de infraestrutura Kubernetes e observabilidade", "kubernetes-infra"),
        ("Referência de infraestrutura do banco", "database-infra")
    ):
        link(label, BASE + "pos-tech-fiap-software-architecture-" + suffix)
    text("Este documento reúne a identificação do grupo, os links de código/documentação, a arquitetura e a estratégia Saga para o envio em um único anexo PDF.")

    output = ROOT / "documents/Entrega-Tech-Challenge-Fase-4.pdf"
    SimpleDocTemplate(str(output), pagesize=(21 * cm, 29.7 * cm),
        leftMargin=1.6 * cm, rightMargin=1.6 * cm, topMargin=1.6 * cm, bottomMargin=1.7 * cm,
        title="Entrega Tech Challenge — Fase 4", author="; ".join(p["name"] for p in config["participants"])
    ).build(story, onFirstPage=footer, onLaterPages=footer)
    if args.copy_to:
        args.copy_to.mkdir(parents=True, exist_ok=True)
        shutil.copy2(output, args.copy_to / output.name)
    print(output)
    print("Video: configured" if video else "Video: PENDING — add before definitive submission")


if __name__ == "__main__":
    main()

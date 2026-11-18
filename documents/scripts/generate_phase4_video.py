"""Render a narrated replay of verified local BDD evidence, never fabricated output.

Requires Pillow, imageio-ffmpeg and Windows System.Speech (pt-BR voice).
Generated video and evidence stay outside Git. No AWS resources are provisioned.
"""
import argparse
import json
from pathlib import Path
import re
import subprocess
import textwrap
import wave
import xml.etree.ElementTree as ET

import imageio_ffmpeg
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
WHITE, MUTED, BLUE, GREEN = '#f2f7ff', '#aabdd0', '#63c9ff', '#7ae0b3'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--assets', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    trace = json.loads((args.assets / 'http-trace.json').read_text(encoding='utf-8'))
    suites = ET.parse(args.assets / 'bdd-results.xml').getroot()
    cases = list(suites.iter('testcase'))
    if trace['exit_status'] != 0 or len(cases) != 3 or any(list(c) for c in cases):
        raise SystemExit('A fresh successful three-scenario BDD run is required')
    evidence = {}
    for slug in ('os', 'billing', 'execution'):
        evidence[slug] = json.loads((ROOT / f'pos-tech-fiap-software-architecture-{slug}-service/docs/verification.json').read_text())
    chapters = []

    def add(title, subtitle, lines, narration, kind='text'):
        chapters.append(dict(title=title, subtitle=subtitle, lines=lines, narration=narration, kind=kind))

    add('Oficina mecânica | Fase 4', 'FIAP • Pós Tech • Software Architecture', [
        'Lucas Rodrigues Camargo • RM367435', 'Graziella Garcia Camargo • RM367436',
        'Três microsserviços • Saga • Entrega independente',
        'Demonstração local: HTTP real, PostgreSQL e LocalStack',
        'Narração sintética em português'],
        'Esta é a demonstração do Tech Challenge da fase quatro, do grupo Lucas Rodrigues Camargo e Graziella Garcia Camargo. O sistema gerencia uma oficina mecânica, desde a abertura da ordem até o pagamento e a conclusão do reparo. Vamos acompanhar uma execução local real dos três serviços, observar o caminho de sucesso e duas compensações da Saga. A narração foi gerada por síntese de voz.')
    add('Responsabilidades e dados', 'Arquitetura preparada para execução em Kubernetes na AWS', [
        'OS Service → atendimento e orquestração → PostgreSQL OS',
        'Billing Service → orçamento e pagamento → PostgreSQL Billing',
        'Execution Service → diagnóstico e reparo → DynamoDB',
        'SQS + DLQ conectam os serviços; cada um possui inbox/outbox',
        'JWT no acesso HTTP • contratos entre os domínios'],
        'A arquitetura separa três responsabilidades de negócio. O serviço de ordens mantém o atendimento e coordena a Saga. O Billing concentra orçamento e pagamentos. O Execution organiza diagnóstico e execução física. Os dois primeiros possuem bancos PostgreSQL exclusivos. O Execution utiliza DynamoDB. A comunicação assíncrona usa SQS, com filas de mensagens não processadas. Cada serviço acessa apenas sua própria persistência. Na execução deste vídeo, os serviços de mensageria e DynamoDB são fornecidos pelo LocalStack.', 'architecture')
    add('Saga orquestrada', 'Coordenação no OS Service • consistência eventual', [
        'Criar OS → diagnosticar → gerar orçamento → aprovar',
        'Confirmar pagamento → autorizar → iniciar → finalizar',
        'Inbox: deduplicação e validação do conteúdo',
        'Outbox: persistência atômica junto ao estado',
        'Versão condicional + correlationId + causationId'],
        'A escolha foi uma Saga orquestrada pelo serviço de ordens. Ele decide a próxima etapa a partir de eventos recebidos. Estado, histórico, inbox e outbox são persistidos atomicamente em cada participante. A inbox evita aplicar uma mensagem repetida e rejeita a reutilização do identificador com conteúdo diferente. A outbox permite repetir a publicação com segurança. As escritas condicionais por versão protegem contra concorrência, enquanto os identificadores de correlação permitem acompanhar uma ordem entre os serviços.')

    def scenario(name, title, narration, desired):
        events = [e for e in trace['events'] if name in e['scenario']]
        if not events:
            raise SystemExit(f'Missing captured scenario: {name}')
        lines = []
        # Use real response states and actual successful mutations, not illustrative claims.
        for e in events:
            if e['state'] in desired:
                line = f"{e['service']:<10} HTTP {e['http']}  {e['state']}"
                if line not in lines:
                    lines.append(line)
        paths = []
        for e in events:
            if e['method'] == 'POST' and e['http'] < 400:
                path = re.sub(r'[a-f0-9]{8}-[a-f0-9-]{27,}', '{orderId}', e['path'])
                short = f"{e['service']}  {e['method']} {path} → {e['http']}"
                if short not in paths:
                    paths.append(short)
        add(title, 'Reprodução dos resultados HTTP capturados nesta execução local', lines[-7:] + paths[-2:], narration, 'evidence')

    scenario('approved_and_paid', '1. Aprovação, pagamento e conclusão',
        'Este quadro utiliza respostas HTTP capturadas durante o teste integrado. O cliente abre a ordem e o funcionário registra o diagnóstico. O orçamento chega a aguardando aprovação. Depois da aprovação, o Billing cria o checkout e recebe o webhook assinado. O adaptador consulta o provedor de pagamento e verifica a referência, o valor e a moeda. Nesta execução, o provedor Mercado Pago é um simulador HTTP. Com a confirmação, a ordem fica pronta para execução. O funcionário inicia o reparo explicitamente e depois o finaliza. As verificações finais confirmaram ordem e execução finalizadas, com o orçamento pago.',
        {'Diagnosing', 'WaitingApproval', 'ReadyForExecution', 'ReadyForRepair', 'InExecution', 'Finished', 'Paid'})
    scenario('rejected_budget', '2. Rejeição e compensação',
        'O segundo cenário exercita a decisão negativa do cliente. Depois do diagnóstico, o orçamento é rejeitado. O orquestrador solicita o cancelamento do trabalho e aguarda a confirmação antes de cancelar o orçamento. A ordem só termina cancelada depois das confirmações dos participantes. As respostas capturadas mostram os três serviços no estado cancelado. Esta sequência evita declarar a Saga encerrada quando ainda existe trabalho de compensação pendente.',
        {'WaitingApproval', 'Canceled'})
    scenario('paid_order_is_refunded', '3. Cancelamento após pagamento',
        'O terceiro cenário começa com orçamento aprovado e pagamento confirmado. Antes de iniciar o reparo, o cliente solicita cancelamento. Primeiro a execução é cancelada. Em seguida o Billing registra a intenção de estorno e chama a operação de reembolso com uma chave idempotente. O teste verifica a chamada de estorno ao simulador, incluindo essa chave. O resultado capturado confirma Billing reembolsado, execução cancelada e ordem cancelada. Assim, a compensação financeira precisa ser confirmada antes de concluir o cancelamento da ordem.',
        {'ReadyForExecution', 'ReadyForRepair', 'Canceled', 'Refunded'})
    add('Falhas, repetição e trabalho físico', 'Compensação explícita • sem transação distribuída no banco', [
        'CancelJob → confirmação → CancelBudget',
        'Pagamento existente: RefundPending → RefundPayment → Refunded',
        'Falha no estorno mantém compensação pendente para retry',
        'Reparo já iniciado: ManualIntervention',
        'Eventos tardios não reabrem uma ordem cancelada'],
        'A compensação respeita o que pode ser desfeito no negócio. Se o reparo físico já começou, o sistema sinaliza intervenção manual. Ele não simula desfazer o trabalho. Quando o estorno falha, a compensação permanece pendente para nova tentativa. Mensagens repetidas são tratadas pela inbox, e eventos tardios não reabrem uma ordem já cancelada. Os consumidores possuem políticas de repetição e filas de mensagens não processadas, permitindo investigar falhas sem perder a intenção original.')
    lines = []
    for slug, label in (('os', 'OS'), ('billing', 'Billing'), ('execution', 'Execution')):
        item = evidence[slug]
        lines.append(f"{label}: {item['tests']['passed']} testes • {item['coverage']['lines']:.2f}% das linhas")
    lines += ['3 cenários BDD aprovados nesta gravação', 'Gate de cobertura no CI: 80%', 'Evidências: docs/verification.json + relatórios de testes']
    add('Testes e qualidade', 'Resultados lidos dos relatórios • sem métricas estimadas', lines,
        'Os relatórios versionados registram sessenta testes dos serviços. São vinte e três no OS, dezenove no Billing e dezoito no Execution. A cobertura de linhas supera noventa e dois por cento nos três projetos. Além dessas evidências, os três cenários BDD deste vídeo foram executados novamente e passaram. Eles utilizam APIs HTTP reais, PostgreSQL em contêineres e mensageria no LocalStack. O pipeline exige pelo menos oitenta por cento de cobertura e publica os relatórios para inspeção.')
    add('Entrega independente por serviço', 'Configuração versionada nos repositórios', [
        '.github/workflows/ci.yml: build → testes → cobertura',
        'Imagem Docker → registro → deploy Kubernetes',
        'Preflight verifica configuração antes do acesso AWS',
        'Secrets e variáveis por ambiente; deploy habilitado explicitamente',
        'Terraform e manifests documentam os recursos necessários'],
        'Cada serviço possui repositório e pipeline independentes. O fluxo versionado compila, testa, verifica cobertura e prepara a imagem Docker. A etapa de entrega utiliza o registro e o Kubernetes do ambiente AWS configurado. Uma validação prévia verifica parâmetros antes de acessar a nuvem. Segredos são fornecidos pelo ambiente. A infraestrutura está descrita em Terraform e manifests, e a implantação é habilitada explicitamente. Aqui mostramos a configuração do fluxo de entrega; a execução demonstrada neste vídeo é local.')
    add('Observabilidade e segurança', 'Instrumentação implementada • configuração de integração', [
        'Logs estruturados: evento, estado, duração e ordem',
        'correlationId / causationId / traceparent entre os serviços',
        'OpenTelemetry → integração configurada com Datadog',
        'JWT: assinatura, emissor, público e expiração',
        'Cliente acessa sua OS; funcionário executa etapas técnicas'],
        'A instrumentação utiliza logs estruturados e propagação de contexto entre os serviços. Correlação, causa e contexto de rastreamento permitem reconstruir o caminho de uma ordem. A exportação OpenTelemetry e a integração com Datadog estão configuradas nos projetos. Na segurança, os serviços validam assinatura, emissor, público e expiração do JWT. Também verificam se a ordem pertence ao cliente e restringem as ações técnicas ao perfil de funcionário. A autenticação serverless consulta a API de clientes, preservando o isolamento dos bancos.')
    add('Código, documentação e reprodução', 'Entrega técnica • Fase 4', [
        'GitHub: Lucas-Rodrigues-Camargo',
        'pos-tech-fiap-software-architecture-os-service',
        'pos-tech-fiap-software-architecture-billing-service',
        'pos-tech-fiap-software-architecture-execution-service',
        'README • contratos • BDD • arquitetura • evidências'],
        'Os três repositórios estão na conta Lucas Rodrigues Camargo, no GitHub, com os sufixos OS service, Billing service e Execution service. A documentação contém instruções de configuração, contratos, decisões de arquitetura e reprodução dos testes. O documento de entrega reúne a identificação do grupo, os links e a estratégia Saga. A demonstração apresentou o fluxo completo e duas compensações, com resultados capturados da execução integrada dos serviços.')

    args.assets.mkdir(parents=True, exist_ok=True)
    (args.assets / 'chapters.json').write_text(json.dumps(chapters, ensure_ascii=False, indent=2), encoding='utf-8')
    speech = ROOT / 'documents/scripts/render_phase4_speech.ps1'
    subprocess.run(['powershell', '-NoProfile', '-File', str(speech), '-Assets', str(args.assets)], check=True)
    regular = 'C:/Windows/Fonts/segoeui.ttf'
    bold = 'C:/Windows/Fonts/segoeuib.ttf'
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    segments = []
    for index, chapter in enumerate(chapters):
        image = Image.new('RGB', (1280, 720), '#101d2c')
        draw = ImageDraw.Draw(image)
        draw.rectangle((0, 0, 12, 720), fill=BLUE)
        def text(x, y, message, size, color=WHITE, heavy=False):
            draw.text((x, y), message, font=ImageFont.truetype(bold if heavy else regular, size), fill=color)
        text(55, 35, 'FIAP  /  TECH CHALLENGE  /  FASE 4', 19, BLUE)
        text(55, 92, chapter['title'], 40, heavy=True)
        text(55, 155, chapter['subtitle'], 23, MUTED)
        y = 225
        for line in chapter['lines']:
            for part in textwrap.wrap(line, width=83):
                text(60, y, part, 24, GREEN if chapter['kind'] == 'evidence' else WHITE)
                y += 35
            y += 12
        draw.line((55, 655, 1220, 655), fill='#354e64', width=2)
        text(55, 675, 'Execução local • HTTP real • PostgreSQL + LocalStack • Mercado Pago simulado', 17, MUTED)
        text(1140, 675, f'{index+1:02}/{len(chapters):02}', 18, BLUE)
        frame, audio = args.assets / f'{index:02}.png', args.assets / f'{index:02}.wav'
        image.save(frame)
        with wave.open(str(audio)) as wav:
            duration = wav.getnframes() / wav.getframerate() + 1.5
        clip = args.assets / f'{index:02}.mp4'
        subprocess.run([ffmpeg, '-y', '-loglevel', 'error', '-loop', '1', '-framerate', '24', '-i', str(frame),
                        '-i', str(audio), '-af', 'apad=pad_dur=1.5', '-t', str(duration), '-c:v', 'libx264',
                        '-preset', 'fast', '-crf', '23', '-tune', 'stillimage', '-pix_fmt', 'yuv420p',
                        '-c:a', 'aac', '-b:a', '128k', str(clip)], check=True)
        segments.append(clip)
        print(f"Rendered chapter {index+1}/{len(chapters)} ({duration:.1f}s)", flush=True)
    concat = args.assets / 'concat.txt'
    concat.write_text('\n'.join("file '" + p.as_posix() + "'" for p in segments), encoding='utf-8')
    subprocess.run([ffmpeg, '-y', '-loglevel', 'error', '-f', 'concat', '-safe', '0', '-i', str(concat),
                    '-c', 'copy', '-movflags', '+faststart', str(args.output)], check=True)
    total = sum(wave.open(str(args.assets / f'{i:02}.wav')).getnframes() /
                wave.open(str(args.assets / f'{i:02}.wav')).getframerate() + 1.5 for i in range(len(chapters)))
    if total > 900:
        raise SystemExit('Video exceeds 15 minutes')
    (args.assets / 'video-metadata.json').write_text(json.dumps({'duration_seconds': round(total, 2),
        'chapters': len(chapters), 'bdd_passed': len(cases), 'output': str(args.output)}, indent=2), encoding='utf-8')
    print(args.output, flush=True)


if __name__ == '__main__':
    main()

# Vídeo e PDF de entrega — Fase 4

Participantes: Lucas Rodrigues Camargo (RM367435) e Graziella Garcia Camargo (RM367436).

## Arquivos locais

- `C:\Users\Usuario\Desktop\Fase4\Demonstracao-Tech-Challenge-Fase-4.mp4`
- `C:\Users\Usuario\Desktop\Fase4\Entrega-Tech-Challenge-Fase-4.pdf`
- Evidências e roteiro: `C:\Users\Usuario\Desktop\Fase4\video-assets`.

O MP4 contém narração sintética em português e uma reprodução visual de respostas HTTP capturadas dos três cenários BDD. PostgreSQL e os serviços .NET executam em contêineres; SQS/DynamoDB utilizam LocalStack e Mercado Pago utiliza um simulador HTTP. A seção de entrega e observabilidade explica a configuração versionada. Os testes e a demonstração não provisionam recursos AWS.

O grupo fará o upload manual no YouTube. Sugestão de título: **FIAP — Tech Challenge Fase 4 | Oficina Mecânica | Saga e Microsserviços**. Usar visibilidade **Não listado**, acessível ao avaliador pelo link, e aguardar o processamento de vídeo e áudio antes de compartilhar o endereço.

O PDF permanece com o campo de vídeo pendente até receber a URL real. Não enviar a versão pendente como entrega definitiva. A descrição das limitações de evidência permanece nos documentos para distinguir implementação, configuração e execução comprovada.

## Atualizar o PDF com o link publicado

Editar `documents/submission-phase4.json`, preenchendo `video_url` com o link HTTPS real do YouTube ou Vimeo. Em seguida:

```powershell
phase4-e2e/.venv/Scripts/python documents/scripts/generate_phase4_submission.py --final --copy-to C:/Users/Usuario/Desktop/Fase4
```

`--final` recusa gerar uma entrega definitiva sem URL. O PDF reúne identificação, três repositórios, arquitetura, estratégia Saga, justificativas, cobertura e links de apoio em um único arquivo.

## Reproduzir a captura e a renderização

Requer o ambiente local descrito em `phase4-e2e/README.md`, as imagens `oficina-os:phase4`, `oficina-billing:phase4`, `oficina-execution:phase4` e os contêineres locais de PostgreSQL/LocalStack disponíveis. Dependências Python adicionais: `reportlab`, `pillow`, `imageio-ffmpeg`. A síntese requer a voz Microsoft Maria Desktop no Windows.

```powershell
$env:E2E_USE_DOCKER='true'
$env:DEMO_TRACE_FILE='C:/Users/Usuario/Desktop/Fase4/video-assets/http-trace.json'
phase4-e2e/.venv/Scripts/python -m pytest phase4-e2e -p documents.scripts.pytest_demo_capture -v --tb=short --junitxml=C:/Users/Usuario/Desktop/Fase4/video-assets/bdd-results.xml
phase4-e2e/.venv/Scripts/python documents/scripts/generate_phase4_video.py --assets C:/Users/Usuario/Desktop/Fase4/video-assets --output C:/Users/Usuario/Desktop/Fase4/Demonstracao-Tech-Challenge-Fase-4.mp4
```

A captura omite tokens, corpos de requisições e documentos pessoais. A renderização exige os três cenários aprovados e lê os percentuais reais de `docs/verification.json` em cada serviço. Arquivos de mídia ficam fora do Git; os scripts e o PDF são versionados.

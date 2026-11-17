# BDD integrado da Fase 4

Requisitos: Docker ativo, .NET 10, Python 3.13 e os três projetos como pastas irmãs dentro deste repositório.

```powershell
docker compose -f phase4-compose.yml up -d
dotnet build pos-tech-fiap-software-architecture-os-service/src -c Release
dotnet build pos-tech-fiap-software-architecture-billing-service/src -c Release
dotnet build pos-tech-fiap-software-architecture-execution-service/src -c Release
python -m venv phase4-e2e/.venv
phase4-e2e/.venv/Scripts/python -m pip install -r phase4-e2e/requirements.txt
phase4-e2e/.venv/Scripts/python -m pytest phase4-e2e -v
```

Não inicie uma segunda cópia dos containers se os nomes acima já existirem. Nesta sessão eles foram criados diretamente com docker run; os mesmos containers podem ser usados nos testes.

O harness sobe três processos HTTP locais (8181–8183), configura filas e tabela de teste no LocalStack e encerra os processos ao final. Os bancos e containers são preservados. Mercado Pago é um servidor HTTP de teste na porta 9199. O teste usa os adaptadores reais de PostgreSQL, DynamoDB, SQS e HTTP, sem dinheiro real.

Cenários Gherkin em features/orders.feature: sucesso com pagamento e início explícito pelo funcionário, rejeição com compensação e cancelamento após pagamento com estorno. Filas e tabela Dynamo têm nomes únicos por execução e são removidas ao final; os containers/base PostgreSQL são preservados. Os arquivos *.log são ignorados pelo Git e contêm somente dados e tokens de teste. A execução completa costuma levar 2–3 minutos devido ao long polling.

## Executar os serviços em Linux/Docker

Em máquinas com políticas do Windows que bloqueiem DLLs .NET, use o modo container:

```powershell
docker build -t oficina-os:phase4 pos-tech-fiap-software-architecture-os-service
docker build -t oficina-billing:phase4 pos-tech-fiap-software-architecture-billing-service
docker build -t oficina-execution:phase4 pos-tech-fiap-software-architecture-execution-service
$env:E2E_USE_DOCKER = 'true'
phase4-e2e/.venv/Scripts/python -m pytest phase4-e2e -v --junitxml=documents/phase4-bdd-results.xml
```

Nesse modo os hosts de serviço são containers temporários, e o simulador HTTP escuta na porta local 9199 para ser alcançado por host.docker.internal. Não use esse servidor de teste em um ambiente público.

Para a demonstração real, configure Mercado Pago sandbox e valide o webhook em URL HTTPS pública; os testes locais não constituem evidência de pagamento real nem de deploy AWS.

## GitHub Actions

No OS Service, execute o workflow Distributed BDD. Os inputs os_ref, billing_ref e execution_ref permitem selecionar branches, tags ou SHAs compatíveis; enquanto os PRs ainda não foram mesclados, selecione feature/phase4-distributed-workflow nos três serviços. Os SHAs usados são publicados no artefato bdd-revisions.txt, junto ao relatório JUnit. O workflow compara os três schemas de mensagens antes de executar. Para repositórios privados, configure CROSS_REPO_READ_TOKEN com leitura dos três repositórios.

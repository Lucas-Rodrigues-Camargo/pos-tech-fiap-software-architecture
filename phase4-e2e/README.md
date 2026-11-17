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

Cenários Gherkin em features/orders.feature: sucesso completo com pagamento e rejeição com compensação. Os arquivos *.log são ignorados pelo Git; contêm somente dados e tokens locais de teste. O tempo típico é de 60–100 segundos por execução, devido ao long polling.

Para a demonstração real, configure Mercado Pago sandbox e valide o webhook em URL HTTPS pública; os testes locais não constituem evidência de pagamento real nem de deploy AWS.

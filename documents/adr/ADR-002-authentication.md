# ADR-002 — Autenticação por perfil

- Status: aceito
- Contexto: clientes devem autenticar por CPF via serverless, enquanto funcionários usam e-mail/usuário e senha na aplicação.
- Decisão: ambos emitem JWT HS256 com o mesmo issuer e segredo rotacionável, audiences distintas e claim `role`. Clientes recebem `cpf` e `sub`; a API confere o CPF contra o proprietário da OS. Funcionários usam ASP.NET Core Identity com roles `Admin` e `User`.
- Consequências: nenhum CPF de outro cliente autoriza acesso por conhecimento do ID da OS. O segredo deve ficar no Secrets Manager/GitHub Secrets e ser rotacionado coordenadamente.

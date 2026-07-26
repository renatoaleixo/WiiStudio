# 004 - Development Workflow

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir o fluxo oficial de desenvolvimento do WiiStudio, garantindo
previsibilidade, qualidade e rastreabilidade desde a criação de uma
feature até a publicação de uma release.

# Fluxo

``` text
Backlog
   ↓
Issue
   ↓
Branch
   ↓
Implementação
   ↓
Testes
   ↓
Pull Request
   ↓
Code Review
   ↓
Merge
   ↓
Release
```

# Backlog

Toda implementação deve nascer de uma Issue.

Cada Issue deve conter:

-   Objetivo
-   Regras de negócio
-   Critérios de aceite
-   Impactos
-   Dependências

# Branches

-   feature/
-   bugfix/
-   hotfix/
-   release/

Exemplos:

``` text
feature/import-wbfs
feature/usb-detection
bugfix/wit-timeout
```

# Commits

Seguir Conventional Commits.

Exemplos:

``` text
feat: add WBFS import
fix: correct USB detection
refactor: simplify WitService
docs: update architecture
```

# Pull Request

Obrigatório conter:

-   Resumo
-   Prints (quando houver UI)
-   Testes executados
-   Checklist

# Definition of Ready

Uma tarefa só pode iniciar quando possuir:

-   Escopo definido
-   Critérios de aceite
-   Dependências conhecidas
-   Documento relacionado em docs/

# Definition of Done

Uma tarefa só é concluída quando:

-   Código compilando
-   Testes passando
-   Code Review aprovado
-   Documentação atualizada
-   Logs implementados
-   Sem warnings

# Testes

Tipos obrigatórios:

-   Unitários
-   Integração (quando aplicável)

# ADR

Mudanças arquiteturais exigem novo ADR.

Exemplo:

``` text
docs/ADR/ADR-003.md
```

# Releases

Sequência:

-   Alpha
-   Beta
-   RC
-   Stable

# Versionamento

Semantic Versioning

MAJOR.MINOR.PATCH

# Checklist

-   [x] Issue criada
-   [x] Branch correta
-   [x] Commits padronizados
-   [x] Testes
-   [x] PR
-   [x] Review
-   [x] ADR (se necessário)
-   [x] Merge

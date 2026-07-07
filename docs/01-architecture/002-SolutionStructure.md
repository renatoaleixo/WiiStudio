# 002 - Solution Structure

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a estrutura física da Solution WiiStudio para garantir
escalabilidade, baixo acoplamento e alta manutenibilidade.

# Estrutura Oficial

``` text
WiiStudio/
│
├── src/
│   ├── WiiStudio.App
│   ├── WiiStudio.Application
│   ├── WiiStudio.Domain
│   ├── WiiStudio.Infrastructure
│   └── WiiStudio.Shared
│
├── tests/
│   ├── WiiStudio.Application.Tests
│   ├── WiiStudio.Domain.Tests
│   └── WiiStudio.Infrastructure.Tests
│
├── docs/
├── assets/
├── scripts/
└── tools/
```

# Responsabilidades

## WiiStudio.App

-   Inicialização.
-   DI. 
-   Navegação.
-   Views.
-   ViewModels.

## WiiStudio.Application

-   Casos de uso.
-   Interfaces.
-   DTOs.
-   Commands.
-   Queries.
-   Validações.

## WiiStudio.Domain

-   Entidades.
-   Value Objects.
-   Regras de negócio.
-   Eventos de domínio.

## WiiStudio.Infrastructure

-   Integração com Wiimms.
-   SQLite.
-   Sistema de arquivos.
-   USB.
-   GameTDB.
-   NKit.

## WiiStudio.Shared

-   Constantes.
-   Helpers sem regra de negócio.
-   Tipos compartilhados.

# Organização de Features

Cada feature deve seguir:

``` text
Feature/
 ├── Commands/
 ├── Queries/
 ├── DTOs/
 ├── Validators/
 └── Services/
```

# Convenções

-   Um projeto = uma responsabilidade.
-   Não criar referências cruzadas.
-   Não colocar lógica de negócio na UI.
-   Toda integração externa fica em Infrastructure.

# Recursos

assets/ - icons/ - themes/ - images/ - covers/

scripts/ - build - publish - release

tools/ - wit - wwt - nkit

# Checklist

-   [ ] Estrutura respeitada
-   [ ] Projetos independentes
-   [ ] Sem dependências circulares
-   [ ] Assets organizados
-   [ ] Ferramentas isoladas

> Alterações nesta estrutura exigem ADR.

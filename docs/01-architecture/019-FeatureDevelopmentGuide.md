# 019 - Feature Development Guide

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Padronizar o desenvolvimento de todas as funcionalidades do WiiStudio.

Este documento é **obrigatório** para qualquer código novo gerado por
humanos ou IA.

------------------------------------------------------------------------

# Ciclo de Vida de uma Feature

``` text
Documento
    ↓
Issue
    ↓
Branch
    ↓
Implementação
    ↓
Testes
    ↓
Review
    ↓
Merge
```

Nenhuma feature deve ser iniciada sem especificação funcional.

------------------------------------------------------------------------

# Estrutura Obrigatória

``` text
Feature/
├── Commands/
├── Queries/
├── DTOs/
├── Validators/
├── Events/
├── Services/
├── Interfaces/
├── ViewModels/
├── Views/
├── Resources/
└── Tests/
```

------------------------------------------------------------------------

# Fluxo Arquitetural

``` text
View
   ↓
ViewModel
   ↓
Command / Query
   ↓
Application Service
   ↓
Domain
   ↓
Infrastructure
```

Nunca quebrar esse fluxo.

------------------------------------------------------------------------

# Checklist de Implementação

Toda Feature deve possuir:

-   Objetivo
-   Casos de uso
-   ViewModel
-   View
-   Commands
-   Queries (quando necessário)
-   DTOs
-   Validators
-   Interfaces
-   Serviços
-   Testes unitários
-   Localização
-   Logging
-   Documentação

------------------------------------------------------------------------

# Estrutura da ViewModel

Responsabilidades:

-   Estado da UI
-   Comandos
-   Navegação
-   Observáveis

Nunca:

-   acessar Process
-   acessar SQLite
-   chamar Wiimms diretamente

------------------------------------------------------------------------

# Commands

Commands modificam estado.

Exemplos:

-   ImportGameCommand
-   RemoveGameCommand
-   ConvertGameCommand

------------------------------------------------------------------------

# Queries

Queries apenas consultam dados.

Nunca alteram estado.

------------------------------------------------------------------------

# Validators

Utilizar FluentValidation.

Toda entrada externa deve ser validada.

------------------------------------------------------------------------

# Logging

Toda Feature registra:

-   início
-   fim
-   duração
-   falhas
-   cancelamentos

------------------------------------------------------------------------

# Testes

Obrigatórios:

-   sucesso
-   erro
-   cancelamento
-   validação
-   exceções

Meta mínima: 90% da lógica da Feature.

------------------------------------------------------------------------

# Localização

Todo texto exibido ao usuário deve utilizar recursos localizáveis.

É proibido texto fixo na ViewModel.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca criar Feature incompleta.
-   Sempre gerar a estrutura completa.
-   Nunca pular testes.
-   Nunca acessar Infrastructure pela ViewModel.
-   Sempre usar Dependency Injection.
-   Sempre usar CancellationToken em operações longas.

------------------------------------------------------------------------

# Definition of Done

Uma Feature só está pronta quando:

-   Compila sem warnings
-   Testes aprovados
-   Documentação atualizada
-   Traduções adicionadas
-   Logs implementados
-   Review aprovado

------------------------------------------------------------------------

# Checklist Final

-   [x] Estrutura completa
-   [x] Testes
-   [x] Logging
-   [x] Localização
-   [x] Documentação
-   [x] Critérios de aceite

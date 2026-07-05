# 023 - Playbook: Feature Development

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Este playbook define o processo oficial para projetar, implementar,
revisar e entregar uma nova funcionalidade no WiiStudio.

------------------------------------------------------------------------

# Filosofia

Uma Feature nunca começa pelo código.

Fluxo obrigatório:

``` text
Ideia
  ↓
Especificação
  ↓
ADR (quando necessário)
  ↓
Issue
  ↓
Implementação
  ↓
Testes
  ↓
Review
  ↓
Merge
  ↓
Release
```

------------------------------------------------------------------------

# Checklist Pré-Implementação

Antes de escrever qualquer código confirme:

-   [ ] Existe documento da Feature.
-   [ ] Critérios de aceite definidos.
-   [ ] Dependências identificadas.
-   [ ] Impacto arquitetural analisado.
-   [ ] ADR criada (quando necessário).

------------------------------------------------------------------------

# Estrutura Obrigatória

``` text
Feature
├── View
├── ViewModel
├── Commands
├── Queries
├── DTOs
├── Validators
├── Services
├── Interfaces
├── Localization
├── Tests
└── Documentation
```

------------------------------------------------------------------------

# Padrões

## UI

-   Sem regra de negócio.
-   Componentes reutilizáveis.
-   Estados Loading, Empty, Error e Success.

## ViewModel

-   Apenas orquestração da UI.
-   Nunca acessar Infrastructure.
-   Comunicação via Application.

## Application

-   Casos de uso.
-   Commands.
-   Queries.
-   DTOs.
-   Interfaces.

## Domain

-   Regras de negócio.
-   Entidades.
-   Value Objects.

## Infrastructure

-   Adaptadores.
-   Providers.
-   Banco.
-   ProcessRunner.

------------------------------------------------------------------------

# Testes Obrigatórios

Cada Feature deve possuir:

-   Teste de sucesso.
-   Teste de erro.
-   Teste de cancelamento.
-   Teste de validação.
-   Teste de regressão (quando aplicável).

------------------------------------------------------------------------

# Pull Request

Toda PR deve conter:

-   Objetivo.
-   Prints (se houver UI).
-   Evidências de testes.
-   Atualização da documentação.

------------------------------------------------------------------------

# Critérios de Revisão

Revisor deve verificar:

-   Arquitetura.
-   Legibilidade.
-   Performance.
-   Testes.
-   Logging.
-   Localização.
-   Acessibilidade.
-   Segurança.

------------------------------------------------------------------------

# Métricas

Meta por Feature:

-   Build sem warnings.
-   Cobertura mínima da lógica: 90%.
-   Complexidade baixa.
-   Sem duplicação de código.

------------------------------------------------------------------------

# Definition of Done

Uma Feature somente é considerada pronta quando:

-   Especificação atendida.
-   Critérios de aceite cumpridos.
-   Testes aprovados.
-   Documentação atualizada.
-   Review aprovado.
-   Merge realizado.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca iniciar código sem especificação.
-   Sempre gerar estrutura completa.
-   Sempre seguir a arquitetura oficial.
-   Nunca ignorar testes.
-   Nunca criar atalhos arquiteturais.

------------------------------------------------------------------------

# Checklist Final

-   [ ] Documento atualizado
-   [ ] Código implementado
-   [ ] Testes aprovados
-   [ ] Review concluído
-   [ ] Logs implementados
-   [ ] Localização adicionada
-   [ ] Release pronta

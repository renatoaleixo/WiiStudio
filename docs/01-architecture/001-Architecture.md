# 001 - Architecture

> Status: Accepted Versão: 1.0.0

# Objetivo

Este documento define a arquitetura oficial do WiiStudio.

## Princípios

-   Clean Architecture
-   SOLID
-   MVVM
-   Dependency Injection
-   Multiplataforma

## Arquitetura

``` text
Presentation
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
```

## Estrutura

``` text
src/
  WiiStudio.App
  WiiStudio.Application
  WiiStudio.Domain
  WiiStudio.Infrastructure
  WiiStudio.Shared

tests/
docs/
assets/
```

## Dependências

  Camada           Pode depender
  ---------------- ---------------
  App              Application
  Application      Domain
  Infrastructure   Domain
  Domain           Nenhuma

## Regras do Codex

-   Nunca colocar lógica na View.
-   Sempre usar MVVM.
-   Sempre usar DI.
-   Async para I/O.
-   CancellationToken em operações longas.
-   Domain nunca referencia Infrastructure.

## Serviços

-   IWitService
-   IWwtService
-   INKitService
-   IUsbDriveService
-   ICoverService

## Logging

Utilizar Serilog.

## Checklist

-   [ ] Clean Architecture
-   [ ] SOLID
-   [ ] MVVM
-   [ ] Testes
-   [ ] Logging

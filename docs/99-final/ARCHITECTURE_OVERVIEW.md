# ARCHITECTURE_OVERVIEW

# Objetivo

Este documento resume a arquitetura do WiiStudio, oferecendo uma visão de alto nível
para desenvolvedores e agentes de IA entenderem as camadas, responsabilidades,
dependências permitidas, módulos principais e regras obrigatórias.

---

# Visão Geral

O WiiStudio segue Clean Architecture com UI desktop em Avalonia.

```text
┌────────────────────────────┐
│ UI                         │
│ Avalonia, Views, ViewModels│
└─────────────┬──────────────┘
              ↓
┌────────────────────────────┐
│ Application                │
│ Use cases, services, DTOs  │
└─────────────┬──────────────┘
              ↓
┌────────────────────────────┐
│ Domain                     │
│ Entities, rules, models    │
└─────────────┬──────────────┘
              ↑
┌────────────────────────────┐
│ Infrastructure             │
│ SQLite, files, tools, APIs │
└────────────────────────────┘
```

---

# Camadas

## Domain

Contém:

- entidades;
- value objects;
- enums;
- regras puras;
- contratos conceituais quando fizer sentido;
- invariantes.

Não deve conter:

- Avalonia;
- SQLite;
- Serilog;
- ProcessRunner concreto;
- filesystem real;
- HTTP;
- plugins concretos.

---

## Application

Contém:

- use cases;
- application services;
- commands;
- DTOs;
- interfaces;
- policies;
- validações;
- orquestração.

Responsável por coordenar regras sem conhecer detalhes de infraestrutura.

---

## Infrastructure

Contém:

- SQLite;
- repositories;
- filesystem;
- ProcessRunner;
- Wiimms integration;
- NKit legacy integration;
- metadata providers;
- asset storage;
- settings storage;
- logging implementation;
- plugin hosting.

---

## UI

Contém:

- Avalonia Views;
- ViewModels;
- App Shell;
- Design System;
- Resources;
- Themes;
- Dialogs;
- Navigation UI.

Não deve conter regra de negócio.

---

# Dependências Permitidas

```text
UI -> Application
Application -> Domain
Infrastructure -> Application + Domain
Domain -> ninguém
```

UI não deve depender diretamente de Infrastructure.

---

# Principais Módulos

## App Shell

Responsável por:

- MainWindow;
- Header;
- Main Menu;
- Sidebar;
- Workspace;
- Panels;
- Status Bar;
- Dialog Host;
- Notification Host.

Documentos:

- `F061-AppShellStructure.md`
- `F062-MainMenu.md`
- `F063-SidebarNavigation.md`
- `F066-WorkspaceRegion.md`

---

## Game Library

Responsável por:

- scan;
- listagem;
- busca;
- filtros;
- favoritos;
- duplicados;
- detalhes.

Documentos:

- `016-GameLibrary.md`
- `F002-ScanGameLibrary.md`
- `F032-LibraryView.md`

---

## Device Manager

Responsável por:

- detecção USB;
- estado de dispositivo;
- permissões;
- integração com Wiimms.

Documentos:

- `010-USBDetection.md`
- `F001-DetectUsbDrive.md`
- `F035-DeviceManager.md`

---

## Conversion Engine

Responsável por:

- conversões;
- batch conversion;
- providers;
- ProcessRunner;
- progresso;
- cancelamento.

Documentos:

- `018-ConversionEngine.md`
- `F007-ConvertGame.md`
- `F008-BatchConversion.md`
- `F078-PluginConversionProviders.md`

---

## Metadata and Assets

Responsável por:

- GameTDB;
- cache offline;
- capas;
- banners;
- disc art;
- providers.

Documentos:

- `014-GameTDB.md`
- `F019-OfflineMetadataCache.md`
- `F020-AssetManager.md`
- `F077-PluginMetadataProviders.md`

---

## Task, Logging and Diagnostics

Responsável por:

- tarefas longas;
- progresso;
- logs;
- diagnósticos;
- notificações.

Documentos:

- `F025-NotificationCenter.md`
- `F026-TaskManager.md`
- `F029-LoggingCenter.md`
- `F030-AdvancedDiagnostics.md`

---

## Plugin SDK

Responsável por:

- contratos;
- manifestos;
- discovery;
- loading;
- permissões;
- providers;
- UI extensions;
- diagnostics.

Documentos:

- `F071-PluginSDKFoundation.md`
- `F072-PluginManifest.md`
- `F073-PluginDiscovery.md`
- `F074-PluginLoading.md`
- `F075-PluginPermissions.md`

---

## Security

Responsável por:

- paths seguros;
- processos externos;
- plugins;
- settings sensíveis;
- redaction;
- backups;
- updates.

Documentos:

- `F081-SecurityFoundation.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F090-SecurityChecklist.md`

---

# ProcessRunner

Toda execução externa deve passar por `IProcessRunner`.

Motivos:

- validação de executável;
- argumentos estruturados;
- timeout;
- cancelamento;
- stdout/stderr;
- redaction;
- logging;
- Task Manager.

Proibido:

```csharp
Process.Start(...)
```

fora da infraestrutura aprovada.

---

# UI e MVVM

Regras:

- ViewModel contém estado e commands.
- View contém binding.
- Code-behind só para inicialização visual mínima quando inevitável.
- Sem regra de negócio na View.
- Textos localizáveis.
- Tokens do Design System.
- Light/Dark.
- Loading/Empty/Error.
- Acessibilidade por teclado.

---

# Plugins

Plugins são externos e não confiáveis por padrão.

Fluxo:

```text
Discovery
    ↓
Manifest Validation
    ↓
Permission Check
    ↓
Loading
    ↓
Initialize
    ↓
Register Capabilities
    ↓
Runtime Monitoring
    ↓
Unload
```

Falha de plugin não pode derrubar a aplicação.

---

# Banco

SQLite deve armazenar:

- biblioteca;
- metadados;
- assets index;
- settings;
- tarefas;
- logs indexados quando aplicável;
- cache;
- plugins.

Regras:

- migrations testadas;
- transações para operações críticas;
- rollback quando necessário;
- repository abstraído.

---

# Segurança Arquitetural

Aplicar sempre:

- validar entrada externa;
- normalizar paths;
- bloquear traversal;
- redigir logs;
- confirmar ações destrutivas;
- isolar plugins;
- usar permissions;
- usar cancellation token;
- falha segura.

---

# Testabilidade

Toda feature deve ser testável.

Padrões:

- interfaces para infraestrutura;
- fakes para processos;
- banco temporário;
- diretórios temporários;
- ViewModels testáveis;
- testes de erro;
- regressões.

---

# Quality Gates

Antes de concluir:

- build passa;
- testes passam;
- segurança validada;
- documentação atualizada;
- logs seguros;
- sem regra de negócio na UI;
- sem `Process.Start` direto;
- sem path traversal.

---

# Resumo para Implementação

Ao criar nova feature:

1. Domain: modelos e regras puras.
2. Application: caso de uso e interfaces.
3. Infrastructure: implementação externa.
4. UI: ViewModel e View.
5. Tests: unit/integration/UI quando aplicável.
6. Docs: atualizar documentação.

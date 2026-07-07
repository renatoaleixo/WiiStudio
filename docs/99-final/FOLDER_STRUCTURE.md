# FOLDER_STRUCTURE

# Objetivo

Definir a estrutura final recomendada de pastas para o projeto WiiStudio e para a
documentação, permitindo que humanos e agentes de IA encontrem rapidamente código,
testes, contratos, recursos, documentação e arquivos auxiliares.

---

# Estrutura Geral do Repositório

```text
WiiStudio/
├── src/
├── tests/
├── docs/
├── tools/
├── samples/
├── scripts/
├── build/
├── assets/
├── README.md
├── ROADMAP.md
├── CONTRIBUTING.md
├── AI_CONTEXT.md
└── DOCUMENT_INDEX.md
```

---

# Pasta src

```text
src/
├── WiiStudio.Domain/
├── WiiStudio.Application/
├── WiiStudio.Infrastructure/
├── WiiStudio.UI/
├── WiiStudio.Plugin.Abstractions/
├── WiiStudio.Plugin.Hosting/
├── WiiStudio.Plugin.Security/
└── WiiStudio.Plugin.Diagnostics/
```

---

# WiiStudio.Domain

```text
WiiStudio.Domain/
├── Games/
├── Devices/
├── Metadata/
├── Assets/
├── Tasks/
├── Plugins/
├── Common/
└── WiiStudio.Domain.csproj
```

Contém:

- entidades;
- value objects;
- enums;
- regras puras;
- invariantes.

Não contém:

- Avalonia;
- SQLite;
- ProcessRunner concreto;
- filesystem real;
- HTTP;
- Serilog concreto.

---

# WiiStudio.Application

```text
WiiStudio.Application/
├── Abstractions/
├── Games/
├── Devices/
├── Import/
├── Conversion/
├── Metadata/
├── Assets/
├── Tasks/
├── Settings/
├── Diagnostics/
├── Plugins/
├── Security/
└── WiiStudio.Application.csproj
```

Contém:

- use cases;
- interfaces;
- DTOs;
- commands;
- validators;
- policies;
- orchestration services.

---

# WiiStudio.Infrastructure

```text
WiiStudio.Infrastructure/
├── Database/
├── FileSystem/
├── ProcessExecution/
├── Wiimms/
├── NKit/
├── Metadata/
├── Assets/
├── Settings/
├── Logging/
├── Security/
├── Backup/
├── Updates/
└── WiiStudio.Infrastructure.csproj
```

Contém:

- SQLite;
- repositories;
- migrations;
- filesystem;
- ProcessRunner;
- Wiimms integration;
- NKit legacy integration;
- storage;
- logging implementations.

---

# WiiStudio.UI

```text
WiiStudio.UI/
├── App.axaml
├── MainWindow.axaml
├── Shell/
├── Views/
├── ViewModels/
├── Controls/
├── Dialogs/
├── Themes/
├── Resources/
├── Navigation/
├── DesignSystem/
├── Accessibility/
└── WiiStudio.UI.csproj
```

---

# UI Shell

```text
Shell/
├── Header/
├── MainMenu/
├── Sidebar/
├── Workspace/
├── StatusBar/
├── Panels/
├── Notifications/
└── DialogHost/
```

---

# Views e ViewModels

```text
Views/
├── Dashboard/
├── Library/
├── Devices/
├── Import/
├── Conversion/
├── Tasks/
├── Metadata/
├── Assets/
├── Diagnostics/
├── Plugins/
└── Settings/

ViewModels/
├── Dashboard/
├── Library/
├── Devices/
├── Import/
├── Conversion/
├── Tasks/
├── Metadata/
├── Assets/
├── Diagnostics/
├── Plugins/
└── Settings/
```

---

# Plugin Projects

```text
WiiStudio.Plugin.Abstractions/
├── IWiiStudioPlugin.cs
├── IPluginContext.cs
├── IPluginManifest.cs
├── Permissions/
├── Capabilities/
└── Providers/

WiiStudio.Plugin.Hosting/
├── Discovery/
├── Loading/
├── Registry/
└── Lifecycle/

WiiStudio.Plugin.Security/
├── Permissions/
├── Isolation/
├── Quarantine/
└── Audit/

WiiStudio.Plugin.Diagnostics/
├── Events/
├── Reports/
└── Export/
```

---

# Pasta tests

```text
tests/
├── WiiStudio.Domain.Tests/
├── WiiStudio.Application.Tests/
├── WiiStudio.Infrastructure.Tests/
├── WiiStudio.UI.Tests/
├── WiiStudio.Plugin.Tests/
├── WiiStudio.Integration.Tests/
├── WiiStudio.Performance.Tests/
└── WiiStudio.Regression.Tests/
```

---

# TestData

```text
tests/
└── TestData/
    ├── Games/
    ├── Manifests/
    ├── Plugins/
    ├── Logs/
    ├── Backups/
    └── Databases/
```

---

# Pasta docs

```text
docs/
├── 00-project/
├── 01-architecture/
├── 02-core-features/
├── 03-ui-design-system/
├── 04-application-shell/
├── 05-plugin-sdk/
├── 06-security/
├── 07-testing-quality/
└── 99-final/
```

---

# docs/00-project

```text
00-project/
├── README.md
├── ROADMAP.md
├── AI_CONTEXT.md
├── DOCUMENT_INDEX.md
├── CONTRIBUTING.md
├── DEVELOPMENT_GUIDE.md
└── DEFINITION_OF_DONE.md
```

---

# docs/01-architecture

```text
01-architecture/
├── 001-Architecture.md
├── 002-SolutionStructure.md
├── 003-CodingStandards.md
├── 004-DevelopmentWorkflow.md
├── 005-DomainModel.md
├── 006-ApplicationLayer.md
├── 007-Infrastructure.md
├── 008-UI-UX.md
├── 009-Database.md
├── 010-USBDetection.md
├── 011-WBFS.md
├── 012-WiimmsIntegration.md
├── 013-NKitLegacy.md
├── 014-GameTDB.md
├── 015-PluginArchitecture.md
├── 016-GameLibrary.md
├── 017-ImportWizard.md
├── 018-ConversionEngine.md
├── 019-FeatureDevelopmentGuide.md
├── 020-Diagnostics.md
├── 021-ThemeSystem.md
├── 022-Localization.md
└── 023-Playbook-FeatureDevelopment.md
```

---

# docs/02-core-features

```text
02-core-features/
├── F001-DetectUsbDrive.md
├── F002-ScanGameLibrary.md
├── ...
└── F030-AdvancedDiagnostics.md
```

---

# docs/03-ui-design-system

```text
03-ui-design-system/
├── F031-MainWindow.md
├── ...
└── F060-ResponsiveBehaviors.md
```

---

# docs/04-application-shell

```text
04-application-shell/
├── F061-AppShellStructure.md
├── ...
└── F070-AccessibilityNavigation.md
```

---

# docs/05-plugin-sdk

```text
05-plugin-sdk/
├── F071-PluginSDKFoundation.md
├── ...
└── F080-PluginMarketplacePreparation.md
```

---

# docs/06-security

```text
06-security/
├── F081-SecurityFoundation.md
├── ...
└── F090-SecurityChecklist.md
```

---

# docs/07-testing-quality

```text
07-testing-quality/
├── F091-TestingStrategy.md
├── ...
└── F100-QualityGates.md
```

---

# docs/99-final

```text
99-final/
├── ARCHITECTURE_OVERVIEW.md
├── FOLDER_STRUCTURE.md
├── MIGRATION_PLAN_TO_FINAL_FOLDERS.md
└── RELEASE_NOTES_TEMPLATE.md
```

---

# tools

```text
tools/
├── wiimms/
├── nkit/
└── README.md
```

Ferramentas externas não devem ser chamadas diretamente pela UI.

---

# samples

```text
samples/
├── plugins/
├── manifests/
├── metadata-providers/
└── conversion-providers/
```

---

# scripts

```text
scripts/
├── build.ps1
├── build.sh
├── test.ps1
├── test.sh
├── package.ps1
└── package.sh
```

---

# assets

```text
assets/
├── icons/
├── screenshots/
├── logos/
└── mockups/
```

---

# Regras de Organização

- Código de produção fica em `src`.
- Testes ficam em `tests`.
- Documentação fica em `docs`.
- Ferramentas externas ficam em `tools`.
- Plugins de exemplo ficam em `samples`.
- Scripts ficam em `scripts`.
- Assets do repositório ficam em `assets`.
- Dados gerados em runtime não devem ser commitados.

---

# Regras para Codex

- Criar arquivos na pasta correta.
- Não misturar UI com Application.
- Não colocar infraestrutura no Domain.
- Não criar testes dentro de `src`.
- Não colocar documentação solta sem atualizar índice.
- Não adicionar ferramenta externa sem documentação.
- Não criar plugin sem manifesto.
- Atualizar `DOCUMENT_INDEX.md` quando criar documento novo.

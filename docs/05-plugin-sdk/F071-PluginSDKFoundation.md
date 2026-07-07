# F071 - Plugin SDK Foundation

> Feature ID: F071
> Status: Ready for Development
> Priority: Critical
> Epic: Plugin SDK

# Objetivo

Definir a fundação oficial do Plugin SDK do WiiStudio, permitindo que funcionalidades
externas sejam adicionadas de forma controlada, segura, versionada e extensível.

O Plugin SDK deve permitir, no futuro, extensões para metadados, assets, diagnósticos,
conversões, integrações externas e componentes de UI, sem comprometer a estabilidade
da aplicação principal.

---

# Requisitos Funcionais

## RF001
Definir arquitetura base do sistema de plugins.

## RF002
Definir contratos públicos do Plugin SDK.

## RF003
Separar aplicação principal de plugins externos.

## RF004
Permitir descoberta, validação e carregamento controlado.

## RF005
Definir modelo de versionamento do SDK.

## RF006
Definir limites de segurança e permissões.

## RF007
Garantir logging e diagnóstico para plugins.

## RF008
Permitir desativação de plugins problemáticos.

## RF009
Permitir evolução futura sem quebrar compatibilidade desnecessariamente.

## RF010
Evitar dependência direta de plugins na camada principal.

---

# Visão Geral

```text
WiiStudio
├── Plugin Host
├── Plugin SDK Contracts
├── Plugin Discovery
├── Plugin Loader
├── Plugin Permission System
├── Plugin Registry
└── Plugins
    ├── Metadata Providers
    ├── Asset Providers
    ├── Conversion Providers
    ├── Diagnostics Providers
    └── UI Extensions
```

---

# Princípios

O sistema de plugins deve seguir:

- contratos explícitos;
- isolamento lógico;
- validação antes de carregamento;
- logging obrigatório;
- permissões declaradas;
- falha segura;
- desativação controlada;
- compatibilidade por versão;
- nenhuma confiança implícita em plugin externo.

---

# Tipos de Plugin

## Metadata Provider Plugin

Fornece metadados de jogos.

Exemplos:

- provider alternativo ao GameTDB;
- provider local;
- provider privado.

## Asset Provider Plugin

Fornece capas, banners, disc arts e imagens.

## Conversion Provider Plugin

Adiciona suporte a fluxos de conversão externos.

## Diagnostics Plugin

Adiciona verificações técnicas.

## UI Extension Plugin

Adiciona componentes visuais controlados em regiões permitidas.

## Automation Plugin

Futuro suporte para tarefas automatizadas.

---

# Estrutura de Plugin

```text
PluginFolder/
├── plugin.json
├── PluginAssembly.dll
├── assets/
├── resources/
└── README.md
```

---

# Contrato Base

```csharp
public interface IWiiStudioPlugin
{
    string Id { get; }

    string Name { get; }

    string Version { get; }

    Task InitializeAsync(
        IPluginContext context,
        CancellationToken cancellationToken = default);

    Task ShutdownAsync(
        CancellationToken cancellationToken = default);
}
```

---

# Plugin Context

```csharp
public interface IPluginContext
{
    ILogger Logger { get; }

    IServiceProvider Services { get; }

    IPluginStorage Storage { get; }

    IPluginPermissionService Permissions { get; }

    IPluginNotificationService Notifications { get; }
}
```

---

# Contratos do SDK

O SDK deve conter apenas contratos estáveis.

Exemplos:

- `IWiiStudioPlugin`
- `IPluginContext`
- `IPluginManifest`
- `IPluginStorage`
- `IPluginPermissionService`
- `IMetadataProviderPlugin`
- `IAssetProviderPlugin`
- `IConversionProviderPlugin`
- `IDiagnosticsPlugin`
- `IUiExtensionPlugin`

---

# Separação de Projetos

Estrutura recomendada:

```text
src/
├── WiiStudio.Plugin.Abstractions/
├── WiiStudio.Plugin.Hosting/
├── WiiStudio.Plugin.Security/
├── WiiStudio.Plugin.Diagnostics/
└── WiiStudio.Plugin.Samples/
```

---

# Responsabilidades

## Plugin.Abstractions

Contém contratos públicos.

Não deve depender da UI principal.

## Plugin.Hosting

Responsável por descoberta, carregamento e ciclo de vida.

## Plugin.Security

Responsável por permissões, validação e restrições.

## Plugin.Diagnostics

Responsável por logs, falhas e relatórios.

## Plugin.Samples

Exemplos oficiais de plugins.

---

# Ciclo de Vida

```text
Discover
    ↓
Read Manifest
    ↓
Validate Manifest
    ↓
Check Compatibility
    ↓
Check Permissions
    ↓
Load Assembly
    ↓
Instantiate Plugin
    ↓
Initialize
    ↓
Register Capabilities
    ↓
Run
    ↓
Shutdown
```

---

# Estados de Plugin

Um plugin pode estar nos estados:

- Discovered
- Validating
- Compatible
- Incompatible
- PermissionRequired
- Disabled
- Loading
- Loaded
- Running
- Failed
- Unloaded
- Blocked

---

# Versionamento

O Plugin SDK deve possuir versão própria.

Exemplo:

```text
PluginSdkVersion: 1.0.0
```

Regras:

- breaking changes incrementam major;
- novos contratos compatíveis incrementam minor;
- correções incrementam patch;
- plugin deve declarar versão mínima e máxima suportada quando necessário.

---

# Compatibilidade

Antes de carregar plugin:

- verificar versão do SDK;
- verificar versão mínima do WiiStudio;
- verificar plataforma;
- verificar arquitetura;
- verificar permissões;
- verificar assinatura quando houver suporte futuro.

---

# Manifesto

Todo plugin deve possuir `plugin.json`.

O manifesto será detalhado em `F072-PluginManifest.md`.

---

# Permissões

Plugins devem declarar permissões.

Exemplos:

- FileSystemRead
- FileSystemWrite
- NetworkAccess
- ExecuteProcess
- AccessGameLibrary
- AccessMetadata
- AccessUiExtensions
- AccessDiagnostics

Permissões serão detalhadas em `F075-PluginPermissions.md`.

---

# Falha Segura

Se um plugin falhar:

- registrar erro;
- desativar plugin se necessário;
- não derrubar a aplicação principal;
- exibir notificação controlada;
- permitir acesso ao diagnóstico;
- permitir desinstalação/desativação.

---

# Logging

Plugins devem usar logger fornecido pelo contexto.

Regras:

- plugin não deve escrever logs diretamente fora do sistema oficial;
- logs devem conter PluginId;
- erros devem ser rastreáveis;
- logs sensíveis devem ser redigidos.

---

# Storage de Plugin

Plugins podem possuir storage isolado.

Regras:

- cada plugin deve ter área própria;
- plugin não deve acessar storage de outro plugin;
- paths devem ser seguros;
- dados sensíveis devem seguir regras de segurança.

---

# UI Extensions

Plugins com UI devem respeitar:

- Design System;
- Theme Engine;
- Localization;
- Accessibility Navigation;
- regiões permitidas;
- limites de performance.

---

# Integração com DI

Plugins podem registrar serviços dentro de escopo controlado.

Regras:

- não sobrescrever serviços core;
- não registrar singletons perigosos sem controle;
- não acessar serviços internos não expostos;
- respeitar contratos públicos.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.Plugin.Abstractions/
    ├── IWiiStudioPlugin.cs
    ├── IPluginContext.cs
    ├── IPluginManifest.cs
    ├── IPluginStorage.cs
    ├── IPluginPermissionService.cs
    ├── PluginCapability.cs
    ├── PluginState.cs
    └── PluginSdkVersion.cs
```

---

# Regras para o Codex

- Nunca permitir que plugin acesse APIs internas sem contrato.
- Sempre usar interfaces do SDK.
- Sempre validar manifesto antes de carregar assembly.
- Sempre checar compatibilidade.
- Sempre aplicar permissões.
- Sempre isolar falhas de plugin.
- Sempre registrar logs com PluginId.
- Sempre permitir desativar plugin problemático.
- Nunca deixar plugin derrubar a aplicação principal.
- Nunca confiar em path fornecido por plugin sem validação.
- Nunca executar processo externo sem permissão explícita.

---

# Casos de Erro

- Manifesto ausente.
- Manifesto inválido.
- SDK incompatível.
- Plugin duplicado.
- Assembly ausente.
- Falha ao carregar assembly.
- Falha no InitializeAsync.
- Permissão negada.
- Plugin lança exceção.
- Plugin tenta acessar recurso não permitido.
- Plugin trava durante shutdown.

---

# Critérios de Aceite

- [ ] Arquitetura base do Plugin SDK definida.
- [ ] Contrato `IWiiStudioPlugin` definido.
- [ ] `IPluginContext` definido.
- [ ] Ciclo de vida documentado.
- [ ] Estados de plugin definidos.
- [ ] Versionamento definido.
- [ ] Compatibilidade documentada.
- [ ] Falha segura documentada.
- [ ] Logging de plugins definido.
- [ ] Storage isolado previsto.
- [ ] UI Extensions previstas.
- [ ] Regras para o Codex documentadas.

---

# Testes

## Unitários

- Plugin válido carrega.
- Plugin inválido falha com erro controlado.
- SDK incompatível bloqueia plugin.
- Plugin duplicado é rejeitado.
- Falha no InitializeAsync não derruba aplicação.
- Shutdown chamado.
- Logger recebe PluginId.
- Storage isolado criado.

## Integração

- Descoberta de plugin.
- Validação de manifesto.
- Carregamento controlado.
- Desativação de plugin.
- Plugin com permissão negada.
- Plugin com erro em runtime.
- Plugin UI Extension simples.
- Plugin Metadata Provider simples.

---

# Dependências

- F015-PluginArchitecture.md
- F025-NotificationCenter.md
- F029-LoggingCenter.md
- F030-AdvancedDiagnostics.md
- F041-DesignSystemFoundation.md
- F070-AccessibilityNavigation.md
- 003-CodingStandards.md
- 006-ApplicationLayer.md
- 007-Infrastructure.md

---

# Definition of Done

- Fundação do Plugin SDK documentada.
- Contratos base definidos.
- Ciclo de vida especificado.
- Segurança inicial prevista.
- Falha segura definida.
- Testes definidos.
- Documentação atualizada.

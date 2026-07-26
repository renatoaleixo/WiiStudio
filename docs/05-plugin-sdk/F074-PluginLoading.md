# F074 - Plugin Loading

> Feature ID: F074
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Plugin SDK

# Objetivo

Definir o processo oficial de carregamento de plugins do WiiStudio, incluindo
validação final, isolamento lógico, criação de contexto, instanciação,
inicialização, registro de capacidades, tratamento de falhas e descarregamento.

O carregamento de plugins deve ser seguro, rastreável e incapaz de derrubar a
aplicação principal em caso de erro.

---

# Requisitos Funcionais

## RF001
Carregar somente plugins descobertos e validados.

## RF002
Validar permissões antes da inicialização.

## RF003
Instanciar o tipo que implementa `IWiiStudioPlugin`.

## RF004
Criar `IPluginContext` isolado.

## RF005
Executar `InitializeAsync`.

## RF006
Registrar capacidades declaradas.

## RF007
Tratar falhas de carregamento sem derrubar a aplicação.

## RF008
Permitir desativação automática de plugin com falha crítica.

## RF009
Executar `ShutdownAsync` no encerramento.

## RF010
Registrar logs e diagnósticos de todo o ciclo.

---

# Fluxo de Carregamento

```text
PluginDescriptor ReadyForLoad
    ↓
Validate Final State
    ↓
Validate Permissions
    ↓
Load Assembly
    ↓
Resolve Entry Type
    ↓
Create Plugin Instance
    ↓
Create Plugin Context
    ↓
InitializeAsync
    ↓
Register Capabilities
    ↓
Set State Running
```

---

# Estados de Loading

```csharp
public enum PluginLoadState
{
    Pending,
    Loading,
    Loaded,
    Initializing,
    Running,
    Failed,
    Disabled,
    Unloading,
    Unloaded
}
```

---

# Plugin Loader Service

```csharp
public interface IPluginLoaderService
{
    Task<PluginLoadResult> LoadAsync(
        PluginDescriptor descriptor,
        CancellationToken cancellationToken = default);

    Task UnloadAsync(
        string pluginId,
        CancellationToken cancellationToken = default);
}
```

---

# Resultado de Carregamento

```csharp
public sealed class PluginLoadResult
{
    public string PluginId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public PluginLoadState State { get; init; }
    public IWiiStudioPlugin? Instance { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

---

# Criação do Plugin Context

O contexto deve conter apenas serviços permitidos.

```csharp
public sealed class PluginContext : IPluginContext
{
    public ILogger Logger { get; }
    public IServiceProvider Services { get; }
    public IPluginStorage Storage { get; }
    public IPluginPermissionService Permissions { get; }
    public IPluginNotificationService Notifications { get; }
}
```

---

# Registro de Capacidades

Após inicialização, o plugin deve registrar capacidades compatíveis com seu manifesto.

Exemplos:

- Metadata Provider.
- Asset Provider.
- Conversion Provider.
- Diagnostics Provider.
- UI Extension.

O plugin não deve registrar capacidade não declarada no manifesto.

---

# Isolamento Lógico

O plugin deve ser isolado logicamente por:

- contexto próprio;
- storage próprio;
- logger com PluginId;
- permissões avaliadas;
- serviços permitidos;
- tratamento de exceções.

---

# Falha Segura

Se o plugin falhar:

- capturar exceção;
- registrar erro;
- marcar estado como Failed;
- não derrubar o WiiStudio;
- remover capacidades parcialmente registradas;
- exibir notificação quando relevante;
- permitir diagnóstico.

---

# Shutdown

No encerramento:

```text
Application Shutdown
    ↓
Plugin Host
    ↓
ShutdownAsync em cada plugin
    ↓
Timeout controlado
    ↓
Remover capacidades
    ↓
Liberar recursos
```

---

# Timeout

Operações de inicialização e shutdown devem ter timeout configurável.

Exemplos:

- InitializeAsync: 30 segundos.
- ShutdownAsync: 10 segundos.

Plugins que excederem timeout devem ser marcados como Failed ou UnloadedWithWarning.

---

# Integração com DI

Plugins podem receber serviços via contexto, mas não devem sobrescrever serviços core.

Regras:

- registrar serviços em escopo controlado;
- não substituir serviços do WiiStudio;
- não acessar container global sem permissão;
- expor apenas interfaces do SDK.

---

# Integração com Permissions

Antes de inicializar:

- verificar permissões declaradas;
- bloquear permissões não aprovadas;
- impedir execução de recursos sensíveis;
- registrar decisão.

---

# Integração com Diagnostics

Registrar:

- tempo de carregamento;
- tempo de inicialização;
- permissões usadas;
- capacidades registradas;
- falhas;
- warnings;
- exceções;
- estado final.

---

# Regras para o Codex

- Nunca carregar plugin sem descriptor válido.
- Nunca carregar plugin sem manifesto validado.
- Sempre verificar permissões antes de inicializar.
- Sempre criar contexto isolado.
- Sempre capturar exceções do plugin.
- Sempre remover capacidades em caso de falha.
- Sempre registrar PluginId nos logs.
- Sempre usar CancellationToken.
- Sempre aplicar timeout.
- Nunca permitir plugin derrubar aplicação.
- Nunca registrar capacidade não declarada.

---

# Casos de Erro

- Assembly ausente.
- Assembly inválido.
- EntryType inexistente.
- EntryType não implementa contrato.
- Falha no construtor.
- Falha no InitializeAsync.
- Timeout de inicialização.
- Permissão negada.
- Capacidade inválida.
- Falha ao registrar serviço.
- Falha no ShutdownAsync.

---

# Critérios de Aceite

- [x] Plugin Loader Service definido.
- [x] Estados de loading definidos.
- [x] PluginContext definido.
- [x] Fluxo de carregamento documentado.
- [x] Permissões verificadas antes de inicializar.
- [x] Falha segura documentada.
- [x] Shutdown documentado.
- [x] Timeout definido.
- [x] Diagnósticos definidos.
- [x] Nenhuma falha de plugin derruba aplicação.

---

# Testes

## Unitários

- Carregar plugin válido.
- Bloquear plugin sem permissão.
- EntryType inválido.
- InitializeAsync falha.
- Timeout de inicialização.
- Capacidade não declarada rejeitada.
- Shutdown executado.
- Shutdown com falha controlada.

## Integração

- Plugin metadata provider carregado.
- Plugin diagnostics carregado.
- Plugin falha sem derrubar app.
- Plugin desativado após erro crítico.
- Logs com PluginId.
- Capacidades removidas após unload.

---

# Dependências

- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F073-PluginDiscovery.md
- F075-PluginPermissions.md
- F079-PluginDiagnostics.md
- F084-PluginSecurity.md
- 007-Infrastructure.md

---

# Definition of Done

- Plugin Loading documentado.
- Serviço de carregamento especificado.
- Contexto isolado definido.
- Falha segura definida.
- Timeout definido.
- Testes definidos.
- Documentação atualizada.

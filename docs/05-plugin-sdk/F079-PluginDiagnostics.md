# F079 - Plugin Diagnostics

> Feature ID: F079
> Status: Implemented and Homologated
> Priority: High
> Epic: Plugin SDK

# Objetivo

Definir o sistema oficial de diagnóstico para plugins do WiiStudio, permitindo rastrear descoberta, manifesto, permissões, carregamento, execução, falhas, performance, uso de recursos e problemas de compatibilidade.

Plugin Diagnostics deve ajudar o usuário e o desenvolvedor a entender por que um plugin não carregou, falhou ou foi bloqueado.

---

# Requisitos Funcionais

## RF001
Registrar eventos de descoberta de plugin.

## RF002
Registrar falhas de manifesto.

## RF003
Registrar decisões de permissão.

## RF004
Registrar carregamento e descarregamento.

## RF005
Registrar falhas em runtime.

## RF006
Registrar métricas de performance.

## RF007
Exibir diagnóstico no Advanced Diagnostics.

## RF008
Permitir exportar relatório de plugin.

## RF009
Integrar com Logging Center.

## RF010
Redigir informações sensíveis dos logs.

---

# Eventos Diagnosticáveis

- Plugin encontrado.
- Manifesto lido.
- Manifesto inválido.
- SDK incompatível.
- Permissão concedida.
- Permissão negada.
- Assembly carregado.
- Inicialização iniciada.
- Inicialização concluída.
- Inicialização falhou.
- Capacidade registrada.
- Capacidade rejeitada.
- Plugin desativado.
- Plugin bloqueado.
- Plugin descarregado.
- Exceção em runtime.

---

# Modelo de Evento

```csharp
public sealed class PluginDiagnosticEvent
{
    public string PluginId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DiagnosticSeverity Severity { get; init; }
    public string MessageKey { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, string> Data { get; init; } =
        new Dictionary<string, string>();
}
```

---

# Severidades

```csharp
public enum DiagnosticSeverity
{
    Info,
    Success,
    Warning,
    Error,
    Critical
}
```

---

# Plugin Diagnostic Service

```csharp
public interface IPluginDiagnosticService
{
    void Record(PluginDiagnosticEvent diagnosticEvent);

    IReadOnlyList<PluginDiagnosticEvent> GetEvents(string pluginId);

    Task<PluginDiagnosticReport> BuildReportAsync(
        string pluginId,
        CancellationToken cancellationToken = default);
}
```

---

# Relatório de Plugin

```csharp
public sealed class PluginDiagnosticReport
{
    public string PluginId { get; init; } = string.Empty;
    public string PluginName { get; init; } = string.Empty;
    public string PluginVersion { get; init; } = string.Empty;
    public PluginState State { get; init; }
    public IReadOnlyList<PluginDiagnosticEvent> Events { get; init; } = [];
    public IReadOnlyList<string> Recommendations { get; init; } = [];
}
```

---

# Métricas

Registrar quando possível:

- tempo de discovery;
- tempo de validação;
- tempo de loading;
- tempo de InitializeAsync;
- tempo de ShutdownAsync;
- quantidade de falhas;
- quantidade de chamadas;
- latência de providers;
- uso de cache;
- timeouts.

---

# Integração com Logging Center

Todo diagnóstico relevante deve aparecer nos logs com:

- PluginId;
- PluginName;
- EventType;
- Severity;
- CorrelationId;
- mensagem amigável;
- detalhes técnicos quando seguro.

---

# Integração com Advanced Diagnostics

A tela de Advanced Diagnostics deve permitir:

- listar plugins;
- ver estado de cada plugin;
- ver eventos;
- filtrar por severidade;
- copiar detalhes;
- exportar relatório;
- abrir pasta do plugin quando seguro;
- desativar plugin problemático.

---

# Redação de Dados Sensíveis

Nunca registrar em claro:

- tokens;
- senhas;
- chaves de API;
- paths sensíveis quando não necessário;
- dados pessoais;
- URLs com credenciais;
- argumentos de processo com segredo.

Usar regras de `F088-LogRedaction.md`.

---

# Recomendações Automáticas

O relatório pode sugerir:

- atualizar plugin;
- verificar versão do SDK;
- revisar permissões;
- reinstalar plugin;
- remover duplicado;
- mover plugin da quarentena;
- verificar logs;
- abrir issue com autor.

---

# Regras para o Codex

- Sempre registrar PluginId.
- Sempre registrar falha de manifesto.
- Sempre registrar decisão de permissão.
- Sempre registrar falha de loading.
- Sempre capturar exceções.
- Sempre redigir dados sensíveis.
- Sempre permitir exportar relatório.
- Sempre integrar com Advanced Diagnostics.
- Nunca derrubar aplicação durante diagnóstico.
- Nunca registrar segredo em log.

---

# Casos de Erro

- Plugin sem ID.
- Evento sem severidade.
- Falha ao salvar diagnóstico.
- Log contém dado sensível.
- Relatório não gerado.
- Plugin removido durante análise.
- Diagnóstico muito grande.
- Falha ao exportar relatório.

---

# Critérios de Aceite

- [x] Eventos diagnosticáveis definidos.
- [x] Modelo de evento definido.
- [x] Serviço de diagnóstico definido.
- [x] Relatório de plugin definido.
- [x] Métricas definidas.
- [x] Integração com Logging Center definida.
- [x] Integração com Advanced Diagnostics definida.
- [x] Redação de dados sensíveis definida.
- [x] Recomendações automáticas previstas.

---

# Testes

## Unitários

- Registrar evento.
- Filtrar eventos por plugin.
- Gerar relatório.
- Redigir segredo.
- Registrar falha de manifesto.
- Registrar permissão negada.
- Registrar loading falho.
- Métrica calculada.

## Integração

- Plugin inválido aparece no diagnóstico.
- Plugin com permissão negada.
- Plugin falha em runtime.
- Relatório exportado.
- Logs exibem PluginId.
- Dados sensíveis redigidos.

---

# Dependências

- F029-LoggingCenter.md
- F030-AdvancedDiagnostics.md
- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F073-PluginDiscovery.md
- F074-PluginLoading.md
- F075-PluginPermissions.md
- F088-LogRedaction.md

---

# Definition of Done

- Plugin Diagnostics documentado.
- Eventos e relatórios definidos.
- Integrações com logs e diagnósticos definidas.
- Redação de dados sensíveis prevista.
- Testes definidos.
- Documentação atualizada.

# F113 - Task and Notification Center Integration

> Feature ID: F113  
> Status: Implemented and Homologated  
> Priority: High  
> Epic: Operations / UX Feedback  
> Area: Task Manager, Notification Center, Logging, Diagnostics, UI Shell

# Objetivo

Integrar operações reais do WiiStudio ao módulo de Tarefas e à Central de Notificações,
eliminando estados vazios como “0 tarefas” e “0 notificações” quando há operações em andamento
ou concluídas.

A feature deve tornar visível o histórico de operações, progresso, falhas, avisos e resultados.

---

# Problema Atual

A UI exibe “0 tarefas” e “0 notificações” em vários pontos, mesmo quando comandos e fluxos
existem ou estão sendo executados.

Isso prejudica:

- feedback ao usuário;
- diagnóstico;
- confiança na aplicação;
- rastreabilidade de operações longas.

---

# Resultado Esperado

Operações como:

- download de capas;
- sincronização de metadados;
- atualização GameTDB;
- download de ferramentas externas;
- instalação de policy;
- importação/conversão Wiimms;
- escrita USB;
- backup/restore;

devem aparecer em:

```text
Task Manager
Notification Center
History
Diagnostics
```

---

# Escopo

## Incluído

- criar modelo unificado de tarefas;
- criar notificações persistentes;
- histórico de operações;
- integração com Task Manager;
- integração com Notification Center;
- atualização da Status Bar;
- persistência básica;
- eventos internos;
- filtros por status;
- testes com fakes.

## Fora do Escopo Inicial

- sincronização em nuvem;
- notificações push do sistema operacional;
- telemetria remota;
- analytics.

---

# Modelo de Tarefa

```csharp
public sealed class AppTaskInfo
{
    public string TaskId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AppTaskStatus Status { get; init; }
    public double? ProgressPercentage { get; init; }
    public bool IsIndeterminate { get; init; }
    public bool CanCancel { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? CorrelationId { get; init; }
}
```

## AppTaskStatus

```csharp
public enum AppTaskStatus
{
    Pending,
    Running,
    WaitingForUser,
    Completed,
    Failed,
    Cancelled,
    TimedOut
}
```

---

# Modelo de Notificação

```csharp
public sealed class AppNotification
{
    public string NotificationId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public AppNotificationSeverity Severity { get; init; }
    public bool IsRead { get; init; }
    public bool IsPersistent { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? CorrelationId { get; init; }
}
```

## Severidade

```csharp
public enum AppNotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}
```

---

# Serviços

## ITaskCenterService

```csharp
public interface ITaskCenterService
{
    Task<AppTaskInfo> StartAsync(
        AppTaskStartRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateProgressAsync(
        string taskId,
        AppTaskProgress progress,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        string taskId,
        AppTaskCompletion completion,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        string taskId,
        CancellationToken cancellationToken = default);
}
```

---

## INotificationCenterService

```csharp
public interface INotificationCenterService
{
    Task PublishAsync(
        AppNotification notification,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        string notificationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppNotification>> GetRecentAsync(
        CancellationToken cancellationToken = default);
}
```

---

# Eventos Internos

Criar eventos:

```text
TaskStartedEvent
TaskProgressChangedEvent
TaskCompletedEvent
TaskFailedEvent
NotificationPublishedEvent
```

A UI deve reagir via binding/event aggregator.

---

# Integrações Obrigatórias

Integrar com features:

- F102 Cover Provider;
- F103 Metadata Provider;
- F108 External Tool Downloader;
- F109 Wiimms Operation Progress;
- F110 Safe USB Write Workflow;
- F111 GameTDB Update Source;
- F112 Application Update Provider.

---

# Persistência

Persistir histórico básico:

```text
tasks/history.json
notifications/history.json
```

ou SQLite quando `F114` estiver implementada.

Dados persistidos:

- últimas tarefas;
- últimas notificações;
- status;
- data;
- resumo;
- correlationId.

Limite recomendado:

```text
500 tarefas
1000 notificações
```

---

# UI

## Task Center

Mostrar:

- tarefas em andamento;
- tarefas concluídas;
- falhas;
- canceladas;
- progresso;
- botão cancelar quando suportado;
- detalhes.

## Notification Center

Mostrar:

- contador não lido;
- notificações recentes;
- marcar como lida;
- limpar lidas;
- abrir diagnóstico relacionado.

---

# Status Bar

Mostrar resumo:

```text
2 tarefas em andamento
Última operação: Download de capas concluído
```

---

# Tratamento de Erros

Erro em tarefa deve gerar notificação de erro.

Exemplo:

```text
Falha ao atualizar GameTDB. Veja o diagnóstico.
```

Sucesso importante deve gerar notificação de sucesso.

---

# Critérios de Aceite

- [x] Tarefas reais aparecem no Task Manager.
- [x] Notificações reais aparecem no Notification Center.
- [x] Contadores deixam de ficar sempre em zero.
- [x] Operações longas mostram progresso.
- [x] Operações finalizadas entram no histórico.
- [x] Falhas geram notificação persistente.
- [x] Cancelamento é refletido na UI.
- [x] Status Bar mostra resumo.
- [x] Testes cobrem serviços com fakes.

---

# Testes Esperados

- iniciar tarefa;
- atualizar progresso;
- concluir tarefa;
- falhar tarefa;
- cancelar tarefa;
- publicar notificação;
- marcar como lida;
- contador não lido correto;
- histórico limitado;
- integração com operação fake;
- correlationId preservado.

---

# Regras para o Codex

- Não deixar operação longa invisível.
- Sempre criar task para operação longa.
- Sempre publicar notificação de erro relevante.
- Sempre atualizar contadores.
- Não bloquear UI.
- Não usar estado global estático.
- Usar serviços da Application.
- Preparar persistência para SQLite/F114.

---

# Dependências

- `F025-NotificationCenter.md`
- `F026-TaskManager.md`
- `F037-StatusBar.md`
- `F057-ProgressIndicators.md`
- `F109-WiimmsOperationProgress.md`
- `F114-LibraryPersistenceWithSQLite.md`

---

# Definition of Done

- Serviços definidos.
- UI integrada.
- Histórico definido.
- Operações principais mapeadas.
- Testes com fakes definidos.

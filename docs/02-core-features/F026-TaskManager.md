# F026 - Task Manager

> Feature ID: F026 Status: Implemented and Homologated Priority: Critical
> Epic: Settings & User Experience

# Objetivo

Centralizar o gerenciamento de todas as tarefas de longa duração do
WiiStudio, como importações, conversões, downloads, verificações e
reparos, oferecendo controle unificado de execução, progresso, pausa,
retomada e cancelamento.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Registrar automaticamente tarefas iniciadas.

## RF002

Exibir progresso individual e global.

## RF003

Permitir pausar, retomar e cancelar tarefas compatíveis.

## RF004

Executar múltiplas tarefas respeitando limites de concorrência.

## RF005

Persistir tarefas recuperáveis entre reinicializações.

## RF006

Exibir histórico de tarefas concluídas.

------------------------------------------------------------------------

# Tipos de Tarefa

-   Importação
-   Conversão
-   Download
-   Verificação
-   Reparo
-   Exportação
-   Atualização

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Iniciar Operação
      │
Task Manager
      │
Fila de Execução
      │
Progresso
      │
Conclusão / Erro
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
Application
     │
ITaskManager
     │
TaskScheduler
     │
ExecutionQueue
     │
Worker
     │
Notification Center
```

------------------------------------------------------------------------

# Máquina de Estados

-   Queued
-   Preparing
-   Running
-   Paused
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface ITaskManager
{
    Task<Guid> EnqueueAsync(TaskRequest request, CancellationToken cancellationToken);
    Task PauseAsync(Guid taskId, CancellationToken cancellationToken);
    Task ResumeAsync(Guid taskId, CancellationToken cancellationToken);
    Task CancelAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskInfo>> GetActiveTasksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskInfo>> GetHistoryAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Prioridade configurável.
-   Limite de concorrência configurável.
-   Tarefas independentes podem executar em paralelo.
-   Histórico com retenção configurável.

------------------------------------------------------------------------

# Eventos

-   TaskQueued
-   TaskStarted
-   TaskProgressChanged
-   TaskPaused
-   TaskResumed
-   TaskCompleted
-   TaskFailed
-   TaskCancelled

------------------------------------------------------------------------

# Interface

-   Lista de tarefas ativas
-   Histórico
-   Barra de progresso geral
-   ETA
-   Velocidade
-   Filtros por estado
-   Ações rápidas

------------------------------------------------------------------------

# Casos de Erro

-   Falha inesperada.
-   Dependência indisponível.
-   Persistência falhou.
-   Cancelamento.

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Fila funcional.
-   [x] Pausa e retomada.
-   [x] Cancelamento.
-   [x] Persistência opcional.
-   [x] Histórico disponível.

------------------------------------------------------------------------

# Testes

## Unitários

-   Enfileiramento.
-   Agendamento.
-   Cancelamento.
-   Persistência.
-   Concorrência.

## Integração

-   Conversões em lote.
-   Downloads simultâneos.
-   Reinício da aplicação.
-   Recuperação de tarefas.

------------------------------------------------------------------------

# Dependências

-   F008-BatchConversion.md
-   F025-NotificationCenter.md
-   F023-SettingsManager.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Scheduler validado.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

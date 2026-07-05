# F008 - Batch Conversion

> Feature ID: F008\
> Status: Ready for Development\
> Priority: Critical\
> Epic: Conversion

# Objetivo

Permitir a conversão em lote de múltiplos jogos utilizando a Conversion
Engine, com gerenciamento de fila, controle de concorrência, retomada,
cancelamento e monitoramento em tempo real.

------------------------------------------------------------------------

# Visão Geral

A conversão em lote deve ser capaz de processar centenas de jogos sem
bloquear a interface e mantendo a consistência dos dados.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Adicionar um ou mais jogos à fila.

## RF002

Permitir reordenação da fila (drag & drop).

## RF003

Pausar e retomar a fila.

## RF004

Cancelar um item ou toda a fila.

## RF005

Executar automaticamente o próximo item após conclusão.

## RF006

Atualizar a biblioteca quando uma conversão terminar.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   UI responsiva durante toda a operação.
-   Persistência opcional da fila entre reinicializações.
-   Limite configurável de jobs paralelos.
-   Recuperação de falhas sem interromper toda a fila.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar Jogos
      │
      ▼
Configurar Conversão
      │
      ▼
Adicionar à Fila
      │
      ▼
Executar
      │
      ▼
Monitorar
      │
      ▼
Concluir
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
BatchConversionView
        │
BatchConversionViewModel
        │
IBatchConversionService
        │
ConversionQueueManager
        │
IConversionEngine
        │
Providers
```

------------------------------------------------------------------------

# Máquina de Estados

Fila:

-   Idle
-   Running
-   Paused
-   Completed
-   Failed
-   Cancelled

Job:

-   Queued
-   Preparing
-   Running
-   Verifying
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Modelo de Dados

## BatchJob

Campos mínimos:

-   JobId
-   GameId
-   SourcePath
-   TargetFormat
-   Status
-   Progress
-   StartedAt
-   FinishedAt
-   ErrorMessage

------------------------------------------------------------------------

# Regras de Negócio

-   Um mesmo jogo não pode entrar duas vezes na fila.
-   Escritas para o mesmo dispositivo devem ser serializadas.
-   Conversões independentes podem ocorrer em paralelo.
-   Falha de um job não interrompe os demais (configurável).

------------------------------------------------------------------------

# Eventos

-   BatchStarted
-   BatchPaused
-   BatchResumed
-   BatchCompleted
-   JobQueued
-   JobStarted
-   JobProgressChanged
-   JobCompleted
-   JobFailed
-   BatchCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IBatchConversionService
{
    Task<Guid> EnqueueAsync(
        BatchConversionRequest request,
        CancellationToken cancellationToken);

    Task PauseAsync();

    Task ResumeAsync();

    Task CancelJobAsync(Guid jobId);

    Task CancelAllAsync();
}
```

------------------------------------------------------------------------

# Interface do Usuário

A tela deverá apresentar:

-   Lista da fila
-   Barra de progresso geral
-   Barra individual por job
-   Velocidade média
-   ETA
-   Botões:
    -   Pausar
    -   Retomar
    -   Cancelar Item
    -   Cancelar Tudo
    -   Limpar Concluídos

------------------------------------------------------------------------

# Casos de Erro

-   Provider indisponível.
-   Arquivo corrompido.
-   Destino desconectado.
-   Espaço insuficiente.
-   Falha de verificação.

Cada erro deve ser registrado sem interromper a fila inteira (salvo
configuração).

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Conversão em lote funcional.
-   [ ] Fila persistente (quando habilitada).
-   [ ] Pausar/Retomar funcionando.
-   [ ] Cancelamento individual e global.
-   [ ] Atualização em tempo real.
-   [ ] Recuperação de falhas.

------------------------------------------------------------------------

# Testes

## Unitários

-   Inserção na fila.
-   Reordenação.
-   Paralelismo.
-   Cancelamento.
-   Persistência.

## Integração

-   Lote com 100+ jogos.
-   Falha de um item.
-   Pausa e retomada.
-   Remoção do dispositivo durante execução.
-   Reinício da aplicação com fila persistida.

------------------------------------------------------------------------

# Dependências

-   018-ConversionEngine.md
-   019-FeatureDevelopmentGuide.md
-   F007-ConvertGame.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Queue Manager implementado.
-   Testes aprovados.
-   Logs estruturados.
-   Performance validada.
-   Documentação atualizada.

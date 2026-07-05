# 018 - Conversion Engine Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a Engine de Conversão do WiiStudio, responsável por orquestrar
todas as conversões de arquivos de forma segura, escalável, observável e
cancelável.

------------------------------------------------------------------------

# Escopo

Conversões suportadas:

  Origem   Destino   Provider
  -------- --------- -----------------------
  ISO      WBFS      Wiimms
  WBFS     ISO       Wiimms
  RVZ      WBFS      Provider RVZ + Wiimms
  RVZ      ISO       Provider RVZ
  NKIT     ISO       NKit
  NKIT     WBFS      NKit → Wiimms

------------------------------------------------------------------------

# Arquitetura

``` text
UI
 │
Application
 │
IConversionEngine
 │
ConversionPipeline
 │
ConversionProviders
 ├── Wiimms
 ├── NKit
 └── Future Providers
```

------------------------------------------------------------------------

# Pipeline

Cada conversão percorre as etapas:

``` text
Queued
 ↓
Validating
 ↓
Preparing
 ↓
Converting
 ↓
Verifying
 ↓
Completed
  └── Failed
  └── Cancelled
```

------------------------------------------------------------------------

# Queue

Características:

-   FIFO por padrão
-   Reordenação manual
-   Pausar fila
-   Retomar fila
-   Cancelar item
-   Cancelar todos

Apenas um job por dispositivo de destino.

------------------------------------------------------------------------

# ConversionJob

Campos mínimos:

-   Id
-   SourceFile
-   Destination
-   SourceFormat
-   TargetFormat
-   Status
-   Progress
-   Speed
-   StartedAt
-   FinishedAt
-   ErrorMessage

------------------------------------------------------------------------

# Validação

Antes da conversão:

-   Arquivo existe
-   Formato suportado
-   Espaço suficiente
-   Provider disponível
-   Destino gravável

------------------------------------------------------------------------

# Paralelismo

Regras:

-   Conversões independentes podem executar em paralelo.
-   Escritas para o mesmo dispositivo devem ser serializadas.
-   Configurável pelo usuário.

------------------------------------------------------------------------

# Monitoramento

Exibir em tempo real:

-   \%
-   Arquivo atual
-   Velocidade
-   ETA
-   Logs recentes

------------------------------------------------------------------------

# Rollback

Em falhas:

-   Remover temporários
-   Liberar locks
-   Atualizar histórico
-   Registrar log estruturado

------------------------------------------------------------------------

# Interfaces

``` csharp
public interface IConversionEngine
{
    Task<Guid> QueueAsync(
        ConversionRequest request,
        CancellationToken cancellationToken);

    Task CancelAsync(Guid jobId);

    Task PauseAsync();

    Task ResumeAsync();
}
```

------------------------------------------------------------------------

# Eventos

-   ConversionQueued
-   ConversionStarted
-   ConversionProgressChanged
-   ConversionCompleted
-   ConversionFailed
-   ConversionCancelled

------------------------------------------------------------------------

# Logging

Registrar:

-   provider
-   duração
-   tamanho
-   throughput
-   exit code
-   exceções

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca converter diretamente pela UI.
-   Toda conversão passa pela Engine.
-   Providers são intercambiáveis.
-   Toda operação suporta CancellationToken.
-   Nenhum estado é perdido após falha.

------------------------------------------------------------------------

# Critérios de Aceite

-   Pipeline documentado.
-   Fila funcional.
-   Rollback definido.
-   Monitoramento em tempo real.
-   Providers desacoplados.

------------------------------------------------------------------------

# Checklist

-   [ ] Pipeline
-   [ ] Queue
-   [ ] ConversionJob
-   [ ] Eventos
-   [ ] Logging
-   [ ] Rollback
-   [ ] Interfaces

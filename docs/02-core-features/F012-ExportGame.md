# F012 - Export Game

> Feature ID: F012\
> Status: Implemented and Homologated\
> Priority: High\
> Epic: Library Management

# Objetivo

Exportar jogos da biblioteca para diferentes formatos e destinos,
preservando a integridade dos dados e utilizando a Conversion Engine e
os Providers oficiais.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exportar um ou mais jogos.

## RF002

Permitir selecionar o formato de saída.

## RF003

Permitir escolher o diretório de destino.

## RF004

Detectar conflitos de arquivos existentes.

## RF005

Validar o resultado da exportação.

## RF006

Registrar a operação no histórico.

------------------------------------------------------------------------

# Formatos de Exportação

  Origem   Destino
  -------- ---------
  WBFS     ISO
  WBFS     WBFS
  ISO      ISO
  ISO      WBFS
  RVZ\*    ISO
  NKIT\*   ISO

(\*) Quando suportado pelos Providers configurados.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
Escolher formato
      │
Escolher destino
      │
Validar
      │
Exportar
      │
Verificar
      │
Concluir
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
Library
   │
ExportViewModel
   │
IExportGameService
   │
IConversionEngine
   │
Providers
   │
ProcessRunner
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Preparing
-   Validating
-   Exporting
-   Verifying
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Validações

-   Jogo existente.
-   Destino gravável.
-   Espaço suficiente.
-   Provider disponível.
-   Conflito de arquivos.

------------------------------------------------------------------------

# Estratégia para Conflitos

Quando o arquivo existir:

-   Sobrescrever
-   Renomear automaticamente
-   Ignorar
-   Cancelar operação

Aplicável ao lote inteiro ou individualmente.

------------------------------------------------------------------------

# Eventos

-   ExportStarted
-   ExportProgressChanged
-   ExportCompleted
-   ExportFailed
-   ExportCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IExportGameService
{
    Task<ExportPreview> AnalyzeAsync(
        ExportRequest request,
        CancellationToken cancellationToken);

    Task<ExportResult> ExportAsync(
        ExportRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Interface do Usuário

Exibir:

-   Lista de jogos
-   Formato de destino
-   Pasta de saída
-   Espaço necessário
-   Barra de progresso
-   Tempo estimado

------------------------------------------------------------------------

# Casos de Erro

-   Destino indisponível.
-   Provider ausente.
-   Falha de gravação.
-   Arquivo bloqueado.
-   Cancelamento.

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Exportação individual.
-   [x] Exportação em lote.
-   [x] Resolução de conflitos.
-   [x] Validação pós-exportação.
-   [x] Histórico atualizado.
-   [x] Cancelamento suportado.

------------------------------------------------------------------------

# Testes

## Unitários

-   Validação.
-   Seleção do provider.
-   Conflitos.
-   Cancelamento.

## Integração

-   Exportar para ISO.
-   Exportar para WBFS.
-   Exportar múltiplos jogos.
-   Espaço insuficiente.
-   Destino removido durante a operação.

------------------------------------------------------------------------

# Dependências

-   018-ConversionEngine.md
-   012-WiimmsIntegration.md
-   F007-ConvertGame.md
-   F008-BatchConversion.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Exportação validada.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# F029 - Logging Center

> Feature ID: F029 Status: Implemented and Homologated Priority: Critical
> Epic: Settings & User Experience

# Objetivo

Centralizar todos os logs do WiiStudio em um único subsistema,
permitindo rastreabilidade, diagnóstico, auditoria e exportação de
informações para suporte técnico e depuração.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Registrar logs estruturados de todos os módulos.

## RF002

Exibir logs em tempo real.

## RF003

Filtrar por nível, origem, período e categoria.

## RF004

Pesquisar mensagens.

## RF005

Exportar logs.

## RF006

Limpar logs por política configurável.

------------------------------------------------------------------------

# Fontes de Log

-   Application
-   UI
-   Conversion Engine
-   Wiimms
-   NKit
-   RVZ
-   Downloads
-   Metadata
-   Atualizações
-   Backup & Restore

------------------------------------------------------------------------

# Níveis

-   Trace
-   Debug
-   Information
-   Warning
-   Error
-   Critical

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Abrir Logging Center
        │
Selecionar filtros
        │
Visualizar eventos
        │
Pesquisar / Exportar
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ILogger
    │
ILoggingCenter
    │
Structured Log Store
    │
SQLite / Arquivos
    │
Presentation
```

------------------------------------------------------------------------

# Máquina de Estados

-   Initializing
-   Collecting
-   Filtering
-   Exporting
-   Completed
-   Failed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface ILoggingCenter
{
    Task<IReadOnlyList<LogEntry>> QueryAsync(
        LogQuery query,
        CancellationToken cancellationToken);

    Task ExportAsync(
        LogExportOptions options,
        CancellationToken cancellationToken);

    Task ClearAsync(
        LogCleanupOptions options,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Logs estruturados em JSON.
-   Correlação por OperationId.
-   Rotação automática de arquivos.
-   Retenção configurável.
-   Dados sensíveis mascarados.

------------------------------------------------------------------------

# Eventos

-   LogRecorded
-   LogExported
-   LogCleanupCompleted
-   RetentionPolicyExecuted

------------------------------------------------------------------------

# Interface

-   Visualização em tempo real
-   Pesquisa
-   Filtros
-   Exportar
-   Copiar linha
-   Abrir detalhes
-   Estatísticas

------------------------------------------------------------------------

# Casos de Erro

-   Armazenamento indisponível.
-   Exportação falhou.
-   Arquivo bloqueado.
-   Espaço insuficiente.

Todos os erros também devem ser registrados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Logs centralizados.
-   [x] Pesquisa rápida.
-   [x] Exportação.
-   [x] Rotação automática.
-   [x] Mascaramento de dados.

------------------------------------------------------------------------

# Testes

## Unitários

-   Escrita.
-   Consulta.
-   Exportação.
-   Limpeza.
-   Retenção.

## Integração

-   Conversão em lote.
-   Downloads.
-   Atualizações.
-   Backup.
-   Grande volume (\>1 milhão de registros).

------------------------------------------------------------------------

# Dependências

-   F025-NotificationCenter.md
-   F026-TaskManager.md
-   F027-ApplicationUpdateManager.md
-   F028-BackupAndRestore.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Visualizador funcional.
-   Exportação validada.
-   Testes aprovados.
-   Documentação atualizada.

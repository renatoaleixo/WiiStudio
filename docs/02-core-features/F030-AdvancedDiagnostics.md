# F030 - Advanced Diagnostics

> Feature ID: F030 Status: Ready for Development Priority: Critical
> Epic: Settings & User Experience

# Objetivo

Executar uma análise completa do ambiente do WiiStudio, verificando
dependências, configurações, dispositivos, permissões e integridade do
sistema para facilitar o suporte técnico, a resolução de problemas e a
preparação do ambiente.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Executar diagnóstico completo do ambiente.

## RF002

Verificar dependências instaladas.

## RF003

Verificar dispositivos conectados.

## RF004

Verificar permissões do sistema.

## RF005

Executar testes de leitura e escrita.

## RF006

Gerar relatório completo para suporte.

------------------------------------------------------------------------

# Componentes Verificados

## Sistema

-   Sistema Operacional
-   Arquitetura
-   Memória
-   CPU
-   Espaço em disco

## Dependências

-   .NET Runtime
-   Wiimms ISO Tools
-   NKit
-   RVZ Provider
-   Git
-   SQLite

## WiiStudio

-   Configurações
-   Perfis
-   Banco local
-   Cache
-   Assets
-   Logs

## Dispositivos

-   USB
-   HD WBFS
-   FAT32
-   NTFS
-   exFAT

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Abrir Diagnóstico
        │
Executar Verificação
        │
Analisar Resultado
        │
Exportar Relatório
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
DiagnosticsView
       │
DiagnosticsViewModel
       │
IDiagnosticsService
       │
SystemCheck
DependencyCheck
StorageCheck
ConfigurationCheck
ReportGenerator
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Scanning
-   Validating
-   GeneratingReport
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IDiagnosticsService
{
    Task<DiagnosticsReport> RunAsync(
        DiagnosticsOptions options,
        CancellationToken cancellationToken);

    Task ExportAsync(
        DiagnosticsReport report,
        string outputFile,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Executar verificações de forma assíncrona.
-   Continuar diagnóstico mesmo quando um teste falhar.
-   Classificar resultados por severidade.
-   Mascarar informações sensíveis nos relatórios.

------------------------------------------------------------------------

# Níveis de Severidade

-   Information
-   Warning
-   Error
-   Critical

------------------------------------------------------------------------

# Eventos

-   DiagnosticsStarted
-   DiagnosticsProgressChanged
-   DiagnosticsCompleted
-   DiagnosticsFailed
-   ReportExported

------------------------------------------------------------------------

# Interface

Tela "Diagnóstico":

-   Resumo geral
-   Lista de verificações
-   Status por item
-   Filtros por severidade
-   Barra de progresso
-   Botão Exportar Relatório
-   Botão Copiar Informações

------------------------------------------------------------------------

# Casos de Erro

-   Dependência ausente.
-   Permissão insuficiente.
-   Dispositivo inacessível.
-   Banco corrompido.
-   Falha ao gerar relatório.

Todos os erros devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Diagnóstico completo do ambiente.
-   [ ] Continuidade após falhas.
-   [ ] Relatório exportável.
-   [ ] Classificação por severidade.
-   [ ] Informações mascaradas quando necessário.

------------------------------------------------------------------------

# Testes

## Unitários

-   Verificação de dependências.
-   Verificação de permissões.
-   Geração de relatório.
-   Exportação.

## Integração

-   Ambiente completo.
-   Dependências ausentes.
-   Dispositivo desconectado.
-   Banco corrompido.
-   Geração de pacote para suporte.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F025-NotificationCenter.md
-   F026-TaskManager.md
-   F029-LoggingCenter.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Relatório funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

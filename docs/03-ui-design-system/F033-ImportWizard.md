# F033 - Import Wizard

> Feature ID: F033 Status: Implemented and Homologated Priority: Critical
> Epic: User Interface

# Objetivo

Definir um assistente visual (Wizard) para importação de jogos em todos
os formatos suportados pelo WiiStudio, guiando o usuário desde a seleção
dos arquivos até a conclusão da importação com validações,
pré-visualização e acompanhamento em tempo real.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais arquivos (ISO, WBFS, RVZ e NKit).

## RF002

Detectar automaticamente o formato.

## RF003

Validar arquivos antes da importação.

## RF004

Selecionar o dispositivo de destino.

## RF005

Exibir resumo antes da execução.

## RF006

Acompanhar progresso e resultado da importação.

------------------------------------------------------------------------

# Etapas do Wizard

1.  Seleção de Arquivos
2.  Validação
3.  Seleção do Destino
4.  Opções de Importação
5.  Resumo
6.  Execução
7.  Conclusão

------------------------------------------------------------------------

# Layout

``` text
+-------------------------------------------------------------+
| Passos                                                      |
+----------------------+--------------------------------------+
| 1 Arquivos           |                                      |
| 2 Validação          |          Conteúdo da Etapa           |
| 3 Destino            |                                      |
| 4 Opções             |                                      |
| 5 Resumo             |                                      |
| 6 Execução           |                                      |
+----------------------+--------------------------------------+
| Voltar | Avançar | Cancelar                                |
+-------------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ImportWizardView
        │
ImportWizardViewModel
        │
IImportWizardService
        │
IIsoImportService
IWbfsImportService
IRvzImportService
INKitImportService
```

------------------------------------------------------------------------

# Máquina de Estados

-   SelectingFiles
-   Validating
-   SelectingTarget
-   Reviewing
-   Importing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IImportWizardService
{
    Task<ImportPreview> AnalyzeAsync(
        IReadOnlyList<string> files,
        CancellationToken cancellationToken);

    Task<ImportResult> ExecuteAsync(
        ImportWizardRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Detectar automaticamente o formato.
-   Impedir avanço com erros críticos.
-   Permitir salvar opções padrão.
-   Exibir conflitos antes da execução.

------------------------------------------------------------------------

# Eventos

-   WizardStarted
-   FilesValidated
-   DestinationSelected
-   ImportStarted
-   ImportProgressChanged
-   ImportCompleted
-   WizardCancelled

------------------------------------------------------------------------

# Casos de Erro

-   Arquivo inválido.
-   Espaço insuficiente.
-   Dispositivo removido.
-   Formato não suportado.
-   Falha na importação.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Fluxo guiado completo.
-   [x] Validação automática.
-   [x] Resumo antes da execução.
-   [x] Progresso em tempo real.
-   [x] Integração com todos os importadores.

------------------------------------------------------------------------

# Testes

## Unitários

-   Navegação entre etapas.
-   Validação.
-   Resumo.
-   Cancelamento.

## Integração

-   Importação ISO.
-   Importação WBFS.
-   Importação RVZ.
-   Importação NKit.
-   Importação em lote.

------------------------------------------------------------------------

# Dependências

-   F003-ImportIso.md
-   F004-ImportWbfs.md
-   F005-ImportRvz.md
-   F006-ImportNkit.md
-   F031-MainWindow.md

------------------------------------------------------------------------

# Definition of Done

-   Wizard implementado.
-   Navegação validada.
-   Testes aprovados.
-   Documentação atualizada.

# F034 - Conversion Wizard

> Feature ID: F034 Status: Ready for Development Priority: Critical
> Epic: User Interface

# Objetivo

Fornecer um assistente visual para conversão de jogos entre todos os
formatos suportados pelo WiiStudio, guiando o usuário desde a seleção da
origem até a conclusão da conversão, utilizando automaticamente a
Conversion Engine.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais jogos da biblioteca ou arquivos locais.

## RF002

Detectar automaticamente o formato de origem.

## RF003

Permitir selecionar o formato de destino.

## RF004

Exibir pipeline de conversão escolhido.

## RF005

Estimar tempo e espaço necessário.

## RF006

Executar a conversão com progresso em tempo real.

------------------------------------------------------------------------

# Formatos Suportados

-   ISO → WBFS
-   WBFS → ISO
-   RVZ → ISO
-   RVZ → WBFS
-   NKit → ISO
-   NKit → WBFS

------------------------------------------------------------------------

# Etapas do Wizard

1.  Seleção dos Jogos
2.  Análise
3.  Formato de Destino
4.  Opções Avançadas
5.  Resumo
6.  Conversão
7.  Conclusão

------------------------------------------------------------------------

# Layout

``` text
+-------------------------------------------------------------+
| Etapas                                                      |
+----------------------+--------------------------------------+
| Origem               |                                      |
| Análise              |        Conteúdo da Etapa             |
| Destino              |                                      |
| Opções               |                                      |
| Resumo               |                                      |
| Conversão            |                                      |
+----------------------+--------------------------------------+
| Voltar | Avançar | Cancelar                                |
+-------------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ConversionWizardView
        │
ConversionWizardViewModel
        │
IConversionWizardService
        │
IGameConversionService
        │
IConversionEngine
        │
Providers
```

------------------------------------------------------------------------

# Máquina de Estados

-   SelectingSource
-   Analyzing
-   SelectingTarget
-   Configuring
-   Reviewing
-   Converting
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IConversionWizardService
{
    Task<ConversionPreview> AnalyzeAsync(
        ConversionWizardRequest request,
        CancellationToken cancellationToken);

    Task<ConversionResult> ExecuteAsync(
        ConversionWizardRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Selecionar automaticamente o melhor pipeline.
-   Impedir conversões incompatíveis.
-   Validar espaço disponível.
-   Permitir salvar opções padrão.
-   Exibir conflitos antes da execução.

------------------------------------------------------------------------

# Eventos

-   WizardStarted
-   SourceAnalyzed
-   TargetSelected
-   ConversionStarted
-   ConversionProgressChanged
-   ConversionCompleted
-   WizardCancelled

------------------------------------------------------------------------

# Casos de Erro

-   Formato incompatível.
-   Provider indisponível.
-   Espaço insuficiente.
-   Arquivo corrompido.
-   Cancelamento do usuário.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Fluxo guiado completo.
-   [ ] Pipeline selecionado automaticamente.
-   [ ] Estimativa de tempo e espaço.
-   [ ] Conversão em tempo real.
-   [ ] Integração completa com Conversion Engine.

------------------------------------------------------------------------

# Testes

## Unitários

-   Navegação entre etapas.
-   Seleção de pipeline.
-   Validações.
-   Cancelamento.

## Integração

-   ISO → WBFS
-   WBFS → ISO
-   RVZ → WBFS
-   NKit → ISO
-   Conversão em lote.

------------------------------------------------------------------------

# Dependências

-   F007-ConvertGame.md
-   F008-BatchConversion.md
-   F031-MainWindow.md
-   F033-ImportWizard.md

------------------------------------------------------------------------

# Definition of Done

-   Wizard implementado.
-   Pipeline integrado.
-   Testes aprovados.
-   Documentação atualizada.

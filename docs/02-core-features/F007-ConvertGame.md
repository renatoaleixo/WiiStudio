# F007 - Convert Game

> Feature ID: F007\
> Status: Implemented and Homologated\
> Priority: Critical\
> Epic: Conversion

# Objetivo

Permitir a conversão de um único jogo entre formatos suportados pelo
WiiStudio, utilizando a Conversion Engine e os Providers apropriados,
mantendo integridade, rastreabilidade e excelente experiência do
usuário.

------------------------------------------------------------------------

# Formatos Suportados

  Origem   Destino   Pipeline
  -------- --------- ---------------------
  ISO      WBFS      Wiimms
  WBFS     ISO       Wiimms
  RVZ      ISO       RVZ Provider
  RVZ      WBFS      RVZ → ISO → Wiimms
  NKIT     ISO       NKit
  NKIT     WBFS      NKit → ISO → Wiimms

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um jogo da biblioteca ou um arquivo local.

## RF002

Permitir escolher o formato de destino.

## RF003

Detectar automaticamente o pipeline necessário.

## RF004

Exibir estimativa de espaço e tempo.

## RF005

Executar a conversão.

## RF006

Atualizar a biblioteca quando aplicável.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar Jogo
      │
Escolher Formato
      │
Validar
      │
Converter
      │
Verificar Resultado
      │
Concluir
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
View
  │
ConversionViewModel
  │
IConversionEngine
  │
PipelineResolver
  │
Provider(s)
  │
ProcessRunner
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Preparing
-   Validating
-   Running
-   Verifying
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Regras de Negócio

-   Nunca sobrescrever arquivo existente sem confirmação.
-   Validar espaço antes de iniciar.
-   Toda conversão deve gerar histórico.
-   Preservar arquivo original por padrão.

------------------------------------------------------------------------

# Eventos

-   ConversionRequested
-   ConversionStarted
-   ConversionProgressChanged
-   ConversionVerified
-   ConversionCompleted
-   ConversionFailed
-   ConversionCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IGameConversionService
{
    Task<ConversionPreview> AnalyzeAsync(
        ConversionRequest request,
        CancellationToken cancellationToken);

    Task<ConversionResult> ConvertAsync(
        ConversionRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Casos de Erro

-   Formato não suportado.
-   Provider indisponível.
-   Espaço insuficiente.
-   Arquivo de origem inválido.
-   Falha de verificação pós-conversão.
-   Cancelamento do usuário.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Conversão individual funcional.
-   [x] Pipeline escolhido automaticamente.
-   [x] Progresso em tempo real.
-   [x] Histórico registrado.
-   [x] Validação pós-conversão.
-   [x] Cancelamento suportado.

------------------------------------------------------------------------

# Testes

## Unitários

-   Resolução de pipeline.
-   Validação.
-   Seleção de provider.
-   Cancelamento.

## Integração

-   ISO → WBFS
-   WBFS → ISO
-   RVZ → WBFS
-   NKIT → WBFS
-   Falha durante conversão.

------------------------------------------------------------------------

# Dependências

-   018-ConversionEngine.md
-   012-WiimmsIntegration.md
-   013-NKitLegacy.md
-   F003-ImportIso.md
-   F005-ImportRvz.md
-   F006-ImportNkit.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Pipeline funcional.
-   Testes aprovados.
-   Histórico registrado.
-   Documentação atualizada.

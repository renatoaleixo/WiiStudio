# F003 - Import ISO

> Feature ID: F003\
> Status: Implemented and Homologated\
> Priority: Critical\
> Epic: Import

# Objetivo

Importar jogos no formato ISO para dispositivos Wii suportados de forma
segura, validando o arquivo antes da gravação e utilizando a Conversion
Engine quando necessário.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais arquivos ISO.

## RF002

Validar integridade básica do arquivo.

## RF003

Detectar automaticamente o dispositivo de destino.

## RF004

Evitar importação de jogos duplicados.

## RF005

Adicionar o jogo utilizando o Wiimms ISO Tools.

## RF006

Atualizar automaticamente a biblioteca após a conclusão.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Operação assíncrona.
-   Suporte a cancelamento.
-   Não bloquear a UI.
-   Barra de progresso em tempo real.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar ISO
      │
      ▼
Validação
      │
      ▼
Escolher destino
      │
      ▼
Resumo
      │
      ▼
Importação
      │
      ▼
Biblioteca Atualizada
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ImportWizard
      │
Application
      │
IConversionEngine
      │
IWiimmsService
      │
ProcessRunner
      │
wit copy
      │
Refresh Library
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   SelectingFiles
-   Validating
-   Ready
-   Importing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Validações

Antes da importação verificar:

-   Arquivo existente
-   Extensão suportada
-   Espaço livre
-   Duplicidade
-   Dispositivo disponível
-   Permissão de escrita

------------------------------------------------------------------------

# Eventos

-   IsoImportStarted
-   IsoValidated
-   IsoImportProgressChanged
-   IsoImported
-   IsoImportFailed
-   IsoImportCancelled

------------------------------------------------------------------------

# Contratos

``` csharp
public interface IIsoImportService
{
    Task<ImportPreview> AnalyzeAsync(
        IReadOnlyList<string> files,
        CancellationToken cancellationToken);

    Task<ImportResult> ImportAsync(
        ImportRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Casos de Erro

-   ISO inválida
-   Espaço insuficiente
-   Dispositivo removido
-   Jogo duplicado
-   Falha do Wiimms

Todas as falhas devem registrar log estruturado e manter a consistência
da biblioteca.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Importa uma ou várias ISOs.
-   [x] Detecta duplicidade.
-   [x] Atualiza a biblioteca automaticamente.
-   [x] Permite cancelamento.
-   [x] Exibe progresso em tempo real.

------------------------------------------------------------------------

# Testes

## Unitários

-   Validação de arquivo.
-   Detecção de duplicidade.
-   Cancelamento.
-   Mapeamento de eventos.

## Integração

-   Importar ISO válida.
-   Importar múltiplas ISOs.
-   Espaço insuficiente.
-   Dispositivo removido durante a operação.

------------------------------------------------------------------------

# Dependências

-   012-WiimmsIntegration.md
-   017-ImportWizard.md
-   018-ConversionEngine.md
-   F001-DetectUsbDrive.md
-   F002-ScanGameLibrary.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Testes aprovados.
-   Biblioteca atualizada automaticamente.
-   Logs implementados.
-   Documentação revisada.

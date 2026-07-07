# F004 - Import WBFS

> Feature ID: F004\
> Status: Ready for Development\
> Priority: Critical\
> Epic: Import

# Objetivo

Importar jogos no formato `.wbfs` para dispositivos Wii suportados,
preservando compatibilidade com HD WBFS legado e dispositivos modernos
com pasta `wbfs/`.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais arquivos `.wbfs`.

## RF002

Validar se o arquivo é reconhecido pelo Wiimms.

## RF003

Detectar Game ID, título, região e tamanho.

## RF004

Verificar duplicidade no dispositivo de destino.

## RF005

Copiar o jogo para o destino selecionado.

## RF006

Atualizar a biblioteca após a importação.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Operação assíncrona.
-   Suporte a cancelamento.
-   Progresso em tempo real.
-   Segurança contra sobrescrita acidental.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar WBFS
      │
      ▼
Validar Arquivo
      │
      ▼
Selecionar Destino
      │
      ▼
Verificar Duplicidade
      │
      ▼
Importar
      │
      ▼
Atualizar Biblioteca
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ImportWizard
      │
IIWbfsImportService
      │
IWiimmsService
      │
ProcessRunner
      │
wit copy / wwt add
      │
Library Refresh
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   SelectingFiles
-   Validating
-   CheckingDuplicates
-   Importing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Validações

Antes da importação:

-   Arquivo existe.
-   Extensão `.wbfs`.
-   Wiimms reconhece o arquivo.
-   Dispositivo de destino está disponível.
-   Espaço suficiente.
-   Jogo não existe no destino.

------------------------------------------------------------------------

# Eventos

-   WbfsImportStarted
-   WbfsValidated
-   WbfsDuplicateFound
-   WbfsImportProgressChanged
-   WbfsImported
-   WbfsImportFailed
-   WbfsImportCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IWbfsImportService
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

-   Arquivo WBFS inválido.
-   Jogo duplicado.
-   Espaço insuficiente.
-   Destino desconectado.
-   Falha do Wiimms.
-   Permissão negada.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Importa um ou múltiplos arquivos `.wbfs`.
-   [ ] Detecta duplicidade antes de gravar.
-   [ ] Não sobrescreve jogos sem confirmação.
-   [ ] Atualiza biblioteca automaticamente.
-   [ ] Permite cancelamento.
-   [ ] Registra logs estruturados.

------------------------------------------------------------------------

# Testes

## Unitários

-   Validação de arquivo.
-   Duplicidade.
-   Falha de espaço.
-   Cancelamento.
-   Mapeamento de eventos.

## Integração

-   Importar WBFS válido.
-   Importar lote de WBFS.
-   Importar para HD WBFS legado.
-   Importar para pasta `wbfs/`.
-   Remover dispositivo durante importação.

------------------------------------------------------------------------

# Dependências

-   011-WBFS.md
-   012-WiimmsIntegration.md
-   017-ImportWizard.md
-   F001-DetectUsbDrive.md
-   F002-ScanGameLibrary.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Importação validada em HD WBFS e pasta `wbfs/`.
-   Testes aprovados.
-   Logs implementados.
-   Documentação atualizada.

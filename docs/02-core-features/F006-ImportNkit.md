# F006 - Import NKit

> Feature ID: F006\
> Status: Ready for Development\
> Priority: High\
> Epic: Import

# Objetivo

Importar jogos no formato **NKit** utilizando uma etapa obrigatória de
restauração para ISO antes da gravação no dispositivo de destino.

A implementação deve utilizar exclusivamente o **INKitService** e a
**Conversion Engine**, preservando a arquitetura desacoplada do
WiiStudio.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais arquivos `.nkit.iso`.

## RF002

Detectar automaticamente a ferramenta NKit configurada.

## RF003

Restaurar o arquivo para ISO em diretório temporário.

## RF004

Validar a ISO restaurada utilizando o Wiimms.

## RF005

Importar a ISO para o destino.

## RF006

Atualizar automaticamente a biblioteca.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Processo totalmente assíncrono.
-   Cancelável em qualquer etapa.
-   Limpeza automática de arquivos temporários.
-   Não bloquear a interface.

------------------------------------------------------------------------

# Pipeline

``` text
Selecionar NKit
      │
      ▼
Validar Arquivo
      │
      ▼
Restaurar ISO
      │
      ▼
Validar ISO
      │
      ▼
Importar
      │
      ▼
Atualizar Biblioteca
      │
      ▼
Limpar Temporários
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ImportWizard
      │
INKitImportService
      │
INKitService
      │
ISO Temporária
      │
IConversionEngine
      │
IWiimmsService
      │
Library Refresh
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   SelectingFiles
-   Validating
-   Restoring
-   ValidatingIso
-   Importing
-   CleaningUp
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Validações

Antes da restauração:

-   Arquivo existe.
-   Extensão suportada.
-   Ferramenta NKit disponível.
-   Espaço para ISO temporária.
-   Destino conectado.

Após restauração:

-   ISO reconhecida pelo Wiimms.
-   Game ID válido.
-   Não existe duplicidade.

------------------------------------------------------------------------

# Arquivos Temporários

``` text
temp/nkit/
```

Regras:

-   Exclusão automática após sucesso.
-   Opção para preservar em modo diagnóstico.

------------------------------------------------------------------------

# Eventos

-   NkitImportStarted
-   NkitRestoreStarted
-   NkitRestoreCompleted
-   IsoValidated
-   NkitImportProgressChanged
-   NkitImported
-   NkitImportFailed
-   NkitImportCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface INKitImportService
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

-   Ferramenta NKit ausente.
-   Restauração falhou.
-   ISO restaurada inválida.
-   Espaço insuficiente.
-   Destino removido.
-   Cancelamento pelo usuário.

Todos os erros devem gerar logs estruturados e preservar a consistência
da biblioteca.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Importa arquivos `.nkit.iso`.
-   [ ] Restaura ISO corretamente.
-   [ ] Valida ISO antes da gravação.
-   [ ] Remove temporários automaticamente.
-   [ ] Atualiza biblioteca ao concluir.
-   [ ] Suporta cancelamento.

------------------------------------------------------------------------

# Testes

## Unitários

-   Seleção da ferramenta.
-   Restauração.
-   Validação da ISO.
-   Limpeza de temporários.
-   Cancelamento.

## Integração

-   Importar NKit válido.
-   Falha na restauração.
-   ISO inválida.
-   Espaço insuficiente.
-   Remoção do dispositivo durante importação.

------------------------------------------------------------------------

# Dependências

-   013-NKitLegacy.md
-   017-ImportWizard.md
-   018-ConversionEngine.md
-   F001-DetectUsbDrive.md
-   F002-ScanGameLibrary.md
-   F003-ImportIso.md

------------------------------------------------------------------------

# Definition of Done

-   Fluxo implementado.
-   Integração com INKitService concluída.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

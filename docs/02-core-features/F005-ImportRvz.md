# F005 - Import RVZ

> Feature ID: F005\
> Status: Implemented and Homologated\
> Priority: High\
> Epic: Import

# Objetivo

Importar jogos no formato **RVZ** utilizando um Provider de conversão
compatível, permitindo que o usuário trabalhe com bibliotecas modernas
sem perder a compatibilidade com dispositivos Nintendo Wii.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Selecionar um ou mais arquivos `.rvz`.

## RF002

Detectar automaticamente o Provider responsável pela conversão.

## RF003

Validar integridade do arquivo antes da conversão.

## RF004

Converter para ISO temporária quando necessário.

## RF005

Importar o jogo para o destino através da Conversion Engine.

## RF006

Atualizar automaticamente a biblioteca.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Conversão assíncrona.
-   Cancelamento em qualquer etapa.
-   Limpeza automática dos arquivos temporários.
-   Não bloquear a interface.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar RVZ
      │
      ▼
Validar Arquivo
      │
      ▼
Selecionar Destino
      │
      ▼
Converter para ISO
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
IRvzImportService
      │
IConversionEngine
      │
IRvzProvider
      │
ISO Temporária
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
-   Preparing
-   Converting
-   Importing
-   CleaningUp
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Validações

Antes da conversão:

-   Arquivo existe.
-   Extensão `.rvz`.
-   Provider disponível.
-   Espaço suficiente para ISO temporária.
-   Destino disponível.
-   Não existe duplicidade.

------------------------------------------------------------------------

# Arquivos Temporários

Diretório padrão:

``` text
temp/rvz/
```

Regras:

-   Remover automaticamente após sucesso.
-   Em falhas, permitir preservação para diagnóstico.

------------------------------------------------------------------------

# Eventos

-   RvzImportStarted
-   RvzValidated
-   RvzConversionStarted
-   RvzConversionProgressChanged
-   RvzImportStartedToDevice
-   RvzImported
-   RvzImportFailed
-   RvzImportCancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IRvzImportService
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

-   Provider indisponível.
-   RVZ inválido.
-   Conversão interrompida.
-   Espaço insuficiente.
-   Destino removido.
-   Falha do Wiimms.

Todas as falhas devem manter a consistência da biblioteca.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Importa um ou vários arquivos RVZ.
-   [x] Converte utilizando Provider configurado.
-   [x] Remove temporários após sucesso.
-   [x] Atualiza biblioteca automaticamente.
-   [x] Permite cancelamento em qualquer etapa.

------------------------------------------------------------------------

# Testes

## Unitários

-   Seleção do Provider.
-   Validação.
-   Conversão.
-   Limpeza de temporários.
-   Cancelamento.

## Integração

-   Importar RVZ válido.
-   Importar lote de RVZ.
-   Falha durante conversão.
-   Espaço insuficiente.
-   Remoção do dispositivo durante importação.

------------------------------------------------------------------------

# Dependências

-   018-ConversionEngine.md
-   012-WiimmsIntegration.md
-   017-ImportWizard.md
-   F001-DetectUsbDrive.md
-   F002-ScanGameLibrary.md
-   F003-ImportIso.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Provider integrado.
-   Conversão validada.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# F002 - Scan Game Library

> Feature ID: F002\
> Status: Implemented and Homologated\
> Priority: Critical\
> Epic: Device Management / Library

# Objetivo

Realizar a varredura da biblioteca de jogos presente em um dispositivo
Wii, identificando automaticamente jogos, metadados básicos e
inconsistências.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Escanear HD WBFS legado.

## RF002

Escanear diretórios `wbfs/` em FAT32, NTFS e exFAT.

## RF003

Atualizar a biblioteca automaticamente após detecção do dispositivo.

## RF004

Permitir atualização manual.

## RF005

Ignorar arquivos inválidos registrando log.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Não bloquear a UI.
-   Operação cancelável.
-   Tempo de resposta proporcional ao número de jogos.
-   Atualização incremental quando possível.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar dispositivo
        │
        ▼
Iniciar varredura
        │
        ▼
Barra de progresso
        │
        ▼
Biblioteca atualizada
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
IUsbDetectionService
        │
        ▼
IWbfsService
        │
        ▼
IWiimmsService
        │
        ▼
GameMapper
        │
        ▼
LibraryStore
        │
        ▼
UI
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Scanning
-   ReadingGames
-   UpdatingLibrary
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Dados Coletados

Para cada jogo:

-   Game ID
-   Título
-   Região
-   Formato
-   Tamanho
-   Caminho
-   Data de descoberta

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IGameLibraryScanner
{
    Task<LibraryScanResult> ScanAsync(
        UsbDrive drive,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   LibraryScanStarted
-   GameDiscovered
-   LibraryProgressChanged
-   LibraryScanCompleted
-   LibraryScanFailed

------------------------------------------------------------------------

# Casos de Erro

-   Dispositivo removido durante a leitura.
-   Falha do Wiimms.
-   Arquivo corrompido.
-   Permissão insuficiente.

Todos devem registrar log estruturado e preservar a biblioteca
existente.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Detecta todos os jogos válidos.
-   [x] Ignora arquivos inválidos.
-   [x] Atualiza a UI em tempo real.
-   [x] Permite cancelamento.
-   [x] Não duplica jogos.

------------------------------------------------------------------------

# Testes

## Unitários

-   Scanner retorna coleção correta.
-   Mapeamento de GameDto.
-   Cancelamento.

## Integração

-   HD WBFS.
-   Pasta wbfs.
-   Biblioteca vazia.
-   Biblioteca com \>100 jogos.

------------------------------------------------------------------------

# Dependências

-   010-USBDetection.md
-   011-WBFS.md
-   012-WiimmsIntegration.md
-   016-GameLibrary.md

------------------------------------------------------------------------

# Definition of Done

-   Scanner implementado.
-   Eventos publicados.
-   Testes aprovados.
-   Performance validada.
-   Documentação atualizada.

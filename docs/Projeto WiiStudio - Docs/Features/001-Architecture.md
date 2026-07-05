# F001 - Detect USB Drive

> Feature ID: F001\
> Status: Ready for Development\
> Priority: Critical\
> Epic: Device Management

# Objetivo

Detectar automaticamente dispositivos USB compatíveis com Nintendo Wii e
mantê-los sincronizados com a interface do WiiStudio.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Detectar automaticamente conexão de dispositivos.

## RF002

Atualizar a interface em tempo real.

## RF003

Identificar:

-   Partição WBFS
-   FAT32 + pasta wbfs
-   NTFS + pasta wbfs
-   exFAT + pasta wbfs

## RF004

Permitir atualização manual.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Atualização em menos de 2 segundos.
-   Não bloquear a UI.
-   Funcionar em Linux, Windows e macOS.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Conectar dispositivo
        │
        ▼
Detecção automática
        │
        ▼
Validação
        │
        ▼
Atualizar Biblioteca
        │
        ▼
Notificação
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
OS Provider
      │
UsbDetectionService
      │
Application
      │
Library Refresh
      │
UI
```

------------------------------------------------------------------------

# Estados

-   Idle
-   Scanning
-   DeviceFound
-   Unsupported
-   Error

------------------------------------------------------------------------

# Eventos

-   UsbConnected
-   UsbDisconnected
-   UsbScanStarted
-   UsbScanCompleted

------------------------------------------------------------------------

# Contratos

``` csharp
public interface IUsbDetectionService
{
    Task ScanAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<UsbDrive> WatchAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Casos de Erro

-   Dispositivo inacessível.
-   Permissão insuficiente.
-   Sistema de arquivos desconhecido.
-   Partição corrompida.

Cada erro deve possuir mensagem amigável e log estruturado.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Detecta conexão automática.
-   [ ] Detecta remoção automática.
-   [ ] Atualiza biblioteca sem reiniciar.
-   [ ] Não bloqueia a interface.
-   [ ] Funciona nas plataformas suportadas.

------------------------------------------------------------------------

# Testes

## Unitários

-   Provider retorna dispositivos.
-   Eventos publicados.
-   Estados corretos.

## Integração

-   Conectar dispositivo.
-   Remover dispositivo.
-   Reconectar dispositivo.

------------------------------------------------------------------------

# Dependências

-   010-USBDetection.md
-   006-ApplicationLayer.md
-   007-Infrastructure.md

------------------------------------------------------------------------

# Definition of Done

-   Código implementado.
-   Testes aprovados.
-   Documentação atualizada.
-   Review aprovado.
-   Logs implementados.

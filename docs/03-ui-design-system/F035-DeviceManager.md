# F035 - Device Manager

> Feature ID: F035 Status: Ready for Development Priority: Critical
> Epic: User Interface

# Objetivo

Fornecer uma interface centralizada para detectar, monitorar e
administrar todos os dispositivos de armazenamento compatíveis com o
WiiStudio, incluindo HDs, SSDs, pendrives e partições utilizadas pela
biblioteca de jogos.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Detectar dispositivos automaticamente.

## RF002

Atualizar a lista sob demanda.

## RF003

Exibir partições e sistemas de arquivos.

## RF004

Exibir espaço total, livre e utilizado.

## RF005

Montar, desmontar e ejetar dispositivos quando suportado.

## RF006

Executar diagnósticos rápidos diretamente pela interface.

------------------------------------------------------------------------

# Informações Exibidas

-   Nome do dispositivo
-   Fabricante
-   Modelo
-   Serial (quando disponível)
-   Interface (USB/SATA)
-   Sistema de arquivos
-   Capacidade
-   Espaço livre
-   Status
-   Compatibilidade Wii

------------------------------------------------------------------------

# Ações

-   Atualizar
-   Abrir Biblioteca
-   Verificar Integridade
-   Reparar
-   Importar Jogos
-   Exportar Jogos
-   Ejetar com Segurança

------------------------------------------------------------------------

# Layout

``` text
+-----------------------------------------------------------+
| Toolbar                                                   |
+-----------------------------------------------------------+
| Lista de Dispositivos | Informações | Ações               |
+-----------------------------------------------------------+
| Partições | Espaço | Diagnóstico | Logs                  |
+-----------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
DeviceManagerView
        │
DeviceManagerViewModel
        │
IDeviceManagerService
        │
IDeviceDetectionService
IDeviceDiagnosticsService
```

------------------------------------------------------------------------

# Máquina de Estados

-   Detecting
-   Ready
-   Refreshing
-   Busy
-   Error

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IDeviceManagerService
{
    Task RefreshAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken);
    Task EjectAsync(string deviceId, CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Atualização automática ao conectar/remover dispositivos.
-   Não permitir ejeção durante operações críticas.
-   Destacar dispositivos incompatíveis.
-   Integrar com Task Manager e Notification Center.

------------------------------------------------------------------------

# Eventos

-   DeviceConnected
-   DeviceDisconnected
-   DeviceRefreshed
-   DeviceEjected
-   DeviceStatusChanged

------------------------------------------------------------------------

# Casos de Erro

-   Permissão insuficiente.
-   Dispositivo inacessível.
-   Sistema de arquivos não suportado.
-   Falha na ejeção.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Detecção automática.
-   [ ] Atualização em tempo real.
-   [ ] Exibição completa de informações.
-   [ ] Ejeção segura.
-   [ ] Integração com diagnóstico.

------------------------------------------------------------------------

# Testes

## Unitários

-   Detecção.
-   Atualização.
-   Ejeção.
-   Estado.

## Integração

-   Conectar HD.
-   Remover dispositivo.
-   Partição WBFS.
-   FAT32/NTFS/exFAT.
-   Múltiplos dispositivos.

------------------------------------------------------------------------

# Dependências

-   F001-DeviceDetection.md
-   F030-AdvancedDiagnostics.md
-   F031-MainWindow.md

------------------------------------------------------------------------

# Definition of Done

-   Tela implementada.
-   Integração validada.
-   Testes aprovados.
-   Documentação atualizada.

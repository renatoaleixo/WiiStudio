# 010 - USB Detection Specification

> Status: Accepted Version: 1.0.0

# Objetivo

Definir a estratégia oficial para detecção, monitoramento e
gerenciamento de dispositivos USB utilizados pelo WiiStudio.

------------------------------------------------------------------------

# Escopo

O WiiStudio deve detectar automaticamente dispositivos conectados que
possam conter jogos do Nintendo Wii.

Sistemas suportados:

-   Linux
-   Windows
-   macOS

------------------------------------------------------------------------

# Responsabilidades

O módulo USB deve:

-   Detectar conexão e remoção.
-   Identificar dispositivos compatíveis.
-   Calcular espaço livre.
-   Informar sistema de arquivos.
-   Publicar eventos para a UI.

Não deve:

-   Alterar dados do dispositivo.
-   Montar ou desmontar volumes automaticamente.

------------------------------------------------------------------------

# Modelo

``` text
IUsbDetectionService
        │
        ▼
UsbDetectionService
        │
        ├── LinuxUsbProvider
        ├── WindowsUsbProvider
        └── MacOsUsbProvider
```

------------------------------------------------------------------------

# Entidade UsbDrive

Campos mínimos:

-   Id
-   DevicePath
-   MountPoint
-   Label
-   Capacity
-   UsedSpace
-   FreeSpace
-   FileSystem
-   IsMounted
-   IsWritable

------------------------------------------------------------------------

# Eventos

-   UsbConnected
-   UsbDisconnected
-   UsbMounted
-   UsbUnmounted

------------------------------------------------------------------------

# Fluxo

1.  Sistema operacional notifica alteração.
2.  Provider identifica dispositivos.
3.  Device é validado.
4.  Estado é atualizado.
5.  Evento publicado.
6.  UI atualizada.

------------------------------------------------------------------------

# Sistemas de Arquivos

Reconhecer:

-   WBFS
-   FAT32
-   NTFS
-   exFAT

Caso desconhecido:

-   Exibir dispositivo como "Formato não suportado".

------------------------------------------------------------------------

# Polling

Quando notificações nativas não estiverem disponíveis:

-   Polling a cada 2 segundos.
-   Configurável.

------------------------------------------------------------------------

# Performance

A detecção não pode bloquear a UI.

Operações devem ser assíncronas.

------------------------------------------------------------------------

# Logging

Registrar:

-   conexão
-   remoção
-   erros
-   tempo de detecção

------------------------------------------------------------------------

# Testes

Cobrir:

-   conexão
-   remoção
-   múltiplos dispositivos
-   dispositivos inválidos
-   reconexão

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca acessar APIs específicas diretamente na UI.
-   Todo acesso passa pelo IUsbDetectionService.
-   Providers devem ser independentes por plataforma.

------------------------------------------------------------------------

# Critérios de Aceite

-   Detecção automática.
-   Atualização em tempo real.
-   Sem travamento da interface.
-   Compatível com Linux, Windows e macOS.

------------------------------------------------------------------------

# Checklist

-   [ ] Providers definidos
-   [ ] Eventos documentados
-   [ ] Fluxo validado
-   [ ] Estratégia de polling definida
-   [ ] Testes especificados

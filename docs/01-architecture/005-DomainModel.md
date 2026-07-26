# 005 - Domain Model

> Status: Accepted Version: 1.0.0

# Objetivo

Definir o modelo de domínio do WiiStudio. Este documento descreve as
entidades, objetos de valor e regras de negócio centrais do sistema.

# Visão do Domínio

O WiiStudio gerencia bibliotecas de jogos de Nintendo Wii, dispositivos
USB, processos de conversão e metadados.

``` text
UsbDrive
   │
   ├── Game
   │      ├── Cover
   │      ├── GameMetadata
   │      └── GameFile
   │
   └── ConversionJob
```

# Entidades

## Game

Representa um jogo.

Propriedades:

-   Id
-   Title
-   GameId
-   Region
-   DiscType
-   Size
-   Format
-   FilePath
-   AddedAt

Regras:

-   GameId é obrigatório.
-   Não permitir duplicidade no mesmo UsbDrive.

## UsbDrive

Representa um dispositivo Wii.

Propriedades:

-   Id
-   DevicePath
-   Label
-   Capacity
-   UsedSpace
-   FreeSpace
-   FileSystem

Regras:

-   Deve ser detectado automaticamente.
-   Pode conter diversos jogos.

## ConversionJob

Representa uma conversão.

Estados:

-   Pending
-   Running
-   Completed
-   Failed
-   Cancelled

## Cover

Representa uma capa.

Campos:

-   GameId
-   ImagePath
-   Source
-   DownloadedAt

## Settings

Configurações globais.

-   Tema
-   Idioma
-   Caminho Wiimms
-   Caminho NKit
-   Pasta Downloads

# Value Objects

## GameId

Código oficial do Wii.

Exemplo:

RMGE01

## FileFormat

-   ISO
-   WBFS
-   RVZ
-   NKIT

## Region

-   NTSC-U
-   PAL
-   NTSC-J
-   KOR

# Serviços de Domínio

-   GameValidationService
-   DuplicateGameService
-   UsbCapacityService

# Regras

-   Nunca remover arquivos sem confirmação.
-   Conversões devem preservar integridade.
-   Jogos duplicados devem ser detectados.

# Eventos

-   GameImported
-   GameRemoved
-   ConversionStarted
-   ConversionCompleted
-   UsbConnected
-   UsbDisconnected

# Critérios de Aceite

-   Entidades independentes da UI.
-   Sem dependência de Infrastructure.
-   Regras de negócio concentradas no Domain.

# Checklist

-   [x] Entidades definidas
-   [x] Value Objects definidos
-   [x] Eventos definidos
-   [x] Regras documentadas

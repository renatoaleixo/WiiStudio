# WiiStudio Architecture v1.0

> Living document – Arquitetura oficial do projeto.

## Visão

O WiiStudio será um gerenciador moderno para jogos de Nintendo Wii, multiplataforma (Linux, Windows e macOS), desenvolvido em .NET 10 + Avalonia UI.

## Objetivos

- Gerenciar HDs WBFS.
- Importar ISO, WBFS, RVZ e NKit (legado).
- Converter formatos utilizando Wiimms ISO Tools.
- Detectar automaticamente dispositivos USB.
- Interface moderna e intuitiva.

## Stack

- .NET 10
- Avalonia UI
- CommunityToolkit.Mvvm
- SQLite
- Serilog
- xUnit
- FluentAssertions
- GitHub Actions

## Arquitetura

- Clean Architecture
- MVVM
- SOLID
- Dependency Injection
- Services
- Repository Pattern (quando aplicável)

## Estrutura

```text
src/
  WiiStudio.App
  WiiStudio.Domain
  WiiStudio.Application
  WiiStudio.Infrastructure
  WiiStudio.Shared

tests/
docs/
assets/
scripts/
```

## Módulos

- Dashboard
- Biblioteca
- HD WBFS
- Conversão
- Capas
- Configurações
- Logs

## Serviços

- IWitService
- IWwtService
- INKitService
- IUsbDriveService
- ICoverService
- ISettingsService

## Roadmap

### MVP
- Detectar HD WBFS
- Listar jogos
- Adicionar jogos
- Remover jogos

### v1.1
- Conversão ISO/WBFS
- Download de capas

### v2
- Suporte GameCube
- RVZ
- Plugins

## ADR

Toda decisão arquitetural deverá ser registrada em docs/ADR.

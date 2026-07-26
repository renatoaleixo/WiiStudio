# 006 - Application Layer

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a responsabilidade da camada **Application**, responsável por
orquestrar os casos de uso do WiiStudio sem conhecer detalhes de
infraestrutura.

------------------------------------------------------------------------

# Responsabilidades

-   Orquestrar casos de uso.
-   Coordenar serviços de domínio.
-   Expor interfaces para a Presentation.
-   Validar entradas.
-   Publicar eventos de aplicação.

A camada **Application NÃO**:

-   acessa banco diretamente;
-   executa comandos do `wit`;
-   conhece Avalonia UI.

------------------------------------------------------------------------

# Estrutura

``` text
Application
│
├── Commands
├── Queries
├── DTOs
├── Interfaces
├── Validators
├── Mappers
├── Services
└── UseCases
```

------------------------------------------------------------------------

# Use Cases

## Importar jogo

Fluxo:

1.  Validar arquivo.
2.  Identificar formato.
3.  Verificar duplicidade.
4.  Solicitar importação.
5.  Atualizar biblioteca.
6.  Publicar evento `GameImported`.

------------------------------------------------------------------------

## Converter jogo

Fluxo:

1.  Validar formato de origem.
2.  Criar ConversionJob.
3.  Executar serviço apropriado.
4.  Atualizar progresso.
5.  Finalizar ou registrar falha.

------------------------------------------------------------------------

## Detectar HD

Fluxo:

1.  Localizar dispositivos.
2.  Validar formato.
3.  Atualizar estado da aplicação.
4.  Publicar evento `UsbConnected`.

------------------------------------------------------------------------

# Commands

Exemplos:

-   ImportGameCommand
-   RemoveGameCommand
-   ConvertGameCommand
-   DownloadCoverCommand

Commands alteram estado.

------------------------------------------------------------------------

# Queries

Exemplos:

-   GetGamesQuery
-   GetUsbDrivesQuery
-   GetSettingsQuery

Queries nunca alteram estado.

------------------------------------------------------------------------

# DTOs

DTOs transportam dados entre camadas.

Exemplos:

-   GameDto
-   UsbDriveDto
-   ConversionJobDto

------------------------------------------------------------------------

# Interfaces

-   IGameLibraryService
-   IConversionService
-   IUsbDetectionService
-   ICoverDownloadService
-   ISettingsService

------------------------------------------------------------------------

# Validação

Utilizar FluentValidation.

Toda entrada externa deve ser validada antes da execução do caso de uso.

------------------------------------------------------------------------

# Comunicação

Presentation ↓ Application ↓ Domain ↓ Infrastructure (via interfaces)

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca colocar regra de negócio em DTO.
-   Nunca acessar Infrastructure diretamente.
-   Commands não retornam coleções.
-   Queries não modificam estado.
-   Preferir composição de serviços.

------------------------------------------------------------------------

# Critérios de Aceite

-   Casos de uso independentes da UI.
-   Interfaces desacopladas.
-   Validações centralizadas.
-   Fluxos documentados.

------------------------------------------------------------------------

# Checklist

-   [x] Commands definidos
-   [x] Queries definidos
-   [x] DTOs definidos
-   [x] Interfaces definidas
-   [x] Casos de uso documentados

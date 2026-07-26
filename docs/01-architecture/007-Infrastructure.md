# 007 - Infrastructure Layer

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a camada Infrastructure do WiiStudio, responsável por
implementar integrações externas mantendo o domínio e a aplicação
desacoplados.

------------------------------------------------------------------------

# Responsabilidades

A camada Infrastructure implementa interfaces da Application e fornece
acesso a:

-   Wiimms ISO Tools (wit/wwt)
-   NKit (legado)
-   Sistema de arquivos
-   Dispositivos USB
-   SQLite
-   GameTDB
-   Sistema operacional

Nunca contém regras de negócio.

------------------------------------------------------------------------

# Estrutura

``` text
Infrastructure/
│
├── DependencyInjection/
├── Wiimms/
├── NKit/
├── Usb/
├── FileSystem/
├── Database/
├── Settings/
├── Covers/
├── Logging/
└── Diagnostics/
```

------------------------------------------------------------------------

# Adapters

Cada tecnologia externa deve possuir seu próprio adapter.

Exemplo:

``` text
IWitService
        │
        ▼
WitService
        │
        ▼
ProcessRunner
        │
        ▼
wit.exe / wit
```

------------------------------------------------------------------------

# ProcessRunner

Toda execução de processos externos deve utilizar um único componente.

Responsabilidades:

-   iniciar processo
-   capturar stdout
-   capturar stderr
-   timeout
-   CancellationToken
-   exit code
-   logging

É proibido executar Process diretamente fora desse componente.

------------------------------------------------------------------------

# Wiimms

Ferramentas suportadas:

-   wit
-   wwt

Operações:

-   Listar jogos
-   Importar
-   Exportar
-   Converter
-   Verificar disco

------------------------------------------------------------------------

# Sistema de Arquivos

Criar abstração:

-   IFileSystem

Nunca utilizar System.IO diretamente na camada Application.

------------------------------------------------------------------------

# USB

Criar:

-   IUsbDetector

Implementações específicas por sistema operacional.

------------------------------------------------------------------------

# SQLite

Responsável apenas por:

-   Configurações
-   Cache
-   Histórico
-   Favoritos

Nunca armazenar jogos do HD WBFS.

------------------------------------------------------------------------

# Logging

Todos os adapters devem registrar:

-   início
-   fim
-   duração
-   erros
-   código de retorno

------------------------------------------------------------------------

# Tratamento de Falhas

Converter exceções técnicas em exceções de infraestrutura
significativas.

Exemplo:

-   WitNotFoundException
-   UsbNotAvailableException
-   InvalidGameFormatException

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca acessar Process diretamente.
-   Nunca executar comandos shell na Application.
-   Nunca misturar regra de negócio com integração.
-   Toda integração deve ser mockável.

------------------------------------------------------------------------

# Critérios de Aceite

-   Todas as integrações implementam interfaces.
-   Infraestrutura isolada.
-   ProcessRunner reutilizado.
-   Logs completos.
-   Timeout configurável.

------------------------------------------------------------------------

# Checklist

-   [x] Adapters definidos
-   [x] ProcessRunner criado
-   [x] Logging centralizado
-   [x] Exceções específicas
-   [x] Interfaces respeitadas

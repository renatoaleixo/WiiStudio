# 003 - Coding Standards

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Padronizar o desenvolvimento do WiiStudio para garantir legibilidade,
consistência, qualidade e facilidade de manutenção.

# Convenções Gerais

-   Linguagem: C# / .NET 10
-   Nullable: habilitado
-   ImplicitUsings: habilitado
-   Warnings tratados como erros

# Nomeação

## Classes

PascalCase

## Interfaces

Prefixo `I`

## Métodos

PascalCase

## Campos privados

``` csharp
private readonly IWitService _witService;
```

## Constantes

``` csharp
public const int MaxRecentGames = 20;
```

# Async

Toda operação de I/O deve ser assíncrona.

``` csharp
Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
```

# CancellationToken

Obrigatório em:

-   Conversões
-   Importações
-   Downloads
-   Operações USB
-   Acesso a banco

# Dependency Injection

Todo serviço deve ser registrado via DI.

Nunca utilizar Service Locator.

# MVVM

-   View sem regra de negócio.
-   ViewModel sem acesso direto à Infrastructure.
-   Comunicação por interfaces da Application.

# Logging

Utilizar Serilog.

Não usar Console.WriteLine.

# Tratamento de Erros

-   Nunca ocultar exceções.
-   Registrar logs.
-   Exibir mensagens amigáveis ao usuário.

# Comentários

Explicar "por quê", não "o quê".

Usar XML Docs em APIs públicas.

# Testes

-   xUnit
-   FluentAssertions
-   Moq

Cobrir regras de negócio críticas.

# Commits

Conventional Commits

-   feat:
-   fix:
-   refactor:
-   docs:
-   test:
-   chore:

# Code Review

Checklist obrigatório:

-   Arquitetura respeitada
-   SOLID
-   Sem duplicação
-   Testes
-   Logs
-   Performance

# Proibido

-   static para lógica de negócio
-   regiões (#region)
-   métodos gigantes
-   God Classes
-   dependências circulares

# Checklist Final

-   [ ] Código limpo
-   [ ] Async
-   [ ] DI
-   [ ] MVVM
-   [ ] Testes
-   [ ] Logging
-   [ ] XML Docs

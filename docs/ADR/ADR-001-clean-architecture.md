# ADR-001: Clean Architecture

## Status

Accepted

## Context

WiiStudio precisa ser multiplataforma, testável e manter a interface isolada da infraestrutura.

## Decision

O projeto será dividido em Domain, Application, Infrastructure, App e Shared. A UI depende de Application e usa serviços registrados por DI. Infrastructure implementa contratos definidos em Application.

## Consequences

As regras de negócio permanecem testáveis e independentes de Avalonia, SQLite, processos externos e APIs do sistema operacional.

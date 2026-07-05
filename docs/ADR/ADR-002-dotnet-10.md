# ADR-002: .NET 10

## Status

Accepted

## Context

A documentacao inicial especificava .NET 9. O ambiente local disponibiliza .NET 10 e o projeto foi autorizado a utilizar essa versao.

## Decision

O WiiStudio passa a mirar `net10.0` como target framework inicial.

## Consequences

O projeto usa uma versao mais recente do runtime e SDK. Contribuidores deverao instalar o SDK .NET 10 para restaurar, compilar, testar e executar a aplicacao.

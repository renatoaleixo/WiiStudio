# WiiStudio

WiiStudio é um gerenciador open source de jogos Nintendo Wii, multiplataforma, desenvolvido com .NET 10, Avalonia UI e Clean Architecture.

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

## Desenvolvimento

O projeto segue a documentação em `docs/`, organizada em:

- `docs/00-project/` para contexto, roadmap e índice.
- `docs/01-architecture/` para arquitetura e decisões técnicas.
- `docs/02-core-features/` para features F001-F030.
- `docs/03-ui-design-system/` para UI e Design System F031-F060.
- `docs/04-application-shell/` em diante para shell, plugins, segurança e testes.

O alvo atual é .NET 10.

```bash
dotnet restore
dotnet test
dotnet run --project src/WiiStudio.App
```

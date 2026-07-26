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
- `docs/08-tools/` para integrações externas, USB/WBFS, GameTDB, updates, marketplace remoto e release readiness F101-F126.
- `docs/09-emulator-integration/` para a nova fase de integração com emuladores externos F127-F130.

O alvo atual é .NET 10.

```bash
dotnet restore
dotnet test WiiStudio.sln
dotnet run --project src/WiiStudio.App
```

## Release

```bash
dotnet build WiiStudio.sln -c Release
dotnet test WiiStudio.sln --no-build -c Release
WIISTUDIO_CHANNEL=dev scripts/release/package.sh 0.1.0-dev.1
WIISTUDIO_CHANNEL=dev scripts/release/smoke-test.sh 0.1.0-dev.1
```

Homologação final em hardware real está registrada em
`docs/99-final/HOMOLOGATION_ACCEPTANCE.md`. Novas integrações devem entrar como
nova fase de roadmap, sem reabrir o aceite de F001-F126.

# WiiStudio

WiiStudio é um gerenciador open source de jogos Nintendo Wii, multiplataforma, desenvolvido com .NET 10, Avalonia UI e Clean Architecture.

## Como surgiu

O WiiStudio nasceu de uma necessidade pessoal: eu não encontrava na internet
um programa visual, moderno e prático que reunisse as operações necessárias
para organizar meus próprios jogos e transferi-los para HDs externos e
pendrives usados no Wii, permitindo jogar sem depender da mídia física no uso
cotidiano.

O projeto começou como uma forma de tornar esse processo mais simples, seguro
e acessível em uma única interface. Com a evolução do desenvolvimento, o
WiiStudio também passou a integrar o Dolphin, permitindo iniciar no computador
os jogos legalmente preservados pelo próprio usuário. A iniciativa continua
com o objetivo de oferecer uma ferramenta multiplataforma bem documentada,
sem distribuir jogos, chaves, BIOS ou qualquer conteúdo protegido.

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
- `docs/10-release-distribution/` para instaladores e distribuição multiplataforma a partir da F131.

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
scripts/release/package.sh 1.0.0 linux
```

Os instaladores oficiais são produzidos no sistema nativo pelo workflow
`Release`: Inno Setup e ZIP no Windows, AppImage e `.deb` no Linux, e `.app`
mais `.dmg` para macOS x64/arm64. Após reunir os artefatos, o job final gera o
manifesto, checksums SHA-256 e executa os smoke tests estruturais.

Homologação final em hardware real está registrada em
`docs/99-final/HOMOLOGATION_ACCEPTANCE.md`. Novas integrações devem entrar como
nova fase de roadmap, sem reabrir o aceite de F001-F126.

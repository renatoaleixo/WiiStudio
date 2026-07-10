# FINALIZATION_REFERENCE

# Objetivo

Registrar a finalização organizacional do repositório WiiStudio após a implementação
das features documentadas de `F001` até `F125`.

---

# Status Final

- Core Features: `F001` até `F030` implementadas.
- UI Design System: `F031` até `F060` implementadas.
- Application Shell: `F061` até `F070` implementadas.
- Plugin SDK: `F071` até `F080` implementadas.
- Security: `F081` até `F090` implementadas.
- Testing & Quality: `F091` até `F100` implementadas.
- Tools, Readiness & Release Engineering: `F101` até `F125` implementadas.

---

# Estrutura Confirmada

A documentação final está organizada em:

```text
docs/
├── 00-project/
├── 01-architecture/
├── 02-core-features/
├── 03-ui-design-system/
├── 04-application-shell/
├── 05-plugin-sdk/
├── 06-security/
├── 07-testing-quality/
├── 08-tools/
└── 99-final/
```

Cada pasta oficial possui `README.md`.

---

# Decisões de Finalização

## Projeto UI

O documento `FOLDER_STRUCTURE.md` sugere `src/WiiStudio.UI`.

Decisão atual:

- manter `src/WiiStudio.App`;
- evitar renomeação ampla de projeto, namespaces e referências;
- tratar `WiiStudio.App` como a implementação UI desktop Avalonia.

## Projetos de Teste Dedicados

O documento final sugere projetos dedicados para:

- `WiiStudio.UI.Tests`;
- `WiiStudio.Integration.Tests`;
- `WiiStudio.Performance.Tests`;
- `WiiStudio.Regression.Tests`.

Decisão atual:

- não criar projetos vazios;
- usar `tests/WiiStudio.Testing` como test kit oficial;
- criar projetos dedicados quando houver testes reais nessas categorias.

## Documentos na Raiz

Atalhos foram criados na raiz para:

- `AI_CONTEXT.md`;
- `ROADMAP.md`;
- `CONTRIBUTING.md`;
- `DOCUMENT_INDEX.md`.

Os documentos canônicos continuam em `docs/00-project/`.

---

# Pastas de Apoio

Pastas finais disponíveis:

- `assets/`;
- `scripts/`;
- `tools/`;
- `build/`;
- `samples/`.

Pastas sem conteúdo funcional mantêm `.gitkeep`.

---

# Referências de Implementação

Durante a implementação foram adicionados:

- `docs/03-ui-design-system/DESIGN_SYSTEM_REFERENCE.md`;
- `docs/03-ui-design-system/DESIGN_SYSTEM_USAGE.md`;
- `docs/04-application-shell/APPLICATION_SHELL_REFERENCE.md`;
- `docs/05-plugin-sdk/PLUGIN_SDK_REFERENCE.md`;
- `docs/06-security/SECURITY_REFERENCE.md`;
- `docs/07-testing-quality/TESTING_QUALITY_REFERENCE.md`.

Após F100, a pasta `docs/08-tools/` passou a concentrar as features de
dependências externas, GameTDB, USB/WBFS, updates, marketplace remoto de plugins
e prontidão de release:

- `F101` até `F105`: ferramentas externas, GameTDB e acesso privilegiado;
- `F106` até `F120`: Polkit, USB multiplataforma, Wiimms, SQLite, diagnósticos,
  first-run wizard e readiness USB;
- `F121`: catálogo do comando real de formatação WBFS;
- `F122`: distribuição e manifesto de atualização do app;
- `F123`: marketplace remoto de plugins e política de confiança;
- `F124`: scripts/pipeline de empacotamento e release;
- `F125`: evidências de homologação de hardware e aceite de release.

---

# Validação Esperada

Antes de considerar o repositório pronto:

```bash
dotnet build WiiStudio.sln -c Release
dotnet test WiiStudio.sln --no-build -c Release
bash -n scripts/release/package.sh
bash -n scripts/release/smoke-test.sh
git status --short
```

O resultado esperado é build limpo, testes verdes e working tree limpo após commit.

Para empacotamento controlado:

```bash
WIISTUDIO_CHANNEL=dev scripts/release/package.sh 0.1.0-dev.1
WIISTUDIO_CHANNEL=dev scripts/release/smoke-test.sh 0.1.0-dev.1
```

Os artefatos são gerados em `artifacts/release/<version>/`, pasta ignorada pelo Git.

---

# Homologação Final

O código possui `IReleaseValidationService` para gerar a estrutura de evidências
da F125:

```text
release-validation/
├── version.txt
├── environment-linux.md
├── environment-windows.md
├── environment-macos.md
├── hardware-matrix.md
├── release-checklist.md
├── screenshots/
├── logs/
└── acceptance-report.md
```

Esse pacote não aprova a release automaticamente. Gates que exigem HD WBFS real,
FAT32 `/wbfs`, pendrive e Wii real permanecem pendentes até homologação manual
com dispositivos dedicados e evidências anexadas.

# F124 - Packaging, Installer and CI/CD

> Feature ID: F124  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Release Engineering / Production Readiness  
> Area: Packaging, Installer, CI/CD, GitHub Releases, Testing  
> Depends on: F122

# Objetivo

Definir o empacotamento final do WiiStudio para Linux, Windows e macOS, incluindo
pipeline de build, versionamento, testes automáticos, artefatos gerados e publicação
no GitHub Releases.

---

# Plataformas Alvo

```text
Linux x64
Windows x64
macOS x64
macOS arm64
macOS universal, se possível
```

---

# Artefatos Linux

Gerar:

```text
WiiStudio-<version>-linux-x64.AppImage
wiistudio_<version>_amd64.deb
wiistudio-<version>-x86_64.rpm
WiiStudio-<version>-linux-x64.tar.gz
```

## Conteúdo

- binários do app;
- assets;
- arquivos desktop;
- ícone;
- policy Polkit, quando pacote suportar;
- helper scripts validados, quando existir;
- documentação de permissões.

---

# Artefatos Windows

Gerar:

```text
WiiStudioSetup-<version>-win-x64.exe
WiiStudio-<version>-win-x64.zip
```

Installer deve suportar:

- instalação por usuário;
- atalho no menu iniciar;
- uninstall;
- upgrade;
- assinatura quando disponível;
- dependências documentadas;
- modo repair/uninstall.

---

# Artefatos macOS

Gerar:

```text
WiiStudio-<version>-macos-x64.dmg
WiiStudio-<version>-macos-arm64.dmg
WiiStudio-<version>-macos-universal.dmg
```

Regras:

- app bundle;
- ícone;
- assinatura;
- notarização quando disponível;
- DMG;
- instruções para permissões de disco.

---

# Versionamento

Usar SemVer:

```text
MAJOR.MINOR.PATCH
```

Tag:

```text
v1.0.0
```

Build metadata opcional:

```text
1.0.0+build.123
```

---

# Pipeline CI/CD

## Stages

```text
Checkout
Restore
Build
Test
Lint/Format
Security checks
Package
Generate checksums
Sign artifacts
Generate update manifest
Publish GitHub Release
Smoke test artifacts
```

---

# Testes Antes da Release

Obrigatórios:

- unit tests;
- integration tests com fakes;
- database migration tests;
- plugin tests;
- process runner tests;
- security checklist;
- packaging smoke tests;
- startup smoke test;
- no real disk formatting;
- no real network dependency in pipeline.

---

# GitHub Releases

Publicar:

- artefatos;
- checksums;
- signatures;
- update manifest;
- release notes;
- SBOM, opcional;
- changelog.

Arquivos:

```text
SHA256SUMS.txt
SHA256SUMS.txt.sig
wiistudio-update-manifest.stable.json
wiistudio-update-manifest.beta.json
```

---

# Checksums

Gerar SHA-256 para todos os artefatos:

```text
sha256sum WiiStudio-1.0.0-linux-x64.AppImage
```

Salvar em:

```text
SHA256SUMS.txt
```

---

# Assinatura

Quando disponível:

- assinar artefatos;
- assinar checksum file;
- assinar manifest;
- documentar chave pública.

Sem assinatura disponível:

- update automático pode ficar restrito;
- checksum continua obrigatório;
- installer deve avisar que assinatura ainda não está ativa.

---

# Manifest de Update

Gerado conforme F122.

Deve conter:

- versão;
- canal;
- URLs;
- sha256;
- assinatura;
- tamanho;
- plataforma;
- release notes.

---

# Branches e Canais

```text
main -> stable candidate
develop -> beta/dev
tags v* -> release
```

Canais:

- stable;
- beta;
- dev.

---

# Empacotamento com .NET

Usar publish self-contained quando apropriado:

```text
dotnet publish -c Release -r linux-x64 --self-contained true
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r osx-arm64 --self-contained true
```

Avaliar trimming com cuidado por causa de Avalonia/plugins.

---

# Segurança do Pipeline

- secrets protegidos;
- assinatura somente em tags;
- permissões mínimas;
- não expor certificados;
- não aceitar artefato externo não validado;
- não executar testes destrutivos;
- verificar dependências vulneráveis.

---

# Smoke Tests

Após empacotar:

- app inicia;
- tela principal abre;
- Diagnostics abre;
- First Run Wizard abre quando settings limpas;
- versão exibida confere;
- artefato contém arquivos esperados;
- installer instala/desinstala, quando ambiente permitir.

---

# Critérios de Aceite

- [ ] Linux AppImage gerado.
- [ ] Linux deb/rpm definidos.
- [ ] Windows installer definido.
- [ ] macOS DMG/app bundle definido.
- [ ] Pipeline build/test/package definido.
- [ ] GitHub Release definido.
- [ ] Checksums gerados.
- [ ] Manifest gerado.
- [ ] Testes automáticos antes da release.
- [ ] Smoke tests definidos.
- [ ] Nenhum teste formata disco real.

---

# Regras para o Codex

- Não publicar release se testes falharem.
- Não gerar manifest sem checksum.
- Não assinar fora de tag protegida.
- Não expor secrets.
- Não habilitar trimming sem validar Avalonia/plugins.
- Não rodar operação destrutiva em CI.
- Sempre gerar artefatos versionados.

---

# Definition of Done

- Empacotamento definido.
- CI/CD definido.
- Artefatos definidos.
- Checksums/assinaturas definidos.
- Publicação no GitHub Releases definida.
- Testes de release definidos.

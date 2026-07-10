# F122 - Application Release and Update Distribution

> Feature ID: F122  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Release Engineering / Application Updates  
> Area: Update Provider, Installer, CI/CD, Security, Distribution  
> Depends on: F112, F124

# Objetivo

Definir como o WiiStudio será distribuído e atualizado com segurança em Linux, Windows
e macOS.

Esta feature complementa `F112-ApplicationUpdateProvider.md` detalhando:

- URL oficial do manifest HTTPS;
- formato do manifest;
- checksum/assinatura dos pacotes;
- pacotes por sistema;
- estratégia de instalação/update;
- rollback;
- versionamento;
- canais stable/beta;
- validações de segurança.

---

# URL Oficial do Manifest

URL oficial recomendada para releases públicas:

```text
https://github.com/<ORG_OR_USER>/WiiStudio/releases/latest/download/wiistudio-update-manifest.json
```

Canal stable:

```text
https://github.com/<ORG_OR_USER>/WiiStudio/releases/latest/download/wiistudio-update-manifest.stable.json
```

Canal beta:

```text
https://github.com/<ORG_OR_USER>/WiiStudio/releases/latest/download/wiistudio-update-manifest.beta.json
```

Regras:

- substituir `<ORG_OR_USER>` pelo owner oficial do repositório;
- usar somente HTTPS;
- validar redirects;
- permitir somente hosts confiáveis;
- cachear manifest local para diagnóstico.

---

# Formato do Manifest

Arquivo:

```text
wiistudio-update-manifest.json
```

Formato:

```json
{
  "schemaVersion": 1,
  "applicationId": "com.wiistudio.app",
  "channel": "stable",
  "latestVersion": "1.0.0",
  "minimumSupportedVersion": "0.9.0",
  "publishedAt": "2026-01-01T10:00:00Z",
  "releaseNotesUrl": "https://github.com/<ORG_OR_USER>/WiiStudio/releases/tag/v1.0.0",
  "packages": [
    {
      "platform": "linux-x64",
      "kind": "appimage",
      "fileName": "WiiStudio-1.0.0-linux-x64.AppImage",
      "url": "https://github.com/<ORG_OR_USER>/WiiStudio/releases/download/v1.0.0/WiiStudio-1.0.0-linux-x64.AppImage",
      "sha256": "...",
      "signatureUrl": "https://github.com/<ORG_OR_USER>/WiiStudio/releases/download/v1.0.0/WiiStudio-1.0.0-linux-x64.AppImage.sig",
      "sizeBytes": 123456789
    }
  ]
}
```

---

# Pacotes por Sistema

## Linux

Gerar:

```text
.deb
.rpm
.AppImage
.tar.gz, opcional
```

Recomendação:

- AppImage para distribuição universal;
- `.deb` para Debian/Ubuntu/Linux Mint;
- `.rpm` para Fedora/openSUSE;
- pacote deve incluir policy Polkit quando suportar;
- pacote deve incluir helper validado quando existir.

## Windows

Gerar:

```text
WiiStudioSetup-<version>-win-x64.exe
WiiStudio-<version>-win-x64.zip
```

Installer recomendado:

- EXE/MSI assinado;
- instalação por usuário ou máquina;
- atalho no menu iniciar;
- uninstall;
- upgrade;
- validação de versão anterior.

## macOS

Gerar:

```text
WiiStudio-<version>-macos-x64.dmg
WiiStudio-<version>-macos-arm64.dmg
WiiStudio-<version>-macos-universal.dmg
```

Regras:

- app bundle;
- assinatura;
- notarização quando aplicável;
- DMG assinado;
- instruções para permissões de disco.

---

# Assinatura e Checksum

Obrigatório:

- SHA-256 para todos os pacotes;
- assinatura quando infraestrutura estiver disponível;
- validação antes de instalar;
- bloqueio em checksum inválido;
- bloqueio em assinatura inválida quando assinatura for exigida.

Campos:

```text
sha256
signatureUrl
signatureAlgorithm
signingKeyId
```

---

# Versionamento

Usar SemVer:

```text
MAJOR.MINOR.PATCH
```

Exemplos:

```text
1.0.0
1.1.0
1.1.1
2.0.0
```

Canais:

```text
stable
beta
dev
```

Regras:

- stable não atualiza para beta;
- beta pode receber beta e stable mais novo;
- downgrade bloqueado por padrão;
- `minimumSupportedVersion` pode forçar atualização.

---

# Estratégia de Update

## Check

```text
Baixar manifest
    ↓
Validar HTTPS/host
    ↓
Validar schema
    ↓
Comparar versão
    ↓
Exibir release notes
```

## Download

```text
Usuário confirma
    ↓
Baixar pacote
    ↓
Validar tamanho
    ↓
Validar SHA-256
    ↓
Validar assinatura
```

## Install

```text
Preparar backup
    ↓
Fechar app, se necessário
    ↓
Executar installer/plataforma
    ↓
Reabrir app
    ↓
Confirmar versão
```

---

# Rollback

## Linux AppImage

- manter AppImage anterior;
- trocar symlink ou launcher;
- rollback manual/assistido.

## Linux deb/rpm

- usar mecanismo do gerenciador de pacotes;
- guardar instruções de downgrade;
- não prometer rollback automático sem implementar.

## Windows Installer

- depender do mecanismo do installer;
- manter pacote anterior quando possível;
- rollback manual documentado.

## macOS

- manter app anterior até nova versão iniciar;
- instrução para restaurar app anterior.

Regra:

```text
Rollback automático só pode ser anunciado quando implementado e testado.
```

---

# Segurança

- HTTPS obrigatório;
- host allowlist;
- checksum obrigatório;
- assinatura recomendada/obrigatória para releases finais;
- não instalar sem confirmação;
- não aceitar manifest inválido;
- não aceitar versão inferior;
- não executar pacote baixado sem validação;
- limpar temporários;
- logs redigidos.

---

# Diagnóstico

Adicionar:

```text
Application Release
Current version
Latest version
Channel
Manifest URL
Manifest schema
Last check
Last download
Package checksum
Signature status
Installer result
Rollback available
Last error
```

---

# Testes Esperados

- manifest válido;
- manifest inválido;
- canal stable;
- canal beta;
- versão maior;
- versão igual;
- downgrade bloqueado;
- pacote Linux;
- pacote Windows;
- pacote macOS;
- checksum correto;
- checksum inválido;
- assinatura inválida;
- rollback disponível;
- erro de rede;
- timeout;
- download cancelado.

---

# Critérios de Aceite

- [ ] URL oficial HTTPS definida.
- [ ] Manifest JSON definido.
- [ ] Pacotes Linux/Windows/macOS definidos.
- [ ] Checksum obrigatório definido.
- [ ] Assinatura definida.
- [ ] Canais stable/beta definidos.
- [ ] Versionamento SemVer definido.
- [ ] Estratégia de instalação/update definida.
- [ ] Rollback definido por plataforma.
- [ ] Validações de segurança definidas.
- [ ] Testes com fakes definidos.

---

# Regras para o Codex

- Nunca aceitar HTTP.
- Nunca instalar sem checksum.
- Nunca instalar sem confirmação.
- Nunca aceitar manifest fora do schema.
- Nunca fazer downgrade automático.
- Sempre validar plataforma.
- Sempre registrar diagnóstico.
- Sempre testar com manifest fake.

---

# Definition of Done

- Manifest definido.
- Distribuição por plataforma definida.
- Segurança definida.
- Rollback definido.
- Testes definidos.

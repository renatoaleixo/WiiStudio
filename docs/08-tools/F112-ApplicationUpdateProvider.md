# F112 - Application Update Provider

> Feature ID: F112  
> Status: Implemented and Homologated  
> Priority: High  
> Epic: Application Updates / Security  
> Area: Infrastructure, Update Manager, Security, Diagnostics, Installer  
> Related: `NullApplicationUpdateProvider`, `NullApplicationUpdateInstaller`

# Objetivo

Documentar a atualização real do WiiStudio, substituindo:

```text
NullApplicationUpdateProvider
NullApplicationUpdateInstaller
```

por providers reais capazes de:

- verificar nova release;
- baixar pacote;
- validar assinatura/checksum;
- instalar com confirmação;
- permitir rollback quando aplicável;
- registrar diagnóstico.

---

# Problema Atual

O app possui abstrações para update, mas usa providers nulos.

Resultado:

```text
O sistema aparenta ter Update Manager, mas não verifica nem instala atualizações reais.
```

---

# Resultado Esperado

Fluxo:

```text
Check for updates
    ↓
Find latest release
    ↓
Show release notes/version
    ↓
User confirms download/install
    ↓
Download package
    ↓
Validate checksum/signature
    ↓
Install safely
    ↓
Restart app when needed
```

---

# Escopo

## Incluído

- update provider real;
- update installer real por plataforma;
- manifesto de atualização;
- release notes;
- download HTTPS;
- checksum obrigatório;
- assinatura quando disponível;
- confirmação do usuário;
- rollback quando possível;
- diagnóstico;
- testes com fakes.

## Fora do Escopo Inicial

- update silencioso obrigatório;
- baixar por HTTP;
- instalar sem validação;
- instalar sem confirmação;
- atualizar plugins;
- store-specific update, como Microsoft Store/App Store, no primeiro ciclo.

---

# Contratos

## IApplicationUpdateProvider

```csharp
public interface IApplicationUpdateProvider
{
    Task<ApplicationUpdateCheckResult> CheckForUpdatesAsync(
        CancellationToken cancellationToken = default);
}
```

## IApplicationUpdateInstaller

```csharp
public interface IApplicationUpdateInstaller
{
    Task<ApplicationUpdateInstallResult> DownloadAndInstallAsync(
        ApplicationUpdateInfo update,
        CancellationToken cancellationToken = default);
}
```

---

# Manifesto de Update

Fonte recomendada:

```text
updates/manifest.json
```

ou release provider equivalente.

Exemplo:

```json
{
  "latestVersion": "1.2.0",
  "channel": "stable",
  "publishedAt": "2026-01-01T10:00:00Z",
  "releaseNotesUrl": "https://...",
  "packages": [
    {
      "platform": "linux-x64",
      "url": "https://...",
      "sha256": "...",
      "signatureUrl": "https://..."
    }
  ]
}
```

---

# Canais

Suportar:

```text
stable
beta
dev
```

Configuração:

```text
Updates.Channel = stable
Updates.AutoCheck = true
Updates.AutoDownload = false
Updates.AutoInstall = false
```

---

# Estratégia por Plataforma

## Linux

Possíveis formatos:

- AppImage;
- `.deb`;
- `.rpm`;
- tarball;
- pacote gerenciado pela distro.

Regra:

- se instalado por package manager, preferir instruir update pelo package manager;
- se AppImage, baixar nova AppImage e substituir com segurança.

## Windows

Possíveis formatos:

- MSI;
- EXE installer;
- ZIP portable.

Regra:

- validar assinatura/checksum;
- pedir confirmação UAC se necessário;
- reiniciar app após atualização.

## macOS

Possíveis formatos:

- DMG;
- ZIP app bundle;
- Sparkle-like future.

Regra:

- considerar assinatura/notarização;
- validar checksum;
- orientar mover app se necessário.

---

# Segurança

Obrigatório:

- HTTPS;
- checksum obrigatório;
- assinatura recomendada;
- não instalar sem confirmação;
- não executar pacote inválido;
- não substituir app sem backup quando aplicável;
- não baixar pacote de host não confiável;
- logs redigidos;
- proteção contra path traversal;
- timeout e cancelamento.

---

# Validação

Antes de instalar:

- versão é maior que a atual;
- plataforma compatível;
- pacote baixado completo;
- SHA-256 confere;
- assinatura válida quando configurada;
- pacote está em temp seguro;
- usuário confirmou.

---

# UX

Tela de update:

```text
Nova versão disponível: 1.2.0
Versão atual: 1.1.0
Canal: stable
Notas da versão: ...
[Baixar e instalar] [Lembrar depois] [Ignorar versão]
```

Durante download:

```text
Baixando atualização... 42%
```

Antes de instalar:

```text
O WiiStudio será fechado para concluir a atualização.
Deseja continuar?
```

---

# Diagnóstico

Adicionar:

```text
Application Updates
Current version: ...
Latest version: ...
Channel: stable
Last check: ...
Last download: ...
Checksum status: ...
Installer status: ...
Last error: ...
```

---

# Rollback

Quando possível:

- preservar versão anterior;
- permitir restaurar se update falhar;
- não apagar pacote anterior até nova versão iniciar corretamente.

Primeira versão pode registrar instruções de rollback manual por plataforma.

---

# Testes Esperados

- sem atualização disponível;
- nova versão disponível;
- versão menor ignorada;
- canal stable;
- canal beta;
- download fake;
- checksum correto;
- checksum inválido;
- assinatura inválida;
- instalação cancelada;
- instalação falha;
- diagnóstico atualizado;
- rollback disponível quando suportado.

---

# Critérios de Aceite

- [x] `NullApplicationUpdateProvider` deixa de ser padrão.
- [x] `NullApplicationUpdateInstaller` deixa de ser padrão quando update real estiver habilitado.
- [x] App verifica release real via manifesto/provider.
- [x] Usuário vê versão, canal e release notes.
- [x] Download exige HTTPS.
- [x] Checksum é validado.
- [x] Instalação exige confirmação.
- [x] Diagnóstico mostra status.
- [x] Testes não usam internet real.

---

# Regras para o Codex

- Nunca instalar update sem confirmação.
- Nunca aceitar HTTP.
- Nunca instalar sem checksum.
- Nunca executar pacote não validado.
- Sempre validar plataforma.
- Sempre permitir cancelar download.
- Sempre registrar diagnóstico.
- Sempre usar fake provider nos testes.

---

# Dependências

- `F027-ApplicationUpdateManager.md`
- `F089-UpdateSecurity.md`
- `F108-ExternalToolNetworkDownloader.md`
- `F030-AdvancedDiagnostics.md`
- `F026-TaskManager.md`

---

# Definition of Done

- Provider real definido.
- Installer por plataforma documentado.
- Manifesto e validação definidos.
- Segurança definida.
- Diagnostics e testes definidos.

# F131 - Cross-Platform Installer Setup Generator

> **Suggested path:** `docs/10-release-distribution/F131-CrossPlatformInstallerSetupGenerator.md`
> **Priority:** Release / Distribution
> **Status:** Proposed technical design
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain
> **Scope type:** Release engineering only
> **Important:** This feature **does not reopen F001-F126**. It defines packaging/installer generation for the already homologated WiiStudio.

---

## 1. Objetivo da Feature

Definir o processo oficial de geração de instaladores e pacotes do **WiiStudio** para:

- Windows;
- Linux;
- macOS.

A feature deve padronizar:

- `dotnet publish` self-contained;
- estrutura de artefatos;
- instaladores por sistema;
- ícone, nome, publisher e versão;
- validação de atalhos;
- instalação/desinstalação;
- smoke tests;
- release artifacts;
- integração com scripts de release existentes.

O objetivo é transformar a build homologada do WiiStudio em pacotes distribuíveis para usuários finais, sem alterar escopo funcional já aprovado em F001-F126.

---

## 2. Princípio de Não Reabertura de Escopo

Esta feature não altera regras de negócio do WiiStudio.

Ela não deve reabrir:

- fluxos USB;
- WBFS;
- Wiimms;
- GameTDB;
- SQLite;
- plugins;
- emulador;
- UI funcional já homologada;
- critérios de aceite F001-F126.

Escopo permitido:

```text
Build
Package
Installer
Artifacts
Signing
Smoke tests
Install/uninstall validation
Release checklist
```

Escopo não permitido:

```text
Alterar comportamento funcional homologado
Adicionar novas operações de negócio
Trocar arquitetura
Reescrever recursos Avalonia
Alterar AssemblyName quebrando avares://WiiStudio.App/...
```

---

## 3. Problema que Resolve

Hoje o WiiStudio pode estar homologado funcionalmente, mas ainda precisa de uma forma oficial e reproduzível para ser distribuído como produto final.

Sem esta feature, podem ocorrer problemas como:

- cada desenvolvedor gera pacote de forma diferente;
- Windows expõe executáveis internos;
- macOS não possui bundle `.app` adequado;
- Linux não possui pacote consistente;
- checksums são gerados em momento errado;
- ícones/metadados podem faltar;
- policy Polkit pode não ser instalada/validada corretamente;
- smoke tests não garantem instalação real;
- update manifest pode apontar para artefato incorreto.

---

## 4. Escopo Funcional Obrigatório

Incluído:

- gerar binários self-contained com `dotnet publish`;
- gerar artefatos para `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`;
- gerar instalador Windows, preferencialmente **Inno Setup** no primeiro ciclo;
- avaliar **Velopack** como opção futura para instalador com auto-update;
- gerar pacote Linux inicial como **AppImage** ou `.deb`;
- gerar pacote macOS como `.app` e futuro `.dmg`;
- incluir ícone, nome `WiiStudio`, versão, publisher e atalhos;
- instalar/validar arquivos auxiliares necessários, como policy Polkit quando aplicável;
- definir estrutura em `artifacts/release/<version>/`;
- integrar com scripts existentes de release/smoke-test;
- definir testes de instalação/desinstalação por plataforma;
- não incluir jogos, ROMs, ISOs, BIOS ou chaves;
- não embutir Dolphin, Wiimms ou NKit sem política clara de licença e checksum.

---

## 5. Fora de Escopo

- Criar loja própria.
- Publicar em Microsoft Store.
- Publicar em Mac App Store.
- Publicar em Flathub no primeiro ciclo.
- Distribuir jogos, ROMs, ISOs, BIOS ou chaves.
- Distribuir Dolphin, Wiimms ou NKit sem análise de licença, fonte, checksum e autorização no manifest.
- Alterar lógica funcional homologada.
- Executar testes destrutivos em hardware real durante CI.
- Criar auto-update completo no primeiro ciclo, salvo avaliação técnica futura com Velopack.

---

## 6. Plataformas e Runtimes

Builds obrigatórias:

```text
win-x64
linux-x64
osx-x64
osx-arm64
```

Comandos base:

```bash
dotnet publish src/WiiStudio.App/WiiStudio.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o artifacts/publish/win-x64
```

```bash
dotnet publish src/WiiStudio.App/WiiStudio.App.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -o artifacts/publish/linux-x64
```

```bash
dotnet publish src/WiiStudio.App/WiiStudio.App.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -o artifacts/publish/osx-x64
```

```bash
dotnet publish src/WiiStudio.App/WiiStudio.App.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  -o artifacts/publish/osx-arm64
```

Observação:

- Avaliar trimming apenas se não quebrar Avalonia, reflection, plugins e resources.
- Primeira versão deve priorizar previsibilidade sobre tamanho mínimo.

---

## 7. Estrutura Oficial de Artefatos

Estrutura esperada:

```text
artifacts/
├── publish/
│   ├── win-x64/
│   ├── linux-x64/
│   ├── osx-x64/
│   └── osx-arm64/
└── release/
    └── <version>/
        ├── windows/
        │   ├── WiiStudioSetup-<version>-win-x64.exe
        │   ├── WiiStudio-<version>-win-x64.zip
        │   └── smoke-test-results.json
        ├── linux/
        │   ├── WiiStudio-<version>-linux-x64.AppImage
        │   ├── wiistudio_<version>_amd64.deb
        │   └── smoke-test-results.json
        ├── macos/
        │   ├── WiiStudio-<version>-macos-x64.dmg
        │   ├── WiiStudio-<version>-macos-arm64.dmg
        │   ├── WiiStudio-<version>-macos-x64.app.zip
        │   ├── WiiStudio-<version>-macos-arm64.app.zip
        │   └── smoke-test-results.json
        ├── manifests/
        │   ├── wiistudio-update-manifest.stable.json
        │   └── wiistudio-update-manifest.beta.json
        ├── checksums/
        │   ├── SHA256SUMS.txt
        │   └── SHA256SUMS.txt.sig
        └── release-notes.md
```

---

## 8. Naming e Branding

Nome público:

```text
WiiStudio
```

Windows:

```text
WiiStudio.exe
WiiStudioSetup-<version>-win-x64.exe
```

Linux:

```text
WiiStudio-<version>-linux-x64.AppImage
wiistudio_<version>_amd64.deb
```

macOS:

```text
WiiStudio.app
WiiStudio-<version>-macos-x64.dmg
WiiStudio-<version>-macos-arm64.dmg
```

Ícones:

```text
Assets/wiistudio.ico
Assets/WiiStudio.icns
Assets/wiistudio.png
```

Metadata:

```text
Product: WiiStudio
FileDescription: WiiStudio
Company/Publisher: WiiStudio ou publisher oficial
Version: <version>
Copyright: Copyright © WiiStudio
```

---

## 9. Compatibilidade com AssemblyName e Avalonia

Regra obrigatória:

```text
Não alterar AssemblyName interno se isso quebrar avares://WiiStudio.App/...
```

Se o assembly interno é:

```text
WiiStudio.App
```

e recursos Avalonia usam:

```text
avares://WiiStudio.App/...
```

o empacotamento deve resolver nome final do aplicativo por:

- rename/copy do executável final;
- bundle metadata;
- atalhos;
- installer metadata;
- package metadata.

Não deve resolver alterando o assembly de forma arriscada.

---

# 10. Windows

## 10.1 Decisão Recomendada para o Primeiro Ciclo

Usar **Inno Setup** no primeiro ciclo.

Motivos:

- simples;
- bom para instalador desktop tradicional;
- gera `.exe` familiar para usuários Windows;
- permite atalhos no Menu Iniciar e Área de Trabalho;
- permite uninstall;
- permite customizar ícone, publisher, versão e diretório de instalação;
- tem baixo atrito para primeira distribuição.

Velopack fica como opção futura para instalador com auto-update.

---

## 10.2 Artefatos Windows

Gerar:

```text
WiiStudioSetup-<version>-win-x64.exe
WiiStudio-<version>-win-x64.zip
```

ZIP portátil:

```text
WiiStudio-<version>-win-x64/
├── WiiStudio.exe
├── WiiStudio.App.dll
├── WiiStudio.App.deps.json
├── WiiStudio.App.runtimeconfig.json
├── *.dll
├── runtimes/
├── Assets/
├── docs/
└── LICENSE.txt
```

---

## 10.3 Inno Setup

Arquivo sugerido:

```text
installer/windows/WiiStudio.iss
```

Campos esperados:

```text
AppName=WiiStudio
AppVersion=<version>
AppPublisher=<publisher>
DefaultDirName={autopf}\WiiStudio
DefaultGroupName=WiiStudio
OutputBaseFilename=WiiStudioSetup-<version>-win-x64
SetupIconFile=Assets\wiistudio.ico
UninstallDisplayIcon={app}\WiiStudio.exe
```

Atalhos:

```text
Menu Iniciar: WiiStudio
Área de Trabalho: opcional
```

Regras:

- target do atalho deve ser `WiiStudio.exe`;
- working directory deve ser `{app}`;
- uninstall deve remover binários instalados;
- uninstall não deve apagar biblioteca/configurações do usuário sem confirmação.

---

## 10.4 Assinatura Digital Windows

Primeiro ciclo:

- assinatura pode ser opcional se ainda não houver certificado;
- release final comercial deve prever assinatura Authenticode.

Futuro:

```text
signtool sign /fd SHA256 /tr <timestamp-url> /td SHA256 WiiStudioSetup-<version>-win-x64.exe
```

Regras:

- assinar installer;
- idealmente assinar executável principal;
- gerar checksum depois da assinatura;
- se assinatura falhar em release final, bloquear publicação.

---

# 11. Linux

## 11.1 Decisão Recomendada para o Primeiro Ciclo

Gerar **AppImage** como pacote inicial universal e avaliar `.deb` em paralelo.

Estratégia recomendada:

```text
Primeiro ciclo:
- AppImage obrigatório
- .deb opcional/recomendado para Ubuntu/Linux Mint/Debian e policy Polkit
```

Motivos:

- AppImage simplifica distribuição inicial;
- `.deb` permite instalação mais integrada em Debian/Ubuntu/Linux Mint;
- Polkit policy e helpers podem ser mais bem tratados por `.deb`.

---

## 11.2 Artefatos Linux

```text
WiiStudio-<version>-linux-x64.AppImage
wiistudio_<version>_amd64.deb
```

AppDir esperado:

```text
WiiStudio.AppDir/
├── AppRun
├── wiistudio.desktop
├── wiistudio.png
└── usr/
    ├── bin/WiiStudio
    ├── lib/wiistudio/
    └── share/
        ├── applications/wiistudio.desktop
        ├── icons/hicolor/...
        └── polkit-1/actions/com.wiistudio.device.policy
```

---

## 11.3 Polkit e Permissões USB/WBFS

Risco importante:

- AppImage não é ideal para instalar policy Polkit em local de sistema;
- `.deb` consegue instalar policy em `/usr/share/polkit-1/actions/`;
- operações WBFS/USB podem exigir autorização administrativa;
- não armazenar senha;
- usar F105/F106 para fluxo de autorização/instalação de policy.

Regras:

- não instalar policy silenciosamente;
- policy deve ser validada;
- helper privilegiado deve aceitar apenas operações catalogadas;
- AppImage pode orientar instalar policy via F106;
- `.deb` pode instalar arquivos de policy de forma controlada, desde que o usuário instale o pacote com privilégios.

---

## 11.4 Assinatura Linux

Primeiro ciclo:

- checksum SHA-256 obrigatório;
- assinatura de checksum recomendada;
- assinatura GPG opcional/recomendada.

Futuro:

- repositório APT próprio;
- pacote `.deb` assinado;
- AppImage com atualização segura, se adotada.

---

# 12. macOS

## 12.1 Decisão Recomendada para o Primeiro Ciclo

Gerar:

```text
WiiStudio.app
WiiStudio-<version>-macos-x64.dmg
WiiStudio-<version>-macos-arm64.dmg
```

Assinatura/notarização podem entrar como requisito antes de distribuição pública ampla.

---

## 12.2 Estrutura do Bundle

```text
WiiStudio.app/
└── Contents/
    ├── Info.plist
    ├── MacOS/
    │   ├── WiiStudio
    │   ├── WiiStudio.App.dll
    │   ├── WiiStudio.App.deps.json
    │   ├── WiiStudio.App.runtimeconfig.json
    │   └── *.dylib / *.dll
    └── Resources/
        ├── WiiStudio.icns
        └── Assets/
```

Campos `Info.plist`:

```text
CFBundleName = WiiStudio
CFBundleDisplayName = WiiStudio
CFBundleIdentifier = app.wiistudio.desktop
CFBundleExecutable = WiiStudio
CFBundleIconFile = WiiStudio
CFBundleShortVersionString = <version>
CFBundleVersion = <build>
LSMinimumSystemVersion = <minimum>
```

---

## 12.3 Assinatura e Notarização macOS

Futuro requisito para release pública:

- assinar app bundle com Developer ID Application;
- assinar DMG;
- enviar para notarização;
- aplicar stapling;
- validar com Gatekeeper.

Regras:

- gerar checksum depois de assinar/notarizar;
- bloquear release pública se notarização falhar, quando esse requisito estiver ativo;
- não alterar bundle após assinatura.

---

# 13. Ferramentas de Packaging - Comparação

| Ferramenta/Formato | Plataforma | Pontos fortes | Limitações | Decisão para 1º ciclo |
|---|---|---|---|---|
| Inno Setup | Windows | Simples, maduro, gera `.exe`, atalhos/uninstall fáceis | Auto-update não é nativo | **Recomendado** |
| WiX | Windows | MSI robusto, enterprise, controle detalhado | Mais complexo, maior curva de aprendizado | Avaliar futuro |
| MSIX | Windows | Instalação limpa, package identity, atualização via App Installer | Pode exigir adaptação e assinatura/sideloading | Não usar no 1º ciclo |
| Velopack | Windows/macOS/Linux | Installer + auto-update cross-platform | Exige integração e política de update mais madura | Avaliar futuro |
| AppImage | Linux | Um arquivo, distribuição simples, sem instalação tradicional | Não instala bem policy Polkit; integração desktop variável | **Recomendado inicial** |
| .deb | Linux Debian/Ubuntu | Integração com sistema, scripts, policy Polkit | Específico para Debian/Ubuntu | Recomendado em paralelo |
| Flatpak | Linux | Sandbox, distribuição via Flathub, portals | Pode dificultar acesso a USB/raw devices/WBFS | Futuro, com análise |
| .dmg | macOS | Formato familiar, bom para `.app` | Assinatura/notarização necessárias para experiência profissional | **Recomendado** |

---

## 14. Decisão Recomendada para Primeiro Ciclo

```text
Windows:
  - Inno Setup + ZIP portátil

Linux:
  - AppImage obrigatório
  - .deb recomendado para Ubuntu/Linux Mint/Debian e policy Polkit

macOS:
  - .app + .dmg
  - assinatura/notarização preparada como requisito futuro/pré-release pública

Auto-update:
  - não implementar como obrigatório no primeiro ciclo
  - avaliar Velopack para ciclo seguinte
```

---

## 15. Dependências Externas e Conteúdo Proibido

Não incluir:

```text
jogos
ROMs
ISOs
BIOS
chaves
keys
arquivos protegidos
```

Não embutir por padrão sem política clara:

```text
Dolphin
Wiimms
NKit
```

Se algum binário externo for incluído no futuro:

- validar licença;
- registrar fonte oficial;
- registrar versão;
- registrar checksum;
- registrar assinatura, se disponível;
- incluir licença;
- declarar no manifest;
- permitir usuário ver detalhes.

---

## 16. Integração com Scripts Existentes

Scripts esperados:

```text
scripts/release/package.sh
scripts/release/smoke-test.sh
scripts/release/generate-manifest.sh
scripts/release/checksums.sh
```

Fluxo:

```text
clean
    ↓
dotnet restore
    ↓
dotnet test
    ↓
dotnet publish self-contained
    ↓
package Windows/Linux/macOS
    ↓
run smoke tests
    ↓
generate checksums
    ↓
generate manifest
    ↓
prepare release folder
```

---

## 17. Manifesto de Release/Update

O manifest deve apontar para artefatos finais:

```json
{
  "version": "1.0.0",
  "channel": "stable",
  "publishedAt": "2026-01-01T10:00:00Z",
  "artifacts": [
    {
      "platform": "win-x64",
      "kind": "inno-setup",
      "fileName": "WiiStudioSetup-1.0.0-win-x64.exe",
      "sha256": "..."
    },
    {
      "platform": "linux-x64",
      "kind": "appimage",
      "fileName": "WiiStudio-1.0.0-linux-x64.AppImage",
      "sha256": "..."
    },
    {
      "platform": "osx-arm64",
      "kind": "dmg",
      "fileName": "WiiStudio-1.0.0-macos-arm64.dmg",
      "sha256": "..."
    }
  ]
}
```

Regras:

- checksum gerado após assinatura/empacotamento final;
- artifact filename deve bater com arquivo real;
- version deve bater com assembly/app metadata;
- manifest inválido bloqueia release.

---

## 18. Rollback e Desinstalação

## Windows

- Inno Setup deve criar uninstall.
- Upgrade deve preservar dados do usuário.
- Desinstalação não deve apagar biblioteca/configurações sem confirmação.
- Rollback automático não é obrigatório no primeiro ciclo.
- Reinstalação de versão anterior pode ser caminho manual.

## Linux AppImage

- Rollback: manter AppImage anterior.
- Desinstalação: apagar AppImage e atalhos criados.
- Configurações do usuário permanecem em AppData/XDG.

## Linux .deb

- Desinstalação via package manager.
- `purge` pode remover arquivos de sistema do pacote.
- Dados do usuário não devem ser apagados sem confirmação.
- Policy Polkit instalada pelo pacote deve ser removida pelo package manager.

## macOS

- Desinstalação: mover `WiiStudio.app` para Lixeira.
- Configurações permanecem em Application Support, salvo limpeza manual.
- Rollback: substituir `.app` por versão anterior.

---

## 19. Critérios de Aceite

- [ ] `dotnet publish` self-contained gera win-x64.
- [ ] `dotnet publish` self-contained gera linux-x64.
- [ ] `dotnet publish` self-contained gera osx-x64.
- [ ] `dotnet publish` self-contained gera osx-arm64.
- [ ] Inno Setup gera `WiiStudioSetup-<version>-win-x64.exe`.
- [ ] ZIP portátil Windows contém `WiiStudio.exe`.
- [ ] Linux gera AppImage.
- [ ] Linux gera `.deb` ou plano documentado para `.deb`.
- [ ] macOS gera `WiiStudio.app`.
- [ ] macOS gera `.dmg`.
- [ ] Ícone e nome `WiiStudio` aparecem por plataforma.
- [ ] Atalhos Windows funcionam.
- [ ] Policy Polkit é instalada/validada quando aplicável.
- [ ] Nenhum jogo/ROM/ISO/BIOS/chave é empacotado.
- [ ] Dolphin/Wiimms/NKit não são embutidos sem política clara.
- [ ] Artefatos ficam em `artifacts/release/<version>/`.
- [ ] Smoke tests passam.
- [ ] Checksums são gerados.
- [ ] Manifest é gerado e validado.
- [ ] Instalação/desinstalação testada por plataforma.

---

## 20. Cenários de Teste

## Windows

- instalar com Inno Setup;
- abrir pelo Menu Iniciar;
- abrir pelo atalho desktop;
- validar nome e ícone;
- validar versão em Apps & Features;
- desinstalar;
- reinstalar;
- instalar upgrade sobre versão anterior.

## Linux AppImage

- tornar executável;
- abrir AppImage;
- validar ícone/nome;
- validar que app inicia sem runtime externo;
- validar diagnóstico de policy;
- validar que AppImage não tenta instalar policy silenciosamente.

## Linux .deb

- instalar `.deb`;
- validar desktop entry;
- validar policy Polkit, se incluída;
- abrir app;
- remover pacote;
- verificar que arquivos do pacote foram removidos.

## macOS

- abrir `.dmg`;
- copiar `WiiStudio.app`;
- abrir pelo Finder;
- validar Dock/Finder/Spotlight;
- validar `Info.plist`;
- validar assinatura/notarização quando ativa;
- mover app para Lixeira.

---

## 21. Smoke Tests por Plataforma

Smoke test mínimo:

```text
1. Instalar/abrir pacote.
2. Confirmar nome WiiStudio.
3. Confirmar ícone.
4. Confirmar janela principal.
5. Abrir Diagnóstico.
6. Exportar relatório de diagnóstico.
7. Fechar app.
8. Desinstalar/remover pacote.
```

Smoke test adicional:

```text
- Verificar recursos Avalonia.
- Verificar que não há jogos/ROMs/BIOS no pacote.
- Verificar versão exibida.
- Verificar manifest/checksum.
- Verificar logs de startup.
```

---

## 22. Riscos Conhecidos

## Windows

- SmartScreen pode alertar se installer não for assinado.
- Certificado de assinatura pode ter custo e processo próprio.
- MSIX pode exigir mudanças adicionais.

## Linux

- Acesso USB/WBFS pode exigir privilégios.
- AppImage não instala policy Polkit de forma natural.
- Flatpak pode restringir acesso a devices/raw disks.
- `.deb` é mais adequado para instalar policy, mas é distro-specific.

## macOS

- Gatekeeper pode bloquear app sem assinatura/notarização.
- `.app` não assinado pode exigir permissão manual do usuário.
- Notarização exige conta Apple Developer e processo de CI seguro.
- Alterar bundle após assinatura invalida assinatura.

---

## 23. Checklist de Homologação do Instalador

```text
[ ] Versão final definida
[ ] Build Release executado
[ ] Testes unitários aprovados
[ ] Testes integração aprovados
[ ] win-x64 publish gerado
[ ] linux-x64 publish gerado
[ ] osx-x64 publish gerado
[ ] osx-arm64 publish gerado
[ ] Windows installer gerado
[ ] Windows portable ZIP gerado
[ ] Linux AppImage gerado
[ ] Linux .deb gerado ou decisão documentada
[ ] macOS .app gerado
[ ] macOS .dmg gerado
[ ] Ícones validados
[ ] Metadata validada
[ ] Atalhos validados
[ ] Polkit validado quando aplicável
[ ] Nenhum conteúdo proibido empacotado
[ ] Checksums gerados
[ ] Manifest gerado
[ ] Smoke test Windows aprovado
[ ] Smoke test Linux aprovado
[ ] Smoke test macOS x64 aprovado
[ ] Smoke test macOS arm64 aprovado
[ ] Instalação/desinstalação Windows aprovada
[ ] Instalação/remoção Linux aprovada
[ ] Instalação/remoção macOS aprovada
[ ] Release notes anexadas
[ ] Artefatos publicados em artifacts/release/<version>/
```

---

## 24. Regras para o Codex

- Não reabrir F001-F126.
- Não alterar regras funcionais homologadas.
- Não incluir jogos, ROMs, ISOs, BIOS ou chaves.
- Não embutir Dolphin/Wiimms/NKit sem licença/checksum/manifest.
- Não gerar checksum antes do empacotamento final.
- Não publicar artefato se smoke test falhar.
- Não instalar policy Polkit silenciosamente.
- Não armazenar senha.
- Não quebrar Avalonia resources.
- Não alterar `AssemblyName` como solução de branding.
- Sempre manter Linux sem regressão.
- Sempre gerar logs de release.
- Sempre gerar artefatos versionados.

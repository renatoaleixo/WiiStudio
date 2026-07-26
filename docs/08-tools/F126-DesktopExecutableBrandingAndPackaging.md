# F126 - Desktop Executable Branding and Packaging

> **Suggested path:** `docs/08-tools/F126-DesktopExecutableBrandingAndPackaging.md`  
> **Priority:** High  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain  
> **Area:** Packaging, Desktop UX, Windows, macOS, Release Engineering  
> **Related:** F122 Application Release and Update Distribution, F124 Packaging Installer and CI/CD

---

## 1. Objetivo da Feature

Definir como o pacote desktop do WiiStudio deve ser gerado para **Windows** e **macOS**, garantindo uma experiência profissional para o usuário final com:

- nome final amigável do aplicativo;
- ícone correto;
- metadata do executável;
- bundle/app package correto;
- ZIP portátil Windows organizado;
- `.app` macOS com `Info.plist`;
- compatibilidade com Avalonia resources;
- compatibilidade com update manifest/release;
- smoke tests por plataforma.

O objetivo principal é que o usuário veja o produto como:

```text
WiiStudio
```

e não como detalhes internos de implementação, por exemplo:

```text
WiiStudio.App.exe
```

---

## 2. Problema que Resolve

Hoje o pacote Windows pode expor:

```text
WiiStudio.App.exe
```

Isso confunde o usuário final porque o nome parece técnico e vinculado à estrutura interna do projeto.

No macOS, também é necessário garantir que o app apareça corretamente como:

```text
WiiStudio.app
```

com:

- ícone correto no Finder;
- nome correto no Dock;
- metadata correta no Spotlight;
- `Info.plist` adequado;
- executável em `Contents/MacOS/WiiStudio`;
- recursos Avalonia preservados.

---

## 3. Observação Técnica Importante

Não alterar diretamente o `AssemblyName` para `WiiStudio` se isso quebrar recursos Avalonia referenciados como:

```text
avares://WiiStudio.App/...
```

O assembly interno pode continuar sendo:

```text
WiiStudio.App
```

A solução deve tratar o **nome final do aplicativo no empacotamento**, preservando compatibilidade com:

- recursos Avalonia;
- XAML;
- assets;
- compiled bindings;
- resource URIs;
- paths internos;
- dependências de assembly.

A regra principal:

```text
Assembly interno: WiiStudio.App
Nome final empacotado: WiiStudio
```

---

## 4. Escopo Funcional

Incluído:

- Branding do executável Windows.
- Renomear/copiar executável final para `WiiStudio.exe`.
- Embutir ícone Windows usando `Assets/wiistudio.ico`.
- Definir metadata Windows.
- Criar estrutura do ZIP portátil Windows.
- Preparar para futuro installer Windows.
- Criar bundle macOS `WiiStudio.app`.
- Gerar/usar ícone `WiiStudio.icns`.
- Definir `Info.plist`.
- Garantir compatibilidade `osx-x64` e `osx-arm64`.
- Integrar com `scripts/release/package.sh`.
- Integrar com manifest de update/release.
- Definir smoke tests.
- Validar visualmente nome/ícone.
- Manter Linux sem regressão.

---

## 5. Fora de Escopo

- Reescrever o projeto Avalonia.
- Alterar todos os namespaces.
- Alterar `AssemblyName` sem validação de recursos.
- Implementar assinatura/codesign completa neste documento.
- Implementar notarização macOS completa neste documento.
- Criar store package, Microsoft Store ou Mac App Store.
- Alterar runtime behavior da aplicação.
- Alterar identidade interna usada por logs/configurações sem plano de migração.

---

## 6. Regras de Negócio

- O usuário final deve ver **WiiStudio** como nome do app.
- O executável final Windows deve ser `WiiStudio.exe`.
- O bundle macOS deve ser `WiiStudio.app`.
- O assembly interno pode permanecer `WiiStudio.App`.
- O empacotamento não pode quebrar URIs Avalonia `avares://WiiStudio.App/...`.
- O update manifest deve apontar para artefatos finais com nomes amigáveis.
- O ZIP portátil Windows deve abrir com um executável claro.
- Atalhos futuros devem apontar para `WiiStudio.exe`.
- No macOS, `CFBundleExecutable` deve apontar para o binário dentro do bundle, preferencialmente `WiiStudio`.

---

## 7. Regras de Segurança

- Não baixar ícones ou metadata em tempo de build sem fonte controlada.
- Ícones oficiais devem estar versionados no repositório.
- O script de package não deve executar comandos arbitrários vindos de input externo.
- Paths de saída devem ser controlados.
- Não sobrescrever arquivos fora do diretório de build/release.
- Não quebrar assinatura/codesign quando ela for adicionada.
- Qualquer renomeação deve ocorrer antes da geração de checksum/update manifest.
- Checksums devem ser gerados sobre os artefatos finais.

---

# 8. Windows

## 8.1 Nome Final Esperado

O executável final distribuído ao usuário deve ser:

```text
WiiStudio.exe
```

Mesmo que o output original do publish seja:

```text
WiiStudio.App.exe
```

---

## 8.2 Ícone Embutido

Ícone oficial:

```text
Assets/wiistudio.ico
```

O projeto Windows deve configurar:

```xml
<ApplicationIcon>Assets/wiistudio.ico</ApplicationIcon>
```

ou equivalente no `.csproj` do projeto executável.

Exemplo conceitual:

```xml
<PropertyGroup>
  <ApplicationIcon>Assets/wiistudio.ico</ApplicationIcon>
  <Product>WiiStudio</Product>
  <AssemblyTitle>WiiStudio</AssemblyTitle>
  <FileDescription>WiiStudio</FileDescription>
  <Company>WiiStudio</Company>
  <Copyright>Copyright © WiiStudio</Copyright>
  <Version>1.0.0</Version>
  <FileVersion>1.0.0.0</FileVersion>
  <InformationalVersion>1.0.0</InformationalVersion>
</PropertyGroup>
```

---

## 8.3 Metadata Windows

A build final deve preencher:

| Campo | Valor esperado |
|---|---|
| Product | `WiiStudio` |
| FileDescription | `WiiStudio` |
| Company | `WiiStudio` ou nome oficial do publisher |
| Version | versão SemVer atual |
| FileVersion | versão Windows compatível, exemplo `1.0.0.0` |
| ProductVersion | versão SemVer atual |
| Copyright | copyright oficial |
| OriginalFilename | preferencialmente `WiiStudio.exe` |

---

## 8.4 Preservar AssemblyName Interno

Se o `.csproj` usa:

```xml
<AssemblyName>WiiStudio.App</AssemblyName>
```

e os recursos Avalonia referenciam:

```text
avares://WiiStudio.App/...
```

não alterar para:

```xml
<AssemblyName>WiiStudio</AssemblyName>
```

sem revisar e testar todos os recursos.

Solução recomendada:

```text
1. Publicar normalmente com assembly interno WiiStudio.App.
2. No estágio de package, copiar/renomear o executável para WiiStudio.exe.
3. Preservar DLLs, deps.json, runtimeconfig.json e resources necessários.
4. Garantir que a aplicação execute normalmente pelo novo nome.
```

---

## 8.5 Renomear/Copiar Executável no Empacotamento

No `scripts/release/package.sh`, após `dotnet publish` Windows:

```bash
PUBLISH_DIR="artifacts/publish/win-x64"
if [ -f "$PUBLISH_DIR/WiiStudio.App.exe" ]; then
  cp "$PUBLISH_DIR/WiiStudio.App.exe" "$PUBLISH_DIR/WiiStudio.exe"
fi
```

Regras:

- Não apagar o executável original até validar execução.
- O ZIP final deve expor `WiiStudio.exe` na raiz.
- Se manter `WiiStudio.App.exe` causar confusão, pode removê-lo do pacote final somente após validar que `WiiStudio.exe` inicia corretamente.
- `WiiStudio.exe` deve continuar encontrando `.dll`, `.deps.json`, `.runtimeconfig.json` e assets.

---

## 8.6 Estrutura Final do ZIP Portátil Windows

Estrutura esperada:

```text
WiiStudio-<version>-win-x64/
├── WiiStudio.exe
├── WiiStudio.App.dll
├── WiiStudio.App.deps.json
├── WiiStudio.App.runtimeconfig.json
├── *.dll
├── runtimes/
├── Assets/
├── tools/
│   └── README.md
├── docs/
│   └── README.md
└── LICENSE.txt
```

Observações:

- Se self-contained single-file for adotado no futuro, a estrutura pode mudar.
- O ZIP deve evitar mostrar `WiiStudio.App.exe` para o usuário final.
- Se `WiiStudio.App.exe` for mantido internamente por necessidade técnica, ele não deve ser o atalho principal.

---

## 8.7 Futuro Instalador Windows

Quando houver installer:

- Nome no Start Menu: `WiiStudio`.
- Atalho desktop: `WiiStudio`.
- Target: `WiiStudio.exe`.
- Ícone do atalho: `WiiStudio.exe` ou `.ico` oficial.
- Nome em Apps & Features: `WiiStudio`.
- Publisher: publisher oficial.
- Upgrade deve preservar configurações do usuário.
- Uninstall deve remover binários, mas não apagar biblioteca/configurações sem confirmação.

---

## 8.8 Atalhos no Menu Iniciar e Área de Trabalho

Atalhos esperados:

```text
Start Menu/WiiStudio/WiiStudio.lnk
Desktop/WiiStudio.lnk, opcional
```

Propriedades:

| Campo | Valor |
|---|---|
| Nome | `WiiStudio` |
| Target | `WiiStudio.exe` |
| Working Directory | pasta de instalação |
| Icon | `WiiStudio.exe` ou `Assets/wiistudio.ico` |

---

## 8.9 Validação de RT_ICON e RT_GROUP_ICON

Smoke test Windows deve validar que o executável final contém recursos de ícone:

```text
RT_ICON
RT_GROUP_ICON
```

Ferramentas possíveis:

- PowerShell script;
- Resource Hacker, quando disponível;
- sigcheck/resource dump em pipeline Windows;
- teste manual em Properties > Details/Icon.

Critério:

```text
WiiStudio.exe deve exibir o ícone oficial no Explorer.
```

---

## 8.10 Cache de Ícones do Windows Explorer

O Windows Explorer pode manter cache de ícones antigo.

Instruções para validação manual:

- renomear pasta de teste;
- limpar cache de ícones se necessário;
- reiniciar Explorer;
- testar em pasta nova;
- validar em Propriedades do arquivo;
- validar em atalho novo.

Mensagem de troubleshooting:

```text
Se o Explorer ainda mostrar o ícone antigo, pode ser cache do Windows. Crie um novo atalho ou reinicie o Explorer.
```

---

# 9. macOS

## 9.1 Nome Final Esperado

Bundle final:

```text
WiiStudio.app
```

Executável interno:

```text
WiiStudio.app/Contents/MacOS/WiiStudio
```

Ícone:

```text
WiiStudio.app/Contents/Resources/WiiStudio.icns
```

Metadata:

```text
WiiStudio.app/Contents/Info.plist
```

---

## 9.2 Estrutura do Bundle

Estrutura esperada:

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

Observação:

- A estrutura final pode variar conforme `dotnet publish`, self-contained ou single-file.
- O binário principal visível ao macOS deve ser `WiiStudio`.

---

## 9.3 Ícone `.icns`

Ícone oficial macOS:

```text
Assets/WiiStudio.icns
```

ou gerado a partir do ícone oficial em alta resolução.

Requisitos recomendados:

- 16x16
- 32x32
- 128x128
- 256x256
- 512x512
- 1024x1024

Fluxo conceitual de geração:

```text
source png/svg
    ↓
iconset
    ↓
iconutil -c icns WiiStudio.iconset
    ↓
WiiStudio.icns
```

---

## 9.4 Info.plist

Arquivo:

```text
WiiStudio.app/Contents/Info.plist
```

Campos obrigatórios/recomendados:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
 "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>WiiStudio</string>

  <key>CFBundleDisplayName</key>
  <string>WiiStudio</string>

  <key>CFBundleIdentifier</key>
  <string>app.wiistudio.desktop</string>

  <key>CFBundleExecutable</key>
  <string>WiiStudio</string>

  <key>CFBundleIconFile</key>
  <string>WiiStudio</string>

  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>

  <key>CFBundleVersion</key>
  <string>1.0.0</string>

  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>

  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
```

---

## 9.5 Compatibilidade osx-x64 e osx-arm64

Builds esperados:

```text
osx-x64
osx-arm64
```

Artefatos:

```text
WiiStudio-<version>-macos-x64.app
WiiStudio-<version>-macos-arm64.app
```

ou DMGs:

```text
WiiStudio-<version>-macos-x64.dmg
WiiStudio-<version>-macos-arm64.dmg
```

Futuro universal:

```text
WiiStudio-<version>-macos-universal.dmg
```

---

## 9.6 Comportamento Esperado no Finder, Dock e Spotlight

Finder:

```text
Nome exibido: WiiStudio.app
Ícone exibido: oficial WiiStudio
```

Dock:

```text
Nome exibido: WiiStudio
Ícone exibido: oficial WiiStudio
```

Spotlight:

```text
Busca por: WiiStudio
Resultado: WiiStudio.app
Ícone: oficial
```

---

## 9.7 Futuro DMG/PKG

DMG futuro deve conter:

```text
WiiStudio.app
Applications symlink
README opcional
```

PKG futuro pode ser avaliado se for necessário instalar helpers/policies.

---

## 9.8 Notarização e Codesign

Futuro requisito para release profissional:

- codesign do `.app`;
- codesign de binários nativos;
- notarização Apple;
- stapling do ticket;
- validação Gatekeeper.

Esta feature deve preparar a estrutura para codesign, mas não exige implementar a assinatura completa agora.

---

## 9.9 Preservar Recursos Avalonia

Não alterar indevidamente o assembly interno.

Se os recursos usam:

```text
avares://WiiStudio.App/...
```

o app bundle pode se chamar:

```text
WiiStudio.app
```

e o executável de bootstrap pode se chamar:

```text
WiiStudio
```

desde que:

- `WiiStudio.App.dll` continue presente;
- deps/runtimeconfig estejam corretos;
- resources Avalonia continuem resolvendo pelo assembly correto;
- smoke test abra telas com assets e styles.

---

# 10. Impactos no `scripts/release/package.sh`

O script de release deve adicionar etapas:

## Windows

```text
dotnet publish win-x64
copy/rename WiiStudio.App.exe -> WiiStudio.exe
validate executable exists
validate icon resource
create portable ZIP with WiiStudio.exe
generate sha256
update manifest
```

## macOS

```text
dotnet publish osx-x64/osx-arm64
create WiiStudio.app structure
copy binaries to Contents/MacOS
copy WiiStudio.icns to Contents/Resources
generate Info.plist
chmod +x Contents/MacOS/WiiStudio
validate bundle
zip/dmg artifact
generate sha256
update manifest
```

## Linux

Garantir sem regressão:

```text
Linux package continua usando nome WiiStudio
Desktop file continua válido
AppImage/deb/rpm continuam funcionando
```

---

# 11. Impactos no Manifesto de Update/Release

O manifest deve usar nomes finais:

Windows:

```json
{
  "platform": "win-x64",
  "fileName": "WiiStudio-1.0.0-win-x64.zip",
  "mainExecutable": "WiiStudio.exe"
}
```

macOS:

```json
{
  "platform": "osx-arm64",
  "fileName": "WiiStudio-1.0.0-macos-arm64.dmg",
  "bundleName": "WiiStudio.app",
  "bundleIdentifier": "app.wiistudio.desktop"
}
```

Linux:

```json
{
  "platform": "linux-x64",
  "fileName": "WiiStudio-1.0.0-linux-x64.AppImage"
}
```

Checksums devem ser calculados após o package final.

---

# 12. Persistência e Configurações

Não há persistência de usuário direta.

Configurações de build/release:

```text
Branding.AppDisplayName = WiiStudio
Branding.WindowsExecutableName = WiiStudio.exe
Branding.MacBundleName = WiiStudio.app
Branding.MacBundleIdentifier = app.wiistudio.desktop
Branding.IconWindows = Assets/wiistudio.ico
Branding.IconMac = Assets/WiiStudio.icns
```

---

# 13. Logs, Tarefas e Notificações Esperadas

Essa feature atua no build/package, não no runtime normal.

Logs de CI/CD esperados:

```text
Branding Windows executable
Embedding Windows icon
Creating Windows portable ZIP
Creating macOS app bundle
Generating Info.plist
Validating app icon
Validating package artifact
Generating checksum
```

Notificações runtime não se aplicam.

---

# 14. Critérios de Aceite

## Windows

- [x] ZIP portátil contém `WiiStudio.exe`.
- [x] Usuário não precisa executar `WiiStudio.App.exe`.
- [x] `WiiStudio.exe` inicia corretamente.
- [x] Recursos Avalonia carregam normalmente.
- [x] Ícone oficial aparece no Explorer.
- [x] Metadata Product/FileDescription exibe `WiiStudio`.
- [x] `RT_ICON` e `RT_GROUP_ICON` existem.
- [x] Start Menu/Desktop shortcuts futuros usam `WiiStudio.exe`.

## macOS

- [x] Artefato contém `WiiStudio.app`.
- [x] Bundle contém `Contents/MacOS/WiiStudio`.
- [x] Bundle contém `Contents/Resources/WiiStudio.icns`.
- [x] Bundle contém `Contents/Info.plist`.
- [x] `CFBundleName = WiiStudio`.
- [x] `CFBundleDisplayName = WiiStudio`.
- [x] `CFBundleExecutable = WiiStudio`.
- [x] Finder mostra `WiiStudio.app`.
- [x] Dock mostra `WiiStudio`.
- [x] Recursos Avalonia carregam normalmente.

## Geral

- [x] `scripts/release/package.sh` gera nomes finais corretos.
- [x] Manifest de update aponta para artefatos finais.
- [x] Checksums são gerados após empacotamento.
- [x] Linux não sofre regressão.
- [x] Smoke tests por plataforma passam.

---

# 15. Cenários de Teste

## Windows

- publicar `win-x64`;
- verificar existência de `WiiStudio.exe`;
- executar `WiiStudio.exe`;
- abrir tela principal;
- abrir tela que usa recursos Avalonia;
- verificar ícone no Explorer;
- verificar metadata em Properties;
- criar atalho e validar ícone.

## macOS

- publicar `osx-x64`;
- publicar `osx-arm64`;
- criar `WiiStudio.app`;
- executar via Finder;
- executar via terminal:

```bash
open WiiStudio.app
```

- validar Finder;
- validar Dock;
- validar Spotlight, quando indexado;
- validar assets Avalonia;
- validar `Info.plist`.

## Linux

- executar package existente;
- garantir que alteração de scripts não afete AppImage/deb/rpm;
- validar desktop file;
- validar ícone Linux.

---

# 16. Smoke Tests por Plataforma

## Windows Smoke Test

```text
1. Extrair ZIP.
2. Confirmar WiiStudio.exe na raiz.
3. Executar WiiStudio.exe.
4. Confirmar janela principal.
5. Confirmar ícone no Explorer.
6. Confirmar Product/FileDescription.
7. Abrir tela com assets Avalonia.
```

## macOS Smoke Test

```text
1. Abrir artefato.
2. Confirmar WiiStudio.app.
3. Executar pelo Finder.
4. Confirmar ícone no Dock.
5. Confirmar nome WiiStudio.
6. Abrir tela com assets Avalonia.
7. Verificar Info.plist.
```

## Linux Smoke Test

```text
1. Executar AppImage/deb/rpm gerado.
2. Confirmar nome WiiStudio.
3. Confirmar ícone.
4. Confirmar que recursos Avalonia carregam.
```

---

# 17. Como Validar Visualmente Ícone e Nome do App

## Windows

- Explorer em modo ícones grandes.
- Propriedades do arquivo.
- Atalho no Desktop.
- Menu Iniciar, quando installer existir.
- Barra de tarefas.

## macOS

- Finder.
- Dock.
- Cmd+Tab.
- Spotlight.
- Get Info.

## Linux

- App launcher.
- Desktop file.
- Dock/panel do ambiente gráfico.
- AppImage launcher.

---

# 18. Como Tratar Cache de Ícones do Sistema

## Windows

O Explorer pode exibir ícone antigo por cache.

Ações:

```text
Criar nova pasta de teste
Recriar atalho
Reiniciar Explorer
Limpar IconCache, se necessário
```

## macOS

O Finder/Dock pode manter cache.

Ações:

```text
Alterar versão do bundle
Mover app para outra pasta
Reiniciar Finder/Dock
Validar com Get Info
```

## Linux

Ambientes desktop podem manter cache.

Ações:

```text
Atualizar desktop database
Atualizar icon cache
Logout/login, se necessário
```

---

# 19. Compatibilidade com Linux sem Regressão

Essa feature não deve quebrar:

- AppImage;
- `.deb`;
- `.rpm`;
- desktop file;
- ícone Linux;
- políticas Polkit;
- caminhos de ferramentas externas;
- recursos Avalonia.

O package script deve tratar Linux separadamente e não aplicar rename Windows/macOS indevidamente.

---

# 20. Casos de Erro e Mensagens para o Build/CI

| Caso | Mensagem |
|---|---|
| Ícone Windows ausente | `Assets/wiistudio.ico não encontrado.` |
| Ícone macOS ausente | `Assets/WiiStudio.icns não encontrado.` |
| Executável original ausente | `WiiStudio.App.exe não foi encontrado no publish output.` |
| Rename falhou | `Não foi possível gerar WiiStudio.exe.` |
| Info.plist inválido | `Info.plist inválido ou incompleto.` |
| CFBundleExecutable ausente | `CFBundleExecutable aponta para arquivo inexistente.` |
| Recurso Avalonia falhou | `Smoke test falhou ao carregar recursos Avalonia.` |
| Manifest desatualizado | `Update manifest não aponta para o artefato final.` |

---

# 21. Regras para o Codex

- Não alterar `AssemblyName` sem revisar `avares://WiiStudio.App/...`.
- Preferir solução de packaging para nome final.
- Não quebrar recursos Avalonia.
- Não gerar checksum antes do empacotamento final.
- Não publicar artefato com `WiiStudio.App.exe` como executável principal.
- Não gerar macOS bundle sem `Info.plist`.
- Não gerar macOS bundle sem `.icns`.
- Não quebrar Linux.
- Sempre incluir smoke tests.
- Sempre validar nome e ícone visualmente quando possível.

---

# 22. Definition of Done

- Windows package expõe `WiiStudio.exe`.
- Windows executable contém ícone e metadata.
- macOS package expõe `WiiStudio.app`.
- macOS bundle contém `Info.plist` e `WiiStudio.icns`.
- Avalonia resources continuam funcionando.
- `scripts/release/package.sh` atualizado.
- Update manifest usa nomes finais.
- Smoke tests definidos e documentados.
- Linux permanece sem regressão.

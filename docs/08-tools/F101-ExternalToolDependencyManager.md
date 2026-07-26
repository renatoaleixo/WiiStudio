# F101 - External Tool Dependency Manager

> Feature ID: F101  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: External Dependencies / Infrastructure  
> Suggested Area: Core Infrastructure, Diagnostics, Settings, Security

# Objetivo

Criar o **External Tool Dependency Manager** do WiiStudio, responsável por detectar,
validar, configurar e instalar, quando seguro, dependências externas usadas pela aplicação.

O foco inicial é suportar:

- Wiimms ISO Tools: `wit` e `wwt`;
- NKit Legacy Tool, quando o usuário precisar importar/restaurar arquivos `.nkit.iso`.

A feature deve facilitar o uso para o usuário final, mas **nunca** deve baixar,
instalar ou executar ferramenta externa silenciosamente.

---

# Estado Atual

## Wiimms

O sistema já procura `wit` e `wwt` em:

1. caminho configurado;
2. `PATH`;
3. `tools/`;
4. pasta da aplicação.

Se não encontrar, lança:

```text
WiimmsNotInstalledException
```

## NKit

A arquitetura já prevê NKit e existe configuração:

```text
NKitExecutablePath
```

Mas ainda faltam:

- `INKitToolLocator`;
- `NKitService`;
- `NKitToolInstaller`, caso exista fonte confiável;
- diagnóstico específico;
- fluxo NKit -> ISO -> Wiimms;
- persistência e validação do executável.

---

# Resultado Esperado

Criar uma tela/seção chamada:

```text
Gerenciador de Dependências Externas
```

ou:

```text
External Tool Manager
```

Ela deve exibir:

```text
Wiimms WIT      Instalado       versão detectada
Wiimms WWT      Instalado       versão detectada
NKit            Não configurado
```

Quando uma ferramenta não estiver disponível, o usuário deve poder:

- selecionar executável manualmente;
- instalar automaticamente, quando houver fonte confiável;
- abrir instruções manuais;
- executar diagnóstico novamente.

---

# Fontes Verificadas para Download

Esta seção define as origens consideradas confiáveis para o primeiro ciclo de implementação.

Regra importante:

- A aplicação pode exibir e usar estas origens como fonte de download.
- A instalação automática só pode ser habilitada quando houver `sha256` preenchido no manifesto interno do WiiStudio.
- Se o checksum não estiver disponível ou não estiver pinado no manifesto da aplicação, o botão de instalação automática deve ficar desabilitado.
- O usuário ainda pode usar instalação manual selecionando o executável.

---

## Wiimms ISO Tools

Fonte oficial:

```text
https://wit.wiimm.de/
```

Página oficial de downloads:

```text
https://wit.wiimm.de/download.html
```

Código-fonte oficial:

```text
https://github.com/Wiimm/wiimms-iso-tools
```

Download de código-fonte mantido pelo autor:

```text
https://download.wiimm.de/source/wiimms-iso-tools
```

Versão estável verificada na página oficial no momento da documentação:

```text
WIT v3.05a, 2022-08-27
```

Downloads oficiais da versão estável:

```text
Linux x86_64:
https://wit.wiimm.de/download/wit-v3.05a-r8638-x86_64.tar.gz

Linux i386:
https://wit.wiimm.de/download/wit-v3.05a-r8638-i386.tar.gz

Windows 64-bit / Cygwin:
https://wit.wiimm.de/download/wit-v3.05a-r8638-cygwin64.zip

Windows 32-bit / Cygwin:
https://wit.wiimm.de/download/wit-v3.05a-r8638-cygwin32.zip

macOS universal binaries:
https://wit.wiimm.de/download/wit-v3.05a-r8638-mac.tar.gz
```

Regras para Wiimms:

- `wit` e `wwt` devem ser extraídos/instalados juntos.
- A aplicação deve validar os dois executáveis.
- A aplicação deve executar `wit --version` e `wwt --version` via `IProcessRunner`.
- A aplicação deve manter os links em um manifesto interno versionado.
- A aplicação deve validar SHA-256 antes de extrair.
- Se o checksum esperado não estiver cadastrado, a instalação automática deve ser bloqueada.

Exemplo de manifesto interno:

```json
{
  "toolId": "wiimms",
  "displayName": "Wiimms ISO Tools",
  "trustedSource": "https://wit.wiimm.de/download.html",
  "version": "3.05a-r8638",
  "packages": [
    {
      "platform": "linux-x64",
      "url": "https://wit.wiimm.de/download/wit-v3.05a-r8638-x86_64.tar.gz",
      "sha256": "TO_BE_PINNED_BY_WIISTUDIO_MAINTAINERS"
    },
    {
      "platform": "win-x64",
      "url": "https://wit.wiimm.de/download/wit-v3.05a-r8638-cygwin64.zip",
      "sha256": "TO_BE_PINNED_BY_WIISTUDIO_MAINTAINERS"
    },
    {
      "platform": "osx-universal",
      "url": "https://wit.wiimm.de/download/wit-v3.05a-r8638-mac.tar.gz",
      "sha256": "TO_BE_PINNED_BY_WIISTUDIO_MAINTAINERS"
    }
  ]
}
```

---

## NKit

Fonte oficial do NKit 2:

```text
https://github.com/Nanook/NKit
```

Wiki oficial:

```text
https://github.com/Nanook/NKit/wiki
```

Documentação de uso:

```text
https://github.com/Nanook/NKit/wiki/Usage
```

Página de features:

```text
https://github.com/Nanook/NKit/wiki/Features
```

Repositório legado NKit v1:

```text
https://github.com/Nanook/NKitv1
```

Situação verificada:

- O repositório oficial `Nanook/NKit` existe e documenta o NKit 2.
- A wiki informa que o NKit 2 é multiplataforma e processa formatos como ISO, WBFS, RVZ e NKit.
- O repositório oficial não deve ser tratado automaticamente como pacote binário instalável enquanto não houver artefato de release/checksum confiável definido.
- O repositório `Nanook/NKitv1` é legado e também não deve ser usado para instalação automática sem pacote validado e checksum pinado.
- O WiiStudio recebeu um pacote legado local `NKit_v1.4.zip`, mantido em `tools/packages/`, para instalação controlada após confirmação do usuário.
- O pacote local deve ser tratado como binário legado pinado pelo projeto, não como download remoto automático.

Regra para NKit:

```text
Auto-download de NKit deve ficar desabilitado por padrão até existir uma fonte binária confiável + checksum pinado no manifesto do WiiStudio.
```

Fluxo permitido inicialmente:

- detectar NKit por caminho configurado;
- permitir seleção manual do executável;
- permitir instalação local a partir de `tools/packages/NKit_v1.4.zip` somente após confirmação do usuário;
- validar SHA-256 do pacote local antes de extrair;
- validar versão/comando via `IProcessRunner`;
- salvar `NKitExecutablePath` em Settings somente após validação;
- exibir instruções para instalação manual;
- bloquear download automático se não houver pacote confiável com SHA-256.

Pacote local pinado:

```text
tools/packages/NKit_v1.4.zip
```

SHA-256:

```text
e600b0a2dbacf784779ce33b01259f197c5f8cfc7135f1831160d3f98bb431c6
```

Conteúdo mínimo esperado:

- `RecoverToISO.exe`;
- `ConvertToISO.exe`;
- `NKit.dll`;
- `NKitProcessingApp.exe`;
- `Readme.txt`.

Observação de plataforma:

- Windows: suporte esperado para o pacote legado.
- Linux/macOS: suporte experimental/manual, dependente de Mono ou Wine.

Exemplo de manifesto interno para NKit ainda bloqueado:

```json
{
  "toolId": "nkit",
  "displayName": "NKit",
  "trustedSource": "https://github.com/Nanook/NKitv1",
  "localPackagePath": "tools/packages/NKit_v1.4.zip",
  "localPackageSha256": "e600b0a2dbacf784779ce33b01259f197c5f8cfc7135f1831160d3f98bb431c6",
  "autoDownloadEnabled": false,
  "localInstallEnabled": true,
  "manualConfigurationOnly": false,
  "platformNotes": "Windows supported. Linux/macOS require Mono or Wine and are treated as experimental."
}
```

---

## Política para Fontes de Terceiros

Sites de terceiros, mirrors, fóruns e reuploads não devem ser usados por padrão para instalação automática.

Podem ser exibidos apenas como referência manual se a equipe decidir documentar,
mas não devem ser tratados como origem confiável pelo instalador sem:

- autoria clara;
- pacote versionado;
- checksum SHA-256;
- validação manual;
- aprovação no Security Checklist;
- registro no manifesto interno da aplicação.

---

## Atualização dos Links

Os links acima devem ser tratados como dados de configuração versionados.

Recomendação:

```text
src/WiiStudio.Infrastructure/ExternalTools/trusted-tools.json
```

Esse arquivo deve conter:

- toolId;
- nome;
- versão;
- plataforma;
- URL;
- SHA-256 esperado;
- origem;
- data de verificação;
- status de auto-install.

Nunca espalhar URLs hardcoded pelo código.


---

# Escopo

## Incluído

- detecção de `wit`;
- detecção de `wwt`;
- detecção de NKit;
- validação de versão;
- instalação manual por seleção de executável;
- instalação automática com confirmação;
- validação de checksum;
- origem oficial/confiável dos downloads;
- extração para `tools/wiimms/` e `tools/nkit/`;
- persistência dos caminhos em Settings;
- diagnóstico de versão;
- integração com Security, ProcessRunner, Settings e Logging;
- fallback offline;
- testes com downloader/extractor fake.

## Fora do Escopo Inicial

- instalação silenciosa;
- download de origem não confiável;
- execução de shell arbitrário;
- atualização automática sem confirmação;
- uso obrigatório de package manager do sistema;
- download real no pipeline de testes.

---

# Requisitos Funcionais

## RF001 - Detectar Wiimms WIT

Localizar `wit` na seguinte ordem:

1. Settings;
2. `PATH`;
3. `tools/wiimms/`;
4. `tools/`;
5. pasta da aplicação.

---

## RF002 - Detectar Wiimms WWT

Localizar `wwt` na seguinte ordem:

1. Settings;
2. `PATH`;
3. `tools/wiimms/`;
4. `tools/`;
5. pasta da aplicação.

---

## RF003 - Detectar NKit

Localizar NKit na seguinte ordem:

1. Settings;
2. `tools/nkit/`;
3. `tools/`;
4. pasta da aplicação;
5. caminho selecionado manualmente.

---

## RF004 - Validar Versão

Executar validação via `IProcessRunner`.

Exemplos:

```text
wit --version
wwt --version
```

Para NKit, o comando depende do executável configurado.

---

## RF005 - Instalação Manual

Fluxo:

```text
Selecionar executável
    ↓
Validar path
    ↓
Validar arquivo
    ↓
Executar comando de versão via ProcessRunner
    ↓
Salvar caminho em Settings
    ↓
Atualizar diagnóstico
```

---

## RF006 - Instalação Automática com Confirmação

Antes de instalar, exibir:

- ferramenta;
- versão;
- origem do download;
- checksum esperado;
- pasta de instalação;
- aviso de que uma ferramenta externa será baixada;
- confirmação explícita do usuário.

---

## RF007 - Checksum Obrigatório

Todo pacote baixado deve validar SHA-256 antes de extração/uso.

Falha de checksum deve:

- bloquear instalação;
- registrar log;
- exibir mensagem amigável;
- não alterar instalação anterior.

---

## RF008 - Origem Oficial ou Confiável

Cada ferramenta deve possuir uma definição de origem baseada na seção
**Fontes Verificadas para Download**.

Exemplo:

```json
{
  "toolId": "wiimms",
  "displayName": "Wiimms ISO Tools",
  "downloadUrl": "https://wit.wiimm.de/download/wit-v3.05a-r8638-x86_64.tar.gz",
  "trustedSource": "https://wit.wiimm.de/download.html",
  "sha256": "TO_BE_PINNED_BY_WIISTUDIO_MAINTAINERS",
  "version": "3.05a-r8638",
  "isTrusted": true
}
```

Para NKit, o download automático deve permanecer desabilitado enquanto não houver
pacote binário confiável e checksum pinado no manifesto interno do WiiStudio.

O pacote local `tools/packages/NKit_v1.4.zip` pode ser usado para instalação local
controlada, desde que o checksum pinado seja validado antes da extração.

---

## RF009 - Extração para tools/

Destinos:

```text
tools/wiimms/
tools/nkit/
```

Regras:

- baixar para diretório temporário;
- validar checksum;
- extrair em temporário;
- impedir zip slip/path traversal;
- validar executável extraído;
- mover para destino final apenas após sucesso;
- preservar instalação anterior quando possível.

---

## RF010 - Persistência em Settings

Salvar:

```text
ExternalTools.Wiimms.WitPath
ExternalTools.Wiimms.WwtPath
ExternalTools.Wiimms.Version
ExternalTools.NKit.ExecutablePath
ExternalTools.NKit.Version
ExternalTools.LastCheckAt
```

---

## RF011 - Diagnóstico

Advanced Diagnostics deve exibir:

- ferramenta;
- status;
- versão;
- caminho detectado;
- origem da detecção;
- último erro;
- última verificação;
- ação recomendada.

---

## RF012 - Fallback Offline

Quando estiver offline:

- não tentar download;
- mostrar status offline;
- permitir seleção manual;
- permitir usar ferramenta já instalada;
- exibir instruções.

---

## RF013 - Integração com `.nkit.iso`

Fluxo esperado:

```text
Arquivo .nkit.iso
    ↓
Verificar NKit
    ↓
Restaurar para ISO
    ↓
Validar ISO
    ↓
Usar Wiimms quando necessário
```

Se NKit não estiver disponível, abrir caminho para configurar ou instalar.

---

# Contratos Propostos

## IExternalToolManager

```csharp
public interface IExternalToolManager
{
    Task<IReadOnlyList<ExternalToolStatus>> CheckAllAsync(
        CancellationToken cancellationToken = default);

    Task<ExternalToolStatus> CheckAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default);

    Task<ExternalToolStatus> ConfigureManualPathAsync(
        ExternalToolManualPathRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## IExternalToolLocator

```csharp
public interface IExternalToolLocator
{
    Task<ExternalToolLocationResult> LocateAsync(
        ExternalToolDefinition definition,
        CancellationToken cancellationToken = default);
}
```

---

## IExternalToolInstaller

```csharp
public interface IExternalToolInstaller
{
    Task<ExternalToolInstallResult> InstallAsync(
        ExternalToolInstallRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## IExternalToolDownloader

```csharp
public interface IExternalToolDownloader
{
    Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        CancellationToken cancellationToken = default);
}
```

---

## IChecksumValidator

```csharp
public interface IChecksumValidator
{
    Task<ChecksumValidationResult> ValidateSha256Async(
        string filePath,
        string expectedSha256,
        CancellationToken cancellationToken = default);
}
```

---

## IArchiveExtractor

```csharp
public interface IArchiveExtractor
{
    Task<ArchiveExtractionResult> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
```

---

# Modelos Conceituais

## ExternalToolState

```csharp
public enum ExternalToolState
{
    Unknown,
    NotInstalled,
    Installed,
    Configured,
    InvalidPath,
    InvalidVersion,
    DownloadAvailable,
    DownloadUnavailable,
    Installing,
    InstallFailed,
    UpdateAvailable,
    Offline
}
```

---

## ExternalToolDefinition

```csharp
public sealed class ExternalToolDefinition
{
    public string ToolId { get; init; } = string.Empty;
    public string DisplayNameKey { get; init; } = string.Empty;
    public IReadOnlyList<string> ExecutableNames { get; init; } = [];
    public IReadOnlyList<string> SearchDirectories { get; init; } = [];
    public string? VersionArgument { get; init; }
    public ExternalToolPackageSource? PackageSource { get; init; }
    public bool IsRequired { get; init; }
}
```

---

## ExternalToolStatus

```csharp
public sealed class ExternalToolStatus
{
    public string ToolId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public ExternalToolState State { get; init; }
    public string? ExecutablePath { get; init; }
    public string? Version { get; init; }
    public string? DetectionSource { get; init; }
    public string? ErrorMessageKey { get; init; }
    public DateTimeOffset CheckedAt { get; init; }
}
```

---

## ExternalToolPackageSource

```csharp
public sealed class ExternalToolPackageSource
{
    public string DownloadUrl { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public bool IsTrusted { get; init; }
}
```

---

# Estrutura Recomendada

```text
src/
├── WiiStudio.Application/
│   └── ExternalTools/
│       ├── IExternalToolManager.cs
│       ├── IExternalToolLocator.cs
│       ├── IExternalToolInstaller.cs
│       ├── ExternalToolDefinition.cs
│       ├── ExternalToolStatus.cs
│       └── ExternalToolState.cs
│
├── WiiStudio.Infrastructure/
│   └── ExternalTools/
│       ├── ExternalToolManager.cs
│       ├── ExternalToolLocator.cs
│       ├── ExternalToolInstaller.cs
│       ├── ExternalToolDownloader.cs
│       ├── ChecksumValidator.cs
│       ├── ArchiveExtractor.cs
│       ├── WiimmsToolLocator.cs
│       ├── WiimmsToolInstaller.cs
│       ├── NKitToolLocator.cs
│       └── NKitToolInstaller.cs
│
└── WiiStudio.UI/
    └── Views/
        └── ExternalTools/
            ├── ExternalToolManagerView.axaml
            └── ExternalToolManagerViewModel.cs
```

---

# UI Sugerida

```text
Configurações
└── Dependências Externas
    ├── Wiimms ISO Tools
    │   ├── wit: Instalado / Não instalado
    │   ├── wwt: Instalado / Não instalado
    │   ├── versão
    │   ├── caminho
    │   ├── Selecionar manualmente
    │   └── Instalar automaticamente
    └── NKit
        ├── status
        ├── versão
        ├── caminho
        ├── Selecionar manualmente
        └── Instalar automaticamente, se disponível
```

---

# Mensagens de UI

## Não instalado

```text
A ferramenta não foi encontrada. Você pode selecionar o executável manualmente ou instalar automaticamente.
```

## Confirmação

```text
O WiiStudio irá baixar uma ferramenta externa de uma origem confiável, validar o checksum e instalar em tools/. Deseja continuar?
```

## Checksum inválido

```text
O pacote baixado não passou na validação de integridade. A instalação foi bloqueada.
```

## Offline

```text
Não foi possível baixar a ferramenta porque você está offline. Selecione o executável manualmente.
```

---

# Fluxo de Instalação Automática

```text
User clicks Install
    ↓
Show confirmation with source/checksum/version
    ↓
User confirms
    ↓
Download package to temp
    ↓
Validate SHA-256
    ↓
Extract to temp
    ↓
Validate executable exists
    ↓
Move to tools/{tool}
    ↓
Run version command through ProcessRunner
    ↓
Save path to Settings
    ↓
Update Diagnostics
```

---

# Integrações Obrigatórias

## Settings

Persistir paths, versões e última verificação.

## Security

Aplicar:

- Safe Path Handling;
- External Process Security;
- Log Redaction;
- Update Security;
- Security Checklist.

## ProcessRunner

Toda validação de versão deve usar `IProcessRunner`.

## Logging

Registrar eventos sem vazar dados sensíveis.

## Diagnostics

Exibir status consolidado em Advanced Diagnostics.

## Task Manager

Instalação/download pode aparecer como tarefa com progresso e cancelamento.

---

# Falha Segura

Se qualquer etapa falhar:

- não usar pacote;
- não salvar path inválido;
- não remover instalação anterior;
- limpar temporários quando seguro;
- registrar logs;
- exibir erro amigável;
- permitir tentativa manual.

---

# Testes Obrigatórios

## Unitários

- localizar `wit` em Settings;
- localizar `wit` no PATH fake;
- localizar `wit` em `tools/wiimms/`;
- não encontrar `wit`;
- localizar `wwt`;
- localizar NKit;
- path inválido;
- versão inválida;
- checksum correto;
- checksum inválido;
- offline;
- usuário cancela instalação.

## Integração

- instalação com downloader fake;
- extração fake;
- checksum fake;
- ProcessRunner fake;
- settings persistidas;
- diagnóstico atualizado;
- falha não remove instalação anterior.

## Segurança

- bloquear zip slip;
- bloquear path traversal;
- bloquear execução sem ProcessRunner;
- redigir logs;
- bloquear origem não confiável;
- não instalar sem confirmação.

---

# Critérios de Aceite

- [x] External Tool Manager definido.
- [x] Detecção de `wit` definida.
- [x] Detecção de `wwt` definida.
- [x] Detecção de NKit definida.
- [x] Instalação manual definida.
- [x] Instalação automática com confirmação definida.
- [x] Checksum obrigatório definido.
- [x] Origem confiável definida.
- [x] Extração para `tools/` definida.
- [x] Persistência em Settings definida.
- [x] Diagnóstico de versão definido.
- [x] Integração com Security definida.
- [x] Integração com ProcessRunner definida.
- [x] Integração com Logging definida.
- [x] Fallback offline definido.
- [x] Testes com fakes definidos.

---

# Regras para o Codex

- Nunca instalar ferramenta silenciosamente.
- Sempre pedir confirmação antes de download/instalação.
- Sempre validar checksum.
- Sempre validar origem.
- Sempre extrair em diretório temporário primeiro.
- Sempre bloquear zip slip/path traversal.
- Sempre validar executável após extração.
- Sempre executar versão via ProcessRunner.
- Sempre salvar path somente após validação.
- Sempre registrar logs redigidos.
- Sempre usar fake downloader/extractor nos testes.
- Nunca baixar arquivo real no pipeline.
- Nunca usar Process.Start diretamente.
- Nunca executar shell arbitrário.
- Nunca remover instalação anterior antes da nova estar validada.

---

# Dependências

- `F023-SettingsManager.md`
- `F030-AdvancedDiagnostics.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F089-UpdateSecurity.md`
- `F090-SecurityChecklist.md`
- `F095-ProcessRunnerTesting.md`
- `012-WiimmsIntegration.md`
- `013-NKitLegacy.md`

---

# Definition of Done

- External Tool Dependency Manager documentado.
- Contratos definidos.
- Fluxo manual definido.
- Fluxo automático seguro definido.
- Wiimms e NKit cobertos.
- Integrações com Settings, Security, ProcessRunner, Logging e Diagnostics definidas.
- Testes com fakes definidos.
- Documentação atualizada.

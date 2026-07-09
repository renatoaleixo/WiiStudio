# F105 - Cross-Platform Privileged Device Access

> Feature ID: F105  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Device Access / Security / External Tools  
> Area: Infrastructure, Security, Diagnostics, Device Manager, External Tool Manager

# Objetivo

Definir como o WiiStudio deve acessar dispositivos USB/WBFS raw com privilégios elevados
em **Linux**, **Windows** e **macOS**, sem armazenar senha do usuário e mantendo
segurança, UX e diagnósticos claros.

O WiiStudio deve delegar autenticação ao sistema operacional e executar apenas operações
conhecidas e seguras, inicialmente com as ferramentas Wiimms:

```text
wit
wwt
```

---

# Problema

Operações com dispositivos USB, discos físicos, partições WBFS e caminhos raw podem
exigir permissões elevadas dependendo do sistema operacional.

Exemplos:

- Linux: acesso a `/dev/sdX`, `/dev/disk/by-id/*` ou partições raw pode exigir Polkit/pkexec, regra de policy ou autorização administrativa.
- Windows: acesso a discos físicos/removíveis pode exigir execução como Administrador.
- macOS: acesso a `/dev/disk*` pode exigir permissões, autorização ou estar limitado por sandbox/notarização.

O WiiStudio não deve pedir, capturar, trafegar nem armazenar senha.

---

# Escopo

## Incluído

- criar abstração `PrivilegedAccessService` ou `DevicePermissionService`;
- detectar o sistema operacional em runtime;
- Linux com Polkit/pkexec;
- Linux com policy própria e `auth_admin_keep`;
- detectar se a policy do WiiStudio está instalada;
- orientar instalação da policy quando ausente;
- Windows com detecção de execução como Administrador;
- Windows com orientação para reiniciar elevado ou solicitar elevação via sistema;
- macOS com detecção de permissão insuficiente para `/dev/disk*`;
- integração com Wiimms ISO Tools;
- integração com External Tool Manager;
- integração com Diagnóstico;
- mensagens claras na UI;
- retry após autorização;
- timeout e cancelamento;
- testes unitários por abstração.

## Fora do Escopo Inicial

- armazenar senha;
- implementar driver próprio;
- executar comando elevado arbitrário;
- instalar policy/helper silenciosamente;
- elevar plugins diretamente;
- manter toda a aplicação sempre elevada;
- burlar permissões do sistema operacional.

---

# Princípios

- Nunca salvar senha.
- Nunca capturar senha.
- Nunca trafegar senha pelo WiiStudio.
- Delegar autenticação ao sistema operacional.
- Elevar apenas a operação necessária.
- Permitir somente comandos conhecidos.
- Validar executável antes da elevação.
- Validar argumentos antes da elevação.
- Não usar shell arbitrário.
- Usar `IProcessRunner` ou runner privilegiado controlado.
- Registrar logs sem dados sensíveis.
- Permitir retry após autorização.

---

# Arquitetura Recomendada

```text
UI / Device Manager
    ↓
Application
    ↓
IDevicePermissionService
    ↓
IPrivilegedAccessService
    ↓
Platform Strategy
    ↓
ProcessRunner / Privileged Runner
    ↓
wit / wwt
```

A UI nunca deve chamar `pkexec`, UAC, `/dev/disk*`, `wit` ou `wwt` diretamente.

---

# Abstrações

## IDevicePermissionService

```csharp
public interface IDevicePermissionService
{
    Task<DevicePermissionStatus> CheckAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default);

    Task<DevicePermissionStatus> RequestAuthorizationAsync(
        DevicePermissionRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## IPrivilegedAccessService

```csharp
public interface IPrivilegedAccessService
{
    Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken = default);

    Task<PrivilegedProcessResult> RunKnownToolAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## IOperatingSystemPrivilegeStrategy

```csharp
public interface IOperatingSystemPrivilegeStrategy
{
    OSPlatformKind Platform { get; }

    Task<PrivilegedAccessStatus> CheckStatusAsync(
        PrivilegedAccessRequest request,
        CancellationToken cancellationToken = default);

    Task<PrivilegedProcessResult> RunAsync(
        PrivilegedToolRequest request,
        CancellationToken cancellationToken = default);
}
```

---

# Modelos Conceituais

## OSPlatformKind

```csharp
public enum OSPlatformKind
{
    Linux,
    Windows,
    MacOS,
    Unknown
}
```

## PrivilegedAccessState

```csharp
public enum PrivilegedAccessState
{
    Unknown,
    NotRequired,
    Required,
    Authorized,
    AuthorizationRequired,
    PolicyMissing,
    HelperMissing,
    PermissionDenied,
    Unsupported,
    Failed
}
```

## DevicePermissionStatus

```csharp
public sealed class DevicePermissionStatus
{
    public OSPlatformKind Platform { get; init; }
    public PrivilegedAccessState State { get; init; }
    public bool CanRetry { get; init; }
    public bool RequiresUserAction { get; init; }
    public string? MessageKey { get; init; }
    public string? RecommendedActionKey { get; init; }
    public string? DiagnosticDetails { get; init; }
}
```

## PrivilegedToolRequest

```csharp
public sealed class PrivilegedToolRequest
{
    public string ToolId { get; init; } = string.Empty;
    public string ExecutablePath { get; init; } = string.Empty;
    public IReadOnlyList<string> Arguments { get; init; } = [];
    public string? WorkingDirectory { get; init; }
    public TimeSpan Timeout { get; init; }
    public string? DevicePath { get; init; }
    public string? CorrelationId { get; init; }
}
```

---

# Detecção de Sistema Operacional

O serviço deve detectar em runtime:

```csharp
OperatingSystem.IsLinux()
OperatingSystem.IsWindows()
OperatingSystem.IsMacOS()
```

Para testes, essa detecção deve ser isolada em uma abstração:

```csharp
public interface IOperatingSystemInfo
{
    OSPlatformKind Current { get; }
}
```

---

# Linux

## Estratégia

No Linux, a estratégia recomendada é usar:

```text
Polkit / pkexec
```

O WiiStudio deve suportar uma policy própria com:

```text
auth_admin_keep
```

para permitir que o sistema operacional reutilize a autorização quando a configuração
do Polkit permitir.

---

## Policy Polkit

Nome sugerido da action:

```text
com.wiistudio.device.raw-access
```

Arquivo sugerido:

```text
/usr/share/polkit-1/actions/com.wiistudio.device.policy
```

Exemplo conceitual:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<policyconfig>
  <vendor>WiiStudio</vendor>
  <vendor_url>https://example.local/wiistudio</vendor_url>

  <action id="com.wiistudio.device.raw-access">
    <description>Allow WiiStudio to access Wii USB/WBFS devices</description>
    <message>Authentication is required to access Wii USB/WBFS devices</message>
    <defaults>
      <allow_any>auth_admin</allow_any>
      <allow_inactive>auth_admin</allow_inactive>
      <allow_active>auth_admin_keep</allow_active>
    </defaults>
  </action>
</policyconfig>
```

A aplicação não deve instalar essa policy silenciosamente. Se estiver ausente, deve
orientar o usuário ou o instalador/pacote oficial.

---

## Detecção da Policy

Verificar:

```text
/usr/share/polkit-1/actions/com.wiistudio.device.policy
```

Também pode ser usado diagnóstico com `pkcheck`, se aplicável.

Estados possíveis:

```text
PolicyInstalled
PolicyMissing
PkexecMissing
AuthorizationRequired
Authorized
PermissionDenied
```

---

## Execução com pkexec

Fluxo recomendado:

```text
Device operation requires privilege
    ↓
Validate tool: wit/wwt
    ↓
Validate executable path
    ↓
Validate arguments
    ↓
Run via pkexec/helper controlled by WiiStudio
    ↓
OS prompts user
    ↓
Run known operation
    ↓
Capture result
```

Não passar comando arbitrário diretamente para `pkexec`.

---

## Helper Controlado

Abordagem recomendada:

```text
pkexec /usr/lib/wiistudio/wiistudio-device-helper --operation list-wbfs --tool wwt --device /dev/sdc
```

O helper deve:

- aceitar somente operações conhecidas;
- aceitar somente `wit` e `wwt` inicialmente;
- validar argumentos;
- não abrir shell;
- retornar stdout/stderr/exit code;
- respeitar timeout/cancelamento quando possível;
- registrar auditoria segura.

---

## UX no Linux

Policy ausente:

```text
O WiiStudio precisa de uma policy Polkit para acessar dispositivos USB/WBFS.
Instale a policy do WiiStudio e tente novamente.
```

Autorização necessária:

```text
O sistema solicitará autorização de administrador para acessar o dispositivo.
O WiiStudio não armazena sua senha.
```

Autorizado:

```text
Autorização concedida. Tentando acessar o dispositivo novamente.
```

---

# Windows

## Estratégia

No Windows, acesso a discos físicos/removíveis pode exigir execução como Administrador.

O WiiStudio deve:

- detectar se o processo está elevado;
- detectar falhas por acesso negado;
- informar que privilégio de Administrador é necessário;
- orientar reiniciar como Administrador;
- considerar solicitação de elevação via sistema;
- integrar com Wiimms ISO Tools para Windows.

---

## Discos Físicos

Operações potencialmente privilegiadas:

```text
\.\PhysicalDriveN
\.\X:
```

ou acesso raw equivalente usado pelo Wiimms.

---

## Detector de Elevação

```csharp
public interface IWindowsElevationDetector
{
    bool IsRunningAsAdministrator();
}
```

O resultado deve alimentar `DevicePermissionStatus`.

---

## UX no Windows

Processo não elevado:

```text
O acesso ao dispositivo pode exigir privilégios de Administrador.
Reinicie o WiiStudio como Administrador e tente novamente.
```

Acesso negado:

```text
O Windows negou acesso ao dispositivo. Execute o WiiStudio como Administrador ou verifique as permissões do dispositivo.
```

---

## Solicitação de Elevação

Opções:

1. instrução manual para executar como Administrador;
2. botão futuro **Reiniciar como Administrador**;
3. solicitação de elevação via sistema, se implementada com segurança.

Primeira versão recomendada:

```text
detectar estado + orientar claramente + permitir retry
```

---

# macOS

## Estratégia

No macOS, acesso a dispositivos raw pode envolver:

```text
/dev/disk*
```

O WiiStudio deve:

- detectar permissão insuficiente;
- orientar autorização segura;
- considerar limitações de sandbox/notarização;
- não instalar helper privilegiado silenciosamente;
- nunca armazenar senha.

---

## Detecção

Detectar:

- erro de permissão ao acessar `/dev/disk*`;
- ferramenta externa retorna erro de acesso;
- ambiente sandbox limita acesso;
- app não possui permissão necessária;
- dispositivo não pode ser aberto.

---

## UX no macOS

Permissão negada:

```text
O macOS bloqueou o acesso ao dispositivo. Pode ser necessário conceder permissão ou executar uma operação autorizada pelo sistema.
```

Sandbox/notarização:

```text
Esta instalação do WiiStudio pode estar limitada pelo sandbox do macOS. Consulte o diagnóstico para instruções de acesso a discos externos.
```

---

## Helper Privilegiado Futuro

Pode ser considerado no futuro, desde que:

- não seja instalado silenciosamente;
- exija confirmação;
- seja assinado/notarizado quando aplicável;
- execute apenas operações conhecidas;
- não aceite comandos arbitrários;
- registre auditoria.

Primeira versão:

```text
detectar + orientar + diagnosticar
```

---

# Integração com External Tool Manager

Antes de qualquer operação privilegiada:

```text
Check External Tools
    ↓
Validate wit/wwt installed
    ↓
Validate version
    ↓
Validate executable path
    ↓
Check device permission
    ↓
Request authorization if needed
    ↓
Run known operation
```

Depende da feature:

```text
F101 - External Tool Dependency Manager
```

---

# Integração com ProcessRunner

Regras:

- toda execução deve passar por `IProcessRunner` ou runner privilegiado controlado;
- stdout/stderr devem ser capturados;
- timeout obrigatório;
- cancelamento obrigatório;
- logs passam por redaction;
- exit code deve ser preservado.

---

# Comandos Permitidos

Inicialmente permitir somente:

```text
wit
wwt
```

Catálogo de operações:

```csharp
public enum PrivilegedDeviceOperation
{
    ListDevices,
    InspectDevice,
    ListWbfsGames,
    ReadWbfsMetadata
}
```

Proibido:

- shell;
- comando arbitrário;
- argumento livre sem validação;
- operação destrutiva sem confirmação adicional;
- execução privilegiada solicitada por plugin sem política específica.

---

# Validação de Executável

Antes de elevar:

- path existe;
- path é arquivo;
- path foi validado pelo External Tool Manager;
- toolId é `wit` ou `wwt`;
- versão foi validada;
- path está normalizado;
- path não contém traversal.

---

# Validação de Argumentos

Antes de elevar:

- argumentos são lista estruturada;
- nenhum shell;
- nenhum `&&`, `|`, `;`, redirect;
- device path validado;
- operação está no catálogo permitido;
- timeout definido;
- cancellation token disponível.

---

# Segurança

Obrigatório:

- nunca salvar senha;
- nunca capturar senha;
- nunca trafegar senha;
- não executar comandos elevados arbitrários;
- permitir somente ferramentas conhecidas;
- validar caminho do executável;
- validar argumentos;
- registrar logs sem dados sensíveis;
- timeout obrigatório;
- cancelamento obrigatório;
- não baixar nem instalar componente privilegiado silenciosamente;
- falha segura.

---

# Diagnóstico

Adicionar seção:

```text
Privileged Device Access
```

Campos:

```text
Platform: Linux/Windows/macOS
Required: true/false
State: Authorized/AuthorizationRequired/PolicyMissing/PermissionDenied
Policy installed: true/false
Running as Administrator: true/false
Tool: wit/wwt
Tool path valid: true/false
Last check: ...
Last error: ...
Recommended action: ...
```

---

# Diagnóstico por Plataforma

## Linux

```text
Policy Polkit do WiiStudio instalada: Sim/Não
pkexec disponível: Sim/Não
Autorização ativa: Sim/Não
Ação recomendada: instalar policy ou autorizar via sistema
```

## Windows

```text
Executando como Administrador: Sim/Não
Acesso ao disco permitido: Sim/Não
Ação recomendada: reiniciar como Administrador
```

## macOS

```text
Acesso a /dev/disk permitido: Sim/Não
Sandbox detectado: Sim/Não
Ação recomendada: conceder permissão ou consultar instruções
```

---

# Fluxo de Retry

```text
Operation returns AuthorizationRequired
    ↓
Show OS-specific message
    ↓
User authorizes/restarts/grants permission
    ↓
Retry button enabled
    ↓
Run permission check again
    ↓
If authorized, continue operation
```

---

# UX Recomendada

Texto geral:

```text
Algumas operações com dispositivos USB/WBFS exigem autorização do sistema operacional.
O WiiStudio não armazena sua senha.
```

Botões:

- Verificar permissões;
- Tentar novamente;
- Abrir diagnóstico;
- Abrir instruções;
- Reiniciar como Administrador, futuro/Windows;
- Instalar policy/helper, somente via pacote confiável e com confirmação.

---

# Logging

Registrar:

- plataforma;
- estado de privilégio;
- policy/helper presente;
- ferramenta usada;
- operação solicitada;
- resultado;
- exit code;
- timeout;
- cancelamento.

Não registrar:

- senha;
- token;
- payload sensível;
- comando completo com paths sensíveis desnecessários.

---

# Tratamento de Erros

## Linux Policy Ausente

Status:

```text
PolicyMissing
```

Ação:

- orientar instalação;
- permitir retry;
- não tentar bypass.

## Linux pkexec Ausente

Status:

```text
HelperMissing
```

Ação:

- orientar instalar pacote necessário;
- exibir diagnóstico.

## Windows Sem Admin

Status:

```text
AuthorizationRequired
```

Ação:

- orientar reiniciar como Administrador;
- permitir retry após reinício.

## macOS Sem Permissão

Status:

```text
AuthorizationRequired
```

Ação:

- orientar autorização/permissão;
- exibir instruções.

## Acesso Negado

Status:

```text
PermissionDenied
```

Ação:

- mensagem clara;
- diagnóstico;
- retry quando fizer sentido.

---

# Impacto no Código

## Criar

```text
IDevicePermissionService
IPrivilegedAccessService
IOperatingSystemPrivilegeStrategy
IOperatingSystemInfo
LinuxPolkitPrivilegeStrategy
WindowsPrivilegeStrategy
MacOSPrivilegeStrategy
IWindowsElevationDetector
PrivilegedToolRequest
DevicePermissionStatus
PrivilegedAccessStatus
PrivilegedAccessState
PrivilegedDeviceOperation
```

## Alterar

```text
DeviceManager
WiimmsIntegration
ExternalToolManager
AdvancedDiagnostics
ProcessRunner integration
Settings, se necessário
```

## Futuro

```text
wiistudio-device-helper
Polkit policy package
Windows elevated restart helper
macOS privileged helper
```

---

# Testes Esperados

## Unitários

- detectar Linux via abstração;
- detectar Windows via abstração;
- detectar macOS via abstração;
- Linux policy instalada;
- Linux policy ausente;
- Linux pkexec ausente;
- Linux autorização requerida;
- Windows processo elevado;
- Windows processo não elevado;
- Windows erro de acesso negado;
- macOS erro de permissão;
- macOS sandbox detectado;
- senha nunca é solicitada/salva;
- comando desconhecido é bloqueado;
- executável inválido é bloqueado;
- argumento inseguro é bloqueado;
- timeout tratado;
- cancelamento tratado.

## Integração com Fakes

- fake Polkit strategy;
- fake Windows elevation detector;
- fake macOS permission checker;
- fake ProcessRunner;
- fake External Tool Manager;
- diagnostics preenchido corretamente;
- retry após autorização fake.

---

# Critérios de Aceite

- [ ] Linux reconhece policy Polkit instalada.
- [ ] Linux informa quando policy está ausente.
- [ ] Linux reaproveita autorização quando o sistema permitir.
- [ ] Windows informa corretamente quando precisa de Administrador.
- [ ] macOS informa corretamente quando precisa de permissão/elevado.
- [ ] Diagnóstico mostra estado de permissão por plataforma.
- [ ] `PrivilegedAccessService` ou `DevicePermissionService` definido.
- [ ] Sistema operacional é detectado em runtime.
- [ ] Nenhuma senha é armazenada.
- [ ] Nenhuma senha trafega pelo WiiStudio.
- [ ] Somente `wit`/`wwt` são permitidos inicialmente.
- [ ] Caminho do executável é validado.
- [ ] Timeout e cancelamento funcionam.
- [ ] Retry após autorização funciona.
- [ ] Testes unitários cobrem Linux, Windows e macOS por abstração.

---

# Regras para o Codex

- Nunca salvar senha.
- Nunca criar prompt próprio para senha.
- Nunca executar comando elevado arbitrário.
- Sempre validar ferramenta.
- Sempre validar argumentos.
- Sempre usar ProcessRunner/abstração controlada.
- Sempre detectar OS por abstração testável.
- Sempre integrar com Diagnostics.
- Sempre mostrar mensagem clara ao usuário.
- Sempre permitir retry.
- Nunca instalar policy/helper silenciosamente.
- Nunca ignorar erro de permissão.
- Nunca usar shell arbitrário.
- Nunca permitir plugin solicitar privilégio elevado diretamente.

---

# Dependências

- `F001-DetectUsbDrive.md`
- `010-USBDetection.md`
- `011-WBFS.md`
- `012-WiimmsIntegration.md`
- `F030-AdvancedDiagnostics.md`
- `F035-DeviceManager.md`
- `F081-SecurityFoundation.md`
- `F082-FileSystemPermissions.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F090-SecurityChecklist.md`
- `F101-ExternalToolDependencyManager.md`

---

# Definition of Done

- Estratégia cross-platform documentada.
- Abstrações de privilégio definidas.
- Linux Polkit/pkexec definido.
- Windows Admin/UAC definido.
- macOS `/dev/disk*` definido.
- Segurança definida.
- Diagnóstico definido.
- Testes por abstração definidos.
- Nenhuma senha é armazenada ou trafegada pelo WiiStudio.

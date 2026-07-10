# F107 - CrossPlatformUsbDeviceProvider

> Feature ID: F107  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Device Manager / Cross-Platform Infrastructure  
> Area: Linux, Windows, macOS, USB Detection, WBFS, Diagnostics  
> Depends on: F105 - Cross-Platform Privileged Device Access

# Objetivo

Criar providers reais e separados por plataforma para listar dispositivos USB, discos
físicos, partições e filesystems em Linux, Windows e macOS.

A feature deve permitir que o WiiStudio detecte dispositivos compatíveis com Wii,
incluindo partições WBFS, FAT32, NTFS e discos raw, com diagnóstico claro de permissões.

---

# Problema Atual

O provider real está mais focado em Linux.

Para Windows e macOS funcionarem de verdade, é necessário criar providers específicos
que entendam:

- como listar discos físicos;
- como listar volumes/partições;
- como identificar USB/removível;
- como detectar filesystem;
- como detectar permissão insuficiente;
- como integrar com Wiimms e Device Permission Service.

---

# Resultado Esperado

A aplicação deve usar uma abstração comum:

```text
IUsbDeviceProvider
```

E registrar uma implementação agregadora:

```text
CrossPlatformUsbDeviceProvider
```

que delega para:

```text
LinuxUsbDeviceProvider
WindowsUsbDeviceProvider
MacOSUsbDeviceProvider
```

---

# Arquitetura

```text
DeviceManager
    ↓
IUsbDeviceProvider
    ↓
CrossPlatformUsbDeviceProvider
    ↓
IPlatformUsbDeviceProvider
    ├── LinuxUsbDeviceProvider
    ├── WindowsUsbDeviceProvider
    └── MacOSUsbDeviceProvider
```

A UI não deve conter lógica específica de sistema operacional.

---

# Contratos

## IUsbDeviceProvider

```csharp
public interface IUsbDeviceProvider
{
    Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(
        CancellationToken cancellationToken = default);
}
```

## IPlatformUsbDeviceProvider

```csharp
public interface IPlatformUsbDeviceProvider
{
    OSPlatformKind Platform { get; }

    Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(
        CancellationToken cancellationToken = default);
}
```

---

# Modelo de Dispositivo

```csharp
public sealed class UsbDeviceInfo
{
    public string DeviceId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? PhysicalPath { get; init; }
    public string? MountPath { get; init; }
    public bool IsUsb { get; init; }
    public bool IsRemovable { get; init; }
    public long? SizeBytes { get; init; }
    public IReadOnlyList<UsbPartitionInfo> Partitions { get; init; } = [];
    public DevicePermissionStatus? PermissionStatus { get; init; }
}
```

## UsbPartitionInfo

```csharp
public sealed class UsbPartitionInfo
{
    public string PartitionId { get; init; } = string.Empty;
    public string? DevicePath { get; init; }
    public string? MountPath { get; init; }
    public string? FileSystem { get; init; }
    public long? SizeBytes { get; init; }
    public long? FreeBytes { get; init; }
    public bool IsWbfsCandidate { get; init; }
    public bool IsWritable { get; init; }
}
```

---

# Filesystems Relevantes

Detectar quando possível:

```text
WBFS
FAT32
exFAT
NTFS
HFS+
APFS
Unknown
```

Regras:

- WBFS pode não aparecer como filesystem comum do SO;
- FAT32/NTFS/exFAT podem armazenar arquivos `.wbfs`;
- raw WBFS pode exigir Wiimms para confirmação;
- filesystem desconhecido não deve ser tratado automaticamente como seguro para escrita.

---

# Linux Provider

## Estratégia

Usar fontes seguras e controladas:

- `/sys/block`;
- `/dev/disk/by-id`;
- `lsblk` com saída JSON, se disponível;
- `findmnt`, se necessário;
- Wiimms `wwt`/`wit` para confirmação WBFS, quando aplicável.

## Dados Esperados

- nome do disco;
- caminho físico;
- se é USB/removível;
- partições;
- mount points;
- filesystem;
- tamanho;
- espaço livre;
- permissão.

## Regras

- não depender apenas de mount point;
- detectar USB/removível;
- detectar partições;
- detectar permissão insuficiente;
- integrar com Polkit/F105;
- não executar comandos arbitrários.

---

# Windows Provider

## Estratégia

Listar discos usando APIs/abstrações compatíveis com Windows.

Fontes possíveis:

- WMI/CIM;
- APIs de volume;
- `DriveInfo` para volumes montados;
- integração controlada com Wiimms;
- caminhos como `\\.\PhysicalDriveN`, quando permitido.

## Informações Desejadas

- disco físico;
- removível;
- USB;
- volumes;
- letras de unidade;
- filesystem;
- tamanho;
- espaço livre;
- permissão/admin required.

## Permissão

Se a operação exigir Administrador:

```text
DevicePermissionStatus.AuthorizationRequired
```

A UI deve orientar o usuário a reiniciar como Administrador.

---

# macOS Provider

## Estratégia

Listar discos usando:

- `diskutil list -plist`;
- `diskutil info -plist`;
- mount points;
- paths `/dev/disk*`;
- integração controlada com Wiimms quando disponível.

## Regras

- interpretar plist com parser seguro;
- detectar external/removable;
- detectar filesystem;
- detectar permissão insuficiente;
- considerar sandbox/notarização;
- não assumir acesso raw disponível.

---

# Integração com Wiimms

Quando o SO não identificar WBFS claramente, usar Wiimms:

```text
wwt list
wit list
```

Somente após:

- ferramenta validada pelo External Tool Manager;
- path validado;
- permissão avaliada;
- comando permitido;
- timeout definido.

---

# Integração com DevicePermissionService

Cada provider deve preencher:

```text
DevicePermissionStatus
```

Exemplos:

- NotRequired;
- AuthorizationRequired;
- PermissionDenied;
- PolicyMissing;
- Unsupported.

---

# Diagnóstico

Device Manager deve mostrar:

```text
Platform: Linux/Windows/macOS
Provider: LinuxUsbDeviceProvider
Devices found: 2
WBFS candidates: 1
Permission issues: 1
Wiimms available: true/false
Last scan: ...
Last error: ...
```

Por dispositivo:

```text
Nome
Path físico
Mount path
Filesystem
Tamanho
Espaço livre
Permissão
Ação recomendada
```

---

# UI

Exibir:

- lista de dispositivos;
- partições;
- tipo de filesystem;
- espaço livre;
- status de permissão;
- botão de atualizar;
- botão de diagnóstico;
- botão de solicitar autorização quando aplicável.

---

# Segurança

Obrigatório:

- validar paths;
- não escrever durante scan;
- não executar comando arbitrário;
- usar ProcessRunner;
- timeout/cancelamento;
- logs sem dados sensíveis;
- não tratar disco desconhecido como alvo seguro;
- bloquear escrita sem `F110-SafeUsbWriteWorkflow`.

---

# Testes Esperados

## Unitários

- seleciona provider Linux;
- seleciona provider Windows;
- seleciona provider macOS;
- Linux parse `lsblk` fake;
- Windows parse WMI fake;
- macOS parse `diskutil` fake;
- filesystem FAT32;
- filesystem NTFS;
- filesystem WBFS candidato;
- permissão insuficiente;
- sem dispositivos;
- erro de provider não derruba UI.

## Integração com Fakes

- fake ProcessRunner;
- fake OperatingSystemInfo;
- fake DevicePermissionService;
- fake ExternalToolManager;
- diagnóstico consolidado.

---

# Critérios de Aceite

- [ ] `CrossPlatformUsbDeviceProvider` criado.
- [ ] Linux provider implementado/definido.
- [ ] Windows provider implementado/definido.
- [ ] macOS provider implementado/definido.
- [ ] Discos físicos são listados quando permitido.
- [ ] Partições são listadas.
- [ ] FAT32/NTFS/exFAT são detectados quando possível.
- [ ] WBFS candidate é detectado.
- [ ] Permissão insuficiente é reportada.
- [ ] Device Manager exibe diagnóstico.
- [ ] Testes cobrem Linux, Windows e macOS com fakes.

---

# Regras para o Codex

- Não misturar lógica de plataforma na UI.
- Usar providers por plataforma.
- Usar abstração testável para OS.
- Não escrever no dispositivo durante scan.
- Não executar shell arbitrário.
- Usar Wiimms somente via ProcessRunner.
- Integrar com F105 para permissão.
- Integrar com F101 para ferramentas externas.
- Não assumir que filesystem desconhecido é seguro.

---

# Dependências

- `F001-DetectUsbDrive.md`
- `F035-DeviceManager.md`
- `F083-ExternalProcessSecurity.md`
- `F101-ExternalToolDependencyManager.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F110-SafeUsbWriteWorkflow.md`

---

# Definition of Done

- Provider cross-platform definido.
- Providers Linux/Windows/macOS definidos.
- Diagnóstico definido.
- Segurança definida.
- Testes com fakes definidos.

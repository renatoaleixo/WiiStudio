# F106 - Polkit Policy Installer

> Feature ID: F106  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: Device Access / Linux Privileged Access  
> Area: Linux, Security, Diagnostics, Installer, Device Manager  
> Depends on: F105 - Cross-Platform Privileged Device Access

# Objetivo

Criar um fluxo guiado para instalar, validar e diagnosticar a policy Polkit do WiiStudio no Linux.

A F105 detecta quando a policy Polkit está ausente, mas ainda falta um caminho claro para o usuário instalar e validar essa policy sem copiar arquivos manualmente e sem o WiiStudio armazenar senha.

A autenticação deve ser feita pelo sistema operacional, usando Polkit/pkexec.

---

# Problema Atual

No Linux, operações com dispositivos raw podem exigir autorização administrativa para acessar:

```text
/dev/sdX
/dev/disk/by-id/*
partições WBFS
dispositivos USB sem mount point comum
```

A F105 define o uso de:

```text
Polkit / pkexec
```

com a action:

```text
com.wiistudio.device.raw-access
```

Mas se a policy estiver ausente, hoje o sistema apenas diagnostica o problema.

Falta:

- fluxo guiado de instalação;
- validação do arquivo policy;
- instruções claras para o usuário;
- retry após instalação;
- integração com Diagnóstico;
- garantia de que nenhuma senha será salva.

---

# Resultado Esperado

Quando a policy Polkit estiver ausente, o WiiStudio deve exibir:

```text
A policy Polkit do WiiStudio não está instalada.
Ela é necessária para autorizar acesso seguro a dispositivos USB/WBFS no Linux.
```

Ações disponíveis:

- Instalar policy;
- Ver instruções manuais;
- Validar novamente;
- Abrir diagnóstico.

A instalação deve usar autenticação do sistema operacional e nunca um prompt de senha próprio do WiiStudio.

---

# Escopo

## Incluído

- detectar policy instalada;
- detectar policy ausente;
- detectar policy inválida;
- detectar `pkexec` ausente;
- validar XML da policy;
- instalar policy oficial;
- pedir confirmação do usuário;
- executar instalação via Polkit/pkexec;
- validar após instalação;
- integrar com F105;
- integrar com Diagnóstico;
- permitir retry;
- testes com fakes.

## Fora do Escopo Inicial

- instalar silenciosamente;
- baixar policy em runtime;
- aceitar path arbitrário de policy;
- executar comando elevado arbitrário;
- criar prompt próprio de senha;
- instalar helper privilegiado completo sem pacote oficial.

---

# Caminho Oficial da Policy

Caminho recomendado:

```text
/usr/share/polkit-1/actions/com.wiistudio.device.policy
```

Action ID:

```text
com.wiistudio.device.raw-access
```

---

# Policy Esperada

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

`auth_admin_keep` permite que o Polkit reaproveite a autorização quando o sistema permitir.

---

# Fluxo de Instalação

```text
Detect Policy Missing
    ↓
Show explanation
    ↓
User clicks Install Policy
    ↓
Show confirmation
    ↓
Validate embedded/source policy
    ↓
Run installer via pkexec
    ↓
OS prompts for admin authorization
    ↓
Install policy in system path
    ↓
Validate installed policy
    ↓
Run permission check again
    ↓
Show result
```

---

# Confirmação do Usuário

Antes da instalação, exibir:

```text
O WiiStudio instalará uma policy Polkit em:
/usr/share/polkit-1/actions/com.wiistudio.device.policy

Essa policy permite que o sistema solicite autorização administrativa para operações
de acesso a dispositivos USB/WBFS.

O WiiStudio não armazena sua senha.
Deseja continuar?
```

---

# Abstrações

## IPolkitPolicyInstaller

```csharp
public interface IPolkitPolicyInstaller
{
    Task<PolkitPolicyInstallStatus> CheckAsync(
        CancellationToken cancellationToken = default);

    Task<PolkitPolicyInstallResult> InstallAsync(
        CancellationToken cancellationToken = default);

    Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(
        CancellationToken cancellationToken = default);
}
```

## IPolkitPolicyLocator

```csharp
public interface IPolkitPolicyLocator
{
    string ExpectedPolicyPath { get; }

    Task<bool> ExistsAsync(
        CancellationToken cancellationToken = default);
}
```

## IPolkitPolicyValidator

```csharp
public interface IPolkitPolicyValidator
{
    Task<PolkitPolicyValidationResult> ValidateSourcePolicyAsync(
        string sourcePolicyPath,
        CancellationToken cancellationToken = default);

    Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(
        string installedPolicyPath,
        CancellationToken cancellationToken = default);
}
```

---

# Modelos

## PolkitPolicyInstallState

```csharp
public enum PolkitPolicyInstallState
{
    Unknown,
    NotLinux,
    Installed,
    Missing,
    Invalid,
    PkexecMissing,
    InstallRequiresAuthorization,
    InstallFailed,
    ValidationFailed
}
```

## PolkitPolicyInstallStatus

```csharp
public sealed class PolkitPolicyInstallStatus
{
    public PolkitPolicyInstallState State { get; init; }
    public string? PolicyPath { get; init; }
    public string? MessageKey { get; init; }
    public string? RecommendedActionKey { get; init; }
}
```

---

# Estratégia de Instalação Recomendada

Usar um helper/instalador controlado:

```text
pkexec /usr/lib/wiistudio/wiistudio-polkit-installer install-policy
```

O helper deve:

- aceitar apenas `install-policy`;
- copiar apenas a policy oficial do WiiStudio;
- validar destino fixo;
- não aceitar path arbitrário;
- não abrir shell;
- retornar exit code e mensagem estruturada.

Se o helper ainda não existir, a primeira versão pode exibir instruções manuais ou orientar instalar o pacote oficial do WiiStudio que contém a policy.

---

# Validação da Policy

Validar:

- arquivo existe;
- XML válido;
- contém action `com.wiistudio.device.raw-access`;
- contém `auth_admin_keep`;
- vendor é WiiStudio;
- não contém actions inesperadas;
- path é exatamente o esperado;
- permissões do arquivo são aceitáveis;
- owner/permissões do sistema são aceitáveis quando possível.

---

# Integração com Diagnóstico

Adicionar seção:

```text
Polkit Policy
```

Campos:

```text
Status: Installed/Missing/Invalid
Path: /usr/share/polkit-1/actions/com.wiistudio.device.policy
Action: com.wiistudio.device.raw-access
pkexec available: true/false
auth_admin_keep: true/false
Last validation: ...
Last error: ...
```

Ações:

- Verificar novamente;
- Instalar policy;
- Abrir instruções;
- Copiar detalhes.

---

# Integração com F105

A `LinuxPolkitPrivilegeStrategy` deve consultar o instalador/validator.

```text
Linux Device Access Check
    ↓
Check pkexec
    ↓
Check policy
    ↓
If missing: PolicyMissing
    ↓
If invalid: ValidationFailed
    ↓
Continue privileged access flow
```

---

# Segurança

Obrigatório:

- nunca salvar senha;
- nunca pedir senha em UI própria;
- usar autenticação do sistema;
- não aceitar path arbitrário;
- validar XML antes e depois;
- não baixar policy em runtime;
- não instalar silenciosamente;
- não executar shell arbitrário;
- logs sem dados sensíveis;
- timeout e cancelamento.

---

# UX

## Policy Ausente

```text
A policy Polkit do WiiStudio não está instalada.
Instale a policy para permitir autorização segura de acesso a dispositivos USB/WBFS.
```

## Policy Inválida

```text
A policy Polkit instalada não corresponde à versão esperada pelo WiiStudio.
Reinstale a policy oficial.
```

## pkexec Ausente

```text
O comando pkexec não foi encontrado. Instale o Polkit/pkexec pelo gerenciador de pacotes da sua distribuição.
```

## Instalação Concluída

```text
Policy instalada com sucesso. Você já pode tentar acessar o dispositivo novamente.
```

---

# Testes Esperados

## Unitários

- policy instalada;
- policy ausente;
- policy inválida;
- `pkexec` ausente;
- XML inválido;
- action ID ausente;
- `auth_admin_keep` ausente;
- destino inválido bloqueado;
- install cancelado;
- install falha;
- install sucesso;
- senha nunca solicitada/salva.

## Integração com Fakes

- fake filesystem;
- fake pkexec runner;
- fake policy validator;
- fake OS Linux;
- diagnóstico preenchido corretamente;
- retry após instalação.

---

# Critérios de Aceite

- [x] Linux detecta policy instalada.
- [x] Linux detecta policy ausente.
- [x] Linux detecta policy inválida.
- [x] UI orienta instalação.
- [x] Instalação exige confirmação.
- [x] Instalação usa autenticação do sistema.
- [x] Policy instalada é validada após instalação.
- [x] Diagnóstico mostra status da policy.
- [x] Retry após instalação funciona.
- [x] Nenhuma senha é armazenada ou trafegada pelo WiiStudio.

---

# Regras para o Codex

- Nunca pedir senha em tela própria.
- Nunca salvar senha.
- Nunca executar `sudo` com senha.
- Nunca montar comando shell arbitrário.
- Sempre validar a policy antes e depois.
- Sempre pedir confirmação antes de instalar.
- Sempre registrar diagnóstico.
- Sempre permitir fallback manual.
- Nunca instalar silenciosamente.
- Nunca aceitar path arbitrário de policy.

---

# Dependências

- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F090-SecurityChecklist.md`
- `F030-AdvancedDiagnostics.md`

---

# Definition of Done

- Fluxo guiado documentado.
- Abstrações definidas.
- Policy validada.
- Instalação segura definida.
- Diagnóstico integrado.
- Testes com fakes definidos.

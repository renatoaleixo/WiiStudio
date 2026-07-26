# F075 - Plugin Permissions

> Feature ID: F075
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Plugin SDK

# Objetivo

Definir o sistema oficial de permissões para plugins do WiiStudio, garantindo que
plugins externos declarem previamente os recursos que desejam acessar e que a
aplicação possa aprovar, negar, auditar e diagnosticar esse acesso.

Permissões são fundamentais para reduzir riscos de segurança, proteger arquivos,
evitar execução indevida de processos e manter previsibilidade para o usuário.

---

# Requisitos Funcionais

## RF001
Definir permissões oficiais.

## RF002
Exigir declaração de permissões no manifesto.

## RF003
Validar permissões antes do carregamento.

## RF004
Bloquear acesso não autorizado.

## RF005
Permitir auditoria de permissões usadas.

## RF006
Permitir desativação de plugin por abuso de permissão.

## RF007
Exibir permissões ao usuário.

## RF008
Integrar com Security, Diagnostics e Plugin Loading.

## RF009
Permitir política padrão por nível de risco.

## RF010
Nunca conceder permissão implícita.

---

# Permissões Oficiais

## FileSystemRead

Permite leitura de arquivos autorizados.

## FileSystemWrite

Permite escrita em áreas autorizadas.

## NetworkAccess

Permite acesso à rede.

## ExecuteProcess

Permite executar processos externos.

## AccessGameLibrary

Permite consultar biblioteca de jogos.

## ModifyGameLibrary

Permite alterar biblioteca de jogos.

## AccessMetadata

Permite consultar metadados.

## ModifyMetadata

Permite alterar metadados.

## AccessAssets

Permite consultar assets.

## ModifyAssets

Permite alterar assets.

## AccessDiagnostics

Permite consultar diagnósticos e logs controlados.

## UiExtension

Permite registrar extensões visuais.

## BackgroundTasks

Permite executar tarefas em segundo plano.

---

# Níveis de Risco

| Nível | Permissões |
|------|------------|
| Baixo | AccessMetadata, AccessAssets, UiExtension limitada |
| Médio | NetworkAccess, AccessGameLibrary, AccessDiagnostics |
| Alto | FileSystemWrite, ModifyGameLibrary, ModifyMetadata |
| Crítico | ExecuteProcess, BackgroundTasks amplo |

---

# Manifesto

Permissões devem ser declaradas em `plugin.json`.

```json
{
  "permissions": [
    "network-access",
    "access-game-library",
    "access-metadata"
  ]
}
```

---

# Política Padrão

A política inicial recomendada:

- permissões de baixo risco podem ser aprovadas automaticamente;
- permissões médias podem exigir aviso;
- permissões altas exigem aprovação explícita;
- permissões críticas exigem aprovação explícita e destaque visual;
- permissões desconhecidas bloqueiam o plugin.

---

# Permission Service

```csharp
public interface IPluginPermissionService
{
    bool HasPermission(string pluginId, PluginPermission permission);

    Task<PermissionDecision> RequestPermissionAsync(
        string pluginId,
        PluginPermission permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PluginPermission>> GetGrantedPermissionsAsync(
        string pluginId,
        CancellationToken cancellationToken = default);
}
```

---

# Permission Decision

```csharp
public enum PermissionDecision
{
    Granted,
    Denied,
    RequiresUserApproval,
    Blocked,
    Unknown
}
```

---

# Plugin Permission

```csharp
public enum PluginPermission
{
    FileSystemRead,
    FileSystemWrite,
    NetworkAccess,
    ExecuteProcess,
    AccessGameLibrary,
    ModifyGameLibrary,
    AccessMetadata,
    ModifyMetadata,
    AccessAssets,
    ModifyAssets,
    AccessDiagnostics,
    UiExtension,
    BackgroundTasks
}
```

---

# Validação

Antes de carregar plugin:

```text
Read Manifest Permissions
    ↓
Validate Known Permissions
    ↓
Calculate Risk Level
    ↓
Check User/Policy Approval
    ↓
Grant or Block
```

---

# Auditoria

Registrar:

- pluginId;
- permissão solicitada;
- permissão concedida;
- permissão negada;
- data/hora;
- operação sensível executada;
- falhas de permissão.

---

# UI de Permissões

A interface de plugins deve mostrar:

- permissões solicitadas;
- permissões concedidas;
- nível de risco;
- explicação amigável;
- opção de revogar quando seguro;
- histórico básico de uso.

---

# Permissões de Arquivo

Plugins não devem acessar paths arbitrários.

Regras:

- usar storage isolado do plugin;
- validar path;
- impedir path traversal;
- impedir acesso fora de diretórios permitidos;
- FileSystemWrite exige cuidado extra.

---

# Permissão de Processo Externo

`ExecuteProcess` é crítica.

Regras:

- exige aprovação explícita;
- comando deve ser validado;
- argumentos devem ser sanitizados;
- logs devem ser registrados;
- nunca executar shell arbitrário;
- usar ProcessRunner controlado.

---

# Permissão de Rede

`NetworkAccess` deve informar:

- finalidade;
- domínio quando possível;
- provider usado;
- falhas de rede;
- timeout.

---

# Permissões de UI

`UiExtension` permite registrar UI apenas em regiões permitidas.

Regras:

- respeitar Design System;
- respeitar Theme Engine;
- respeitar Accessibility;
- não bloquear UI;
- não capturar foco indevidamente.

---

# Integração com Security

O sistema de permissões deve se integrar com:

- Safe Path Handling;
- External Process Security;
- Log Redaction;
- Plugin Security;
- Security Checklist.

---

# Regras para o Codex

- Nunca conceder permissão implícita.
- Sempre validar permissões do manifesto.
- Sempre bloquear permissão desconhecida.
- Sempre verificar permissão antes de operação sensível.
- Sempre auditar operações sensíveis.
- Sempre usar path seguro.
- Sempre usar ProcessRunner para execução externa.
- Sempre exibir permissões ao usuário.
- Sempre permitir desativar plugin abusivo.
- Nunca permitir shell arbitrário.
- Nunca permitir path traversal.

---

# Casos de Erro

- Permissão desconhecida.
- Permissão não declarada usada.
- Usuário nega permissão.
- Plugin tenta acesso fora do escopo.
- Plugin tenta executar processo sem permissão.
- Plugin tenta escrever arquivo sem permissão.
- Permissão revogada durante uso.
- Falha ao salvar decisão.
- Política inválida.

---

# Critérios de Aceite

- [x] Permissões oficiais definidas.
- [x] Níveis de risco definidos.
- [x] Permission Service especificado.
- [x] Validação antes do loading definida.
- [x] Auditoria definida.
- [x] UI de permissões prevista.
- [x] FileSystem permissions documentadas.
- [x] ExecuteProcess documentado como crítico.
- [x] NetworkAccess documentado.
- [x] Regras para o Codex documentadas.
- [x] Nenhuma permissão implícita.

---

# Testes

## Unitários

- Permissão conhecida validada.
- Permissão desconhecida bloqueada.
- Permissão não declarada negada.
- Permissão concedida.
- Permissão negada.
- Risk level calculado.
- Auditoria registrada.
- Path traversal bloqueado.
- ExecuteProcess sem permissão bloqueado.

## Integração

- Plugin com permissão baixa.
- Plugin com permissão crítica.
- Usuário nega permissão.
- Plugin tenta operação não autorizada.
- Revogar permissão.
- Diagnóstico de permissão.
- UI exibe permissões.

---

# Dependências

- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F074-PluginLoading.md
- F081-SecurityFoundation.md
- F083-ExternalProcessSecurity.md
- F084-PluginSecurity.md
- F086-SafePathHandling.md
- F088-LogRedaction.md

---

# Definition of Done

- Plugin Permissions documentado.
- Permissões oficiais definidas.
- Política de risco definida.
- Permission Service especificado.
- Auditoria definida.
- Testes definidos.
- Documentação atualizada.

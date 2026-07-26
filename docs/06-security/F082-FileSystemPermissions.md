# F082 - File System Permissions

> Feature ID: F082
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial para permissões de sistema de arquivos no WiiStudio,
garantindo leitura, escrita, importação, exportação, backup, cache, plugins e
operações com dispositivos de forma segura, previsível e auditável.

---

# Requisitos Funcionais

## RF001
Validar permissões antes de ler arquivos.

## RF002
Validar permissões antes de escrever arquivos.

## RF003
Validar diretórios de destino.

## RF004
Validar espaço disponível quando aplicável.

## RF005
Impedir path traversal.

## RF006
Impedir escrita fora de escopos permitidos.

## RF007
Auditar falhas de permissão.

## RF008
Integrar com Plugin Permissions.

## RF009
Integrar com Safe Path Handling.

## RF010
Exibir mensagens amigáveis ao usuário.

---

# Operações Cobertas

- Importar ISO/WBFS/RVZ/NKit.
- Exportar jogos.
- Converter arquivos.
- Criar backup.
- Restaurar backup.
- Baixar capas.
- Atualizar GameTDB.
- Ler logs.
- Exportar logs.
- Storage de plugins.
- Cache offline.
- Arquivos temporários.

---

# Escopos Oficiais

## UserSelectedFile

Arquivo explicitamente selecionado pelo usuário.

## UserSelectedDirectory

Diretório explicitamente selecionado pelo usuário.

## AppData

Dados internos da aplicação.

## Cache

Arquivos temporários ou regeneráveis.

## PluginStorage

Área isolada por plugin.

## BackupDirectory

Destino configurado para backups.

## ExportDirectory

Destino configurado para exportações.

## ExternalDevice

Dispositivo externo ou partição montada.

---

# Modelo de Permissão

```csharp
public enum FileSystemAccess
{
    Read,
    Write,
    ReadWrite,
    Delete,
    Enumerate
}
```

```csharp
public sealed class FileSystemPermissionRequest
{
    public string Path { get; init; } = string.Empty;
    public FileSystemAccess Access { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string? PluginId { get; init; }
}
```

---

# Serviço de Permissões

```csharp
public interface IFileSystemPermissionService
{
    Task<FileSystemPermissionResult> CheckAsync(
        FileSystemPermissionRequest request,
        CancellationToken cancellationToken = default);
}
```

---

# Resultado

```csharp
public sealed class FileSystemPermissionResult
{
    public bool Allowed { get; init; }
    public string? ReasonKey { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

---

# Validações Obrigatórias

Antes de acessar arquivo ou diretório:

- normalizar path;
- verificar path traversal;
- verificar existência quando leitura;
- verificar diretório pai quando escrita;
- verificar permissões do sistema;
- verificar espaço disponível quando escrita grande;
- verificar escopo permitido;
- verificar se plugin tem permissão quando aplicável.

---

# Leitura

Para leitura:

- arquivo deve existir;
- extensão deve ser permitida quando aplicável;
- usuário ou plugin deve ter permissão;
- path deve estar dentro do escopo permitido;
- erro deve ser tratado de forma amigável.

---

# Escrita

Para escrita:

- diretório de destino deve existir ou ser criável;
- usuário deve ter permissão;
- deve haver espaço suficiente;
- sobrescrita deve seguir política;
- escrita deve usar arquivo temporário quando possível;
- falha não deve corromper arquivo existente.

---

# Exclusão

Exclusão é operação sensível.

Regras:

- exigir confirmação para dados importantes;
- preferir lixeira/quarentena quando aplicável;
- registrar auditoria;
- impedir exclusão fora do escopo;
- plugins precisam de permissão explícita.

---

# Plugins

Plugins devem respeitar:

- `file-system-read`;
- `file-system-write`;
- storage isolado;
- paths permitidos;
- auditoria;
- bloqueio de path traversal.

Plugin não deve acessar arbitrariamente arquivos do usuário sem seleção ou permissão.

---

# Dispositivos Externos

Dispositivos externos podem exigir:

- permissão elevada;
- montagem;
- leitura especial;
- tratamento de somente leitura;
- validação de filesystem;
- diagnóstico quando acesso falhar.

---

# Mensagens Amigáveis

Exemplos:

```text
Não foi possível acessar este arquivo. Verifique as permissões.
```

```text
O diretório de destino não permite gravação.
```

```text
Espaço insuficiente para concluir a operação.
```

```text
O plugin não possui permissão para acessar este caminho.
```

---

# Auditoria

Registrar:

- operação;
- path sanitizado;
- escopo;
- resultado;
- motivo de bloqueio;
- pluginId quando aplicável;
- timestamp;
- correlationId.

---

# Regras para o Codex

- Sempre verificar permissão antes de ler/escrever.
- Sempre normalizar path.
- Sempre bloquear path traversal.
- Sempre validar espaço antes de escrita grande.
- Sempre usar escrita temporária para arquivos importantes.
- Sempre tratar UnauthorizedAccessException.
- Sempre exibir mensagem amigável.
- Sempre registrar auditoria para falha.
- Nunca permitir plugin acessar path arbitrário.
- Nunca excluir arquivo sem confirmação quando destrutivo.
- Nunca concatenar path inseguramente.

---

# Casos de Erro

- Arquivo inexistente.
- Diretório inexistente.
- Sem permissão.
- Somente leitura.
- Espaço insuficiente.
- Path traversal.
- Path absoluto proibido.
- Plugin sem permissão.
- Dispositivo removido.
- Arquivo em uso.
- Falha ao criar temporário.

---

# Critérios de Aceite

- [x] Escopos oficiais definidos.
- [x] Serviço de permissão definido.
- [x] Leitura validada.
- [x] Escrita validada.
- [x] Exclusão protegida.
- [x] Plugins restritos.
- [x] Dispositivos externos considerados.
- [x] Mensagens amigáveis definidas.
- [x] Auditoria definida.
- [x] Path traversal bloqueado.

---

# Testes

## Unitários

- Leitura permitida.
- Leitura negada.
- Escrita permitida.
- Escrita negada.
- Path traversal bloqueado.
- Espaço insuficiente.
- Plugin sem permissão.
- Arquivo em uso.
- Exclusão exige confirmação.

## Integração

- Importar arquivo sem permissão.
- Exportar para diretório protegido.
- Backup sem espaço.
- Plugin storage isolado.
- Dispositivo removido durante operação.
- Cache criado com sucesso.
- Log exportado.

---

# Dependências

- F075-PluginPermissions.md
- F081-SecurityFoundation.md
- F086-SafePathHandling.md
- F087-BackupSecurity.md
- F088-LogRedaction.md
- 007-Infrastructure.md

---

# Definition of Done

- File System Permissions documentado.
- Escopos definidos.
- Serviço especificado.
- Validações definidas.
- Auditoria definida.
- Testes definidos.
- Documentação atualizada.

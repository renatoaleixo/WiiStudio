# F086 - Safe Path Handling

> Feature ID: F086
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial para manipulação segura de paths no WiiStudio, protegendo importação, exportação, conversão, backup, cache, plugins, processos externos e configurações contra path traversal, paths inválidos, sobrescrita indevida e acesso fora de escopo.

---

# Requisitos Funcionais

## RF001
Normalizar paths antes do uso.

## RF002
Bloquear path traversal.

## RF003
Validar paths absolutos e relativos conforme contexto.

## RF004
Garantir que paths de plugins fiquem dentro do escopo permitido.

## RF005
Validar extensões permitidas.

## RF006
Validar diretórios de destino.

## RF007
Evitar sobrescrita silenciosa.

## RF008
Usar escrita temporária para arquivos importantes.

## RF009
Integrar com File System Permissions.

## RF010
Integrar com External Process Security.

---

# Riscos Cobertos

- path traversal;
- path absoluto indevido;
- symlink perigoso;
- sobrescrita acidental;
- acesso fora de sandbox;
- diretório inexistente;
- arquivo em uso;
- extensão inválida;
- path muito longo;
- caracteres inválidos;
- case sensitivity entre plataformas.

---

# Serviço de Paths

```csharp
public interface ISafePathService
{
    SafePathResult Normalize(string path);

    SafePathResult EnsureInsideRoot(string rootPath, string candidatePath);

    SafePathResult ValidateExtension(string path, IReadOnlyList<string> allowedExtensions);

    SafePathResult ValidateWritableDestination(string path);

    SafePathResult ValidateReadableFile(string path);
}
```

---

# Resultado

```csharp
public sealed class SafePathResult
{
    public bool Success { get; init; }
    public string? NormalizedPath { get; init; }
    public string? ErrorKey { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

---

# Normalização

Antes de usar path:

- expandir quando permitido;
- converter separadores;
- resolver path completo;
- remover segmentos redundantes;
- validar caracteres;
- validar comprimento;
- preservar case conforme sistema.

---

# Path Traversal

Bloquear padrões como:

```text
../
..\
%2e%2e
```

E qualquer path que, após normalização, escape do root permitido.

---

# Paths Relativos

Paths relativos só são permitidos quando:

- há root explícito;
- path final permanece dentro do root;
- contexto exige path relativo, como manifesto de plugin.

---

# Paths Absolutos

Paths absolutos podem ser permitidos para:

- arquivo selecionado pelo usuário;
- diretório configurado pelo usuário;
- executável externo configurado;
- dispositivo montado.

Devem ser validados por permissão e escopo.

---

# Extensões Permitidas

Validar extensão por contexto.

## Jogos

- `.iso`
- `.wbfs`
- `.rvz`
- `.nkit.iso`
- `.wia`
- `.ciso`

## Imagens

- `.png`
- `.jpg`
- `.jpeg`
- `.webp`

## Configurações

- `.json`

## Logs

- `.log`
- `.txt`

## Backups

- extensão oficial futura definida pelo Backup System.

---

# Escrita Segura

Para arquivos importantes:

```text
write temp file
    ↓
flush
    ↓
validate
    ↓
atomic move/replace when possible
```

Não sobrescrever arquivo existente sem política clara.

---

# Diretórios Temporários

Temporários devem:

- ficar em área controlada;
- possuir nome único;
- ser limpos após sucesso;
- ser preservados em falha quando necessário para diagnóstico;
- não expor dados sensíveis.

---

# Symlinks

Quando aplicável:

- detectar symlink;
- resolver target quando possível;
- validar target dentro do escopo;
- bloquear symlink suspeito em plugins;
- registrar warning.

---

# Multiplataforma

Considerar diferenças:

- separador `/` e `\`;
- case sensitivity;
- permissões Linux;
- paths Windows;
- paths com espaços;
- caracteres inválidos;
- tamanho máximo de path.

---

# Integração com Plugins

Plugins só podem usar paths dentro:

- storage do plugin;
- paths selecionados pelo usuário;
- escopos concedidos por permissão.

Paths do manifesto devem ser relativos e seguros.

---

# Integração com ProcessRunner

Antes de enviar path para processo externo:

- normalizar;
- validar existência;
- validar permissão;
- validar escopo;
- escapar somente via API estruturada, não shell.

---

# Regras para o Codex

- Sempre normalizar path antes do uso.
- Sempre validar root permitido.
- Sempre bloquear path traversal.
- Sempre validar extensão.
- Sempre validar destino antes de escrever.
- Sempre usar escrita temporária quando possível.
- Sempre tratar symlink com cuidado.
- Sempre considerar multiplataforma.
- Nunca concatenar path manualmente de forma insegura.
- Nunca confiar em path vindo de plugin.
- Nunca permitir path do manifesto escapar da pasta do plugin.

---

# Casos de Erro

- Path vazio.
- Path inválido.
- Path muito longo.
- Path traversal.
- Path fora do root.
- Extensão inválida.
- Diretório inexistente.
- Sem permissão.
- Arquivo em uso.
- Symlink inseguro.
- Sobrescrita não permitida.

---

# Critérios de Aceite

- [x] SafePathService definido.
- [x] Normalização definida.
- [x] Path traversal bloqueado.
- [x] Paths relativos controlados.
- [x] Paths absolutos validados.
- [x] Extensões permitidas definidas.
- [x] Escrita segura definida.
- [x] Temporários definidos.
- [x] Symlinks considerados.
- [x] Multiplataforma considerado.
- [x] Plugins restritos.
- [x] ProcessRunner integrado.

---

# Testes

## Unitários

- Normalizar path válido.
- Bloquear path traversal.
- Bloquear path fora do root.
- Permitir path dentro do root.
- Validar extensão.
- Rejeitar extensão inválida.
- Validar destino gravável.
- Detectar path vazio.
- Detectar path muito longo.
- Symlink inseguro.

## Integração

- Importar jogo com path válido.
- Importar path inválido.
- Exportar para diretório sem permissão.
- Plugin tenta escapar do storage.
- Manifesto com path inseguro.
- ProcessRunner recebe path validado.
- Backup usa escrita temporária.

---

# Dependências

- F082-FileSystemPermissions.md
- F083-ExternalProcessSecurity.md
- F084-PluginSecurity.md
- F087-BackupSecurity.md
- F090-SecurityChecklist.md
- 007-Infrastructure.md

---

# Definition of Done

- Safe Path Handling documentado.
- Serviço especificado.
- Regras de normalização definidas.
- Proteção contra traversal definida.
- Testes definidos.
- Documentação atualizada.

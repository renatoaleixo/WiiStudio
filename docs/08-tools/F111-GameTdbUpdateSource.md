# F111 - GameTDB Update Source

> Feature ID: F111  
> Status: Ready for Development  
> Priority: High  
> Epic: Metadata & Assets / Update Management  
> Area: Infrastructure, Metadata, Cache, Diagnostics, Settings  
> Related: `NullGameTdbUpdateSource`, `F103-GameTDBMetadataProvider`

# Objetivo

Implementar um provider real para atualização da base local do GameTDB/WiiTDB,
substituindo o `NullGameTdbUpdateSource`.

A feature deve baixar, validar, versionar e atualizar a base local do GameTDB com
segurança, mantendo controle de:

- versão/fonte;
- data da última atualização;
- rollback;
- status da base local;
- diagnóstico;
- cache offline.

---

# Problema Atual

Existe um `NullGameTdbUpdateSource`, então o fluxo de atualização da base GameTDB
não busca dados reais.

Resultado atual:

```text
O sistema aparenta ter suporte a atualização, mas nenhuma base real é atualizada.
```

---

# Resultado Esperado

Criar provider real:

```csharp
GameTdbUpdateSource : IGameTdbUpdateSource
```

ou equivalente, responsável por:

```text
Verificar fonte GameTDB
    ↓
Baixar wiitdb.zip
    ↓
Validar tamanho e formato
    ↓
Extrair wiitdb.xml em temporário
    ↓
Validar XML
    ↓
Atualizar base local
    ↓
Salvar manifesto
    ↓
Manter rollback
```

---

# Fonte Oficial

Fonte recomendada:

```text
https://www.gametdb.com/wiitdb.zip
```

Páginas relacionadas:

```text
https://www.gametdb.com/
https://www.gametdb.com/Wii
https://www.gametdb.com/Wii/downloads
```

Formato:

```text
ZIP contendo wiitdb.xml
```

---

# Escopo

## Incluído

- provider real de atualização GameTDB;
- download seguro via HTTPS;
- validação de ZIP;
- validação de XML;
- controle de versão/data;
- manifesto local;
- backup da versão anterior;
- rollback em falha;
- integração com cache;
- integração com Diagnostics;
- integração com Task Manager;
- testes com HTTP fake e ZIP/XML fake.

## Fora do Escopo Inicial

- atualização incremental complexa por jogo;
- dependência de internet real nos testes;
- processamento de assets/capas;
- atualização silenciosa obrigatória;
- assinatura criptográfica do XML, se não existir fonte oficial com assinatura.

---

# Contratos

## IGameTdbUpdateSource

```csharp
public interface IGameTdbUpdateSource
{
    Task<GameTdbUpdateCheckResult> CheckForUpdatesAsync(
        CancellationToken cancellationToken = default);

    Task<GameTdbUpdateResult> UpdateAsync(
        GameTdbUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<GameTdbRollbackResult> RollbackAsync(
        CancellationToken cancellationToken = default);
}
```

---

## GameTdbUpdateRequest

```csharp
public sealed class GameTdbUpdateRequest
{
    public bool ForceRefresh { get; init; }
    public bool CreateBackup { get; init; } = true;
}
```

---

## GameTdbUpdateResult

```csharp
public sealed class GameTdbUpdateResult
{
    public bool Success { get; init; }
    public string? Version { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int? GameCount { get; init; }
    public string? LocalDatabasePath { get; init; }
    public string? ErrorMessageKey { get; init; }
}
```

---

# Estrutura Local

```text
metadata/
└── gametdb/
    ├── current/
    │   ├── wiitdb.xml
    │   └── manifest.json
    ├── previous/
    │   ├── wiitdb.xml
    │   └── manifest.json
    └── temp/
```

---

# Manifesto Local

```json
{
  "provider": "GameTDB",
  "sourceUrl": "https://www.gametdb.com/wiitdb.zip",
  "downloadedAt": "2026-01-01T10:00:00Z",
  "version": "detected-or-unknown",
  "gameCount": 12345,
  "xmlSha256": "...",
  "zipSha256": "...",
  "fileSizeBytes": 12345678
}
```

---

# Fluxo de Atualização

```text
Start Update
    ↓
Check network/source
    ↓
Download wiitdb.zip to temp
    ↓
Validate HTTPS/source
    ↓
Validate max size
    ↓
Validate ZIP structure
    ↓
Extract wiitdb.xml to temp
    ↓
Validate XML safely
    ↓
Count games
    ↓
Backup current to previous
    ↓
Move temp to current
    ↓
Write manifest
    ↓
Update Diagnostics
```

---

# Validação

Obrigatório:

- URL HTTPS;
- timeout;
- cancellation token;
- tamanho máximo;
- ZIP válido;
- proteção contra zip slip;
- XML válido;
- DTD/XXE desabilitado;
- arquivo `wiitdb.xml` presente;
- contagem mínima razoável de jogos;
- escrita em temp antes de substituir base atual.

---

# Rollback

Se a atualização falhar depois de alterar arquivos:

```text
Restaurar previous/
```

Rollback também deve estar disponível manualmente no diagnóstico.

Regras:

- nunca apagar `previous/` antes da nova base estar validada;
- se `current/` falhar, usar `previous/`;
- se não houver rollback, informar claramente.

---

# Configurações

```text
GameTdb.UpdateUrl = https://www.gametdb.com/wiitdb.zip
GameTdb.AutoCheckEnabled = true
GameTdb.AutoUpdateEnabled = false
GameTdb.DatabaseTtlDays = 7
GameTdb.MaxZipBytes = 52428800
GameTdb.LastUpdateAt
GameTdb.CurrentVersion
```

Auto-check pode verificar disponibilidade. Auto-update deve exigir política clara e não deve quebrar uso offline.

---

# Integração com F103

O `GameTdbMetadataProvider` deve usar a base local atualizada por esta feature.

Regra:

```text
F111 atualiza a base.
F103 consulta a base.
```

---

# Diagnóstico

Adicionar:

```text
GameTDB Database
Provider: GameTDB
Source URL: https://www.gametdb.com/wiitdb.zip
Current version: ...
Last update: ...
Game count: ...
Current XML hash: ...
Previous version available: true/false
Last error: ...
```

Ações:

- verificar atualização;
- atualizar agora;
- forçar atualização;
- rollback;
- abrir pasta local;
- limpar cache.

---

# Task Manager

Atualização deve aparecer como tarefa:

```text
Atualizando base GameTDB
Baixando...
Validando...
Aplicando...
Concluído
```

Com cancelamento enquanto ainda estiver em etapa segura.

---

# Tratamento de Erro

## Sem Internet

Usar base local existente, se disponível.

## ZIP Inválido

Bloquear atualização e manter base atual.

## XML Inválido

Bloquear atualização e manter base atual.

## Provider Fora do Ar

Registrar erro e manter base atual.

## Cache Corrompido

Tentar rollback para `previous/`.

---

# Testes Esperados

- update source disponível;
- download fake bem-sucedido;
- ZIP sem `wiitdb.xml`;
- XML inválido;
- XML com XXE bloqueado;
- zip slip bloqueado;
- rollback após falha;
- manifesto gerado;
- versão/data atualizada;
- cache atual preservado quando falha;
- ForceRefresh ignora TTL;
- sem internet usa base local.

---

# Critérios de Aceite

- [ ] `NullGameTdbUpdateSource` deixa de ser provider padrão.
- [ ] `GameTdbUpdateSource` implementado.
- [ ] Baixa `wiitdb.zip` por HTTPS.
- [ ] Valida ZIP e XML.
- [ ] Atualiza `current/`.
- [ ] Mantém `previous/`.
- [ ] Rollback funciona.
- [ ] Manifesto local é salvo.
- [ ] Diagnostics mostra versão/data/status.
- [ ] Testes não dependem de internet real.

---

# Regras para o Codex

- Não substituir base atual antes da nova ser validada.
- Não processar XML com DTD/XXE habilitado.
- Não aceitar ZIP com path traversal.
- Não depender de internet real nos testes.
- Sempre usar temp.
- Sempre manter rollback.
- Sempre atualizar Diagnostics.
- Nunca usar `NullGameTdbUpdateSource` como padrão final.

---

# Dependências

- `F103-GameTDBMetadataProvider.md`
- `F108-ExternalToolNetworkDownloader.md`
- `F030-AdvancedDiagnostics.md`
- `F026-TaskManager.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F090-SecurityChecklist.md`

---

# Definition of Done

- Provider real definido.
- Download/validação definidos.
- Manifesto e rollback definidos.
- Diagnostics e Task Manager integrados.
- Testes com fakes definidos.

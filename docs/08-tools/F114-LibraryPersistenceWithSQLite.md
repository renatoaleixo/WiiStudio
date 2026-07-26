# F114 - Library Persistence with SQLite

> Feature ID: F114  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: Persistence / Game Library  
> Area: Database, Infrastructure, Game Library, Metadata, Assets  
> Related: Roadmap SQLite, JSON/memory stores

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 3.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: 009 Database, F002, F032, F096, F102, F103, F104.
- Reforço de escopo: schema SQLite inicial, migrations versionadas, repositories, unit of work, importação de JSON legado, backup antes de migração, índices, transações, diagnóstico do banco e testes com SQLite temporário.
- Fora de escopo reforçado: banco remoto, sincronização em nuvem, multiusuário, criptografia completa, replicação e edição avançada de metadados.
- Regras adicionais: migration deve ser idempotente, reversão deve preservar backup, UI não deve acessar SQLite diretamente e falhas de banco devem ser diagnosticáveis/exportáveis.
- Impacto de UI: indicador de migração, resumo de banco, diagnóstico de integridade e mensagens claras para falha de abertura/migração.

# Objetivo

Migrar/padronizar a persistência da biblioteca do WiiStudio para SQLite, substituindo
gradualmente stores em memória/JSON quando forem responsáveis por dados centrais da biblioteca.

A feature deve tornar o app mais robusto, profissional e preparado para bibliotecas grandes.

---

# Problema Atual

O roadmap cita SQLite, mas parte da implementação atual usa:

- stores JSON;
- stores em memória;
- caches soltos;
- estado não relacional.

Isso dificulta:

- performance;
- busca/filtro;
- histórico;
- integridade;
- migração;
- biblioteca grande;
- sincronização de metadados/assets.

---

# Resultado Esperado

Criar persistência robusta da biblioteca em SQLite para:

- jogos;
- arquivos;
- formatos;
- metadados;
- assets;
- tags/favoritos;
- histórico de importação;
- estado de cache;
- operações recentes quando aplicável.

---

# Escopo

## Incluído

- schema SQLite inicial;
- migrations;
- repositories;
- índices;
- transações;
- importação de dados JSON existentes;
- backup antes de migração;
- testes de banco;
- performance básica;
- integração com LibraryView.

## Fora do Escopo Inicial

- sincronização em nuvem;
- multiusuário;
- banco remoto;
- criptografia total do banco;
- replicação.

---

# Estrutura do Banco

## Games

```sql
CREATE TABLE Games (
    Id TEXT PRIMARY KEY,
    GameId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Region TEXT NULL,
    Format TEXT NULL,
    FilePath TEXT NULL,
    FileSizeBytes INTEGER NULL,
    ImportedAt TEXT NULL,
    UpdatedAt TEXT NOT NULL,
    IsFavorite INTEGER NOT NULL DEFAULT 0
);
```

## GameMetadata

```sql
CREATE TABLE GameMetadata (
    GameId TEXT PRIMARY KEY,
    Developer TEXT NULL,
    Publisher TEXT NULL,
    Year INTEGER NULL,
    Description TEXT NULL,
    Players TEXT NULL,
    Rating TEXT NULL,
    LanguagesJson TEXT NULL,
    GenresJson TEXT NULL,
    GameTdbVersion TEXT NULL,
    UpdatedAt TEXT NOT NULL
);
```

## GameAssets

```sql
CREATE TABLE GameAssets (
    Id TEXT PRIMARY KEY,
    GameId TEXT NOT NULL,
    AssetKind TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    Source TEXT NULL,
    UpdatedAt TEXT NOT NULL,
    UNIQUE(GameId, AssetKind)
);
```

## LibraryFiles

```sql
CREATE TABLE LibraryFiles (
    Id TEXT PRIMARY KEY,
    GameId TEXT NOT NULL,
    Path TEXT NOT NULL,
    Format TEXT NOT NULL,
    SizeBytes INTEGER NULL,
    Hash TEXT NULL,
    LastSeenAt TEXT NULL,
    UNIQUE(Path)
);
```

---

# Índices

```sql
CREATE INDEX IX_Games_GameId ON Games(GameId);
CREATE INDEX IX_Games_Title ON Games(Title);
CREATE INDEX IX_Games_IsFavorite ON Games(IsFavorite);
CREATE INDEX IX_GameAssets_GameId ON GameAssets(GameId);
CREATE INDEX IX_LibraryFiles_GameId ON LibraryFiles(GameId);
```

---

# Repositories

Criar/usar:

```text
IGameLibraryRepository
IGameMetadataRepository
IGameAssetRepository
ILibraryFileRepository
ILibraryUnitOfWork
```

---

# Migrations

Toda alteração de schema deve ter migration.

Regras:

- migration versionada;
- idempotência quando possível;
- backup antes de migration crítica;
- rollback quando aplicável;
- testes de migration.

---

# Migração de JSON/Memória

Fluxo:

```text
Detect existing JSON store
    ↓
Backup JSON
    ↓
Create SQLite DB
    ↓
Import games
    ↓
Import metadata
    ↓
Import assets
    ↓
Validate counts
    ↓
Mark migration complete
```

Nunca apagar JSON antigo automaticamente sem backup.

---

# Local do Banco

```text
AppData/WiiStudio/database/wiistudio.db
```

Configurações:

```text
Database.Path
Database.EnableWal = true
Database.BackupBeforeMigration = true
```

---

# Performance

Obrigatório:

- WAL mode;
- índices básicos;
- paginação para biblioteca grande;
- busca por título/Game ID;
- evitar carregar tudo quando não necessário;
- transações em importação em lote.

---

# Integração com UI

LibraryView deve carregar dados de repository/Application Service.

A UI não acessa SQLite diretamente.

Fluxo:

```text
LibraryViewModel
    ↓
Application Service
    ↓
Repository
    ↓
SQLite
```

---

# Integração com F102/F103/F104

- F102 salva assets no cache e registra paths em `GameAssets`;
- F103 salva metadados em `GameMetadata`;
- F104 exibe galeria a partir de `GameAssets`;
- LibraryView usa `Games` + `GameAssets`.

---

# Segurança

- validar paths antes de persistir;
- não armazenar dados sensíveis desnecessários;
- tratar corrupção de DB;
- backup antes de migration;
- logs sem dados sensíveis;
- transações para operações críticas.

---

# Diagnóstico

Adicionar:

```text
Database
Path: ...
Size: ...
WAL enabled: true/false
Schema version: ...
Games count: ...
Metadata count: ...
Assets count: ...
Last migration: ...
Last error: ...
```

---

# Testes Esperados

- cria banco vazio;
- aplica migration inicial;
- insere jogo;
- atualiza jogo;
- busca por Game ID;
- busca por título;
- favoritos;
- salva metadados;
- salva assets;
- transação rollback;
- migração JSON fake;
- banco corrompido tratado;
- índices existem;
- paginação funciona.

---

# Critérios de Aceite

- [x] Biblioteca persiste em SQLite.
- [x] Repositories criados.
- [x] Migrations funcionam.
- [x] JSON antigo é migrado com backup.
- [x] Busca/filtro usam banco.
- [x] Assets são registrados.
- [x] Metadados são registrados.
- [x] Diagnóstico mostra status do banco.
- [x] Testes de banco passam.
- [x] UI não acessa SQLite diretamente.

---

# Regras para o Codex

- Não acessar SQLite pela UI.
- Não misturar schema no ViewModel.
- Sempre usar repository/Application.
- Sempre criar migration.
- Sempre testar migration.
- Sempre usar transação em lote.
- Nunca apagar JSON antigo sem backup.
- Preparar para bibliotecas grandes.

---

# Dependências

- `009-Database.md`
- `016-GameLibrary.md`
- `F002-ScanGameLibrary.md`
- `F032-LibraryView.md`
- `F096-DatabaseTesting.md`
- `F102-GameTdbCoverProvider.md`
- `F103-GameTDBMetadataProvider.md`
- `F104-CoverAssetGalleryAndBatchSelection.md`

---

# Definition of Done

- Schema inicial definido.
- Repositories definidos.
- Migração JSON definida.
- UI integrada via Application.
- Tests de DB definidos.

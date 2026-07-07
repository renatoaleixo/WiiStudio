# F096 - Database Testing

> Feature ID: F096
> Status: Ready for Development
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes de banco de dados do WiiStudio, cobrindo SQLite,
migrations, repositories, transações, constraints, queries críticas, seed de dados,
rollback e cenários de corrupção ou inconsistência.

Banco local é parte essencial da biblioteca, metadados, cache, tarefas, settings e
histórico da aplicação.

---

# Requisitos Funcionais

## RF001
Testar criação do banco.

## RF002
Testar migrations.

## RF003
Testar repositories.

## RF004
Testar constraints.

## RF005
Testar transações e rollback.

## RF006
Testar queries críticas.

## RF007
Testar concorrência básica.

## RF008
Testar dados inválidos.

## RF009
Testar backup/restore envolvendo banco.

## RF010
Garantir isolamento entre testes.

---

# Estratégia

Usar:

- SQLite in-memory para testes rápidos;
- SQLite em arquivo temporário para migrations reais;
- seed controlado;
- cleanup automático;
- factories/builders.

---

# Organização

```text
tests/
└── WiiStudio.Infrastructure.Tests/
    └── Database/
        ├── Migrations/
        ├── Repositories/
        ├── Transactions/
        ├── Queries/
        └── TestData/
```

---

# Migrations

Testar:

- banco vazio para versão atual;
- migração incremental;
- dados preservados;
- migration idempotente quando aplicável;
- falha controlada;
- rollback quando suportado.

---

# Repositories

Testar:

- inserir;
- atualizar;
- buscar por id;
- buscar lista;
- remover lógico quando aplicável;
- filtros;
- ordenação;
- paginação;
- constraints.

---

# Transações

Operações críticas devem testar rollback.

Exemplos:

- importar jogo;
- atualizar metadados;
- remover jogo;
- restaurar backup;
- registrar tarefa e logs.

---

# Constraints

Testar:

- chaves primárias;
- chaves únicas;
- foreign keys;
- not null;
- índices;
- integridade referencial.

---

# Queries Críticas

Testar queries de:

- biblioteca de jogos;
- busca e filtros;
- duplicidade;
- favoritos;
- tarefas;
- logs;
- metadados;
- assets.

---

# Concorrência Básica

Testar:

- leituras simultâneas;
- escrita durante leitura quando aplicável;
- lock tratado;
- timeout de banco;
- retry controlado quando definido.

---

# Dados Inválidos

Testar:

- GameId inválido;
- título vazio;
- path inválido;
- metadados incompletos;
- status desconhecido;
- data inválida;
- FK inexistente.

---

# Banco Corrompido

Quando possível, testar:

- arquivo de banco inválido;
- schema inesperado;
- versão desconhecida;
- recovery amigável;
- diagnóstico.

---

# Backup/Restore

Testar:

- backup do banco;
- hash;
- restauração;
- banco incompatível;
- banco corrompido;
- safety backup.

---

# Test Fixtures

```csharp
public sealed class DatabaseTestFixture : IAsyncLifetime
{
    public string DatabasePath { get; private set; } = string.Empty;

    public Task InitializeAsync() { /* create temp database */ }

    public Task DisposeAsync() { /* cleanup */ }
}
```

---

# Regras para o Codex

- Sempre usar banco isolado por teste ou fixture segura.
- Sempre limpar dados.
- Sempre testar migrations.
- Sempre testar constraints.
- Sempre testar rollback em operação crítica.
- Sempre testar dados inválidos.
- Nunca usar banco real do usuário.
- Nunca depender de ordem de testes.
- Nunca ignorar falha de migration.
- Nunca mascarar erro de integridade.

---

# Casos de Erro

- Migration falha.
- Schema divergente.
- Constraint violada.
- Rollback não ocorre.
- Query retorna duplicados.
- Dados órfãos.
- Lock de banco.
- Banco corrompido.
- Seed inválido.
- Teste suja estado global.

---

# Critérios de Aceite

- [ ] Criação do banco testada.
- [ ] Migrations testadas.
- [ ] Repositories testados.
- [ ] Constraints testadas.
- [ ] Transações testadas.
- [ ] Rollback testado.
- [ ] Queries críticas testadas.
- [ ] Concorrência básica testada.
- [ ] Dados inválidos testados.
- [ ] Banco real do usuário proibido.

---

# Definition of Done

- Database Testing documentado.
- Estratégia SQLite definida.
- Migrations e repositories cobertos.
- Rollback e constraints definidos.
- Documentação atualizada.

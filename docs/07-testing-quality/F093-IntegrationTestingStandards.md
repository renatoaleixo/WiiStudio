# F093 - Integration Testing Standards

> Feature ID: F093
> Status: Ready for Development
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes de integração do WiiStudio, cobrindo banco SQLite,
repositórios, serviços de infraestrutura, sistema de arquivos controlado, cache,
settings, plugins, ProcessRunner fake, Task Manager e fluxos entre camadas.

Testes de integração devem validar o comportamento real entre componentes sem
depender de ambiente do usuário, rede real, ferramentas externas reais ou dados
não controlados.

---

# Requisitos Funcionais

## RF001
Definir escopo dos testes de integração.

## RF002
Padronizar uso de banco temporário ou in-memory.

## RF003
Padronizar uso de diretórios temporários.

## RF004
Padronizar fakes para processos externos.

## RF005
Testar integração entre Application e Infrastructure.

## RF006
Testar repositories e migrations.

## RF007
Testar plugins em ambiente controlado.

## RF008
Testar cenários de erro e rollback.

## RF009
Garantir limpeza de recursos após testes.

## RF010
Evitar dependência do ambiente local do desenvolvedor.

---

# Escopo

Testes de integração devem cobrir:

- repositories;
- SQLite;
- migrations;
- settings;
- cache;
- file system controlado;
- backup/restore;
- plugin discovery;
- plugin loading;
- permission services;
- ProcessRunner fake;
- Task Manager;
- metadata providers fake;
- asset providers fake;
- conversion providers fake.

---

# Fora do Escopo

Testes de integração não devem depender de:

- rede real;
- Wiimms real;
- NKit real;
- dispositivos USB reais;
- paths reais do usuário;
- plugins externos reais;
- ordem de execução;
- horário real sem abstração.

Esses cenários podem existir como testes manuais ou opcionais.

---

# Organização Recomendada

```text
tests/
└── WiiStudio.Integration.Tests/
    ├── Database/
    ├── Repositories/
    ├── FileSystem/
    ├── Settings/
    ├── Plugins/
    ├── ProcessRunner/
    ├── BackupRestore/
    ├── TestInfrastructure/
    └── TestData/
```

---

# Banco de Dados

Usar:

- SQLite in-memory quando possível;
- SQLite em arquivo temporário quando migrations reais forem necessárias;
- seed controlado;
- cleanup automático.

Testar:

- criação de schema;
- migrations;
- constraints;
- queries críticas;
- transações;
- rollback;
- concorrência básica.

---

# File System

Usar diretórios temporários.

Regras:

- nunca usar pasta real do usuário;
- criar estrutura no setup;
- limpar no teardown;
- simular arquivos válidos e inválidos;
- testar path traversal;
- testar permissão quando possível;
- usar abstração para espaço insuficiente.

---

# ProcessRunner

Usar fake ou stub.

Cenários:

- exit code zero;
- exit code diferente de zero;
- stdout;
- stderr;
- timeout;
- cancelamento;
- logs redigidos;
- progresso simulado.

Não chamar processo externo real como regra padrão.

---

# Plugins

Testar com plugins de teste.

Cenários:

- manifesto válido;
- manifesto inválido;
- discovery;
- loading;
- permissions;
- plugin falha no initialize;
- plugin registra capability;
- unload remove capability;
- diagnóstico é registrado.

---

# Settings

Testar:

- carregar settings padrão;
- salvar settings;
- migrar versão de settings;
- redigir settings sensíveis;
- importar/exportar settings;
- settings de plugin isoladas.

---

# Cache

Testar:

- escrita;
- leitura;
- expiração;
- invalidação;
- cache corrompido;
- modo offline;
- fallback.

---

# Backup/Restore

Testar:

- criação de backup;
- manifesto;
- hash;
- restauração;
- backup corrompido;
- path traversal;
- safety backup;
- rollback quando possível.

---

# Transações

Operações críticas devem ser testadas com rollback.

Exemplos:

- importação parcial;
- atualização de metadados;
- restauração de backup;
- remoção de jogo;
- atualização de biblioteca.

---

# Test Fixtures

Criar fixtures reutilizáveis:

```csharp
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public string TempRoot { get; private set; } = string.Empty;
    public IServiceProvider Services { get; private set; } = default!;

    public Task InitializeAsync() { /* setup */ }

    public Task DisposeAsync() { /* cleanup */ }
}
```

---

# Dados de Teste

Organizar dados em:

```text
TestData/
├── Games/
├── Manifests/
├── Plugins/
├── Databases/
├── Backups/
└── Logs/
```

Evitar arquivos grandes salvo quando indispensável.

---

# Regras de Isolamento

Cada teste deve:

- ter banco próprio ou transação própria;
- ter diretório temporário próprio;
- não depender de estado global;
- limpar recursos;
- evitar ordem implícita;
- evitar relógio real quando relevante.

---

# Logging em Testes

Logs podem ser capturados para validar:

- erro esperado;
- PluginId;
- CorrelationId;
- redaction;
- diagnóstico.

Não testar texto exato quando isso for frágil.

---

# Regras para o Codex

- Sempre usar ambiente controlado.
- Sempre usar banco temporário.
- Sempre usar diretórios temporários.
- Sempre limpar recursos.
- Sempre testar sucesso e erro.
- Sempre testar rollback em operação crítica.
- Sempre usar fakes para processos externos.
- Nunca usar paths reais do usuário.
- Nunca depender de rede real.
- Nunca depender de ferramenta externa real.
- Nunca depender de ordem de testes.

---

# Casos de Erro

- Banco não inicializa.
- Migration falha.
- Arquivo temporário não limpo.
- Teste depende de ordem.
- Processo real chamado.
- Path real usado.
- Plugin de teste falha fora do esperado.
- Cache de teste reaproveitado indevidamente.
- Teste flaky por timing.

---

# Critérios de Aceite

- [ ] Escopo de integração definido.
- [ ] Organização de projeto definida.
- [ ] Banco temporário definido.
- [ ] File system temporário definido.
- [ ] ProcessRunner fake definido.
- [ ] Plugins de teste definidos.
- [ ] Rollback testado.
- [ ] Cleanup obrigatório.
- [ ] Rede real proibida.
- [ ] Ferramenta externa real proibida por padrão.

---

# Definition of Done

- Integration Testing Standards documentado.
- Infraestrutura de teste definida.
- Regras de isolamento definidas.
- Testes críticos mapeados.
- Documentação atualizada.

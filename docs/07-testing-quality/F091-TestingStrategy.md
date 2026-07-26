# F091 - Testing Strategy

> Feature ID: F091
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir a estratégia oficial de testes do WiiStudio, cobrindo testes unitários, integração, UI, infraestrutura, banco, processos externos, plugins, performance, regressão e qualidade contínua.

A estratégia deve garantir que o projeto evolua com segurança, especialmente por envolver arquivos, processos externos, plugins, banco local, UI desktop e operações longas.

---

# Requisitos Funcionais

## RF001
Definir camadas de teste.

## RF002
Definir responsabilidades por tipo de teste.

## RF003
Definir ferramentas recomendadas.

## RF004
Definir padrões para mocks e fakes.

## RF005
Definir testes obrigatórios para features críticas.

## RF006
Definir critérios mínimos de qualidade.

## RF007
Integrar testes ao workflow de desenvolvimento.

## RF008
Garantir cobertura de falhas e cancelamento.

## RF009
Garantir testes para segurança.

## RF010
Garantir testes para plugins e ProcessRunner.

---

# Pirâmide de Testes

```text
UI / E2E Tests            menor quantidade
Integration Tests         quantidade média
Unit Tests                maior quantidade
```

---

# Tipos de Teste

## Unit Tests

Testam regra isolada.

Usar para:

- Domain;
- Application Services;
- validações;
- mapeamentos;
- policies;
- ViewModels;
- serviços pequenos.

## Integration Tests

Testam integração entre componentes.

Usar para:

- SQLite;
- repositories;
- ProcessRunner fake;
- plugin discovery;
- plugin loading;
- file system controlado;
- cache;
- settings.

## UI Tests

Testam comportamento visual e interação.

Usar para:

- navegação;
- dialogs;
- wizards;
- DataGrid;
- acessibilidade básica;
- temas.

## Regression Tests

Garantem que bugs corrigidos não voltem.

## Performance Tests

Validam cenários pesados.

Exemplos:

- biblioteca grande;
- conversão em lote;
- muitos logs;
- muitos plugins.

---

# Ferramentas

Recomendadas:

- xUnit;
- FluentAssertions;
- Moq ou NSubstitute;
- testes com arquivos temporários;
- SQLite in-memory ou arquivo temporário;
- Avalonia testing utilities quando aplicável;
- snapshots controlados quando úteis.

---

# Organização

```text
tests/
├── WiiStudio.Domain.Tests/
├── WiiStudio.Application.Tests/
├── WiiStudio.Infrastructure.Tests/
├── WiiStudio.UI.Tests/
├── WiiStudio.Plugin.Tests/
├── WiiStudio.Integration.Tests/
└── WiiStudio.Performance.Tests/
```

---

# Convenções de Nome

```text
MethodName_ShouldExpectedBehavior_WhenCondition
```

Exemplo:

```csharp
Validate_ShouldRejectPathTraversal_WhenPathEscapesRoot()
```

---

# Testes Obrigatórios por Feature

Toda feature deve ter:

- testes de sucesso;
- testes de erro;
- testes de cancelamento quando async;
- testes de permissão quando sensível;
- testes de logging/redaction quando aplicável;
- testes de ViewModel quando tiver UI.

---

# Async e CancellationToken

Operações async devem testar:

- sucesso;
- falha;
- cancelamento;
- timeout quando aplicável;
- não bloqueio da UI;
- propagação correta de exceções controladas.

---

# ProcessRunner

Nunca testar chamando ferramenta externa real como requisito padrão.

Usar:

- fake process runner;
- stub de stdout/stderr;
- exit code simulado;
- timeout simulado;
- cancelamento simulado.

Testes reais podem ser separados como opcionais/manual.

---

# File System

Usar diretórios temporários controlados.

Testar:

- permissão negada quando possível;
- path inválido;
- path traversal;
- espaço insuficiente via abstração;
- arquivo em uso quando possível;
- escrita temporária.

---

# Banco SQLite

Testar:

- migrations;
- repositories;
- constraints;
- queries críticas;
- concorrência básica;
- rollback;
- dados inválidos.

---

# Plugins

Testar:

- manifesto válido;
- manifesto inválido;
- discovery;
- loading;
- permissions;
- diagnostics;
- unload;
- falha isolada.

---

# UI / MVVM

ViewModels devem ser testáveis sem abrir janela real.

Testar:

- commands;
- loading state;
- error state;
- selection;
- navigation request;
- validation;
- disabled state;
- notification request.

---

# Segurança

Features sensíveis devem testar:

- path traversal;
- permissão negada;
- secret redigido;
- operação destrutiva exige confirmação;
- plugin sem permissão;
- processo externo bloqueado;
- update checksum inválido.

---

# Dados de Teste

Usar builders/factories.

```text
TestData/
├── Games/
├── Manifests/
├── Plugins/
├── Logs/
└── Backups/
```

Evitar dados grandes no repositório salvo exceções necessárias.

---

# Critérios de Qualidade

Antes de merge:

- testes passam;
- nova regra tem teste;
- bug corrigido tem regressão;
- feature sensível passou checklist;
- testes não dependem de ordem;
- testes não dependem do ambiente do desenvolvedor.

---

# Regras para o Codex

- Sempre criar testes para nova feature.
- Sempre testar cenários de erro.
- Sempre testar cancelamento em operações longas.
- Sempre usar fakes para processo externo.
- Sempre usar temp directories para arquivos.
- Sempre evitar teste frágil.
- Sempre nomear testes claramente.
- Nunca depender de ferramenta externa real em teste unitário.
- Nunca acessar paths reais do usuário.
- Nunca ignorar segurança em testes.

---

# Casos de Erro

- Teste depende de ordem.
- Teste usa path real.
- Teste chama processo externo real.
- Teste falha por cultura/localização.
- Teste depende de horário atual sem abstração.
- Teste flaky por timing.
- Teste não limpa arquivo temporário.
- Mock não representa comportamento real.

---

# Critérios de Aceite

- [x] Pirâmide de testes definida.
- [x] Tipos de teste definidos.
- [x] Ferramentas definidas.
- [x] Organização de projetos definida.
- [x] Convenção de nomes definida.
- [x] Testes obrigatórios por feature definidos.
- [x] ProcessRunner testável.
- [x] File System testável.
- [x] SQLite testável.
- [x] Plugins testáveis.
- [x] Segurança testável.
- [x] Regras para Codex definidas.

---

# Definition of Done

- Testing Strategy documentada.
- Camadas definidas.
- Convenções definidas.
- Critérios mínimos definidos.
- Testes sensíveis mapeados.
- Documentação atualizada.

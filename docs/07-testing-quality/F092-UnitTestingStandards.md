# F092 - Unit Testing Standards

> Feature ID: F092
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes unitários do WiiStudio, incluindo estrutura, nomenclatura, isolamento, mocks, assertions, dados de teste, testes async, ViewModels, Domain, Application Services e regras para evitar testes frágeis.

---

# Requisitos Funcionais

## RF001
Padronizar nomenclatura de testes unitários.

## RF002
Definir estrutura Arrange/Act/Assert.

## RF003
Definir uso de FluentAssertions.

## RF004
Definir uso de mocks e fakes.

## RF005
Garantir isolamento entre testes.

## RF006
Definir padrões para testes async.

## RF007
Definir padrões para ViewModels.

## RF008
Definir padrões para Domain e Application.

## RF009
Evitar dependência de infraestrutura real.

## RF010
Garantir testes legíveis e sustentáveis.

---

# Ferramentas

- xUnit
- FluentAssertions
- Moq ou NSubstitute
- Builders de teste
- Fakes manuais quando mais claros que mocks

---

# Nomenclatura

Formato recomendado:

```text
MethodName_ShouldExpectedBehavior_WhenCondition
```

Exemplos:

```csharp
CalculateProgress_ShouldReturn100_WhenAllItemsCompleted()
Validate_ShouldFail_WhenPathContainsTraversal()
ImportCommand_ShouldBeDisabled_WhenNoFileSelected()
```

---

# Estrutura AAA

Todo teste deve seguir:

```csharp
// Arrange
var service = CreateService();

// Act
var result = service.Execute();

// Assert
result.Should().NotBeNull();
```

Evitar misturar múltiplos comportamentos no mesmo teste.

---

# Testes Pequenos

Um teste unitário deve:

- testar um comportamento;
- ser rápido;
- ser determinístico;
- não depender de rede;
- não depender de disco real;
- não depender de processo externo real;
- não depender de ordem de execução.

---

# FluentAssertions

Preferir:

```csharp
result.Success.Should().BeTrue();
items.Should().HaveCount(3);
exception.Message.Should().Contain("invalid");
```

Evitar asserts genéricos pouco legíveis.

---

# Mocks

Usar mocks para dependências externas.

Exemplos:

- logger;
- repository;
- notification service;
- navigation service;
- process runner;
- clock;
- file system abstraction;
- permission service.

Não mockar demais quando fake simples for mais legível.

---

# Fakes

Preferir fakes quando:

- comportamento é simples;
- múltiplos testes usam a mesma dependência;
- mock ficaria verboso;
- estado interno ajuda validação.

Exemplo:

```text
FakeProcessRunner
FakeClock
FakeFileSystem
FakeNotificationService
```

---

# Builders

Usar builders para objetos complexos.

Exemplo:

```csharp
var game = GameBuilder.Default()
    .WithTitle("Mario Kart Wii")
    .WithRegion("USA")
    .Build();
```

---

# Async Tests

Testes async devem:

- usar `async Task`;
- evitar `.Result`;
- evitar `.Wait()`;
- testar cancelamento;
- testar exceções assíncronas.

Exemplo:

```csharp
Func<Task> act = () => service.ExecuteAsync(ct);
await act.Should().ThrowAsync<OperationCanceledException>();
```

---

# CancellationToken

Quando método recebe token:

- testar cancelamento antes de iniciar;
- testar cancelamento durante operação quando possível;
- verificar que dependências recebem token;
- garantir que falha é controlada.

---

# Domain Tests

Testar:

- entidades;
- value objects;
- invariantes;
- regras puras;
- validações;
- eventos de domínio quando houver.

Domain tests não devem depender de infraestrutura.

---

# Application Tests

Testar:

- use cases;
- services;
- commands;
- validações;
- policies;
- orquestração;
- tratamento de erro;
- chamadas a interfaces.

Usar mocks/fakes para Infrastructure.

---

# ViewModel Tests

Testar:

- estado inicial;
- commands;
- `CanExecute`;
- loading;
- error;
- seleção;
- validação;
- chamadas de navegação;
- notificações;
- cancelamento.

Não abrir janela real em unit test.

---

# Segurança

Testes unitários de segurança devem cobrir:

- path traversal;
- permissão negada;
- redaction;
- plugin sem permissão;
- processo externo bloqueado;
- operação destrutiva exige confirmação.

---

# Logging

Não testar texto exato de log salvo quando isso for frágil.

Testar:

- evento registrado;
- nível correto;
- dados sensíveis redigidos;
- PluginId presente quando aplicável.

---

# O que Evitar

Evitar:

- testes gigantes;
- múltiplas responsabilidades;
- sleeps;
- dependência de horário real;
- dependência de cultura;
- dependência de paths reais;
- asserts em detalhes irrelevantes;
- mocks excessivamente rígidos;
- chamar ferramentas externas reais.

---

# Estrutura Recomendada

```text
tests/
└── WiiStudio.Application.Tests/
    ├── Features/
    ├── Services/
    ├── ViewModels/
    ├── Builders/
    ├── Fakes/
    └── TestData/
```

---

# Regras para o Codex

- Sempre criar teste unitário para regra nova.
- Sempre usar Arrange/Act/Assert.
- Sempre usar nomes claros.
- Sempre usar FluentAssertions.
- Sempre evitar infraestrutura real.
- Sempre testar erro e sucesso.
- Sempre testar cancelamento quando existir.
- Sempre usar builders para objetos complexos.
- Nunca usar Thread.Sleep.
- Nunca depender de rede, disco real ou processo externo real.
- Nunca deixar teste flaky.

---

# Casos de Erro

- Teste depende de ordem.
- Teste falha por horário.
- Teste falha por idioma/cultura.
- Teste usa path do usuário.
- Teste chama `wit` real.
- Teste não limpa estado.
- Mock rígido quebra refatoração.
- Teste não verifica comportamento importante.

---

# Critérios de Aceite

- [x] Padrão de nomenclatura definido.
- [x] AAA definido.
- [x] FluentAssertions definido.
- [x] Mocks e fakes definidos.
- [x] Builders definidos.
- [x] Async tests definidos.
- [x] CancellationToken testado.
- [x] Domain tests definidos.
- [x] Application tests definidos.
- [x] ViewModel tests definidos.
- [x] Segurança coberta.
- [x] Regras para Codex definidas.

---

# Definition of Done

- Unit Testing Standards documentado.
- Convenções definidas.
- Padrões de mocks/fakes definidos.
- Regras de async e cancelamento definidas.
- Testes de segurança considerados.
- Documentação atualizada.

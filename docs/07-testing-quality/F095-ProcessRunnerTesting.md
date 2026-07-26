# F095 - ProcessRunner Testing

> Feature ID: F095
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes do ProcessRunner do WiiStudio, garantindo que
execução de processos externos seja validada com segurança, previsibilidade,
cancelamento, timeout, captura de logs, redaction e integração com Task Manager.

ProcessRunner é crítico porque controla ferramentas como Wiimms, NKit Legacy e
providers externos de plugins.

---

# Requisitos Funcionais

## RF001
Testar execução bem-sucedida.

## RF002
Testar exit code de falha.

## RF003
Testar stdout e stderr.

## RF004
Testar timeout.

## RF005
Testar cancelamento.

## RF006
Testar validação de executável.

## RF007
Testar validação de argumentos.

## RF008
Testar redaction de logs.

## RF009
Testar integração com Task Manager.

## RF010
Evitar dependência de processos externos reais em testes unitários.

---

# Estratégia

Usar três níveis:

## Unit Tests

Com fake executor.

## Integration Tests

Com processo controlado e seguro, quando necessário.

## Manual/Optional Tests

Com ferramentas reais como `wit`, separados do pipeline obrigatório.

---

# Fake Process Executor

Criar abstração interna para simular:

- stdout;
- stderr;
- exit code;
- duração;
- timeout;
- cancelamento;
- processo travado;
- exceção de inicialização.

---

# Cenários Obrigatórios

## Sucesso

- ExitCode = 0.
- stdout capturado.
- duração registrada.
- TimedOut = false.
- Cancelled = false.

## Falha

- ExitCode != 0.
- stderr capturado.
- resultado indica falha.
- logs registrados.

## Timeout

- processo excede timeout;
- processo é encerrado;
- TimedOut = true;
- log de timeout registrado.

## Cancelamento

- CancellationToken é acionado;
- processo é encerrado;
- Cancelled = true;
- temporários são tratados pelo chamador.

## Executável Inválido

- path inexistente;
- sem permissão;
- diretório no lugar de arquivo;
- path inseguro.

## Argumentos

- argumentos estruturados;
- argumento inválido rejeitado;
- shell arbitrário bloqueado;
- path traversal bloqueado.

## Redaction

- token no stderr redigido;
- Authorization header redigido;
- connection string redigida;
- logs exportados redigidos.

---

# Testes com Wiimms

Testes com Wiimms real devem ser opcionais.

Categoria sugerida:

```text
Manual
ExternalTool
RequiresWiimms
```

Não devem bloquear pipeline padrão.

---

# Modelo de Teste

```csharp
[Fact]
public async Task RunAsync_ShouldReturnTimedOut_WhenProcessExceedsTimeout()
{
    // Arrange
    var runner = CreateRunnerWithFakeProcess(delay: TimeSpan.FromMinutes(5));

    // Act
    var result = await runner.RunAsync(request);

    // Assert
    result.TimedOut.Should().BeTrue();
}
```

---

# Integração com Task Manager

Testar:

- tarefa criada;
- progresso atualizado;
- cancelamento propagado;
- falha registrada;
- logs associados;
- resultado final.

---

# Integração com Plugins

Testar:

- plugin sem `execute-process` é bloqueado;
- plugin com permissão executa via ProcessRunner;
- PluginId aparece no log;
- tentativa de shell arbitrário é bloqueada.

---

# Segurança

Testar obrigatoriamente:

- `cmd /c` bloqueado quando não permitido;
- `bash -c` bloqueado;
- path traversal;
- argumento com secret redigido;
- executável fora de escopo bloqueado.

---

# Regras para o Codex

- Sempre usar fake executor em unit tests.
- Sempre testar timeout.
- Sempre testar cancelamento.
- Sempre testar stdout e stderr.
- Sempre testar exit code diferente de zero.
- Sempre testar redaction.
- Sempre testar plugin permission.
- Nunca depender de Wiimms real em unit test.
- Nunca usar Process.Start diretamente no teste da aplicação.
- Nunca ignorar shell injection.

---

# Casos de Erro

- Processo trava.
- Processo não inicia.
- Processo retorna stderr grande.
- stdout contém segredo.
- Timeout não mata processo.
- Cancelamento não propaga.
- Exit code ignorado.
- Plugin executa sem permissão.
- Shell arbitrário aceito indevidamente.

---

# Critérios de Aceite

- [x] Fake executor definido.
- [x] Sucesso testado.
- [x] Falha testada.
- [x] Timeout testado.
- [x] Cancelamento testado.
- [x] stdout/stderr testados.
- [x] Executável inválido testado.
- [x] Argumentos inseguros testados.
- [x] Redaction testada.
- [x] Plugins testados.
- [x] Task Manager testado.
- [x] Wiimms real opcional.

---

# Definition of Done

- ProcessRunner Testing documentado.
- Cenários obrigatórios definidos.
- Segurança coberta.
- Integrações definidas.
- Documentação atualizada.

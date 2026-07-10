# F109 - Wiimms Operation Progress

> Feature ID: F109  
> Status: Ready for Development  
> Priority: High  
> Epic: Wiimms Integration / Task Manager  
> Area: ProcessRunner, Import, Export, Conversion, USB Operations, UI Feedback

# Objetivo

Exibir progresso real ou estimado das operações executadas com `wit` e `wwt`,
evitando que importação, remoção, exportação e conversão pareçam travadas.

A feature deve integrar stdout/stderr do Wiimms com:

- Task Manager;
- Status Bar;
- Notification Center;
- Logging Center;
- Advanced Diagnostics.

---

# Problema

Operações com Wiimms podem demorar, especialmente:

- importar jogo para HD USB;
- remover jogo;
- converter ISO/WBFS;
- exportar;
- listar/verificar disco grande;
- copiar em lote.

Sem feedback progressivo, a UI pode parecer congelada.

---

# Resultado Esperado

Durante uma operação:

```text
Convertendo Mario Kart Wii
Progresso: 43%
Velocidade: 18 MB/s
Tempo estimado: 2m 10s
```

Quando progresso real não estiver disponível:

```text
Processando...
```

com estado indeterminado, logs e último status conhecido.

---

# Escopo

## Incluído

- parser de progresso Wiimms;
- leitura incremental de stdout/stderr;
- progresso indeterminado quando não houver percentual;
- integração com Task Manager;
- integração com Status Bar;
- integração com Notification Center;
- cancelamento;
- timeout;
- logs;
- testes com saída fake do Wiimms.

## Fora do Escopo Inicial

- alterar código-fonte do Wiimms;
- depender de uma única versão do Wiimms sem fallback;
- bloquear operação se progresso não puder ser parseado;
- interpretar todo texto de saída como erro.

---

# Abstrações

## IWiimmsProgressParser

```csharp
public interface IWiimmsProgressParser
{
    bool TryParse(
        string line,
        out WiimmsOperationProgress progress);
}
```

## WiimmsOperationProgress

```csharp
public sealed class WiimmsOperationProgress
{
    public double? Percentage { get; init; }
    public string? CurrentOperation { get; init; }
    public string? CurrentFile { get; init; }
    public long? ProcessedBytes { get; init; }
    public long? TotalBytes { get; init; }
    public double? SpeedBytesPerSecond { get; init; }
    public TimeSpan? EstimatedRemaining { get; init; }
    public bool IsIndeterminate { get; init; }
}
```

---

# Integração com ProcessRunner

ProcessRunner deve suportar callback/progress para linhas de saída.

Exemplo conceitual:

```csharp
public sealed class ProcessRequest
{
    public Action<string>? OnStandardOutputLine { get; init; }
    public Action<string>? OnStandardErrorLine { get; init; }
}
```

ou mecanismo equivalente via `IProgress<ProcessOutputLine>`.

---

# Fluxo

```text
Start Wiimms Operation
    ↓
ProcessRunner starts wit/wwt
    ↓
Read stdout/stderr line by line
    ↓
WiimmsProgressParser parses line
    ↓
Task Manager updates progress
    ↓
Status Bar updates short status
    ↓
Notification Center receives final result
```

---

# Operações Cobertas

- Import to USB;
- Export from USB;
- Convert ISO/WBFS;
- Batch conversion;
- Remove game;
- Verify;
- List WBFS games;
- Copy.

---

# Estados

```csharp
public enum WiimmsTaskState
{
    Pending,
    Starting,
    Running,
    Indeterminate,
    Cancelling,
    Completed,
    Failed,
    Cancelled,
    TimedOut
}
```

---

# Fallback Indeterminado

Se o parser não identificar progresso:

```text
IsIndeterminate = true
CurrentOperation = last known operation
```

UI deve mostrar spinner/progress indeterminado.

Isso não deve ser tratado como falha.

---

# Task Manager

Mostrar:

- nome da operação;
- jogo atual;
- percentual;
- progresso indeterminado quando necessário;
- logs resumidos;
- botão cancelar;
- duração;
- exit code ao final.

Exemplo:

```text
Copiando RMCP01 para USB
Status: Em andamento
Progresso: 43%
Cancelar
```

---

# Status Bar

Mostrar texto curto:

```text
Convertendo RMCP01... 43%
```

ou:

```text
Executando Wiimms...
```

quando indeterminado.

---

# Notification Center

Final de sucesso:

```text
Operação Wiimms concluída com sucesso.
```

Falha:

```text
A operação Wiimms falhou. Veja os detalhes no diagnóstico.
```

Cancelamento:

```text
Operação cancelada pelo usuário.
```

---

# Cancelamento

O cancelamento deve:

- acionar CancellationToken;
- pedir encerramento do processo;
- matar processo se necessário após timeout;
- marcar tarefa como Cancelled;
- preservar arquivos originais;
- limpar temporários quando seguro;
- registrar diagnóstico.

---

# Timeout

Toda operação deve ter timeout definido por tipo:

```text
Version check: 10s
List device: 30s
Verify: configurável
Copy/convert: configurável ou sem timeout rígido, mas com cancelamento
```

Operações longas devem continuar canceláveis.

---

# Logs

Registrar:

- operação;
- tool;
- argumentos sanitizados;
- progresso parseado;
- saída relevante;
- exit code;
- duração;
- cancelamento;
- timeout;
- falhas.

Não registrar:

- payload sensível;
- paths completos desnecessários;
- saída binária.

---

# Parser

O parser deve ser tolerante.

Regras:

- aceitar múltiplos formatos de linha;
- ignorar linha irrelevante;
- não lançar exceção por linha desconhecida;
- detectar percentual quando existir;
- detectar warnings;
- detectar mensagens finais;
- preservar último status útil.

---

# Testes Esperados

## Unitários

- parse percentual;
- parse operação textual;
- parse linha irrelevante;
- fallback indeterminado;
- progresso com bytes;
- progresso com velocidade;
- saída stderr com warning;
- operação concluída;
- operação falha.

## Integração com Fakes

- ProcessRunner fake emite linhas;
- Task Manager recebe updates;
- Status Bar recebe último status;
- Notification final emitida;
- cancelamento interrompe processo fake;
- timeout marca tarefa corretamente.

---

# Critérios de Aceite

- [ ] Operações Wiimms aparecem no Task Manager.
- [ ] Progresso é atualizado quando possível.
- [ ] Estado indeterminado é exibido quando não há percentual.
- [ ] Status Bar mostra operação atual.
- [ ] Notificação final é exibida.
- [ ] Cancelamento funciona.
- [ ] Timeout funciona.
- [ ] Falha mostra diagnóstico.
- [ ] Testes usam saída fake do Wiimms.

---

# Regras para o Codex

- Não bloquear UI.
- Não depender de internet.
- Não exigir progresso real para concluir.
- Sempre aceitar CancellationToken.
- Sempre capturar stdout/stderr.
- Sempre redigir logs.
- Sempre atualizar Task Manager.
- Nunca tratar linha não parseada como erro fatal.
- Nunca chamar Wiimms fora do ProcessRunner.

---

# Dependências

- `012-WiimmsIntegration.md`
- `F026-TaskManager.md`
- `F037-StatusBar.md`
- `F057-ProgressIndicators.md`
- `F083-ExternalProcessSecurity.md`
- `F095-ProcessRunnerTesting.md`

---

# Definition of Done

- Parser de progresso definido.
- ProcessRunner com callback definido.
- Task Manager integrado.
- Status Bar e Notification integrados.
- Testes com saída fake definidos.

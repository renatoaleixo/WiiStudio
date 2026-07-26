# F083 - External Process Security

> Feature ID: F083
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial de segurança para execução de processos externos no WiiStudio, incluindo Wiimms ISO Tools, NKit Legacy e qualquer ferramenta acionada por plugins ou integrações futuras.

Processos externos são uma área crítica de segurança e estabilidade. Toda execução deve passar por validação, controle, logging, timeout, cancelamento e sanitização de argumentos.

---

# Requisitos Funcionais

## RF001
Obrigar uso de `ProcessRunner` para qualquer processo externo.

## RF002
Proibir uso direto de `Process.Start` fora da infraestrutura aprovada.

## RF003
Validar executável antes da execução.

## RF004
Sanitizar argumentos.

## RF005
Bloquear shell arbitrário.

## RF006
Aplicar timeout configurável.

## RF007
Suportar `CancellationToken`.

## RF008
Capturar stdout e stderr.

## RF009
Redigir dados sensíveis em logs.

## RF010
Integrar com Plugin Permissions quando processo for solicitado por plugin.

---

# Ferramentas Cobertas

- `wit`
- `wwt`
- NKit Legacy tools
- ferramentas externas de plugins
- utilitários futuros de conversão
- validadores externos
- ferramentas de diagnóstico

---

# Regra Principal

Nunca executar processo externo diretamente pela UI, Application Layer ou plugin.

Obrigatório:

```text
UI / ViewModel
    ↓
Application Service
    ↓
Infrastructure
    ↓
ProcessRunner
    ↓
External Tool
```

---

# Proibições

É proibido:

- chamar `Process.Start` diretamente em ViewModel;
- executar shell arbitrário;
- concatenar argumentos em string única sem sanitização;
- executar comando vindo de plugin sem permissão;
- executar path não validado;
- ignorar stdout/stderr;
- ignorar timeout;
- logar argumentos com segredo;
- bloquear thread da UI.

---

# Interface Recomendada

```csharp
public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        ProcessRequest request,
        CancellationToken cancellationToken = default);
}
```

---

# ProcessRequest

```csharp
public sealed class ProcessRequest
{
    public string ExecutablePath { get; init; } = string.Empty;
    public IReadOnlyList<string> Arguments { get; init; } = [];
    public string? WorkingDirectory { get; init; }
    public TimeSpan Timeout { get; init; }
    public bool CaptureOutput { get; init; } = true;
    public string? CorrelationId { get; init; }
    public string? PluginId { get; init; }
}
```

---

# ProcessResult

```csharp
public sealed class ProcessResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public bool TimedOut { get; init; }
    public bool Cancelled { get; init; }
}
```

---

# Validação do Executável

Antes de executar:

- verificar se path existe;
- verificar se é arquivo;
- verificar permissão de execução;
- validar se está em local permitido;
- validar configuração do usuário;
- validar assinatura/checksum quando disponível;
- bloquear path suspeito.

---

# Argumentos

Argumentos devem ser passados como lista estruturada.

Permitido:

```csharp
Arguments = ["copy", sourcePath, destinationPath]
```

Evitar:

```csharp
Arguments = [$"copy {sourcePath} {destinationPath}"]
```

---

# Shell Arbitrário

Não utilizar:

```text
cmd.exe /c
bash -c
sh -c
powershell -Command
```

Exceto se houver caso extremamente justificado, documentado, protegido e aprovado por checklist de segurança.

---

# Timeout

Timeouts recomendados:

| Operação | Timeout |
|---------|--------:|
| Verificar versão da ferramenta | 10 s |
| Listar dispositivo | 30 s |
| Verificar arquivo | 2 min |
| Conversão simples | configurável |
| Conversão em lote | controlado por tarefa |
| Diagnóstico externo | 1 min |

Operações longas devem ser rastreadas pelo Task Manager.

---

# Cancelamento

Toda execução deve aceitar `CancellationToken`.

Quando cancelada:

- tentar encerrar processo de forma limpa;
- matar processo se necessário após tempo limite;
- registrar cancelamento;
- preservar arquivos originais;
- limpar temporários quando seguro.

---

# Captura de Logs

Capturar:

- stdout;
- stderr;
- exit code;
- duração;
- comando lógico;
- ferramenta;
- correlationId;
- pluginId quando aplicável.

Não registrar segredos em claro.

---

# Redação de Logs

Aplicar `F088-LogRedaction.md`.

Redigir:

- tokens;
- senhas;
- chaves;
- paths sensíveis quando necessário;
- argumentos marcados como sensíveis.

---

# Integração com Plugins

Plugins precisam da permissão:

```text
execute-process
```

Além disso:

- comando deve ser validado;
- argumentos devem ser validados;
- logs devem incluir PluginId;
- usuário deve conseguir bloquear plugin;
- falhas devem aparecer em Plugin Diagnostics.

---

# Integração com Task Manager

Processos longos devem ser tarefas.

A tarefa deve conter:

- nome;
- ferramenta;
- progresso quando disponível;
- logs;
- cancelamento;
- resultado;
- exit code.

---

# Integração com Safe Path Handling

Todos os paths enviados ao processo devem ser normalizados e validados.

Exemplos:

- origem da conversão;
- destino;
- diretório temporário;
- working directory;
- executável.

---

# Regras para o Codex

- Nunca usar `Process.Start` diretamente.
- Sempre usar `IProcessRunner`.
- Sempre passar argumentos como lista.
- Sempre validar executável.
- Sempre validar paths.
- Sempre aplicar timeout.
- Sempre aceitar `CancellationToken`.
- Sempre capturar stdout/stderr.
- Sempre redigir logs.
- Sempre integrar processos longos ao Task Manager.
- Sempre exigir permissão para plugins.
- Nunca usar shell arbitrário.
- Nunca concatenar argumentos perigosamente.

---

# Casos de Erro

- Executável inexistente.
- Sem permissão de execução.
- Argumento inválido.
- Path inseguro.
- Timeout.
- Cancelamento.
- Exit code diferente de zero.
- stderr com erro.
- Working directory inválido.
- Plugin sem permissão.
- Processo trava.
- Falha ao matar processo.

---

# Critérios de Aceite

- [x] Uso obrigatório de ProcessRunner definido.
- [x] `Process.Start` direto proibido.
- [x] Executável validado.
- [x] Argumentos estruturados.
- [x] Shell arbitrário bloqueado.
- [x] Timeout definido.
- [x] Cancelamento definido.
- [x] Logs capturados.
- [x] Redação de logs definida.
- [x] Integração com plugins definida.
- [x] Integração com Task Manager definida.
- [x] Paths seguros exigidos.

---

# Testes

## Unitários

- Executável inexistente bloqueado.
- Argumento estruturado aceito.
- Timeout tratado.
- Cancelamento tratado.
- Exit code capturado.
- stderr capturado.
- Plugin sem permissão bloqueado.
- Log redigido.

## Integração

- Executar `wit --version`.
- Executar comando com sucesso.
- Executar comando com falha.
- Cancelar processo longo.
- Timeout de processo.
- Task Manager acompanha processo.
- Plugin tenta executar sem permissão.
- Path inseguro bloqueado.

---

# Dependências

- F026-TaskManager.md
- F057-ProgressIndicators.md
- F075-PluginPermissions.md
- F078-PluginConversionProviders.md
- F081-SecurityFoundation.md
- F086-SafePathHandling.md
- F088-LogRedaction.md
- 012-WiimmsIntegration.md
- 013-NKitLegacy.md

---

# Definition of Done

- External Process Security documentado.
- ProcessRunner obrigatório.
- Segurança de argumentos definida.
- Timeout e cancelamento definidos.
- Logs e redaction definidos.
- Testes definidos.
- Documentação atualizada.

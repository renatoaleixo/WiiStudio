# F078 - Plugin Conversion Providers

> Feature ID: F078
> Status: Ready for Development
> Priority: High
> Epic: Plugin SDK

# Objetivo

Definir o modelo oficial para plugins provedores de conversão no WiiStudio, permitindo que ferramentas externas ou fluxos customizados sejam integrados ao motor de conversão sem acoplar a aplicação principal a implementações específicas.

Conversion Provider Plugins devem respeitar segurança, permissões, ProcessRunner, Task Manager, cancelamento, progresso, diagnóstico e regras de preservação dos arquivos originais.

---

# Requisitos Funcionais

## RF001
Definir contrato oficial para Conversion Provider Plugin.

## RF002
Permitir declarar formatos de entrada e saída suportados.

## RF003
Permitir validação prévia de conversão.

## RF004
Integrar com Conversion Engine.

## RF005
Integrar com Task Manager.

## RF006
Integrar com ProcessRunner quando executar ferramenta externa.

## RF007
Suportar progresso, cancelamento e logs.

## RF008
Validar permissões antes da execução.

## RF009
Garantir segurança no tratamento de paths e argumentos.

## RF010
Nunca substituir o motor oficial Wiimms sem configuração explícita.

---

# Contrato Base

```csharp
public interface IConversionProviderPlugin : IWiiStudioPlugin
{
    string ProviderId { get; }

    string DisplayName { get; }

    IReadOnlyList<string> SupportedInputFormats { get; }

    IReadOnlyList<string> SupportedOutputFormats { get; }

    Task<ConversionValidationResult> ValidateAsync(
        ConversionProviderRequest request,
        CancellationToken cancellationToken = default);

    Task<ConversionProviderResult> ConvertAsync(
        ConversionProviderRequest request,
        IProgress<ConversionProgress> progress,
        CancellationToken cancellationToken = default);
}
```

---

# Request

```csharp
public sealed class ConversionProviderRequest
{
    public string SourcePath { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public string InputFormat { get; init; } = string.Empty;
    public string OutputFormat { get; init; } = string.Empty;
    public bool PreserveOriginal { get; init; } = true;
    public IReadOnlyDictionary<string, string> Options { get; init; } =
        new Dictionary<string, string>();
}
```

---

# Resultado

```csharp
public sealed class ConversionProviderResult
{
    public bool Success { get; init; }
    public string? OutputPath { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<string> Logs { get; init; } = [];
}
```

---

# Progresso

```csharp
public sealed class ConversionProgress
{
    public double? Percentage { get; init; }
    public string? CurrentOperationKey { get; init; }
    public string? CurrentFile { get; init; }
    public bool IsIndeterminate { get; init; }
}
```

---

# Formatos

Providers podem declarar suporte a:

- ISO
- WBFS
- RVZ
- NKit
- WIA
- CISO
- formatos futuros

O suporte declarado deve ser validado antes da conversão.

---

# Permissões Necessárias

Conversion Provider pode exigir:

- `file-system-read`
- `file-system-write`
- `execute-process`
- `access-game-library`
- `modify-game-library`
- `background-tasks`

`execute-process` é crítico e exige validação rigorosa.

---

# Integração com ProcessRunner

Plugins que executam ferramenta externa devem usar ProcessRunner controlado.

Regras:

- nunca usar shell arbitrário;
- validar executável;
- sanitizar argumentos;
- capturar stdout/stderr;
- aplicar timeout;
- respeitar CancellationToken;
- registrar logs;
- bloquear paths inseguros.

---

# Integração com Task Manager

Conversões devem ser executadas como tarefas rastreáveis.

A tarefa deve conter:

- tipo;
- provider;
- origem;
- destino;
- progresso;
- status;
- logs;
- cancelamento;
- resultado.

---

# Validação Prévia

Antes de converter:

- verificar arquivo de origem;
- verificar permissão de leitura;
- verificar permissão de escrita;
- verificar espaço em disco;
- validar formato;
- validar destino;
- validar ferramenta externa;
- validar opções;
- verificar se arquivo de destino já existe.

---

# Política de Arquivos

Regras:

- preservar original por padrão;
- nunca excluir origem sem confirmação;
- usar arquivo temporário durante conversão;
- mover para destino apenas após sucesso;
- limpar temporários em falha quando seguro;
- registrar arquivos gerados.

---

# Falha Segura

Se a conversão falhar:

- preservar arquivo original;
- remover saída parcial quando seguro;
- registrar logs;
- marcar tarefa como Failed;
- notificar usuário;
- permitir ver detalhes;
- não corromper biblioteca.

---

# Prioridade de Providers

A aplicação pode escolher provider por:

- formato;
- preferência do usuário;
- disponibilidade;
- compatibilidade;
- estabilidade;
- prioridade declarada.

Wiimms permanece o provider oficial para operações WBFS, salvo configuração explícita.

---

# Regras para o Codex

- Sempre validar provider antes de executar.
- Sempre validar permissões.
- Sempre usar ProcessRunner para processo externo.
- Sempre usar Task Manager para conversões longas.
- Sempre usar CancellationToken.
- Sempre reportar progresso.
- Sempre preservar arquivo original por padrão.
- Sempre usar paths seguros.
- Sempre capturar logs.
- Nunca usar Process.Start diretamente.
- Nunca executar shell arbitrário.
- Nunca sobrescrever arquivo sem política definida.

---

# Casos de Erro

- Formato não suportado.
- Arquivo de origem inexistente.
- Sem permissão de leitura.
- Sem permissão de escrita.
- Espaço insuficiente.
- Ferramenta externa ausente.
- Processo externo falha.
- Timeout.
- Cancelamento.
- Saída inválida.
- Arquivo destino já existe.
- Path inseguro.

---

# Critérios de Aceite

- [ ] Contrato `IConversionProviderPlugin` definido.
- [ ] Formatos suportados declarados.
- [ ] Validação prévia definida.
- [ ] Progresso definido.
- [ ] Resultado definido.
- [ ] Integração com Task Manager definida.
- [ ] Integração com ProcessRunner definida.
- [ ] Permissões definidas.
- [ ] Falha segura definida.
- [ ] Política de arquivos definida.

---

# Testes

## Unitários

- Provider suporta formato.
- Provider rejeita formato.
- Validação de origem.
- Validação de destino.
- Permissão negada.
- Progresso reportado.
- Cancelamento.
- Falha preserva original.
- Timeout tratado.

## Integração

- Plugin provider simples.
- Conversão com processo externo mockado.
- Conversão em lote.
- Falha parcial.
- Logs exibidos.
- Task Manager acompanha progresso.
- Cancelamento pelo usuário.

---

# Dependências

- F007-ConvertGame.md
- F008-BatchConversion.md
- F012-WiimmsIntegration.md
- F018-MetadataSynchronization.md
- F026-TaskManager.md
- F057-ProgressIndicators.md
- F071-PluginSDKFoundation.md
- F075-PluginPermissions.md
- F083-ExternalProcessSecurity.md
- F086-SafePathHandling.md

---

# Definition of Done

- Plugin Conversion Providers documentado.
- Contratos definidos.
- Permissões e segurança definidas.
- Integração com Task Manager e ProcessRunner documentada.
- Testes definidos.
- Documentação atualizada.

# F118 - Diagnostics Repair Actions

> Feature ID: F118  
> Status: Ready for Development  
> Priority: High  
> Epic: Diagnostics / Repair UX  
> Area: Advanced Diagnostics, External Tools, Polkit, Settings, Logs  
> Depends on: F030, F101, F106

# Objetivo

Adicionar ações seguras de reparo ao módulo de Diagnóstico do WiiStudio.

Hoje o diagnóstico informa problemas, mas o usuário ainda precisa descobrir manualmente
como corrigir. Esta feature adiciona botões “Corrigir” para ações conhecidas e seguras.

---

# Problema

O diagnóstico pode apontar:

- policy Polkit ausente;
- `wwt`/`wit` não configurado;
- NKit ausente;
- logs difíceis de localizar;
- GameTDB desatualizado;
- cache corrompido;
- permissões insuficientes.

Mas falta ação direta para corrigir problemas comuns.

---

# Resultado Esperado

Na tela de diagnóstico:

```text
Wiimms não encontrado
[Corrigir]

Policy Polkit ausente
[Corrigir]

Cache de capas inválido
[Corrigir]

Logs disponíveis
[Abrir pasta]
```

---

# Escopo

## Incluído

- catálogo de problemas diagnosticáveis;
- ações de reparo seguras;
- botão “Corrigir”;
- confirmação quando necessário;
- execução com Task Manager;
- logs;
- histórico;
- rollback quando aplicável;
- testes com fakes.

## Fora do Escopo Inicial

- reparos destrutivos automáticos;
- formatar dispositivo;
- instalar ferramenta sem confirmação;
- executar comando arbitrário;
- corrigir erro desconhecido automaticamente.

---

# Ações Seguras Iniciais

## Instalar Policy Polkit

Integra com:

```text
F106 - Polkit Policy Installer
```

## Configurar Caminho do wwt/wit

Integra com:

```text
F101 - External Tool Dependency Manager
```

Ação:

- abrir seletor de executável;
- validar versão;
- salvar settings.

## Abrir Pasta de Logs

Ação simples:

```text
Open logs folder
```

Sem privilégio elevado.

## Validar Ferramentas

Ação:

- executar `wit --version`;
- executar `wwt --version`;
- validar NKit;
- atualizar diagnóstico.

## Limpar Cache Corrompido

Ação segura com confirmação:

- limpar item corrompido;
- não apagar tudo sem confirmar;
- permitir rebuild.

## Atualizar GameTDB

Integra com:

```text
F111 - GameTDB Update Source
```

---

# Modelo

## DiagnosticRepairAction

```csharp
public sealed class DiagnosticRepairAction
{
    public string ActionId { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string DescriptionKey { get; init; } = string.Empty;
    public bool RequiresConfirmation { get; init; }
    public bool IsDestructive { get; init; }
    public DiagnosticRepairRiskLevel RiskLevel { get; init; }
}
```

## DiagnosticRepairRiskLevel

```csharp
public enum DiagnosticRepairRiskLevel
{
    Safe,
    RequiresConfirmation,
    Elevated,
    Destructive
}
```

---

# Serviço

## IDiagnosticsRepairService

```csharp
public interface IDiagnosticsRepairService
{
    Task<IReadOnlyList<DiagnosticRepairAction>> GetAvailableActionsAsync(
        DiagnosticItem item,
        CancellationToken cancellationToken = default);

    Task<DiagnosticRepairResult> ExecuteAsync(
        string actionId,
        CancellationToken cancellationToken = default);
}
```

---

# Fluxo

```text
Diagnostics finds issue
    ↓
Resolve repair actions
    ↓
Show "Corrigir"
    ↓
User clicks
    ↓
Show confirmation if required
    ↓
Run repair as task
    ↓
Refresh diagnostics
    ↓
Show notification
```

---

# Segurança

Obrigatório:

- permitir apenas ações catalogadas;
- confirmar ações sensíveis;
- não executar shell arbitrário;
- não instalar nada silenciosamente;
- não armazenar senha;
- usar serviços existentes;
- logs redigidos;
- rollback quando aplicável.

---

# UX

## Problema Corrigível

```text
Problema encontrado: Wiimms não configurado.
Ação sugerida: selecionar executável wit/wwt.
[Corrigir]
```

## Confirmação

```text
Esta ação instalará uma policy do sistema usando autorização administrativa.
O WiiStudio não armazena sua senha.
Deseja continuar?
```

## Sucesso

```text
Problema corrigido com sucesso.
```

## Falha

```text
Não foi possível corrigir automaticamente. Veja os detalhes no diagnóstico.
```

---

# Diagnóstico Pós-Reparo

Após executar reparo:

```text
Run diagnostics again
```

E atualizar status.

---

# Critérios de Aceite

- [ ] Diagnóstico exibe ações de reparo.
- [ ] Policy Polkit pode ser corrigida via F106.
- [ ] Caminho do Wiimms pode ser configurado.
- [ ] Pasta de logs pode ser aberta.
- [ ] Ferramentas podem ser validadas.
- [ ] GameTDB pode ser atualizado.
- [ ] Reparo gera tarefa/notificação.
- [ ] Diagnóstico atualiza após reparo.
- [ ] Testes usam fakes.

---

# Testes Esperados

- item sem ação;
- item com ação segura;
- ação exige confirmação;
- usuário cancela;
- reparo sucesso;
- reparo falha;
- diagnóstico atualiza;
- notificação sucesso;
- notificação erro;
- ação desconhecida bloqueada.

---

# Regras para o Codex

- Nunca executar reparo não catalogado.
- Nunca instalar silenciosamente.
- Nunca pedir senha diretamente.
- Sempre confirmar ação sensível.
- Sempre atualizar diagnóstico após reparo.
- Sempre publicar notificação.
- Sempre criar teste com fake service.

---

# Dependências

- `F030-AdvancedDiagnostics.md`
- `F101-ExternalToolDependencyManager.md`
- `F106-PolkitPolicyInstaller.md`
- `F111-GameTdbUpdateSource.md`
- `F113-TaskAndNotificationCenterIntegration.md`
- `F090-SecurityChecklist.md`

---

# Definition of Done

- Catálogo de reparos definido.
- Serviço definido.
- UI e UX definidos.
- Segurança definida.
- Testes com fakes definidos.

# F068 - Window Management

> Feature ID: F068
> Status: Ready for Development
> Priority: High
> Epic: Application Shell

# Objetivo

Definir o gerenciamento oficial da janela principal do WiiStudio, incluindo tamanho
mínimo, tamanho inicial, maximização, restauração, persistência de posição,
comportamento em múltiplos monitores, fechamento seguro e integração com App Shell,
Settings, Task Manager e Responsive Behaviors.

---

# Requisitos Funcionais

## RF001
Definir tamanho inicial da janela.

## RF002
Definir tamanho mínimo suportado.

## RF003
Persistir tamanho e estado da janela.

## RF004
Restaurar posição quando seguro.

## RF005
Suportar maximizado e restaurado.

## RF006
Tratar fechamento seguro com tarefas em execução.

## RF007
Tratar fechamento com alterações pendentes.

## RF008
Suportar múltiplos monitores.

## RF009
Garantir compatibilidade com DPI scaling.

## RF010
Evitar lógica de negócio no code-behind.

---

# Tamanhos Oficiais

| Propriedade | Valor Recomendado |
|------------|------------------:|
| Largura inicial | 1280 px |
| Altura inicial | 800 px |
| Largura mínima | 1024 px |
| Altura mínima | 640 px |
| Largura ideal | 1440 px |
| Altura ideal | 900 px |

---

# Estados da Janela

A janela deve suportar:

- Normal
- Maximized
- Minimized
- FullScreen
- Closing
- Restoring
- InvalidPosition

---

# Inicialização

Fluxo recomendado:

```text
App Startup
    ↓
Load Window Settings
    ↓
Validate Saved Position
    ↓
Apply Size
    ↓
Apply Window State
    ↓
Show MainWindow
```

---

# Fechamento Seguro

Fluxo recomendado:

```text
Close Requested
    ↓
Check Running Tasks
    ↓
Check Unsaved Changes
    ↓
Ask Confirmation if Needed
    ↓
Persist Window State
    ↓
Shutdown Services
    ↓
Close Application
```

---

# Tarefas em Execução

Quando houver tarefas em execução:

- exibir confirmação;
- informar quantidade de tarefas;
- indicar se podem ser canceladas;
- permitir voltar para a aplicação;
- permitir cancelar tarefas quando seguro;
- impedir fechamento silencioso.

Exemplo:

```text
Existem 2 tarefas em execução. Deseja cancelar e sair?
```

---

# Alterações Pendentes

Quando houver alterações não salvas:

- exibir confirmação;
- informar o módulo afetado;
- permitir salvar, descartar ou cancelar;
- não perder dados silenciosamente.

---

# Persistência

Persistir:

- largura;
- altura;
- posição X;
- posição Y;
- estado maximizado;
- monitor quando aplicável;
- densidade/layout quando aplicável.

Não persistir:

- posição inválida;
- posição fora de tela;
- tamanho menor que o mínimo;
- estado inconsistente.

---

# Modelo Conceitual

```csharp
public sealed class WindowStateSettings
{
    public double Width { get; init; }
    public double Height { get; init; }
    public double? X { get; init; }
    public double? Y { get; init; }
    public bool IsMaximized { get; init; }
    public string? DisplayId { get; init; }
}
```

---

# Window State Service

```csharp
public interface IWindowStateService
{
    Task<WindowStateSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(WindowStateSettings settings, CancellationToken cancellationToken = default);
    bool IsValidPosition(WindowStateSettings settings);
}
```

---

# Close Guard Service

```csharp
public interface ICloseGuardService
{
    Task<CloseDecision> CanCloseAsync(CancellationToken cancellationToken = default);
}
```

---

# Decisão de Fechamento

```csharp
public enum CloseDecision
{
    Allow,
    Cancel,
    AskConfirmation
}
```

---

# Múltiplos Monitores

A aplicação deve:

- validar se o monitor salvo ainda existe;
- mover janela para monitor principal se necessário;
- evitar abrir fora da área visível;
- respeitar escala DPI do monitor atual;
- lidar com desconexão de monitor externo.

---

# DPI Scaling

A janela deve funcionar corretamente com:

- 100%
- 125%
- 150%
- 175%
- 200%

O layout interno deve ser validado junto com `F060-ResponsiveBehaviors.md`.

---

# Fullscreen

Fullscreen deve ser opcional.

Regras:

- manter atalho de saída;
- não esconder notificações críticas;
- preservar estado anterior;
- permitir retorno ao modo normal;
- evitar ativar automaticamente.

---

# Integração com Settings

Configurações relacionadas:

- restaurar última janela;
- iniciar maximizado;
- lembrar posição;
- confirmar saída com tarefas;
- confirmar saída com alterações pendentes;
- iniciar em modo compacto.

---

# Integração com Task Manager

Antes de fechar:

- consultar tarefas em execução;
- identificar tarefas canceláveis;
- avisar sobre tarefas não canceláveis;
- registrar cancelamento quando aplicável;
- aguardar encerramento seguro quando necessário.

---

# Integração com Dialogs

Confirmações de saída devem usar Dialog System.

Exemplos:

- tarefas em execução;
- alterações pendentes;
- erro ao salvar estado;
- confirmação de cancelamento.

---

# Integração com App Shell

O App Shell deve expor comandos globais:

- minimizar;
- maximizar/restaurar;
- alternar fullscreen;
- fechar;
- restaurar layout;
- redefinir posição da janela.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Windowing/
        ├── WindowStateService.cs
        ├── CloseGuardService.cs
        ├── WindowStateSettings.cs
        ├── WindowMetrics.cs
        └── WindowCommands.cs
```

---

# Acessibilidade

Obrigatório:

- fechamento por teclado conforme padrão do sistema;
- dialogs de confirmação acessíveis;
- foco preservado após cancelar fechamento;
- fullscreen com saída acessível;
- não abrir janela fora da tela;
- respeitar escala de fonte;
- respeitar DPI.

---

# Regras de UX

- Nunca fechar silenciosamente com tarefas críticas.
- Nunca perder alterações pendentes.
- Restaurar posição apenas se válida.
- Não abrir janela fora do monitor.
- Não usar fullscreen automaticamente.
- Tamanho mínimo deve preservar usabilidade.
- Confirmações devem ser claras e curtas.
- A janela deve respeitar comportamento nativo do sistema operacional.

---

# Regras para o Codex

- Nunca colocar regras de fechamento no code-behind diretamente.
- Sempre usar serviços abstratos.
- Sempre validar posição antes de restaurar.
- Sempre consultar Task Manager antes de fechar.
- Sempre consultar alterações pendentes antes de fechar.
- Sempre usar Dialog Service para confirmação.
- Sempre persistir estado de forma segura.
- Sempre respeitar DPI scaling.
- Sempre garantir tamanho mínimo.
- Nunca usar valores hardcoded fora dos tokens/configurações oficiais.

---

# Casos de Erro

- Estado salvo inválido.
- Monitor salvo inexistente.
- Tamanho salvo menor que o mínimo.
- Falha ao salvar estado.
- Falha ao carregar estado.
- Fechamento bloqueado por tarefa.
- Fechamento bloqueado por alteração pendente.
- Dialog de confirmação falha.
- Janela abre fora da tela.

---

# Critérios de Aceite

- [ ] Tamanho inicial definido.
- [ ] Tamanho mínimo definido.
- [ ] Estado da janela persistido.
- [ ] Posição validada.
- [ ] Maximizado/restaurado suportado.
- [ ] Fechamento seguro definido.
- [ ] Tarefas em execução tratadas.
- [ ] Alterações pendentes tratadas.
- [ ] Múltiplos monitores considerados.
- [ ] DPI scaling considerado.
- [ ] Integração com Dialog System.
- [ ] Sem lógica de negócio no code-behind.

---

# Testes

## Unitários

- Carregar estado válido.
- Ignorar estado inválido.
- Validar posição fora de tela.
- Persistir maximizado.
- Bloquear fechamento com tarefa.
- Bloquear fechamento com alteração pendente.
- Permitir fechamento seguro.
- Tratar falha ao salvar estado.

## Integração

- Abrir janela inicial.
- Restaurar tamanho.
- Restaurar maximizado.
- Mudar monitor.
- Fechar com tarefa em execução.
- Fechar com alteração pendente.
- Cancelar fechamento.
- Confirmar fechamento.
- DPI 150%.
- Redimensionamento até mínimo.

---

# Dependências

- F023-SettingsManager.md
- F026-TaskManager.md
- F048-DialogsAndModals.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- F070-AccessibilityNavigation.md
- 004-DevelopmentWorkflow.md
- 008-UI-UX.md

---

# Definition of Done

- Window Management documentado.
- Tamanhos oficiais definidos.
- Persistência especificada.
- Fechamento seguro definido.
- Múltiplos monitores considerados.
- DPI scaling considerado.
- Testes definidos.
- Documentação atualizada.

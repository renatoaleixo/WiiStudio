# F065 - Footer & Status Area

> Feature ID: F065
> Status: Ready for Development
> Priority: High
> Epic: Application Shell

# Objetivo

Definir o padrão oficial do Footer e da Status Area do WiiStudio, incluindo Status Bar,
indicadores globais, mensagens curtas, progresso resumido, estado de tarefas,
estado de dispositivos, avisos técnicos e integração com Task Manager, Notification
Center, Device Manager, Diagnostics e App Shell.

---

# Requisitos Funcionais

## RF001
Definir estrutura visual da Status Bar.

## RF002
Exibir mensagens globais curtas.

## RF003
Exibir estado de tarefas em execução.

## RF004
Exibir progresso global quando aplicável.

## RF005
Exibir estado de dispositivo ativo quando aplicável.

## RF006
Exibir indicadores de erro, warning e status técnico.

## RF007
Integrar com Task Manager e Notification Center.

## RF008
Garantir compatibilidade com Light e Dark Theme.

## RF009
Garantir acessibilidade.

## RF010
Garantir responsividade.

---

# Estrutura Visual

```text
+------------------------------------------------------------------+
| Pronto | 2 tarefas em execução | USB conectado | Último erro: ... |
+------------------------------------------------------------------+
```

---

# Anatomia

A Status Area pode possuir:

- mensagem principal;
- indicador de progresso;
- contador de tarefas;
- indicador de dispositivo;
- indicador de logs/erros;
- estado de banco local;
- estado de provider;
- versão ou ambiente;
- ações rápidas discretas.

---

# Estados Globais

A Status Bar deve suportar:

- Ready
- Busy
- Loading
- RunningTask
- Warning
- Error
- Offline
- DeviceConnected
- DeviceDisconnected
- Updating
- DiagnosticsRunning

---

# Mensagens Recomendadas

## Ready

```text
Pronto
```

## Loading

```text
Carregando biblioteca...
```

## RunningTask

```text
2 tarefas em execução
```

## DeviceConnected

```text
Dispositivo USB conectado
```

## Warning

```text
Concluído com avisos
```

## Error

```text
Falha ao executar operação
```

---

# Indicadores Oficiais

## Task Indicator

Exibe resumo de tarefas.

Exemplos:

- 1 tarefa em execução.
- 3 tarefas em execução.
- Última tarefa falhou.
- Todas as tarefas concluídas.

## Progress Indicator

Exibe progresso global compacto.

Exemplos:

```text
Convertendo... 62%
```

```text
Sincronizando metadados...
```

## Device Indicator

Exibe estado de dispositivo.

Exemplos:

- USB conectado.
- Nenhum dispositivo.
- Dispositivo sem permissão.
- WBFS detectado.

## Diagnostics Indicator

Exibe estado técnico.

Exemplos:

- Diagnóstico OK.
- Avisos encontrados.
- Erro crítico detectado.
- Logs disponíveis.

## Notification Indicator

Exibe resumo de notificações.

Exemplos:

- 2 avisos.
- 1 erro.
- Nenhuma notificação.

---

# Responsividade

## Compact

- Exibir apenas mensagem principal.
- Ocultar indicadores secundários.
- Manter progresso global.
- Usar ícones com tooltip.

## Standard

- Exibir mensagem principal.
- Exibir tarefas.
- Exibir dispositivo.
- Exibir erro recente.

## Wide

- Exibir mais indicadores.
- Permitir ações rápidas.
- Exibir provider ou ambiente.

---

# Tokens Utilizados

A Status Area deve consumir:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Info`
- `Color.Warning`
- `Color.Error`
- `Color.Success`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Layout.StatusBarHeight`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Shell/
        └── StatusBar/
            ├── StatusBarView.axaml
            ├── StatusBarViewModel.cs
            ├── StatusIndicatorView.axaml
            └── StatusMessageView.axaml
```

---

# StatusBarViewModel

Responsável por:

- expor mensagem principal;
- expor estado global;
- expor progresso compacto;
- expor contadores;
- refletir tarefas em andamento;
- refletir dispositivo ativo;
- refletir avisos e erros;
- expor comandos de detalhes.

Exemplo conceitual:

```csharp
public sealed partial class StatusBarViewModel : ObservableObject
{
    [ObservableProperty]
    private string _messageKey = "Status.Ready";

    [ObservableProperty]
    private double? _progress;

    [ObservableProperty]
    private int _runningTaskCount;

    [ObservableProperty]
    private bool _hasWarnings;

    [ObservableProperty]
    private bool _hasErrors;
}
```

---

# Integrações

## Task Manager

A Status Bar deve observar tarefas em execução, progresso global e falhas.

## Notification Center

A Status Bar pode indicar notificações não lidas, último erro e warnings.

## Device Manager

A Status Bar pode indicar dispositivo conectado, ativo ou sem permissão.

## Diagnostics

A Status Bar pode indicar severidade técnica atual.

---

# Ações Rápidas

A Status Bar pode permitir ações discretas:

- abrir Task Manager;
- abrir Notification Center;
- abrir logs;
- abrir detalhes do dispositivo;
- abrir diagnóstico.

Ações devem ser acessíveis por teclado e tooltip.

---

# Acessibilidade

Obrigatório:

- texto acessível para indicadores;
- foco visível em ações;
- não depender apenas de cor;
- progresso anunciado quando relevante;
- mensagens importantes anunciadas;
- contraste WCAG AA;
- tooltips em ícones;
- não roubar foco durante atualizações.

---

# Regras de UX

- Status Bar deve ser discreta.
- Não usar Status Bar para mensagens longas.
- Erros importantes devem ir para Notification Center ou Dialog.
- Progresso longo deve estar no Task Manager.
- Status Bar deve resumir, não detalhar.
- Evitar excesso de indicadores.
- Indicadores críticos devem ser claros.
- Em modo Compact, priorizar progresso e erro.

---

# Regras para o Codex

- Nunca colocar regra de negócio na Status Bar.
- Sempre consumir eventos/estado de serviços.
- Sempre usar tokens do Design System.
- Sempre usar textos localizáveis.
- Sempre suportar Light/Dark.
- Sempre garantir acessibilidade.
- Sempre usar tooltips em ícones.
- Sempre integrar com Task Manager.
- Sempre integrar com Notification Center.
- Nunca usar valores hardcoded.

---

# Casos de Erro

- Mensagem sem localização.
- Progresso inválido.
- Contador negativo.
- Estado de tarefa inconsistente.
- Dispositivo removido durante exibição.
- Indicador sem tooltip.
- Status Bar com overflow.
- Erro crítico exibido apenas como texto curto.

---

# Critérios de Aceite

- [ ] Status Bar definida.
- [ ] Mensagem principal suportada.
- [ ] Task Indicator definido.
- [ ] Progress Indicator definido.
- [ ] Device Indicator definido.
- [ ] Diagnostics Indicator definido.
- [ ] Notification Indicator definido.
- [ ] Responsividade documentada.
- [ ] Compatível com Light/Dark.
- [ ] Acessibilidade validada.
- [ ] Textos localizáveis.
- [ ] Sem lógica de negócio na View.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Estado Ready.
- Estado Busy.
- Progresso atualizado.
- Tarefas em execução atualizadas.
- Warning exibido.
- Error exibido.
- Dispositivo conectado.
- Texto localizado.

## Integração

- Status Bar na MainWindow.
- Tarefa iniciada.
- Tarefa concluída.
- Tarefa falhou.
- Dispositivo conectado.
- Dispositivo removido.
- Notification Center com erro.
- Alternância Light/Dark.
- Modo Compact.
- Navegação por teclado.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F030-AdvancedDiagnostics.md
- F035-DeviceManager.md
- F037-StatusBar.md
- F041-DesignSystemFoundation.md
- F056-BadgesAndStatusIndicators.md
- F057-ProgressIndicators.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- 022-Localization.md

---

# Definition of Done

- Footer & Status Area documentados.
- Indicadores oficiais definidos.
- Integrações com Task Manager e Notification Center especificadas.
- Responsividade considerada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

# F057 - Progress Indicators

> Feature ID: F057
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de indicadores de progresso do WiiStudio, incluindo ProgressBar, Spinner, indicadores indeterminados, progresso em lote, progresso por etapa, progresso em dialogs, cards, task manager e status bar.

Progress Indicators devem comunicar claramente ao usuário que uma operação está em andamento, quanto já foi concluído, quando é possível cancelar e qual é o estado atual da operação.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de indicadores de progresso.

## RF002
Suportar progresso determinado e indeterminado.

## RF003
Suportar progresso por tarefa e por lote.

## RF004
Integrar com Task Manager.

## RF005
Integrar com Dialogs, Status Bar, Cards e DataGrid.

## RF006
Garantir compatibilidade com Light e Dark Theme.

## RF007
Garantir acessibilidade.

## RF008
Suportar cancelamento quando a operação permitir.

## RF009
Evitar múltiplos cliques e operações duplicadas.

## RF010
Exibir mensagens claras durante operações longas.

---

# Tipos Oficiais

## ProgressBar

Usada para progresso percentual determinado.

Exemplos:

- conversão de arquivo;
- importação;
- exportação;
- verificação de integridade;
- restauração de backup.

## Indeterminate ProgressBar

Usada quando não é possível calcular percentual.

Exemplos:

- detectando dispositivos;
- consultando provider remoto;
- carregando metadados;
- inicializando ferramenta externa.

## Spinner

Usado para carregamento breve ou localizado.

Exemplos:

- carregando card;
- atualizando botão;
- validando campo;
- buscando resultados.

## Step Progress

Usado para fluxos com etapas.

Exemplos:

- Import Wizard;
- Conversion Wizard;
- Backup Restore;
- Plugin Installation.

## Batch Progress

Usado para múltiplos itens.

Exemplos:

- convertendo 10 jogos;
- baixando 300 capas;
- verificando biblioteca inteira;
- exportando múltiplos arquivos.

## Inline Progress

Usado dentro de linha, card ou painel.

Exemplos:

- tarefa em linha no DataGrid;
- card de tarefa;
- status de provider;
- item de download.

## Global Progress

Usado na Status Bar para operação global.

Exemplos:

- sincronização em segundo plano;
- atualização de GameTDB;
- verificação agendada;
- indexação da biblioteca.

---

# Estrutura Visual

## ProgressBar

```text
Convertendo arquivo...
[████████████░░░░░░░░] 62%
```

## Batch Progress

```text
Convertendo jogos...
Item 4 de 12 - Mario Kart Wii
[██████░░░░░░░░░░░░░░] 32%
```

## Step Progress

```text
1. Selecionar arquivo  ✓
2. Validar             ✓
3. Converter           em andamento
4. Concluir            pendente
```

---

# Estados

Indicadores de progresso devem suportar:

- Idle
- Starting
- Running
- Paused
- Cancelling
- Cancelled
- Completed
- Failed
- Partial
- Indeterminate

---

# Modelo Conceitual

```csharp
public sealed class ProgressState
{
    public string TaskId { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string? CurrentOperationKey { get; init; }
    public double? Percentage { get; init; }
    public int? CurrentItem { get; init; }
    public int? TotalItems { get; init; }
    public bool IsIndeterminate { get; init; }
    public bool CanCancel { get; init; }
    public OperationStatus Status { get; init; }
}
```

---

# Percentual

Quando o percentual for conhecido:

- usar valor entre 0 e 100;
- nunca ultrapassar 100;
- arredondar visualmente quando necessário;
- evitar oscilações regressivas sem justificativa.

---

# Progresso Indeterminado

Usar apenas quando não houver forma confiável de calcular progresso.

Exemplos:

- aguardando processo externo iniciar;
- lendo metadados desconhecidos;
- detectando dispositivos;
- carregando dados remotos.

Assim que houver progresso mensurável, alternar para ProgressBar determinada.

---

# Progresso em Lote

Deve informar:

- total de itens;
- item atual;
- nome do item atual;
- percentual geral;
- percentual do item quando disponível;
- falhas parciais;
- possibilidade de cancelamento.

---

# Progress Dialog

Operações longas ou bloqueantes podem usar Progress Dialog.

Deve exibir:

- título;
- mensagem atual;
- progresso;
- item atual;
- botão cancelar se aplicável;
- detalhes expansíveis quando técnico;
- resultado final.

---

# Progress em Botões

Botões podem exibir loading quando a ação é curta ou local.

Regras:

- bloquear múltiplos cliques;
- manter feedback visual;
- preservar largura quando possível;
- não usar para operações longas sem Task Manager.

---

# Progress em DataGrid

Linhas de tarefas devem exibir:

- status;
- progresso;
- ação de cancelar;
- tempo decorrido;
- mensagem curta.

---

# Progress em Status Bar

A Status Bar deve mostrar operações globais de fundo.

Exemplos:

```text
Sincronizando metadados... 45%
```

```text
Atualizando biblioteca...
```

---

# Tokens Utilizados

Progress Indicators devem consumir tokens do Design System:

- `Color.Primary`
- `Color.Success`
- `Color.Warning`
- `Color.Error`
- `Color.Surface`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Small`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── ProgressIndicators.axaml
    ├── ProgressBar.axaml
    ├── Spinner.axaml
    ├── StepProgress.axaml
    ├── BatchProgress.axaml
    └── ProgressTokens.axaml
```

---

# Integração com Task Manager

Task Manager deve ser a fonte principal de progresso para operações longas.

Fluxo recomendado:

```text
Command
  ↓
Application Service
  ↓
Task Manager
  ↓
Progress Reporter
  ↓
UI Components
```

---

# Integração com CancellationToken

Operações canceláveis devem receber `CancellationToken`.

Exemplo conceitual:

```csharp
public Task ConvertAsync(
    ConvertGameRequest request,
    IProgress<ProgressState> progress,
    CancellationToken cancellationToken);
```

---

# Integração com Notification Center

Ao finalizar operação:

- sucesso deve gerar notificação;
- falha deve gerar notificação;
- cancelamento pode gerar notificação discreta;
- resultado parcial deve explicar quantos itens falharam.

---

# Acessibilidade

Obrigatório:

- Texto descritivo para progresso.
- Percentual anunciado quando aplicável.
- Não depender apenas de animação.
- Estado de erro textual.
- Estado concluído textual.
- Contraste WCAG AA.
- Animações não excessivas.
- Suporte a redução de movimento quando disponível.

---

# Regras de UX

- Operações acima de 1 segundo devem exibir feedback.
- Operações longas devem permitir cancelamento quando possível.
- Não mostrar progresso falso.
- Não reiniciar progress bar sem necessidade.
- Mensagem atual deve ser compreensível.
- Em lote, informar item atual.
- Em falha parcial, explicar resultado.
- Evitar múltiplos spinners simultâneos na mesma área.
- Usar Progress Dialog apenas quando o fluxo precisar bloquear a tela.

---

# Regras para o Codex

- Sempre mostrar feedback para operação longa.
- Sempre usar Task Manager para operações longas.
- Sempre usar `IProgress<T>` ou padrão equivalente.
- Sempre usar `CancellationToken` quando operação suportar cancelamento.
- Nunca bloquear thread da UI.
- Nunca usar progresso hardcoded ou falso.
- Sempre tratar sucesso, erro e cancelamento.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre garantir acessibilidade textual.
- Sempre evitar duplo clique em ações em execução.

---

# Casos de Erro

- Operação falha.
- Operação cancelada.
- Percentual inválido.
- Progresso trava.
- Processo externo não retorna progresso.
- Falha parcial em lote.
- Task Manager indisponível.
- UI tenta atualizar tarefa finalizada.

---

# Critérios de Aceite

- [x] ProgressBar definida.
- [x] Spinner definido.
- [x] Progresso indeterminado definido.
- [x] Step Progress definido.
- [x] Batch Progress definido.
- [x] Integração com Task Manager documentada.
- [x] Cancelamento previsto.
- [x] Estados Completed, Failed e Cancelled definidos.
- [x] Compatível com Light/Dark.
- [x] Acessibilidade validada.
- [x] Sem valores hardcoded.
- [x] Sem bloqueio da UI.

---

# Testes

## Unitários

- Percentual válido.
- Percentual limitado a 100.
- Estado indeterminado.
- Cancelamento.
- Falha.
- Conclusão.
- Falha parcial.
- Mensagem atualizada.

## Integração

- Import Wizard.
- Conversion Wizard.
- Task Manager.
- Progress Dialog.
- Status Bar.
- DataGrid de tarefas.
- Button Loading.
- Alternância Light/Dark.
- Cancelamento de operação.
- Processo externo sem progresso determinado.

---

# Dependências

- F007-ConvertGame.md
- F008-BatchConversion.md
- F017-DownloadCovers.md
- F022-GameTDBUpdateManager.md
- F025-NotificationCenter.md
- F026-TaskManager.md
- F033-ImportWizard.md
- F034-ConversionWizard.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F048-DialogsAndModals.md
- F049-DataGrid.md
- 006-ApplicationLayer.md

---

# Definition of Done

- Indicadores de progresso documentados.
- Estados definidos.
- Integração com Task Manager especificada.
- Cancelamento documentado.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

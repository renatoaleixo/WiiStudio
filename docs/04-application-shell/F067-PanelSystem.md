# F067 - Panel System

> Feature ID: F067
> Status: Ready for Development
> Priority: High
> Epic: Application Shell

# Objetivo

Definir o sistema oficial de painéis do WiiStudio, incluindo painéis laterais,
painéis recolhíveis, painéis contextuais, painéis fixos, overlays e áreas auxiliares
da interface.

O Panel System deve permitir que a aplicação apresente detalhes, filtros, propriedades,
logs, ações contextuais e informações complementares sem sobrecarregar o Workspace.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de painéis.

## RF002
Suportar painéis fixos, recolhíveis e em overlay.

## RF003
Integrar com App Shell, Workspace e Navigation System.

## RF004
Suportar responsividade em Compact, Standard, Wide e UltraWide.

## RF005
Permitir painéis contextuais baseados na tela atual.

## RF006
Suportar estados aberto, fechado, recolhido, carregando e erro.

## RF007
Garantir compatibilidade com Light e Dark Theme.

## RF008
Garantir acessibilidade por teclado.

## RF009
Preservar estado quando aplicável.

## RF010
Evitar lógica de negócio na View.

---

# Tipos Oficiais de Painel

## Right Panel

Painel lateral direito para detalhes e propriedades.

Exemplos:

- detalhes rápidos de um jogo;
- propriedades de dispositivo;
- detalhes de tarefa;
- detalhes de log;
- filtros avançados.

## Left Panel

Painel lateral esquerdo adicional quando necessário, sem substituir a Sidebar.

Exemplos:

- árvore de categorias;
- agrupamento de filtros;
- navegação secundária complexa.

## Bottom Panel

Painel inferior para informações técnicas ou logs.

Exemplos:

- saída do processo `wit`;
- console de diagnóstico;
- logs técnicos;
- detalhes de tarefas.

## Floating Panel

Painel flutuante usado para conteúdo auxiliar.

Exemplos:

- ajuda contextual;
- propriedades rápidas;
- pré-visualização.

## Overlay Panel

Painel sobreposto ao Workspace, usado principalmente em modo Compact.

Exemplos:

- filtros em telas pequenas;
- detalhes de item;
- painel de notificações;
- painel de ajuda.

---

# Estrutura Visual

```text
+--------------------------------------------------------------+
| Header                                                       |
+-------------+------------------------------------+-----------+
| Sidebar     | Workspace                          | Panel     |
|             |                                    |           |
|             |                                    |           |
+-------------+------------------------------------+-----------+
| Status Bar                                                   |
+--------------------------------------------------------------+
```

---

# Estados dos Painéis

Todo painel deve suportar:

- Closed
- Opening
- Open
- Closing
- Collapsed
- Loading
- Error
- Disabled

---

# Anatomia de um Painel

Um painel pode possuir:

- Header;
- título;
- subtítulo;
- botão fechar;
- botão recolher;
- ações rápidas;
- conteúdo;
- footer;
- estado loading;
- estado error;
- scroll interno.

---

# Tamanhos Oficiais

| Painel | Largura/Altura Recomendada |
|-------|-----------------------------|
| Right Panel Small | 280 px |
| Right Panel Medium | 320 px |
| Right Panel Large | 420 px |
| Bottom Panel Small | 180 px |
| Bottom Panel Medium | 260 px |
| Bottom Panel Large | 360 px |
| Overlay Panel | até 90% da janela |

---

# Comportamento Responsivo

## Compact

- Painéis devem abrir como overlay.
- Right Panel não deve reduzir excessivamente o Workspace.
- O botão fechar deve estar sempre visível.
- ESC deve fechar quando seguro.

## Standard

- Right Panel pode aparecer recolhido por padrão.
- Painéis secundários devem ser opcionais.
- Workspace deve manter prioridade.

## Wide

- Right Panel pode permanecer visível.
- Painéis de detalhes podem coexistir com DataGrid.

## UltraWide

- Painéis extras podem ser exibidos simultaneamente.
- Workspace pode ser dividido com detalhes persistentes.

---

# Painéis por Área

## Biblioteca

Painéis recomendados:

- filtros avançados;
- detalhes rápidos do jogo;
- assets;
- metadados;
- integridade.

## Dispositivos

Painéis recomendados:

- detalhes do dispositivo;
- partições;
- logs de leitura;
- permissões.

## Tarefas

Painéis recomendados:

- detalhes da tarefa;
- progresso;
- logs;
- erros.

## Diagnósticos

Painéis recomendados:

- detalhes técnicos;
- filtros de logs;
- resultado por categoria;
- saída de processo.

## Plugins

Painéis recomendados:

- detalhes do plugin;
- permissões;
- configurações;
- logs.

---

# PanelViewModel

Exemplo conceitual:

```csharp
public sealed partial class PanelViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private bool _isCollapsed;

    [ObservableProperty]
    private string _titleKey = string.Empty;

    [ObservableProperty]
    private object? _contentViewModel;

    public IRelayCommand CloseCommand { get; }
    public IRelayCommand ToggleCollapseCommand { get; }
}
```

---

# Panel Service

A aplicação pode possuir um serviço para orquestrar painéis globais.

```csharp
public interface IPanelService
{
    Task OpenRightPanelAsync(object viewModel, CancellationToken cancellationToken = default);
    Task OpenBottomPanelAsync(object viewModel, CancellationToken cancellationToken = default);
    Task ClosePanelAsync(string panelId, CancellationToken cancellationToken = default);
    Task TogglePanelAsync(string panelId, CancellationToken cancellationToken = default);
}
```

---

# Integração com Workspace

O Workspace deve enviar contexto ao painel quando necessário.

Exemplos:

- item selecionado no DataGrid;
- tarefa selecionada;
- dispositivo selecionado;
- log selecionado;
- provider selecionado.

---

# Integração com Navigation System

Ao navegar para outra rota:

- painéis contextuais devem ser fechados ou atualizados;
- painéis globais podem permanecer;
- estado persistido deve ser restaurado somente quando fizer sentido.

---

# Integração com Dialogs

Painéis não devem substituir Dialogs quando uma decisão obrigatória for necessária.

Usar Dialog para:

- confirmação destrutiva;
- erro bloqueante;
- entrada obrigatória;
- decisão crítica.

Usar Panel para:

- detalhes;
- filtros;
- propriedades;
- logs;
- ajuda contextual.

---

# Tokens Utilizados

O Panel System deve consumir:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Layout.RightPanelWidth`
- `Radius.Medium`
- `Elevation.Panel`
- `Typography.Title`
- `Typography.Body`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Shell/
        └── Panels/
            ├── PanelHost.axaml
            ├── PanelHostViewModel.cs
            ├── RightPanelView.axaml
            ├── BottomPanelView.axaml
            ├── OverlayPanelView.axaml
            ├── PanelService.cs
            └── PanelState.cs
```

---

# Acessibilidade

Obrigatório:

- foco visível;
- painel anunciado quando aberto;
- botão fechar acessível;
- ESC fecha quando seguro;
- foco retorna ao elemento de origem ao fechar;
- navegação por teclado;
- scroll acessível;
- contraste WCAG AA;
- não depender apenas de animação.

---

# Regras de UX

- Painéis devem complementar o Workspace.
- Não abrir múltiplos painéis desnecessários.
- Painel contextual deve ter título claro.
- Painéis devem poder ser fechados facilmente.
- Em Compact, painéis devem virar overlay.
- Painéis não devem esconder ações críticas.
- Logs extensos devem ter scroll interno.
- Estados Loading e Error devem ser claros.
- Painel não deve substituir Wizard em fluxos sequenciais.

---

# Regras para o Codex

- Nunca colocar regra de negócio no painel.
- Sempre usar ViewModel para estado.
- Sempre usar serviço ou host centralizado para painéis globais.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre respeitar breakpoints.
- Sempre garantir acessibilidade.
- Sempre fechar ou atualizar painel ao trocar contexto.
- Sempre tratar conteúdo não carregado.
- Nunca usar largura hardcoded sem token.

---

# Casos de Erro

- Painel sem título.
- Conteúdo não registrado.
- ViewModel não resolvida.
- Painel não fecha.
- Painel perde foco.
- Painel fica grande demais em Compact.
- Estado persistido inválido.
- Painel contextual mostra item antigo.
- Erro ao carregar detalhes.

---

# Critérios de Aceite

- [ ] Tipos oficiais de painéis definidos.
- [ ] Estados dos painéis documentados.
- [ ] Right Panel definido.
- [ ] Bottom Panel definido.
- [ ] Overlay Panel definido.
- [ ] Panel Service especificado.
- [ ] Integração com Workspace definida.
- [ ] Integração com Navigation definida.
- [ ] Responsividade documentada.
- [ ] Compatível com Light/Dark.
- [ ] Acessibilidade validada.
- [ ] Sem lógica de negócio na View.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Abrir painel.
- Fechar painel.
- Recolher painel.
- Atualizar conteúdo.
- Restaurar estado.
- Fechar ao navegar.
- Tratar erro de conteúdo.

## Integração

- Right Panel na LibraryView.
- Painel de filtros.
- Painel de detalhes de tarefa.
- Bottom Panel de logs.
- Overlay Panel em Compact.
- Alternância Light/Dark.
- Navegação por teclado.
- ESC fecha painel.
- Foco retorna ao elemento de origem.

---

# Dependências

- F038-DockablePanels.md
- F041-DesignSystemFoundation.md
- F045-SpacingAndLayout.md
- F050-CardsAndContainers.md
- F051-NavigationSystem.md
- F058-EmptyErrorAndLoadingStates.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- F066-WorkspaceRegion.md
- F070-AccessibilityNavigation.md
- 022-Localization.md

---

# Definition of Done

- Panel System documentado.
- Tipos e estados definidos.
- Panel Service especificado.
- Responsividade considerada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

# F063 - Sidebar Navigation

> Feature ID: F063
> Status: Ready for Development
> Priority: Critical
> Epic: Application Shell

# Objetivo

Definir o padrão oficial da Sidebar Navigation do WiiStudio, incluindo itens,
grupos, ícones, estado ativo, modo expandido, modo recolhido, responsividade,
contadores, permissões, atalhos, acessibilidade e integração com App Shell e
Navigation System.

A Sidebar é a navegação principal da aplicação e deve permitir acesso rápido às
áreas mais importantes sem sobrecarregar a interface.

---

# Requisitos Funcionais

## RF001
Definir estrutura oficial da Sidebar.

## RF002
Definir grupos e itens de navegação.

## RF003
Suportar modo expandido e recolhido.

## RF004
Suportar item ativo, hover, focus, disabled e loading.

## RF005
Suportar tooltips no modo recolhido.

## RF006
Integrar com Navigation System.

## RF007
Integrar com Keyboard Shortcuts e Command Palette.

## RF008
Garantir compatibilidade com Light e Dark Theme.

## RF009
Garantir acessibilidade por teclado e leitor de tela.

## RF010
Suportar responsividade em modo Compact.

---

# Estrutura Visual

## Expandida

```text
+-------------------------+
| WiiStudio               |
|-------------------------|
| Principal               |
|  🏠 Dashboard           |
|  🎮 Biblioteca          |
|  💾 Dispositivos        |
|-------------------------|
| Operações               |
|  📥 Importação          |
|  🔄 Conversão           |
|  📋 Tarefas             |
|-------------------------|
| Sistema                 |
|  🧩 Plugins             |
|  🛠 Diagnósticos        |
|  ⚙ Configurações        |
+-------------------------+
```

## Recolhida

```text
+------+
| WS   |
|------|
| 🏠   |
| 🎮   |
| 💾   |
|------|
| 📥   |
| 🔄   |
| 📋   |
|------|
| 🧩   |
| 🛠   |
| ⚙   |
+------+
```

---

# Grupos Oficiais

## Principal

- Dashboard
- Biblioteca
- Dispositivos

## Operações

- Importação
- Conversão
- Tarefas

## Gestão

- Metadados & Assets
- Backups
- Plugins

## Sistema

- Diagnósticos
- Configurações
- Ajuda

---

# Itens Oficiais

| Item | Rota | Ícone |
|------|------|-------|
| Dashboard | `/dashboard` | `Icon.Dashboard` |
| Biblioteca | `/library` | `Icon.Library` |
| Dispositivos | `/devices` | `Icon.Device` |
| Importação | `/import` | `Icon.Import` |
| Conversão | `/conversion` | `Icon.Convert` |
| Tarefas | `/tasks` | `Icon.Tasks` |
| Metadados | `/metadata` | `Icon.Metadata` |
| Backups | `/backups` | `Icon.Backup` |
| Plugins | `/plugins` | `Icon.Plugin` |
| Diagnósticos | `/diagnostics` | `Icon.Diagnostics` |
| Configurações | `/settings` | `Icon.Settings` |
| Ajuda | `/help` | `Icon.Help` |

---

# Estados Visuais

Cada item deve suportar:

- Default
- Hover
- Focus
- Active
- Disabled
- Loading
- Warning
- Error

---

# Estado Ativo

O item ativo deve ser claramente identificado por:

- cor de destaque;
- indicador lateral;
- peso visual diferente;
- texto acessível;
- ícone no mesmo padrão.

Não depender apenas de cor.

---

# Modo Expandido

No modo expandido, exibir:

- ícone;
- texto;
- grupo;
- badge opcional;
- tooltip opcional;
- indicador ativo.

Largura recomendada:

```text
Layout.SidebarWidth = 260 px
```

---

# Modo Recolhido

No modo recolhido, exibir:

- ícone;
- tooltip obrigatório;
- indicador ativo;
- badge compacto quando necessário.

Largura recomendada:

```text
Layout.SidebarCollapsedWidth = 64 px
```

---

# Modo Compact

Em janelas pequenas:

- sidebar pode iniciar recolhida;
- sidebar pode abrir como overlay;
- clicar fora pode fechar overlay;
- estado deve ser previsível;
- tooltips devem continuar disponíveis.

---

# Badges e Contadores

A Sidebar pode exibir contadores opcionais.

Exemplos:

- tarefas em execução;
- notificações;
- plugins com atualização;
- erros de diagnóstico;
- dispositivos conectados.

Badges devem seguir `F056-BadgesAndStatusIndicators.md`.

---

# Persistência

Preferências de Sidebar podem ser persistidas:

- expandida/recolhida;
- último grupo aberto;
- item ativo;
- comportamento compacto.

Persistência deve respeitar perfil do usuário quando aplicável.

---

# Modelo Conceitual

```csharp
public sealed class SidebarItem
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string IconKey { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string GroupKey { get; init; } = string.Empty;
    public int Order { get; init; }
    public bool IsEnabled { get; init; } = true;
    public int? BadgeCount { get; init; }
}
```

---

# SidebarViewModel

Responsável por:

- listar grupos e itens;
- controlar item ativo;
- controlar expandido/recolhido;
- executar navegação;
- atualizar badges;
- reagir ao breakpoint;
- aplicar permissões ou disponibilidade.

Exemplo conceitual:

```csharp
public sealed partial class SidebarViewModel : ObservableObject
{
    public ObservableCollection<SidebarGroupViewModel> Groups { get; } = new();

    [ObservableProperty]
    private bool _isCollapsed;

    [ObservableProperty]
    private SidebarItemViewModel? _activeItem;

    public IAsyncRelayCommand<SidebarItemViewModel> NavigateCommand { get; }
    public IRelayCommand ToggleCollapseCommand { get; }
}
```

---

# Integração com Navigation System

Fluxo recomendado:

```text
SidebarItem Click
    ↓
SidebarViewModel.NavigateCommand
    ↓
INavigationService.NavigateToAsync(route)
    ↓
Workspace Region
    ↓
Active Item Updated
```

A Sidebar não deve resolver Views diretamente.

---

# Integração com Command Palette

Itens principais da Sidebar devem ser acessíveis pela Command Palette.

Exemplos:

- Abrir Dashboard.
- Abrir Biblioteca.
- Abrir Dispositivos.
- Abrir Configurações.
- Abrir Diagnósticos.

---

# Integração com Keyboard Shortcuts

Atalhos sugeridos:

| Atalho | Ação |
|-------|------|
| Ctrl+1 | Dashboard |
| Ctrl+2 | Biblioteca |
| Ctrl+3 | Dispositivos |
| Ctrl+4 | Importação |
| Ctrl+5 | Conversão |
| Ctrl+6 | Tarefas |
| Ctrl+, | Configurações |
| Ctrl+B | Expandir/Recolher Sidebar |

---

# Integração com Responsividade

A Sidebar deve responder aos breakpoints:

| Breakpoint | Comportamento |
|-----------|---------------|
| Compact | Recolhida ou overlay |
| Standard | Expandida ou estado do usuário |
| Wide | Expandida |
| UltraWide | Expandida |

---

# Integração com Localization

Todos os textos devem usar chaves localizáveis.

Exemplos:

```text
Navigation.Group.Main
Navigation.Group.Operations
Navigation.Dashboard
Navigation.Library
Navigation.Devices
Navigation.Settings
```

---

# Tokens Utilizados

A Sidebar deve consumir tokens do Design System:

- `Color.Background`
- `Color.Surface`
- `Color.Primary`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Layout.SidebarWidth`
- `Layout.SidebarCollapsedWidth`
- `Radius.Medium`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    ├── Shell/
    │   └── Sidebar/
    │       ├── SidebarView.axaml
    │       ├── SidebarViewModel.cs
    │       ├── SidebarGroupViewModel.cs
    │       ├── SidebarItemViewModel.cs
    │       └── SidebarItemTemplate.axaml
```

---

# Acessibilidade

Obrigatório:

- navegação por teclado;
- foco visível;
- item ativo anunciado;
- tooltip no modo recolhido;
- badge anunciado com texto;
- contraste WCAG AA;
- ordem de tabulação previsível;
- não depender apenas de cor;
- suporte a leitores de tela;
- área clicável adequada.

---

# Regras de UX

- Itens devem ser claros e curtos.
- Grupos devem reduzir complexidade.
- O item ativo deve ser sempre evidente.
- O modo recolhido deve preservar usabilidade.
- Tooltips são obrigatórios no modo recolhido.
- Badges devem ser usados com moderação.
- Não ocultar áreas principais sem motivo.
- Em modo Compact, o overlay deve ser fácil de fechar.
- Sidebar não deve competir visualmente com o Workspace.

---

# Regras para o Codex

- Nunca implementar navegação no code-behind.
- Sempre usar `INavigationService`.
- Sempre usar Commands.
- Sempre usar textos localizáveis.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre adicionar tooltip no modo recolhido.
- Sempre atualizar item ativo via Navigation State.
- Sempre respeitar breakpoints.
- Sempre garantir acessibilidade.
- Nunca usar valores hardcoded de largura, cor ou espaçamento.

---

# Casos de Erro

- Rota inválida.
- Item disabled clicado.
- Item ativo não encontrado.
- Badge com valor inválido.
- Texto não localizado.
- Ícone não registrado.
- Falha ao persistir estado.
- Sidebar overlay não fecha.
- Navegação falha.
- Estado ativo inconsistente após GoBack.

---

# Critérios de Aceite

- [ ] Grupos oficiais definidos.
- [ ] Itens oficiais definidos.
- [ ] Modo expandido definido.
- [ ] Modo recolhido definido.
- [ ] Modo Compact definido.
- [ ] Estado ativo documentado.
- [ ] Badges opcionais documentados.
- [ ] SidebarViewModel especificado.
- [ ] Integração com NavigationService.
- [ ] Integração com Command Palette.
- [ ] Integração com Keyboard Shortcuts.
- [ ] Compatível com Light/Dark.
- [ ] Textos localizáveis.
- [ ] Acessibilidade validada.
- [ ] Sem lógica de negócio na View.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Grupos carregados.
- Itens ordenados.
- Navegação executada.
- Item ativo atualizado.
- Sidebar alterna collapse.
- Tooltip obrigatório no modo recolhido.
- Badge atualizado.
- Rota inválida tratada.
- Estado persistido.

## Integração

- Sidebar na MainWindow.
- Navegação para Dashboard.
- Navegação para Library.
- Navegação para Devices.
- Navegação para Settings.
- Sidebar recolhida.
- Sidebar expandida.
- Sidebar overlay em Compact.
- Alternância Light/Dark.
- Navegação por teclado.
- Badges de tarefas.
- GoBack atualiza item ativo.

---

# Dependências

- F040-CommandPalette.md
- F044-Iconography.md
- F051-NavigationSystem.md
- F056-BadgesAndStatusIndicators.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- F069-KeyboardShortcuts.md
- F070-AccessibilityNavigation.md
- 022-Localization.md

---

# Definition of Done

- Sidebar Navigation documentada.
- Grupos e itens oficiais definidos.
- Estados visuais definidos.
- Modo expandido/recolhido documentado.
- Responsividade documentada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

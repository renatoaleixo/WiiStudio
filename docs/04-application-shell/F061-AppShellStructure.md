# F061 - App Shell Structure

> Feature ID: F061
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Application Shell

# Objetivo

Definir a estrutura oficial do App Shell do WiiStudio, estabelecendo a base visual e funcional da aplicação desktop.

O App Shell é responsável por organizar as regiões principais da interface: Header, Main Menu, Sidebar, Workspace, Right Panel, Status Bar, Notification Host, Dialog Host e Overlay Host.

---

# Requisitos Funcionais

## RF001
Definir a estrutura base da janela principal.

## RF002
Organizar as regiões oficiais da aplicação.

## RF003
Integrar Navigation System, Main Menu, Sidebar, Workspace e Status Bar.

## RF004
Permitir carregamento dinâmico de Views no Workspace.

## RF005
Suportar temas Light e Dark.

## RF006
Suportar responsividade e redimensionamento.

## RF007
Integrar Dialog Host e Notification Host.

## RF008
Garantir acessibilidade por teclado.

## RF009
Garantir separação MVVM.

## RF010
Evitar lógica de negócio no code-behind.

---

# Estrutura Geral

```text
+------------------------------------------------------------------+
| Header / Main Menu                                                |
+------------------------+-----------------------------------------+
| Sidebar Navigation     | Workspace                               |
|                        |                                         |
|                        |                                         |
+------------------------+-----------------------------------------+
| Status Bar                                                       |
+------------------------------------------------------------------+

Camadas globais:
- Dialog Host
- Notification Host
- Overlay Host
```

---

# Regiões Oficiais

## AppWindow

Janela principal da aplicação.

Responsabilidades:

- hospedar o App Shell;
- controlar tamanho mínimo;
- aplicar tema;
- controlar estado da janela;
- restaurar posição e tamanho quando aplicável.

## Header Region

Área superior da aplicação.

Pode conter:

- título da aplicação;
- contexto atual;
- ações globais;
- Main Menu;
- Command Palette trigger;
- status de perfil.

## Main Menu Region

Área do menu principal.

Responsável por comandos globais como:

- Arquivo;
- Editar;
- Exibir;
- Ferramentas;
- Ajuda.

## Sidebar Region

Área de navegação principal.

Responsável por:

- navegação entre módulos;
- agrupamento de áreas;
- estado ativo;
- modo expandido/recolhido.

## Workspace Region

Área central da aplicação.

Responsável por:

- renderizar a View atual;
- carregar conteúdo por rota;
- exibir estados loading/error/empty;
- preservar contexto de navegação.

## Right Panel Region

Área lateral opcional.

Responsável por:

- detalhes;
- filtros;
- propriedades;
- painéis auxiliares;
- informações contextuais.

## Status Bar Region

Área inferior.

Responsável por:

- estado global;
- tarefas em andamento;
- progresso resumido;
- mensagens rápidas;
- indicadores de ambiente.

## Notification Host

Camada global para Toasts, Snackbars e notificações visuais.

## Dialog Host

Camada global para Dialogs e Modals.

## Overlay Host

Camada opcional para loading global, bloqueio temporário de UI e overlays contextuais.

---

# Layout Recomendado em Avalonia

```text
MainWindow
└── ShellView
    ├── Header
    ├── MainMenu
    ├── RootGrid
    │   ├── Sidebar
    │   ├── Workspace
    │   └── RightPanel
    ├── StatusBar
    ├── NotificationHost
    ├── DialogHost
    └── OverlayHost
```

---

# Responsabilidades do Shell

O Shell deve:

- coordenar regiões visuais;
- consumir NavigationService;
- aplicar tema;
- hospedar notificações;
- hospedar dialogs;
- expor estado global;
- controlar comportamento responsivo;
- delegar comandos para ViewModels.

O Shell não deve:

- executar regra de negócio;
- acessar banco diretamente;
- executar processos externos;
- conhecer detalhes internos de features;
- decidir regras de domínio.

---

# ViewModels Recomendadas

```text
ShellViewModel
├── HeaderViewModel
├── MainMenuViewModel
├── SidebarViewModel
├── WorkspaceViewModel
├── RightPanelViewModel
├── StatusBarViewModel
├── NotificationHostViewModel
└── DialogHostViewModel
```

---

# ShellViewModel

Responsável por:

- estado global da aplicação;
- rota atual;
- região ativa;
- estado da sidebar;
- estado do painel lateral;
- comandos globais;
- integração com serviços globais.

Exemplo conceitual:

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    public NavigationViewModel Navigation { get; }
    public StatusBarViewModel StatusBar { get; }
    public NotificationHostViewModel Notifications { get; }

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [ObservableProperty]
    private bool _isRightPanelVisible;
}
```

---

# Serviços Relacionados

O App Shell deve integrar:

- `INavigationService`
- `IDialogService`
- `INotificationService`
- `IThemeService`
- `IResponsiveLayoutService`
- `IWindowStateService`
- `ICommandRegistry`
- `ITaskManager`
- `ILocalizationService`

---

# Fluxo de Navegação

```text
Usuário clica no item da Sidebar
        ↓
NavigationViewModel
        ↓
INavigationService
        ↓
Route Resolver
        ↓
WorkspaceViewModel
        ↓
CurrentViewModel
        ↓
Workspace Region renderiza a View
```

---

# Fluxo de Dialog

```text
ViewModel solicita Dialog
        ↓
IDialogService
        ↓
DialogHost
        ↓
Dialog ViewModel + View
        ↓
Resultado retorna para ViewModel
```

---

# Fluxo de Notificação

```text
Operação gera feedback
        ↓
INotificationService
        ↓
NotificationHostViewModel
        ↓
Toast / Snackbar / Banner
        ↓
Notification Center registra quando aplicável
```

---

# Estados Globais

O App Shell deve suportar:

- Ready
- Loading
- Navigating
- Offline
- Busy
- Error
- Maintenance
- Updating

---

# Inicialização

Fluxo recomendado:

```text
App Startup
    ↓
Load Configuration
    ↓
Load Theme
    ↓
Load Localization
    ↓
Initialize Database
    ↓
Initialize Services
    ↓
Detect External Tools
    ↓
Build Shell
    ↓
Navigate to Dashboard
```

---

# Encerramento

Fluxo recomendado:

```text
Close Requested
    ↓
Check Running Tasks
    ↓
Check Unsaved Changes
    ↓
Confirm if needed
    ↓
Persist Window State
    ↓
Persist User Preferences
    ↓
Shutdown Services
```

---

# Regras de Janela

A janela principal deve possuir:

- tamanho mínimo;
- tamanho inicial recomendado;
- restauração de tamanho anterior;
- suporte a maximizado;
- persistência de posição quando seguro;
- comportamento previsível em múltiplos monitores.

---

# Responsividade

O Shell deve respeitar os breakpoints definidos em `F060-ResponsiveBehaviors.md`.

Comportamentos esperados:

- Sidebar recolhida em modo Compact.
- Right Panel oculto ou em overlay em modo Compact.
- Toolbars com overflow.
- Workspace sem scroll horizontal desnecessário.
- Status Bar adaptada ao espaço disponível.

---

# Tokens Utilizados

O App Shell deve consumir tokens do Design System:

- `Color.Background`
- `Color.Surface`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Layout.SidebarWidth`
- `Layout.SidebarCollapsedWidth`
- `Layout.ToolbarHeight`
- `Layout.StatusBarHeight`
- `Layout.PagePadding`
- `Typography.Body`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    ├── Shell/
    │   ├── ShellView.axaml
    │   ├── ShellView.axaml.cs
    │   ├── ShellViewModel.cs
    │   ├── Regions/
    │   │   ├── HeaderRegion.axaml
    │   │   ├── SidebarRegion.axaml
    │   │   ├── WorkspaceRegion.axaml
    │   │   ├── RightPanelRegion.axaml
    │   │   └── StatusBarRegion.axaml
    │   └── Hosts/
    │       ├── DialogHost.axaml
    │       ├── NotificationHost.axaml
    │       └── OverlayHost.axaml
```

---

# Integração com DI

Todas as ViewModels e serviços usados pelo Shell devem ser registrados no container de DI.

Exemplo conceitual:

```csharp
services.AddSingleton<ShellViewModel>();
services.AddSingleton<INavigationService, NavigationService>();
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<INotificationService, NotificationService>();
```

---

# Acessibilidade

Obrigatório:

- foco inicial previsível;
- navegação por teclado;
- atalhos globais;
- regiões semânticas;
- foco preservado após navegação;
- Dialog Host com foco preso durante dialogs;
- Notification Host sem roubar foco indevidamente;
- suporte a leitores de tela;
- contraste WCAG AA.

---

# Regras para o Codex

- Nunca colocar regra de negócio no Shell.
- Nunca acessar banco diretamente pelo Shell.
- Nunca executar processos externos pelo Shell.
- Sempre usar serviços abstratos.
- Sempre usar ViewModels para estado.
- Sempre usar DI.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre respeitar responsividade.
- Sempre garantir acessibilidade.
- Sempre preservar separação entre regiões.
- Sempre registrar novas Views no sistema de navegação.

---

# Casos de Erro

- View não registrada.
- ViewModel não registrada.
- Falha ao navegar.
- Falha ao carregar tema.
- Falha ao carregar localização.
- Falha ao restaurar estado da janela.
- Tarefa em execução impede fechamento.
- Alterações pendentes impedem navegação.
- Dialog Host indisponível.
- Notification Host indisponível.

---

# Critérios de Aceite

- [x] App Shell definido.
- [x] Regiões oficiais documentadas.
- [x] ShellViewModel especificado.
- [x] NavigationService integrado.
- [x] DialogHost integrado.
- [x] NotificationHost integrado.
- [x] StatusBar integrada.
- [x] Sidebar integrada.
- [x] Workspace com carregamento dinâmico.
- [x] Responsividade prevista.
- [x] Light/Dark suportado.
- [x] Acessibilidade considerada.
- [x] Sem lógica de negócio no Shell.
- [x] Sem valores hardcoded.

---

# Testes

## Unitários

- Estado inicial do Shell.
- Navegação altera CurrentViewModel.
- Sidebar colapsa.
- Right Panel alterna visibilidade.
- Shell bloqueia fechamento com tarefa ativa.
- Shell bloqueia navegação com alteração pendente.
- Tema carregado.
- Localização carregada.

## Integração

- Inicialização da MainWindow.
- Navegação para Dashboard.
- Navegação para Library.
- Exibição de Dialog.
- Exibição de Toast.
- Alteração Light/Dark.
- Redimensionamento da janela.
- Sidebar em modo Compact.
- Status Bar exibindo tarefa.
- Fechamento da aplicação.

---

# Dependências

- F031-MainWindow.md
- F037-StatusBar.md
- F038-DockablePanels.md
- F039-ThemeEngine.md
- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F045-SpacingAndLayout.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- F058-EmptyErrorAndLoadingStates.md
- F059-ToastsAndInlineFeedback.md
- F060-ResponsiveBehaviors.md
- 006-ApplicationLayer.md
- 008-UI-UX.md
- 022-Localization.md

---

# Definition of Done

- Estrutura do App Shell documentada.
- Regiões oficiais definidas.
- Fluxos principais documentados.
- Integrações globais especificadas.
- Acessibilidade considerada.
- Responsividade considerada.
- Testes definidos.
- Documentação atualizada.

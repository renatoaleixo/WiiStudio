# F051 - Navigation System

> Feature ID: F051
> Status: Ready for Development
> Priority: Critical
> Epic: Advanced UI Components

# Objetivo

Definir o sistema oficial de navegação do WiiStudio, incluindo navegação principal,
rotas internas, sidebar, estados ativos, breadcrumbs, comandos globais e integração
com MVVM, Application Shell e Design System.

O Navigation System deve permitir que o usuário navegue de forma clara entre as
principais áreas da aplicação, mantendo contexto, histórico, acessibilidade e
consistência visual.

---

# Requisitos Funcionais

## RF001
Definir a navegação principal da aplicação.

## RF002
Definir grupos de navegação e itens oficiais.

## RF003
Suportar estado ativo, hover, focus, disabled e loading.

## RF004
Suportar navegação por teclado.

## RF005
Integrar navegação com ViewModels e Commands.

## RF006
Permitir navegação entre Views sem lógica no code-behind.

## RF007
Suportar rotas internas e parâmetros.

## RF008
Suportar breadcrumb quando aplicável.

## RF009
Suportar navegação colapsada na Sidebar.

## RF010
Garantir compatibilidade com Light e Dark Theme.

---

# Áreas Principais de Navegação

A aplicação deve possuir as seguintes áreas principais:

- Dashboard
- Biblioteca
- Dispositivos
- Importação
- Conversão
- Tarefas
- Metadados & Assets
- Backups
- Diagnósticos
- Configurações
- Plugins
- Ajuda / Sobre

---

# Estrutura de Navegação

```text
Navigation
├── Dashboard
├── Library
├── Devices
├── Import
├── Conversion
├── Tasks
├── MetadataAssets
├── BackupRestore
├── Diagnostics
├── Plugins
├── Settings
└── Help
```

---

# Modelo Conceitual

```csharp
public sealed class NavigationItem
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string IconKey { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public int Order { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IsEnabled { get; init; } = true;
}
```

---

# Rotas Oficiais

| Rota | Tela |
|-----|------|
| `/dashboard` | Dashboard |
| `/library` | Biblioteca |
| `/library/details/{gameId}` | Detalhes do jogo |
| `/devices` | Gerenciador de dispositivos |
| `/import` | Import Wizard |
| `/conversion` | Conversion Wizard |
| `/tasks` | Task Manager |
| `/metadata` | Metadados & Assets |
| `/backups` | Backup & Restore |
| `/diagnostics` | Diagnósticos |
| `/plugins` | Plugins |
| `/settings` | Configurações |
| `/help` | Ajuda |

---

# Tipos de Navegação

## Primary Navigation

Navegação principal da aplicação, normalmente exibida na Sidebar.

## Secondary Navigation

Navegação complementar dentro de uma área.

Exemplos:

- Abas de configurações.
- Seções de detalhes de jogo.
- Categorias de diagnóstico.

## Contextual Navigation

Navegação baseada no item selecionado.

Exemplos:

- Abrir detalhes do jogo.
- Abrir logs de uma tarefa.
- Abrir dispositivo selecionado.

## Breadcrumb Navigation

Navegação de contexto hierárquico.

Exemplo:

```text
Biblioteca > The Legend of Zelda > Integridade
```

---

# Sidebar Navigation

A Sidebar deve suportar:

- modo expandido;
- modo recolhido;
- ícones;
- texto;
- tooltip no modo recolhido;
- grupos;
- item ativo;
- contadores opcionais;
- separadores;
- navegação por teclado.

---

# Estados Visuais

Cada item de navegação deve suportar:

- Default
- Hover
- Focus
- Active
- Disabled
- Loading
- Error

---

# Agrupamento Recomendado

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

# Serviço de Navegação

A aplicação deve possuir uma abstração de navegação.

## Interface Conceitual

```csharp
public interface INavigationService
{
    Task NavigateToAsync(
        string route,
        object? parameter = null,
        CancellationToken cancellationToken = default);

    Task GoBackAsync(CancellationToken cancellationToken = default);

    Task GoForwardAsync(CancellationToken cancellationToken = default);

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    string CurrentRoute { get; }
}
```

---

# Estado de Navegação

O estado da navegação deve conter:

- rota atual;
- item ativo;
- histórico anterior;
- histórico futuro;
- parâmetros da rota;
- estado de carregamento;
- erro de navegação quando aplicável.

---

# ViewModel de Navegação

Exemplo conceitual:

```csharp
public sealed partial class NavigationViewModel : ObservableObject
{
    public ObservableCollection<NavigationItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private NavigationItemViewModel? _selectedItem;

    public IAsyncRelayCommand<NavigationItemViewModel> NavigateCommand { get; }
}
```

---

# Regras de Roteamento

- Rotas devem ser centralizadas.
- Rotas não devem ser strings soltas espalhadas pelo projeto.
- Parâmetros devem ser tipados sempre que possível.
- Rotas inválidas devem direcionar para tela de erro amigável.
- Navegação deve ser assíncrona quando depender de carregamento.
- Navegação não deve bloquear a thread da UI.

---

# Integração com Application Shell

O Navigation System deve ser integrado ao App Shell.

Fluxo recomendado:

```text
Sidebar Item Click
        ↓
NavigationViewModel
        ↓
INavigationService
        ↓
Route Resolver
        ↓
Workspace Region
        ↓
Target ViewModel + View
```

---

# Integração com Command Palette

A navegação deve expor comandos pesquisáveis para a Command Palette.

Exemplos:

- Abrir Dashboard
- Abrir Biblioteca
- Abrir Configurações
- Abrir Diagnósticos
- Abrir Tarefas
- Abrir Plugins

---

# Integração com Keyboard Shortcuts

Atalhos recomendados:

| Atalho | Ação |
|-------|------|
| Ctrl+1 | Dashboard |
| Ctrl+2 | Biblioteca |
| Ctrl+3 | Dispositivos |
| Ctrl+4 | Importação |
| Ctrl+5 | Conversão |
| Ctrl+, | Configurações |
| Alt+Left | Voltar |
| Alt+Right | Avançar |

---

# Integração com Localization

Todos os títulos de navegação devem usar chaves localizáveis.

Exemplos:

```text
Navigation.Dashboard
Navigation.Library
Navigation.Devices
Navigation.Import
Navigation.Conversion
Navigation.Settings
```

Nunca usar texto fixo diretamente na View.

---

# Integração com Theme Engine

Itens de navegação devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Primary`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Medium`
- `Typography.Body`
- `Icon.Default`

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Foco visível.
- Item ativo anunciado.
- Tooltip quando sidebar estiver recolhida.
- Contraste WCAG AA.
- Não depender apenas de cor para indicar item ativo.
- Ordem de tabulação previsível.
- Leitor de tela deve anunciar título da área.

---

# Regras de UX

- O item ativo deve ser claramente visível.
- O usuário deve sempre entender onde está.
- Grupos devem reduzir complexidade visual.
- A Sidebar recolhida deve preservar ícones e tooltips.
- Navegação não deve causar perda silenciosa de dados.
- Telas com alterações pendentes devem solicitar confirmação antes de sair.
- Rotas inválidas devem exibir estado de erro amigável.

---

# Regras para o Codex

- Nunca implementar navegação diretamente no code-behind.
- Nunca espalhar strings de rota em múltiplos arquivos.
- Sempre usar `INavigationService`.
- Sempre usar Commands para navegação.
- Sempre usar chaves de localização para títulos.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre preservar estado quando aplicável.
- Sempre validar alterações pendentes antes de sair da tela.
- Sempre tratar rota inválida.
- Sempre garantir foco visível.

---

# Casos de Erro

- Rota não encontrada.
- View não registrada.
- ViewModel não registrada no DI.
- Falha ao carregar dados da tela.
- Parâmetro inválido.
- Usuário tenta sair com alterações pendentes.
- Plugin adiciona rota inválida.
- Falha de permissão em rota protegida.

---

# Critérios de Aceite

- [ ] Áreas principais de navegação definidas.
- [ ] Rotas oficiais documentadas.
- [ ] `INavigationService` definido.
- [ ] Sidebar suporta modo expandido.
- [ ] Sidebar suporta modo recolhido.
- [ ] Item ativo destacado.
- [ ] Navegação por teclado funcional.
- [ ] Breadcrumb suportado quando aplicável.
- [ ] Integração com Command Palette prevista.
- [ ] Integração com atalhos prevista.
- [ ] Compatível com Light/Dark.
- [ ] Textos localizáveis.
- [ ] Sem lógica de navegação no code-behind.
- [ ] Rota inválida tratada.

---

# Testes

## Unitários

- Navegar para rota válida.
- Rota inválida retorna erro controlado.
- Item ativo atualizado.
- Histórico de navegação atualizado.
- `CanGoBack` atualizado.
- `CanGoForward` atualizado.
- Navegação bloqueada por alterações pendentes.
- Parâmetro de rota validado.

## Integração

- Navegação pela Sidebar.
- Navegação pela Command Palette.
- Navegação por atalho.
- GoBack.
- GoForward.
- Rota de detalhes do jogo.
- Rota de configurações.
- Rota de diagnóstico.
- Alternância Light/Dark.
- Sidebar recolhida.
- Navegação por teclado.

---

# Dependências

- F031-MainWindow.md
- F036-Dashboard.md
- F039-ThemeEngine.md
- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F050-CardsAndContainers.md
- 006-ApplicationLayer.md
- 008-UI-UX.md
- 022-Localization.md

---

# Definition of Done

- Navigation System definido.
- Rotas oficiais documentadas.
- Serviço de navegação especificado.
- Sidebar documentada.
- Estados visuais documentados.
- Acessibilidade validada.
- Integração com App Shell prevista.
- Testes definidos.
- Documentação atualizada.

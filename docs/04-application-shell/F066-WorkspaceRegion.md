# F066 - Workspace Region

> Feature ID: F066
> Status: Ready for Development
> Priority: Critical
> Epic: Application Shell

# Objetivo

Definir a Workspace Region oficial do WiiStudio, responsável por hospedar e
renderizar as telas principais da aplicação conforme a navegação ativa.

O Workspace é a área central de trabalho onde módulos como Dashboard, Biblioteca,
Dispositivos, Import Wizard, Conversion Wizard, Tarefas, Diagnósticos, Plugins e
Configurações serão exibidos.

---

# Requisitos Funcionais

## RF001
Definir a região central de renderização das Views.

## RF002
Integrar com Navigation System.

## RF003
Suportar troca dinâmica de ViewModels.

## RF004
Suportar estados Loading, Empty, Error e Ready.

## RF005
Suportar transições leves entre telas quando aplicável.

## RF006
Preservar contexto quando apropriado.

## RF007
Suportar responsividade.

## RF008
Garantir acessibilidade por teclado.

## RF009
Evitar lógica de negócio na View.

## RF010
Garantir compatibilidade com Light e Dark Theme.

---

# Estrutura Visual

```text
+--------------------------------------------------------------+
| Workspace                                                    |
|                                                              |
|   View atual renderizada aqui                                |
|                                                              |
|   Dashboard / Library / Devices / Settings / etc.            |
|                                                              |
+--------------------------------------------------------------+
```

---

# Responsabilidades

A Workspace Region deve:

- renderizar a View atual;
- receber ViewModel atual do Navigation Service;
- exibir estado de carregamento;
- exibir erro de navegação;
- preservar foco;
- suportar layout responsivo;
- aplicar padding e tokens oficiais;
- hospedar conteúdo rolável quando necessário.

A Workspace Region não deve:

- resolver regras de domínio;
- acessar banco;
- executar processos externos;
- decidir fluxo de negócio;
- manipular navegação diretamente no code-behind.

---

# Estados da Workspace

## Loading

Exibido durante navegação ou carregamento de View.

## Ready

Exibido quando a View atual está carregada.

## Empty

Exibido quando a rota atual não possui conteúdo.

## Error

Exibido quando a View não pôde ser carregada.

## Unauthorized

Reservado para futuras rotas protegidas ou plugins sem permissão.

## NotFound

Exibido quando a rota não existe ou não foi registrada.

---

# Modelo Conceitual

```csharp
public enum WorkspaceState
{
    Loading,
    Ready,
    Empty,
    Error,
    NotFound,
    Unauthorized
}
```

```csharp
public sealed partial class WorkspaceViewModel : ObservableObject
{
    [ObservableProperty]
    private WorkspaceState _state;

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private string? _errorMessageKey;
}
```

---

# Fluxo de Navegação

```text
NavigationService.NavigateToAsync(route)
        ↓
Route Resolver localiza ViewModel
        ↓
WorkspaceViewModel.CurrentViewModel atualizado
        ↓
WorkspaceView renderiza View correspondente
        ↓
Header e Sidebar atualizam contexto
```

---

# Renderização de Views

A Workspace deve usar mecanismo de resolução de View por ViewModel.

Exemplo conceitual:

```text
DashboardViewModel -> DashboardView
LibraryViewModel -> LibraryView
SettingsViewModel -> SettingsView
```

A associação deve ser centralizada e previsível.

---

# Rotas Esperadas

A Workspace deve suportar renderização das rotas:

- `/dashboard`
- `/library`
- `/library/details/{gameId}`
- `/devices`
- `/import`
- `/conversion`
- `/tasks`
- `/metadata`
- `/backups`
- `/diagnostics`
- `/plugins`
- `/settings`
- `/help`

---

# Layout Interno

Cada View dentro do Workspace deve respeitar:

- padding oficial;
- estado de loading próprio quando aplicável;
- toolbar contextual quando necessário;
- scroll vertical quando necessário;
- ausência de scroll horizontal desnecessário;
- responsividade;
- tokens do Design System.

---

# Padding

A Workspace deve utilizar:

```text
Layout.PagePadding
```

Em modo Compact:

```text
Layout.CompactPagePadding
```

---

# Scroll

Regras:

- permitir scroll vertical quando o conteúdo exceder a altura;
- evitar scroll horizontal;
- DataGrid pode possuir scroll interno;
- Dialogs não devem depender do scroll da Workspace;
- Header e Status Bar não devem rolar junto com Workspace.

---

# Transições

Transições entre telas devem ser:

- leves;
- rápidas;
- não obrigatórias;
- sem prejudicar acessibilidade;
- desativáveis ou reduzidas quando houver preferência de redução de movimento.

Evitar animações longas ou que atrasem navegação.

---

# Preservação de Estado

A Workspace pode preservar estado de algumas telas.

Exemplos:

- filtros da biblioteca;
- última tab de configurações;
- posição de scroll quando útil;
- seleção atual;
- rota anterior.

Não preservar estado quando isso causar inconsistência ou uso excessivo de memória.

---

# Integrações

## Navigation System

A Workspace deve ser controlada por `INavigationService`.

## App Shell

A Workspace deve coexistir com Header, Sidebar, Right Panel, Status Bar, Dialog Host, Notification Host e Overlay Host.

## Empty/Error/Loading States

A Workspace deve usar padrões definidos em `F058-EmptyErrorAndLoadingStates.md`.

## Right Panel

Quando uma tela precisar de painel lateral:

- Workspace deve permitir espaço;
- Right Panel deve receber contexto;
- em Compact, painel deve virar overlay;
- Workspace deve preservar foco.

---

# Tokens Utilizados

A Workspace deve consumir:

- `Color.Background`
- `Color.Surface`
- `Color.Text.Primary`
- `Spacing.MD`
- `Spacing.LG`
- `Spacing.XL`
- `Layout.PagePadding`
- `Layout.CompactPagePadding`
- `Typography.Body`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Shell/
        └── Workspace/
            ├── WorkspaceView.axaml
            ├── WorkspaceViewModel.cs
            ├── ViewLocator.cs
            ├── RouteViewRegistry.cs
            └── WorkspaceStateView.axaml
```

---

# ViewLocator

Responsável por mapear ViewModels para Views.

Exemplo conceitual:

```csharp
public interface IViewLocator
{
    Control Build(object viewModel);
}
```

O ViewLocator deve tratar ViewModel não registrado com estado de erro amigável.

---

# RouteViewRegistry

Responsável por registrar as rotas disponíveis.

Exemplo conceitual:

```csharp
public interface IRouteViewRegistry
{
    bool TryResolve(string route, out Type viewModelType);
}
```

---

# Acessibilidade

Obrigatório:

- foco inicial na nova View;
- preservação de foco quando apropriado;
- estado de loading anunciado;
- erro de navegação anunciado;
- navegação por teclado;
- contraste WCAG AA;
- suporte a redução de movimento;
- não prender foco indevidamente.

---

# Regras de UX

- Workspace não deve ficar em branco.
- Navegação deve ter feedback.
- Erros de rota devem ser amigáveis.
- Telas devem manter estrutura consistente.
- Transições não devem atrapalhar produtividade.
- Estados de erro devem oferecer ação de retorno ou retry.
- Em modo Compact, priorizar conteúdo principal.
- Views complexas devem dividir conteúdo em seções.

---

# Regras para o Codex

- Nunca renderizar Views manualmente no code-behind.
- Sempre usar ViewModel como fonte de estado.
- Sempre usar NavigationService para troca de tela.
- Sempre tratar rota inválida.
- Sempre usar estados Loading/Error quando aplicável.
- Sempre respeitar tokens de layout.
- Sempre garantir Light/Dark.
- Sempre garantir acessibilidade.
- Sempre evitar scroll horizontal desnecessário.
- Sempre registrar nova View no mecanismo central.

---

# Casos de Erro

- Rota não encontrada.
- ViewModel não registrada.
- View não registrada.
- Falha ao construir View.
- Erro durante carregamento inicial.
- Navegação cancelada.
- Estado preservado inválido.
- Foco perdido.
- Conteúdo maior que a área disponível.

---

# Critérios de Aceite

- [ ] Workspace Region definida.
- [ ] WorkspaceViewModel especificado.
- [ ] ViewLocator previsto.
- [ ] RouteViewRegistry previsto.
- [ ] Estados Loading, Ready, Error e NotFound definidos.
- [ ] Integração com NavigationService.
- [ ] Integração com App Shell.
- [ ] Integração com Right Panel prevista.
- [ ] Responsividade documentada.
- [ ] Acessibilidade validada.
- [ ] Sem lógica de negócio na View.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Rota válida resolve ViewModel.
- Rota inválida retorna NotFound.
- ViewModel não registrado retorna Error.
- Navegação altera CurrentViewModel.
- Loading exibido durante navegação.
- Estado preservado quando aplicável.
- Erro anunciado.

## Integração

- Dashboard renderizado.
- Library renderizada.
- Game Details renderizado.
- Settings renderizado.
- Rota inválida.
- Alternância Light/Dark.
- Redimensionamento Compact.
- Right Panel com contexto.
- Navegação por teclado.
- Foco inicial após navegação.

---

# Dependências

- F031-MainWindow.md
- F041-DesignSystemFoundation.md
- F045-SpacingAndLayout.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F058-EmptyErrorAndLoadingStates.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- F067-PanelSystem.md
- 006-ApplicationLayer.md
- 008-UI-UX.md
- 022-Localization.md

---

# Definition of Done

- Workspace Region documentada.
- Estados definidos.
- ViewLocator especificado.
- RouteViewRegistry especificado.
- Integração com NavigationService documentada.
- Responsividade considerada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

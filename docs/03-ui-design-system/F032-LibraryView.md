# F032 - Library View

> Feature ID: F032 Status: Ready for Development Priority: Critical
> Epic: User Interface

# Objetivo

Definir a tela principal da biblioteca de jogos do WiiStudio, oferecendo
uma experiência moderna para navegação, pesquisa, seleção e
gerenciamento de coleções de jogos.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exibir todos os jogos da biblioteca.

## RF002

Alternar entre modos de visualização.

## RF003

Selecionar um ou múltiplos jogos.

## RF004

Integrar pesquisa e filtros.

## RF005

Exibir status de integridade e favoritos.

## RF006

Abrir menu de ações rápidas.

------------------------------------------------------------------------

# Modos de Visualização

-   Grade (Grid)
-   Lista (List)
-   Compacto
-   Detalhado

------------------------------------------------------------------------

# Layout

``` text
+-----------------------------------------------------------+
| Toolbar | Search | Filters | Sort                         |
+-----------------------------------------------------------+
| Sidebar |                                               |
|         |             Library Grid/List                 |
|         |                                               |
+-----------------------------------------------------------+
| Status Bar | Selected | Tasks | Notifications           |
+-----------------------------------------------------------+
```

------------------------------------------------------------------------

# Componentes

-   Search Box
-   Filter Panel
-   Sort Selector
-   Game Grid
-   Game List
-   Selection Manager
-   Context Menu
-   Pagination / Virtualization

------------------------------------------------------------------------

# Fluxo Técnico

``` text
LibraryView
    │
LibraryViewModel
    │
IGameLibraryService
    │
IGameSearchService
    │
LibraryRepository
```

------------------------------------------------------------------------

# Máquina de Estados

-   Loading
-   Ready
-   Filtering
-   Searching
-   Selecting
-   Refreshing
-   Error

------------------------------------------------------------------------

# Contrato

``` csharp
public interface ILibraryViewService
{
    Task RefreshAsync(CancellationToken cancellationToken);
    Task ChangeViewModeAsync(ViewMode mode, CancellationToken cancellationToken);
    Task SelectGamesAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Virtualização obrigatória para grandes bibliotecas.
-   Seleção múltipla via teclado e mouse.
-   Atualização automática após alterações na biblioteca.
-   Persistir modo de visualização escolhido.

------------------------------------------------------------------------

# Eventos

-   LibraryLoaded
-   LibraryRefreshed
-   SelectionChanged
-   ViewModeChanged
-   FiltersApplied

------------------------------------------------------------------------

# Casos de Erro

-   Biblioteca indisponível.
-   Falha ao carregar miniaturas.
-   Banco inacessível.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Suporte a mais de 10.000 jogos.
-   [ ] Virtualização funcional.
-   [ ] Pesquisa integrada.
-   [ ] Filtros em tempo real.
-   [ ] Seleção múltipla.

------------------------------------------------------------------------

# Testes

## Unitários

-   Mudança de modo.
-   Seleção.
-   Atualização.
-   Filtros.

## Integração

-   Biblioteca grande.
-   Atualização após importação.
-   Alteração de favoritos.
-   Alteração de integridade.

------------------------------------------------------------------------

# Dependências

-   F002-ScanGameLibrary.md
-   F013-GameDetails.md
-   F015-SearchAndFilter.md
-   F031-MainWindow.md

------------------------------------------------------------------------

# Definition of Done

-   Tela implementada.
-   Virtualização validada.
-   Testes aprovados.
-   Documentação atualizada.

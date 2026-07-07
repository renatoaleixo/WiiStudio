# F038 - Dockable Panels

> Feature ID: F038 Status: Ready for Development Priority: Critical
> Epic: User Interface

# Objetivo

Permitir que todos os painéis do WiiStudio possam ser acoplados,
desacoplados, movidos, ocultados e organizados livremente pelo usuário,
oferecendo uma experiência semelhante a IDEs profissionais como Visual
Studio, Rider e Qt Creator.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Acoplar painéis em qualquer região da janela.

## RF002

Desacoplar painéis em janelas independentes.

## RF003

Permitir arrastar e reorganizar painéis.

## RF004

Salvar layouts personalizados.

## RF005

Restaurar layouts previamente salvos.

## RF006

Alternar rapidamente entre layouts.

------------------------------------------------------------------------

# Painéis Compatíveis

-   Biblioteca
-   Dashboard
-   Detalhes do Jogo
-   Device Manager
-   Task Manager
-   Notification Center
-   Logging Center
-   Diagnostics
-   Properties
-   Preview
-   Search
-   Favorites

------------------------------------------------------------------------

# Layout

``` text
+---------------------------------------------------------------+
| Toolbar                                                       |
+-----------+---------------------------+------------------------+
| Explorer  |                           | Properties             |
|           |      Workspace            |------------------------|
|           |                           | Details                |
+-----------+---------------------------+------------------------+
| Logs                     | Tasks                    | Status   |
+---------------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
MainWindow
     │
DockLayoutManager
     │
IDockService
     │
DockLayoutRepository
     │
profiles/layouts/
```

------------------------------------------------------------------------

# Máquina de Estados

-   LoadingLayout
-   Docking
-   Floating
-   SavingLayout
-   RestoringLayout
-   Ready

------------------------------------------------------------------------

# Estrutura

``` text
profiles/
└── layouts/
    ├── Default.layout.json
    ├── Development.layout.json
    └── Gaming.layout.json
```

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IDockService
{
    Task SaveLayoutAsync(
        string layoutName,
        CancellationToken cancellationToken);

    Task LoadLayoutAsync(
        string layoutName,
        CancellationToken cancellationToken);

    Task ResetLayoutAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetLayoutsAsync(
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Cada perfil pode possuir múltiplos layouts.
-   Restaurar automaticamente o último layout utilizado.
-   Layout padrão não pode ser removido.
-   Compatível com múltiplos monitores.

------------------------------------------------------------------------

# Eventos

-   PanelDocked
-   PanelUndocked
-   LayoutSaved
-   LayoutLoaded
-   LayoutReset

------------------------------------------------------------------------

# Interface

Permitir:

-   Arrastar painéis.
-   Acoplar.
-   Desacoplar.
-   Auto Hide.
-   Fechar painel.
-   Restaurar layout.

------------------------------------------------------------------------

# Casos de Erro

-   Layout incompatível.
-   Arquivo corrompido.
-   Painel indisponível.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Drag & Drop funcional.
-   [ ] Layout persistente.
-   [ ] Múltiplos layouts.
-   [ ] Multi-monitor.
-   [ ] Auto Hide.

------------------------------------------------------------------------

# Testes

## Unitários

-   Dock.
-   Undock.
-   Salvar layout.
-   Restaurar layout.

## Integração

-   Alteração entre layouts.
-   Reinício da aplicação.
-   Múltiplos monitores.
-   Painéis ocultos.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F024-PreferencesAndProfiles.md
-   F031-MainWindow.md
-   F037-StatusBar.md

------------------------------------------------------------------------

# Definition of Done

-   Sistema de docking implementado.
-   Persistência validada.
-   Testes aprovados.
-   Documentação atualizada.

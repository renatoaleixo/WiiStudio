# F031 - Main Window

> Feature ID: F031 Status: Implemented and Homologated Priority: Critical
> Epic: User Interface

# Objetivo

Definir a janela principal do WiiStudio como ponto central de navegação,
integrando biblioteca, dispositivos, tarefas, notificações e
configurações em uma interface moderna, responsiva e extensível.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exibir janela principal após inicialização.

## RF002

Disponibilizar navegação lateral entre módulos.

## RF003

Exibir barra superior com ações globais.

## RF004

Permitir múltiplos painéis redimensionáveis.

## RF005

Persistir layout personalizado.

## RF006

Atualizar informações em tempo real.

------------------------------------------------------------------------

# Layout

``` text
+-------------------------------------------------------------+
| Menu | Toolbar | Pesquisa | Perfil                          |
+-------+---------------------------------------------+-------+
|       |                                             |       |
| Nav   |               Área Principal                | Side  |
|       |                                             | Panel |
|       |                                             |       |
+-------+---------------------------------------------+-------+
| Status Bar | Tarefas | Notificações | Dispositivo           |
+-------------------------------------------------------------+
```

------------------------------------------------------------------------

# Componentes

-   Navigation Drawer
-   Toolbar
-   Search Bar
-   Workspace
-   Side Panel
-   Status Bar
-   Notification Area
-   Task Indicator

------------------------------------------------------------------------

# Fluxo Técnico

``` text
App
 │
MainWindow
 │
ShellViewModel
 │
NavigationService
 │
Feature Modules
```

------------------------------------------------------------------------

# Máquina de Estados

-   Initializing
-   Loading
-   Ready
-   Busy
-   Error

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IMainWindowService
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task RestoreLayoutAsync(CancellationToken cancellationToken);
    Task SaveLayoutAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   MainWindowLoaded
-   LayoutChanged
-   ModuleOpened
-   ModuleClosed
-   ThemeChanged

------------------------------------------------------------------------

# Regras de Negócio

-   Restaurar layout automaticamente.
-   Adaptar-se a diferentes resoluções.
-   Permitir múltiplos monitores.
-   Suportar temas claro/escuro.

------------------------------------------------------------------------

# Casos de Erro

-   Layout inválido.
-   Módulo indisponível.
-   Falha ao restaurar preferências.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Inicialização em menos de 3 segundos.
-   [x] Layout persistente.
-   [x] Navegação fluida.
-   [x] Componentes responsivos.
-   [x] Atualização em tempo real.

------------------------------------------------------------------------

# Testes

## Unitários

-   Inicialização.
-   Persistência do layout.
-   Navegação.

## Integração

-   Abrir módulos.
-   Alterar tema.
-   Restaurar layout.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F024-PreferencesAndProfiles.md
-   F025-NotificationCenter.md
-   F026-TaskManager.md

------------------------------------------------------------------------

# Definition of Done

-   Shell implementado.
-   Layout persistente.
-   Testes aprovados.
-   Documentação atualizada.

# F039 - Theme Engine

> Feature ID: F039 Status: Implemented and Homologated Priority: High Epic:
> User Interface

# Objetivo

Implementar um mecanismo de temas moderno e extensível para o WiiStudio,
permitindo personalização completa da aparência da aplicação, suporte a
temas claro/escuro, temas personalizados e alternância dinâmica sem
reinicialização.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Suportar temas Light e Dark nativos.

## RF002

Permitir criação de temas personalizados.

## RF003

Trocar temas em tempo real.

## RF004

Persistir tema por perfil.

## RF005

Importar e exportar temas.

## RF006

Suportar temas desenvolvidos por terceiros.

------------------------------------------------------------------------

# Elementos Personalizáveis

-   Cores
-   Tipografia
-   Ícones
-   Bordas
-   Espaçamentos
-   DataGrid
-   Cards
-   Dialogs
-   Wizards
-   Dock Panels

------------------------------------------------------------------------

# Estrutura

``` text
themes/
├── Light/
│   └── theme.json
├── Dark/
│   └── theme.json
└── Custom/
    └── MyTheme/
        └── theme.json
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ThemeView
    │
ThemeViewModel
    │
IThemeService
    │
ThemeRepository
    │
ThemeLoader
```

------------------------------------------------------------------------

# Máquina de Estados

-   Loading
-   Applying
-   Ready
-   Failed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IThemeService
{
    Task<IReadOnlyList<ThemeInfo>> GetThemesAsync(CancellationToken cancellationToken);
    Task ApplyThemeAsync(string themeId, CancellationToken cancellationToken);
    Task ImportThemeAsync(string filePath, CancellationToken cancellationToken);
    Task ExportThemeAsync(string themeId,string filePath,CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Aplicação sem reiniciar.
-   Tema padrão protegido.
-   Compatibilidade entre versões.
-   Validação antes da importação.

------------------------------------------------------------------------

# Eventos

-   ThemeLoaded
-   ThemeApplied
-   ThemeImported
-   ThemeExported

------------------------------------------------------------------------

# Interface

-   Galeria de temas
-   Pré-visualização
-   Tema ativo
-   Importar
-   Exportar
-   Restaurar padrão

------------------------------------------------------------------------

# Casos de Erro

-   Tema inválido.
-   Arquivo corrompido.
-   Recursos ausentes.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Alternância instantânea.
-   [x] Persistência por perfil.
-   [x] Importação/exportação.
-   [x] Temas personalizados.

------------------------------------------------------------------------

# Testes

## Unitários

-   Carregamento.
-   Aplicação.
-   Persistência.
-   Importação.

## Integração

-   Mudança entre Light/Dark.
-   Tema personalizado.
-   Reinício da aplicação.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F024-PreferencesAndProfiles.md
-   F031-MainWindow.md
-   F038-DockablePanels.md

------------------------------------------------------------------------

# Definition of Done

-   Engine implementada.
-   Temas funcionais.
-   Testes aprovados.
-   Documentação atualizada.

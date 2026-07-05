# 021 - Theme System Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir o sistema oficial de temas do WiiStudio, permitindo
personalização da interface sem alterar a lógica da aplicação.

------------------------------------------------------------------------

# Objetivos

-   Suporte nativo a Dark e Light Mode.
-   Permitir temas personalizados.
-   Troca de tema em tempo real.
-   Separação entre identidade visual e código.

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
    │
IThemeService
    │
ThemeManager
    │
├── DefaultDark
├── DefaultLight
└── Custom Themes
```

------------------------------------------------------------------------

# Estrutura

``` text
assets/
└── themes/
    ├── Dark/
    ├── Light/
    └── Custom/
```

Cada tema pode conter:

-   Colors.axaml
-   Icons/
-   Fonts/
-   Images/

------------------------------------------------------------------------

# Interface

``` csharp
public interface IThemeService
{
    Task<IReadOnlyList<ThemeInfo>> GetThemesAsync();
    Task ApplyThemeAsync(string themeId);
    ThemeInfo GetCurrentTheme();
}
```

------------------------------------------------------------------------

# Componentes Tematizáveis

-   Janela principal
-   Barra lateral
-   Toolbar
-   Botões
-   Cards
-   Tabelas
-   Dialogs
-   Toasts
-   Ícones

------------------------------------------------------------------------

# Regras

-   Nunca utilizar cores hardcoded.
-   Utilizar recursos (`DynamicResource`) para todas as cores.
-   Ícones devem se adaptar ao tema.

------------------------------------------------------------------------

# Persistência

O tema selecionado será salvo em `Settings`.

Na inicialização:

1.  Carregar configuração.
2.  Validar existência.
3.  Aplicar tema.
4.  Caso inválido, utilizar DefaultDark.

------------------------------------------------------------------------

# Acessibilidade

Suporte para:

-   Alto contraste.
-   Escala de fontes.
-   Indicadores de foco.
-   Contraste mínimo WCAG AA.

------------------------------------------------------------------------

# Performance

-   Troca de tema em menos de 200 ms.
-   Sem reiniciar a aplicação.
-   Sem recriar ViewModels.

------------------------------------------------------------------------

# Logging

Registrar:

-   tema aplicado
-   falhas
-   tempo de aplicação

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca usar cores literais na UI.
-   Todo componente deve consumir recursos de tema.
-   Novos controles devem funcionar em todos os temas.

------------------------------------------------------------------------

# Critérios de Aceite

-   Dark e Light funcionais.
-   Temas personalizados carregados dinamicamente.
-   Persistência entre execuções.
-   Compatibilidade com acessibilidade.

------------------------------------------------------------------------

# Checklist

-   [ ] ThemeManager definido
-   [ ] IThemeService especificado
-   [ ] Persistência documentada
-   [ ] Recursos centralizados
-   [ ] Acessibilidade contemplada

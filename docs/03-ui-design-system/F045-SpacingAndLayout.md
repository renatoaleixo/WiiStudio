# F045 - Spacing & Layout

> Feature ID: F045
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o sistema oficial de espaçamentos, alinhamentos, grids, margens, paddings,
larguras, áreas de conteúdo e regras de composição visual do WiiStudio.

Este documento deve orientar o Codex na criação de telas, componentes, dialogs,
wizards, dashboards, painéis e layouts responsivos, evitando inconsistências
visuais e uso de valores hardcoded.

---

# Requisitos Funcionais

## RF001
Definir escala oficial de espaçamento baseada em múltiplos de 4 px.

## RF002
Padronizar margens e paddings de telas, cards, dialogs, forms e containers.

## RF003
Definir regras oficiais para Grid System e composição de layout.

## RF004
Garantir consistência entre telas do WiiStudio.

## RF005
Garantir compatibilidade com Light e Dark Theme.

## RF006
Garantir responsividade para diferentes tamanhos de janela.

## RF007
Eliminar margens, paddings e larguras hardcoded nas Views.

## RF008
Disponibilizar tokens reutilizáveis para espaçamento e layout.

---

# Escala Oficial de Espaçamentos

Todos os espaçamentos devem seguir múltiplos de 4 px.

| Token | Valor | Uso recomendado |
|------|------:|-----------------|
| Spacing.XXS | 2 px | Ajustes muito finos e separações internas raras |
| Spacing.XS | 4 px | Espaço mínimo entre elementos próximos |
| Spacing.SM | 8 px | Espaçamento compacto |
| Spacing.MD | 12 px | Espaçamento intermediário |
| Spacing.LG | 16 px | Espaçamento padrão entre componentes |
| Spacing.XL | 24 px | Espaçamento entre grupos e seções pequenas |
| Spacing.XXL | 32 px | Espaçamento entre seções |
| Spacing.XXXL | 48 px | Espaçamento entre grandes blocos |
| Spacing.Huge | 64 px | Espaçamento de destaque ou composição ampla |

---

# Tokens de Layout

Além dos tokens de espaçamento, o WiiStudio deve possuir tokens específicos de layout.

| Token | Valor sugerido | Uso |
|------|---------------:|-----|
| Layout.PagePadding | 24 px | Padding padrão de telas |
| Layout.PanelPadding | 16 px | Padding padrão de painéis |
| Layout.DialogPadding | 24 px | Padding interno de dialogs |
| Layout.CardPadding | 16 px | Padding interno de cards |
| Layout.FormGap | 16 px | Espaço entre campos de formulário |
| Layout.SectionGap | 32 px | Espaço entre seções |
| Layout.ToolbarHeight | 48 px | Altura padrão de toolbar |
| Layout.StatusBarHeight | 28 px | Altura padrão da status bar |
| Layout.SidebarWidth | 260 px | Largura da sidebar expandida |
| Layout.SidebarCollapsedWidth | 64 px | Largura da sidebar recolhida |

---

# Estrutura Recomendada

```text
styles/
└── tokens/
    ├── Spacing.axaml
    ├── Layout.axaml
    ├── Grid.axaml
    ├── Breakpoints.axaml
    └── Responsive.axaml
```

---

# Grid System

O WiiStudio deve utilizar um Grid System flexível para composição das telas.

## Regras Gerais

- Utilizar layout fluido.
- Evitar posicionamento absoluto.
- Priorizar `Grid`, `StackPanel`, `DockPanel` e containers responsivos do Avalonia.
- Manter alinhamento visual entre áreas da aplicação.
- Separar áreas principais em regiões claras:
  - Header
  - Sidebar
  - Toolbar
  - Workspace
  - Side Panel
  - Footer / Status Bar

---

# Layout Base da Aplicação

```text
+-----------------------------------------------------------+
| Header / Main Menu                                        |
+----------------------+------------------------------------+
| Sidebar              | Workspace                          |
|                      |                                    |
|                      |                                    |
|                      |                                    |
+----------------------+------------------------------------+
| Status Bar                                                |
+-----------------------------------------------------------+
```

---

# Regiões Oficiais

## Header

Área superior para título, contexto atual, comandos globais e menu principal.

## Sidebar

Área lateral para navegação principal.

## Workspace

Área central onde as Views principais serão carregadas.

## Right Panel

Área lateral opcional para detalhes, filtros ou propriedades.

## Footer / Status Bar

Área inferior para progresso global, mensagens e estado da aplicação.

---

# Padding Padrão

| Elemento | Padding recomendado |
|---------|--------------------:|
| Tela principal | 24 px |
| Painel lateral | 16 px |
| Card | 16 px |
| Dialog | 24 px |
| Formulário | 16 px |
| Toolbar | 8 px horizontal |
| DataGrid Toolbar | 12 px |
| Dashboard | 24 px |
| Wizard Step | 24 px |

---

# Margens Padrão

| Relação | Margem recomendada |
|--------|-------------------:|
| Elementos próximos | 8 px |
| Componentes irmãos | 16 px |
| Grupos relacionados | 24 px |
| Seções distintas | 32 px |
| Grandes blocos | 48 px |

---

# Larguras Oficiais

| Elemento | Valor recomendado |
|---------|------------------:|
| Dialog Small | 360 px |
| Dialog Medium | 520 px |
| Dialog Large | 720 px |
| Dialog ExtraLarge | 960 px |
| Sidebar expandida | 260 px |
| Sidebar recolhida | 64 px |
| Right Panel | 320 px |
| Conteúdo máximo de formulário | 720 px |
| Conteúdo máximo de leitura | 920 px |

---

# Breakpoints

| Breakpoint | Largura | Comportamento |
|-----------|--------:|---------------|
| Small | até 800 px | Layout compacto |
| Medium | 801-1200 px | Layout padrão |
| Large | 1201-1600 px | Layout amplo |
| UltraWide | acima de 1600 px | Uso de painéis adicionais |

---

# Responsividade

A aplicação deve adaptar:

- Sidebar expandida ou recolhida.
- Painéis laterais visíveis ou ocultos.
- Grids com colunas reduzidas.
- Cards em uma ou múltiplas colunas.
- Toolbar com overflow.
- Dialogs limitados ao tamanho da janela.

---

# Regras para Telas

Toda tela deve possuir:

- padding externo consistente;
- título ou contexto visual claro;
- espaçamento entre seções;
- área principal bem definida;
- estado loading;
- estado empty quando aplicável;
- estado error quando aplicável.

---

# Regras para Forms

- Campos devem possuir gap vertical consistente.
- Labels devem manter alinhamento.
- Mensagens de erro devem ficar próximas ao campo.
- Formulários longos devem ser divididos em seções.
- Ações principais devem ficar ao final do formulário.

---

# Regras para Cards

- Cards de mesmo tipo devem ter padding e estrutura iguais.
- Cards em grid devem usar gap consistente.
- Cards não devem usar espaçamentos internos arbitrários.
- Cards interativos devem preservar área clicável adequada.

---

# Regras para Dialogs

- Dialogs devem possuir padding interno fixo.
- Botões devem ficar em área de ações.
- Conteúdo deve respeitar largura máxima.
- Dialogs grandes devem permitir scroll interno quando necessário.

---

# Regras para DataGrid

- Toolbar e grid devem possuir alinhamento comum.
- Filtros devem manter espaçamento consistente.
- Linhas devem possuir altura mínima confortável.
- Estados vazio, loading e erro devem ocupar área clara.

---

# Tokens Utilizados

Exemplos de tokens esperados:

```text
Spacing.XS
Spacing.SM
Spacing.MD
Spacing.LG
Spacing.XL
Spacing.XXL
Spacing.XXXL
Layout.PagePadding
Layout.CardPadding
Layout.DialogPadding
Layout.SectionGap
Layout.ToolbarHeight
Layout.SidebarWidth
```

---

# Regras para o Codex

- Nunca usar `Margin` hardcoded.
- Nunca usar `Padding` hardcoded.
- Nunca usar largura fixa sem justificativa.
- Nunca usar posicionamento absoluto sem necessidade.
- Sempre consumir tokens de espaçamento e layout.
- Sempre respeitar o Grid System.
- Sempre considerar responsividade.
- Sempre validar telas em Light e Dark Theme.
- Sempre manter alinhamento visual entre componentes.
- Sempre evitar duplicação de estilos.

---

# Casos de Erro

- Conteúdo maior que a área disponível.
- Tela muito estreita.
- Dialog maior que a janela.
- Toolbar com muitos comandos.
- Painel lateral sem espaço suficiente.
- Formulário com campos quebrados.
- DataGrid com muitas colunas.

---

# Critérios de Aceite

- [x] Escala de espaçamento definida.
- [x] Tokens de layout definidos.
- [x] Grid System documentado.
- [x] Breakpoints definidos.
- [x] Responsividade documentada.
- [x] Padding e margin oficiais definidos.
- [x] Regras para telas, forms, dialogs, cards e grids definidas.
- [x] Sem valores hardcoded em novas Views.
- [x] Compatível com Light/Dark.
- [x] Documentação suficiente para o Codex implementar.

---

# Testes

## Unitários

- Carregamento de tokens.
- Resolução de recursos de espaçamento.
- Validação dos tokens obrigatórios.

## Integração

- MainWindow.
- Dashboard.
- LibraryView.
- ImportWizard.
- ConversionWizard.
- Dialogs.
- DataGrid.
- Settings.
- Alternância Light/Dark.
- Redimensionamento da janela.

---

# Dependências

- F031-MainWindow.md
- F036-Dashboard.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- 008-UI-UX.md
- 021-ThemeSystem.md

---

# Definition of Done

- Tokens de espaçamento criados.
- Tokens de layout criados.
- Grid System definido.
- Breakpoints definidos.
- Regras de composição documentadas.
- Testes aprovados.
- Documentação atualizada.

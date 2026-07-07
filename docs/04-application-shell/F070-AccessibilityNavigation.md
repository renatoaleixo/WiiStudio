# F070 - Accessibility Navigation

> Feature ID: F070
> Status: Ready for Development
> Priority: Critical
> Epic: Application Shell

# Objetivo

Definir o padrão oficial de navegação acessível do WiiStudio, garantindo que toda a aplicação possa ser utilizada por teclado, leitores de tela, tecnologias assistivas e usuários com diferentes necessidades de acessibilidade.

A navegação acessível deve ser tratada como requisito estrutural do App Shell, Design System e de todos os módulos funcionais.

---

# Requisitos Funcionais

## RF001
Garantir navegação completa por teclado.

## RF002
Garantir foco visível e previsível.

## RF003
Definir ordem de tabulação consistente.

## RF004
Garantir suporte a leitores de tela.

## RF005
Garantir acessibilidade em Dialogs, Menus, Toolbars, DataGrid, Sidebar e Forms.

## RF006
Evitar dependência exclusiva de cor, hover ou mouse.

## RF007
Garantir atalhos documentados.

## RF008
Garantir textos acessíveis em ícones, botões e estados.

## RF009
Garantir compatibilidade com Light e Dark Theme.

## RF010
Garantir que fluxos críticos sejam acessíveis sem mouse.

---

# Princípios

A navegação acessível deve seguir estes princípios:

- toda ação importante deve ser acessível por teclado;
- o usuário deve saber onde está o foco;
- a ordem de navegação deve ser previsível;
- mudanças de contexto devem ser anunciáveis;
- ícones devem possuir descrição acessível;
- mensagens de erro devem ser associadas ao contexto;
- estados visuais não devem depender apenas de cor;
- dialogs devem prender o foco enquanto abertos;
- menus devem seguir comportamento esperado de desktop.

---

# Áreas Cobertas

A navegação acessível deve abranger:

- App Shell;
- Header;
- Main Menu;
- Sidebar;
- Workspace;
- Panels;
- Status Bar;
- Dialogs;
- DataGrid;
- Forms;
- Toolbars;
- Menus;
- Command Palette;
- Notification Center;
- Wizards;
- Cards interativos.

---

# Navegação por Teclado

## Teclas Globais

| Tecla | Comportamento |
|------|---------------|
| Tab | Próximo elemento focável |
| Shift+Tab | Elemento focável anterior |
| Enter | Executar ação principal quando seguro |
| Space | Acionar item focado |
| Esc | Fechar dialog/panel/menu quando seguro |
| Setas | Navegar em menus, tabs, grids e listas |
| Home | Ir ao primeiro item quando aplicável |
| End | Ir ao último item quando aplicável |
| PageUp/PageDown | Navegar páginas quando aplicável |

---

# Ordem de Tabulação

A ordem de tabulação deve seguir fluxo lógico:

```text
Header
  ↓
Main Menu / Global Actions
  ↓
Sidebar
  ↓
Workspace
  ↓
Right Panel
  ↓
Status Bar
```

Dialogs e overlays devem possuir ordem própria enquanto ativos.

---

# Foco Visível

Todo elemento interativo deve possuir foco visível.

Elementos obrigatórios:

- botões;
- icon buttons;
- links;
- inputs;
- menu items;
- tabs;
- sidebar items;
- cards interativos;
- data grid rows;
- toolbar actions;
- chips removíveis;
- dialogs;
- panels.

---

# Gerenciamento de Foco

## Ao abrir Dialog

- foco deve ir para o título ou primeira ação segura;
- foco deve ficar preso dentro do dialog;
- ESC deve fechar somente quando seguro;
- ao fechar, foco retorna ao elemento de origem.

## Ao abrir Panel

- foco pode ir para o título ou primeira ação;
- ESC fecha quando seguro;
- foco retorna ao elemento de origem.

## Ao navegar entre telas

- foco deve ir para o título da nova tela ou primeiro elemento principal;
- o leitor de tela deve conseguir identificar a nova área.

## Ao exibir erro

- foco deve ir para a área de erro quando o erro bloquear o fluxo;
- erros de campo devem estar associados aos inputs.

---

# Leitores de Tela

Elementos devem possuir:

- nome acessível;
- função adequada;
- estado quando aplicável;
- descrição quando necessário.

Exemplos:

```text
Botão: Importar jogo
Estado: desabilitado
Descrição: conecte um dispositivo USB para importar diretamente.
```

---

# Componentes Obrigatórios

## Sidebar

- item ativo anunciado;
- tooltip no modo recolhido;
- foco visível;
- navegação por setas quando aplicável.

## Main Menu

- suporte a Alt;
- navegação por setas;
- atalhos anunciados;
- ESC fecha menu.

## Command Palette

- foco inicial no campo de pesquisa;
- resultados navegáveis por teclado;
- Enter executa comando;
- ESC fecha.

## Dialogs

- foco preso;
- título anunciado;
- botões acessíveis;
- mensagens de erro anunciáveis.

## DataGrid

- cabeçalhos acessíveis;
- linhas navegáveis;
- seleção anunciada;
- ações por teclado;
- status textual.

## Forms

- labels associados;
- erros associados aos campos;
- help text acessível;
- ordem de tabulação lógica.

---

# Não Dependência de Cor

Nunca usar apenas cor para indicar:

- erro;
- sucesso;
- warning;
- item ativo;
- seleção;
- status;
- progresso;
- obrigatoriedade.

Sempre combinar com:

- texto;
- ícone;
- borda;
- badge;
- tooltip;
- mensagem acessível.

---

# Atalhos

Atalhos devem:

- ser documentados;
- aparecer em menus quando aplicável;
- aparecer em tooltips quando aplicável;
- não conflitar com navegação básica;
- não conflitar com tecnologias assistivas comuns;
- possuir alternativa visual.

---

# Estados Acessíveis

Estados que devem ser comunicados:

- loading;
- erro;
- sucesso;
- warning;
- disabled;
- selected;
- active;
- expanded/collapsed;
- checked/unchecked;
- running/cancelled/completed.

---

# Redução de Movimento

Animações devem:

- ser discretas;
- não impedir uso;
- permitir redução quando preferência do sistema estiver disponível;
- não transmitir informação essencial sozinhas.

---

# Contraste

A aplicação deve buscar conformidade WCAG AA.

Áreas críticas:

- texto principal;
- texto secundário;
- botões;
- links;
- badges;
- mensagens de erro;
- foco;
- estados selecionados.

---

# Tokens Utilizados

A navegação acessível deve utilizar tokens do Design System:

- `Color.Focus`
- `Color.Border.Focus`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Error`
- `Color.Warning`
- `Color.Success`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Small`
- `Typography.Body`
- `Typography.Caption`

Nunca usar foco ou contraste hardcoded sem token.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Accessibility/
        ├── FocusManagerService.cs
        ├── AccessibilityService.cs
        ├── KeyboardNavigationPolicy.cs
        ├── ScreenReaderAnnouncer.cs
        └── AccessibilityOptions.cs
```

---

# Serviços Recomendados

## FocusManagerService

Responsável por controlar foco em dialogs, panels e navegação.

## AccessibilityService

Responsável por centralizar regras acessíveis.

## ScreenReaderAnnouncer

Responsável por anunciar mudanças importantes quando suportado.

## KeyboardNavigationPolicy

Responsável por padronizar navegação por teclado.

---

# Regras para o Codex

- Sempre garantir navegação por teclado em componente interativo.
- Sempre definir foco visível.
- Sempre associar label ao input.
- Sempre adicionar texto acessível para IconButton.
- Sempre usar tooltip em ícones interativos.
- Sempre prender foco em Dialog.
- Sempre retornar foco ao elemento de origem após fechar overlay.
- Sempre evitar informação transmitida apenas por cor.
- Sempre usar textos localizáveis.
- Sempre respeitar tokens do Design System.
- Nunca colocar lógica de foco crítica espalhada em code-behind.
- Nunca criar componente interativo sem estado de foco.

---

# Casos de Erro

- Foco perdido.
- Foco preso indevidamente.
- Dialog abre sem foco.
- Tooltip não acessível por teclado.
- Ícone sem descrição.
- Erro de campo não anunciado.
- Item ativo identificado apenas por cor.
- Atalho conflitante.
- Tab order inconsistente.
- Leitor de tela sem contexto.

---

# Critérios de Aceite

- [ ] Navegação por teclado definida.
- [ ] Foco visível definido.
- [ ] Ordem de tabulação documentada.
- [ ] Dialogs com foco preso.
- [ ] Panels com retorno de foco.
- [ ] Sidebar acessível.
- [ ] DataGrid acessível.
- [ ] Forms acessíveis.
- [ ] Command Palette acessível.
- [ ] Atalhos documentados.
- [ ] Não há dependência exclusiva de cor.
- [ ] Contraste considerado.
- [ ] Textos localizáveis.
- [ ] Sem estilos hardcoded de foco.

---

# Testes

## Unitários

- Foco inicial definido.
- Retorno de foco após dialog.
- Tab order esperado.
- Atalho resolvido.
- Estado ativo acessível.
- IconButton com descrição.
- Campo com erro associado.

## Integração

- Navegação completa sem mouse.
- Abrir e fechar Dialog por teclado.
- Abrir e fechar Panel por teclado.
- Navegar Main Menu.
- Navegar Sidebar.
- Navegar DataGrid.
- Usar Command Palette.
- Preencher formulário.
- Validar erro de campo.
- Alternância Light/Dark.

---

# Dependências

- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F046-Buttons.md
- F047-InputsAndForms.md
- F048-DialogsAndModals.md
- F049-DataGrid.md
- F051-NavigationSystem.md
- F054-MenusAndContextMenus.md
- F055-TooltipsAndHelpHints.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- F063-SidebarNavigation.md
- F069-KeyboardShortcuts.md
- 022-Localization.md

---

# Definition of Done

- Navegação acessível documentada.
- Regras de foco definidas.
- Regras para teclado definidas.
- Componentes críticos cobertos.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

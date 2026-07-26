# F060 - Responsive Behaviors

> Feature ID: F060
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de comportamentos responsivos do WiiStudio, garantindo
que a aplicação se adapte corretamente a diferentes tamanhos de janela, resoluções,
densidades de tela, escalas de fonte e modos de layout, mantendo usabilidade,
acessibilidade e consistência visual.

Embora o WiiStudio seja uma aplicação desktop, ele deve se comportar bem em janelas
redimensionadas, monitores pequenos, notebooks, telas grandes e ambientes com
escala de DPI elevada.

---

# Requisitos Funcionais

## RF001
Definir breakpoints oficiais da aplicação.

## RF002
Padronizar comportamento de Sidebar, Workspace, Panels, Toolbars, DataGrid e Cards.

## RF003
Garantir que telas sejam utilizáveis em janelas menores.

## RF004
Garantir compatibilidade com DPI scaling.

## RF005
Garantir suporte a aumento de fonte.

## RF006
Evitar overflow visual e conteúdo cortado.

## RF007
Garantir responsividade com Light e Dark Theme.

## RF008
Definir regras para layouts compactos, padrão, amplo e ultrawide.

---

# Breakpoints Oficiais

| Breakpoint | Largura | Descrição |
|-----------|--------:|-----------|
| Compact | até 800 px | Janela pequena ou modo compacto |
| Standard | 801-1200 px | Uso padrão em desktop |
| Wide | 1201-1600 px | Monitores maiores |
| UltraWide | acima de 1600 px | Uso avançado com painéis extras |

---

# Comportamento por Breakpoint

## Compact

- Sidebar recolhida por padrão.
- Toolbars com overflow.
- Cards em coluna única.
- DataGrid com colunas essenciais.
- Painéis laterais ocultos ou em overlay.
- Dialogs limitados à largura da janela.
- Menus devem evitar excesso de níveis.

## Standard

- Sidebar expandida.
- Cards em uma ou duas colunas.
- DataGrid com colunas principais.
- Painel lateral opcional.
- Toolbars com grupos visíveis.

## Wide

- Sidebar expandida.
- Cards em múltiplas colunas.
- Painel lateral visível quando útil.
- DataGrid com mais colunas.
- Dashboard com layout amplo.

## UltraWide

- Permite painéis adicionais.
- Dashboard com mais métricas.
- Detalhes e lista podem coexistir.
- Logs e filtros podem ficar lado a lado.

---

# Regiões Responsivas

## Sidebar

A Sidebar deve suportar:

- expandida;
- recolhida;
- overlay quando compacta;
- tooltips no modo recolhido;
- persistência de estado quando aplicável.

## Workspace

O Workspace deve:

- adaptar conteúdo ao espaço disponível;
- evitar scroll horizontal desnecessário;
- permitir scroll vertical;
- preservar ações principais;
- exibir estados adequados.

## Right Panel

O painel lateral deve:

- ficar visível em Wide e UltraWide;
- recolher em Standard quando necessário;
- virar overlay em Compact;
- preservar estado interno.

## Toolbar

Toolbars devem:

- mover ações secundárias para overflow;
- preservar ações principais;
- reduzir labels quando apropriado;
- manter tooltips;
- não quebrar layout.

## DataGrid

DataGrid deve:

- ocultar colunas secundárias em Compact;
- permitir seleção de colunas quando aplicável;
- usar horizontal scroll apenas quando inevitável;
- manter ações principais acessíveis;
- preservar ordenação e filtros.

## Cards

Cards devem:

- reorganizar em uma ou múltiplas colunas;
- preservar legibilidade;
- evitar conteúdo truncado sem tooltip;
- manter área clicável adequada.

---

# Layouts Oficiais

## Compact Layout

```text
+--------------------------------+
| Header / Toolbar               |
+--------------------------------+
| Workspace                      |
|                                |
+--------------------------------+
| Status Bar                     |
+--------------------------------+
```

## Standard Layout

```text
+------------------------------------------+
| Header                                   |
+-------------+----------------------------+
| Sidebar     | Workspace                  |
+-------------+----------------------------+
| Status Bar                               |
+------------------------------------------+
```

## Wide Layout

```text
+------------------------------------------------------+
| Header                                               |
+-------------+----------------------------+-----------+
| Sidebar     | Workspace                  | Panel     |
+-------------+----------------------------+-----------+
| Status Bar                                           |
+------------------------------------------------------+
```

---

# Regras de Redimensionamento

- Nunca quebrar ações críticas.
- Nunca cortar texto essencial sem alternativa.
- Evitar scroll horizontal em telas principais.
- Manter botões principais acessíveis.
- Preservar foco durante redimensionamento.
- Evitar reposicionamento abrupto sem necessidade.
- Preferir overflow controlado.
- Usar tooltips quando texto for truncado.

---

# DPI Scaling

A aplicação deve funcionar corretamente com:

- 100%
- 125%
- 150%
- 175%
- 200%

Componentes não devem depender de pixels fixos quando isso prejudicar escalonamento.

---

# Aumento de Fonte

Com fontes ampliadas:

- labels não devem ser cortados;
- botões devem crescer quando necessário;
- dialogs devem permitir scroll;
- DataGrid deve manter legibilidade;
- cards devem adaptar altura;
- tooltips devem continuar legíveis.

---

# Tokens Utilizados

Responsividade deve consumir tokens de layout:

- `Layout.Breakpoint.Compact`
- `Layout.Breakpoint.Standard`
- `Layout.Breakpoint.Wide`
- `Layout.Breakpoint.UltraWide`
- `Layout.SidebarWidth`
- `Layout.SidebarCollapsedWidth`
- `Layout.RightPanelWidth`
- `Layout.PagePadding`
- `Layout.CompactPagePadding`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Spacing.XL`

Nunca utilizar valores soltos sem token ou justificativa documentada.

---

# Estrutura Recomendada

```text
styles/
└── tokens/
    ├── Breakpoints.axaml
    ├── Responsive.axaml
    └── Layout.axaml
```

```text
ui/
└── responsive/
    ├── ResponsiveLayoutService.cs
    ├── BreakpointService.cs
    └── WindowMetrics.cs
```

---

# Serviço de Responsividade

A aplicação pode possuir serviço para detectar métricas da janela.

## Interface Conceitual

```csharp
public interface IResponsiveLayoutService
{
    Breakpoint CurrentBreakpoint { get; }

    bool IsCompact { get; }

    bool IsWide { get; }

    event EventHandler<BreakpointChangedEventArgs> BreakpointChanged;
}
```

---

# Enum de Breakpoint

```csharp
public enum Breakpoint
{
    Compact,
    Standard,
    Wide,
    UltraWide
}
```

---

# Integração com MVVM

ViewModels podem expor comportamento baseado em breakpoint quando necessário.

Exemplo conceitual:

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [ObservableProperty]
    private bool _isRightPanelVisible;
}
```

A decisão visual simples pode ficar na View, mas regra de estado persistente deve
ficar na ViewModel ou serviço apropriado.

---

# Integração com Settings

O usuário pode ter preferências relacionadas:

- iniciar com sidebar recolhida;
- modo compacto automático;
- densidade visual;
- tamanho da fonte;
- exibir painel lateral por padrão;
- preservar tamanho da janela.

---

# Integração com Window Management

Responsive Behaviors dependem de Window Management para:

- tamanho mínimo;
- tamanho inicial;
- restauração de posição;
- restauração de maximizado;
- persistência de dimensões.

---

# Integração com DataGrid

Responsividade do DataGrid deve considerar:

- colunas prioritárias;
- colunas opcionais;
- menu de colunas;
- filtros compactos;
- toolbar com overflow;
- actions menu.

---

# Integração com Dashboard

Dashboard deve adaptar:

- número de colunas;
- tamanho dos MetricCards;
- visibilidade de gráficos;
- organização de painéis;
- prioridade de informações.

---

# Acessibilidade

Obrigatório:

- Suporte a DPI scaling.
- Suporte a fonte ampliada.
- Foco preservado após redimensionamento.
- Elementos interativos com área mínima.
- Conteúdo essencial acessível em layout compacto.
- Tooltips para texto truncado.
- Contraste preservado.
- Navegação por teclado preservada.

---

# Regras de UX

- Compact Layout não deve remover funcionalidade essencial.
- Ações secundárias podem ir para overflow.
- Conteúdo técnico pode ser recolhido.
- Painéis devem preservar estado ao recolher.
- Layout deve ser previsível.
- Evitar mudanças bruscas em redimensionamento pequeno.
- Mensagens devem continuar legíveis.
- Dialogs devem ser menores que a janela.
- Aplicação deve ter tamanho mínimo definido.

---

# Regras para o Codex

- Sempre considerar breakpoints ao criar tela nova.
- Nunca usar largura fixa sem justificativa.
- Nunca criar layout que dependa apenas de monitor grande.
- Sempre testar Compact, Standard e Wide.
- Sempre usar tokens de layout.
- Sempre prever toolbar overflow.
- Sempre garantir que dialogs caibam na janela.
- Sempre considerar DPI scaling.
- Sempre preservar foco.
- Sempre manter acessibilidade por teclado.
- Nunca esconder ação crítica sem alternativa.

---

# Casos de Erro

- Conteúdo cortado.
- Toolbar quebra layout.
- Dialog maior que janela.
- Sidebar cobre conteúdo indevidamente.
- DataGrid perde ações.
- Foco perdido após redimensionamento.
- Texto truncado sem tooltip.
- Cards com altura inconsistente.
- Painel lateral inacessível.

---

# Critérios de Aceite

- [x] Breakpoints oficiais definidos.
- [x] Comportamento Compact definido.
- [x] Comportamento Standard definido.
- [x] Comportamento Wide definido.
- [x] Comportamento UltraWide definido.
- [x] Sidebar responsiva definida.
- [x] Toolbar responsiva definida.
- [x] DataGrid responsivo definido.
- [x] Cards responsivos definidos.
- [x] DPI scaling considerado.
- [x] Aumento de fonte considerado.
- [x] Compatível com Light/Dark.
- [x] Acessibilidade validada.
- [x] Sem larguras hardcoded injustificadas.

---

# Testes

## Unitários

- Cálculo de breakpoint.
- Mudança de breakpoint.
- Sidebar compacta.
- Painel lateral visível/oculto.
- Preferência de layout.
- Estado preservado.

## Integração

- Redimensionamento da MainWindow.
- Dashboard em Compact.
- Dashboard em Wide.
- LibraryView em Compact.
- DataGrid com colunas reduzidas.
- Toolbar com overflow.
- Dialog em janela pequena.
- DPI 150%.
- Fonte ampliada.
- Navegação por teclado após redimensionamento.

---

# Dependências

- F031-MainWindow.md
- F035-DeviceManager.md
- F036-Dashboard.md
- F037-StatusBar.md
- F038-DockablePanels.md
- F041-DesignSystemFoundation.md
- F045-SpacingAndLayout.md
- F049-DataGrid.md
- F050-CardsAndContainers.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- F058-EmptyErrorAndLoadingStates.md
- F068-WindowManagement.md
- F070-AccessibilityNavigation.md
- 008-UI-UX.md
- 021-ThemeSystem.md

---

# Definition of Done

- Breakpoints documentados.
- Comportamentos responsivos definidos.
- Regras por componente documentadas.
- DPI scaling considerado.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

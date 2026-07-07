# F064 - Header Area

> Feature ID: F064
> Status: Ready for Development
> Priority: High
> Epic: Application Shell

# Objetivo

Definir o padrão oficial da área de Header do WiiStudio, incluindo título da tela,
contexto atual, ações globais, breadcrumb opcional, Command Palette, notificações,
indicadores de tarefas e integração com App Shell, Navigation System, Theme Engine,
Localization e Design System.

---

# Requisitos Funcionais

## RF001
Exibir o título da tela atual.

## RF002
Exibir contexto secundário quando aplicável.

## RF003
Suportar breadcrumb em telas hierárquicas.

## RF004
Disponibilizar ações globais discretas.

## RF005
Integrar com Command Palette.

## RF006
Integrar com Notification Center.

## RF007
Integrar com Task Manager.

## RF008
Suportar responsividade.

## RF009
Garantir compatibilidade com Light e Dark Theme.

## RF010
Garantir acessibilidade por teclado e leitor de tela.

---

# Estrutura Visual

```text
+------------------------------------------------------------------+
| WiiStudio / Título Atual                     [Ações Globais]     |
| Contexto opcional / Breadcrumb opcional      [Command Palette]   |
+------------------------------------------------------------------+
```

---

# Elementos Oficiais

## App Title

Identificação da aplicação.

Exemplo:

```text
WiiStudio
```

## Page Title

Título da tela atual.

Exemplos:

- Dashboard
- Biblioteca
- Dispositivos
- Importação
- Conversão
- Configurações

## Context Subtitle

Informação auxiliar do contexto atual.

Exemplos:

- 120 jogos na biblioteca.
- 2 dispositivos conectados.
- 3 tarefas em execução.
- Perfil: Padrão.

## Breadcrumb

Usado em telas com hierarquia real.

Exemplo:

```text
Biblioteca > Mario Kart Wii > Integridade
```

## Global Actions

Ações globais recomendadas:

- Abrir Command Palette.
- Abrir Notification Center.
- Abrir Settings.
- Abrir Help.
- Alternar tema quando aplicável.

---

# Estados

O Header deve suportar:

- Default
- Loading
- Navigating
- Busy
- Error
- Compact
- Wide

---

# Responsividade

## Compact

- Preservar título principal.
- Ocultar subtítulo quando necessário.
- Mover ações secundárias para overflow.
- Manter Command Palette acessível.

## Standard

- Exibir título.
- Exibir contexto quando disponível.
- Exibir ações principais.

## Wide

- Permitir breadcrumb completo.
- Exibir mais ações.
- Permitir contexto adicional.

---

# Tokens Utilizados

O Header deve consumir:

- `Color.Background`
- `Color.Surface`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Layout.ToolbarHeight`
- `Typography.Title`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Shell/
        └── Header/
            ├── HeaderView.axaml
            ├── HeaderViewModel.cs
            ├── HeaderActionsView.axaml
            └── BreadcrumbView.axaml
```

---

# HeaderViewModel

Responsável por:

- expor título da tela;
- expor subtítulo;
- expor breadcrumb;
- expor ações globais;
- refletir estado de navegação;
- refletir estado de tarefas globais;
- responder a mudanças de rota.

Exemplo conceitual:

```csharp
public sealed partial class HeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _titleKey = "Navigation.Dashboard";

    [ObservableProperty]
    private string? _subtitleKey;

    public ObservableCollection<BreadcrumbItemViewModel> Breadcrumbs { get; } = new();

    public IRelayCommand OpenCommandPaletteCommand { get; }
    public IRelayCommand OpenNotificationsCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }
}
```

---

# Integrações

## Navigation System

O Header deve atualizar título, subtítulo e breadcrumb conforme a rota atual.

## Command Palette

O Header deve fornecer acesso rápido à Command Palette.

## Notification Center

O Header pode exibir contador ou indicador de notificações.

## Task Manager

O Header pode exibir indicador compacto de tarefas em execução.

## Localization

Todos os textos devem usar chaves localizáveis.

Exemplos:

```text
Header.Title.Dashboard
Header.Title.Library
Header.Action.OpenCommandPalette
Header.Action.OpenNotifications
```

---

# Acessibilidade

Obrigatório:

- título da tela anunciado por leitor de tela;
- ações acessíveis por teclado;
- foco visível;
- ordem de tabulação previsível;
- breadcrumb navegável quando interativo;
- contraste WCAG AA;
- ícones com texto acessível;
- não depender apenas de cor.

---

# Regras de UX

- Header não deve virar uma segunda toolbar cheia.
- Título deve ser curto e claro.
- Subtítulo deve complementar, não repetir.
- Ações globais devem ser poucas.
- Ações contextuais específicas devem ficar na Toolbar da tela.
- Breadcrumb deve ser usado apenas quando houver hierarquia real.
- Em modo Compact, priorizar título e Command Palette.

---

# Regras para o Codex

- Nunca colocar regra de negócio no Header.
- Nunca buscar dados diretamente no banco pelo Header.
- Sempre consumir estado de serviços ou ViewModels.
- Sempre usar tokens do Design System.
- Sempre usar textos localizáveis.
- Sempre suportar Light/Dark.
- Sempre respeitar responsividade.
- Sempre garantir acessibilidade.
- Sempre usar Commands para ações.
- Nunca usar valores hardcoded.

---

# Casos de Erro

- Rota sem título.
- Breadcrumb inválido.
- Texto não localizado.
- Ação global indisponível.
- Notificação sem contador válido.
- Estado de tarefa inconsistente.
- Header com overflow em janela pequena.
- Falha ao abrir Command Palette.

---

# Critérios de Aceite

- [ ] Header oficial definido.
- [ ] Título da tela suportado.
- [ ] Subtítulo/contexto suportado.
- [ ] Breadcrumb opcional suportado.
- [ ] Ações globais definidas.
- [ ] Command Palette acessível.
- [ ] Notification indicator previsto.
- [ ] Task indicator previsto.
- [ ] Responsividade documentada.
- [ ] Compatível com Light/Dark.
- [ ] Acessibilidade validada.
- [ ] Textos localizáveis.
- [ ] Sem lógica de negócio na View.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Atualização de título por rota.
- Atualização de subtítulo.
- Breadcrumb gerado.
- Command Palette acionada.
- Notificação atualiza contador.
- Estado Compact reduz ações.
- Texto localizado.

## Integração

- Header na MainWindow.
- Navegação para Dashboard.
- Navegação para Library.
- Navegação para Game Details com breadcrumb.
- Abertura da Command Palette.
- Indicador de notificações.
- Indicador de tarefas.
- Alternância Light/Dark.
- Redimensionamento da janela.
- Navegação por teclado.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- F060-ResponsiveBehaviors.md
- F061-AppShellStructure.md
- 022-Localization.md

---

# Definition of Done

- Header Area documentada.
- Estrutura visual definida.
- ViewModel especificada.
- Integrações globais definidas.
- Responsividade considerada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

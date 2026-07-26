# F062 - Main Menu

> Feature ID: F062
> Status: Implemented and Homologated
> Priority: High
> Epic: Application Shell

# Objetivo

Definir o menu principal oficial do WiiStudio, incluindo grupos, comandos globais,
atalhos, estados, acessibilidade, localização, integração com Command Palette,
Navigation System, Toolbar System e Application Shell.

O Main Menu deve oferecer acesso estruturado às funções globais da aplicação,
mantendo consistência com aplicações desktop profissionais.

---

# Requisitos Funcionais

## RF001
Definir grupos oficiais do menu principal.

## RF002
Definir comandos globais da aplicação.

## RF003
Integrar itens do menu com Commands e ViewModels.

## RF004
Exibir atalhos de teclado quando aplicável.

## RF005
Integrar comandos relevantes com Command Palette.

## RF006
Garantir textos localizáveis.

## RF007
Garantir compatibilidade com Light e Dark Theme.

## RF008
Garantir acessibilidade por teclado.

## RF009
Suportar estados Enabled, Disabled, Checked e Loading.

## RF010
Evitar lógica de negócio no code-behind.

---

# Grupos Oficiais

O Main Menu deve possuir os seguintes grupos principais:

```text
Arquivo
Editar
Exibir
Biblioteca
Ferramentas
Plugins
Ajuda
```

---

# Menu Arquivo

Responsável por operações de entrada, saída e encerramento.

```text
Arquivo
├── Importar Jogo...
├── Importar Pasta...
├── Exportar Selecionados...
├── Backup
│   ├── Criar Backup...
│   └── Restaurar Backup...
├── Abrir Pasta da Biblioteca
├── Configurações
└── Sair
```

---

# Menu Editar

Responsável por ações gerais de edição e seleção.

```text
Editar
├── Copiar
├── Copiar Detalhes
├── Selecionar Tudo
├── Limpar Seleção
├── Limpar Filtros
└── Preferências
```

---

# Menu Exibir

Responsável por navegação visual e layout.

```text
Exibir
├── Dashboard
├── Biblioteca
├── Dispositivos
├── Tarefas
├── Diagnósticos
├── Alternar Tema
├── Sidebar
│   ├── Expandir
│   └── Recolher
├── Painéis
│   ├── Mostrar Painel Lateral
│   └── Ocultar Painel Lateral
└── Tela Cheia
```

---

# Menu Biblioteca

Responsável por operações sobre jogos.

```text
Biblioteca
├── Atualizar Biblioteca
├── Verificar Integridade
├── Sincronizar Metadados
├── Baixar Capas
├── Detectar Duplicados
├── Favoritos
├── Exportar Lista
└── Remover Selecionados
```

---

# Menu Ferramentas

Responsável por utilitários e operações técnicas.

```text
Ferramentas
├── Configurar Wiimms
├── Configurar NKit Legacy
├── Executar Diagnóstico
├── Ver Logs
├── Limpar Cache
├── Atualizar GameTDB
└── Abrir Command Palette
```

---

# Menu Plugins

Responsável por gestão de plugins.

```text
Plugins
├── Gerenciar Plugins
├── Instalar Plugin...
├── Recarregar Plugins
├── Ver Permissões
└── Abrir Pasta de Plugins
```

---

# Menu Ajuda

Responsável por documentação e suporte.

```text
Ajuda
├── Documentação
├── Guia do Usuário
├── Atalhos de Teclado
├── Relatar Problema
├── Abrir Logs
├── Verificar Atualizações
└── Sobre o WiiStudio
```

---

# Modelo Conceitual

```csharp
public sealed class MainMenuItem
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string? IconKey { get; init; }
    public string? ShortcutText { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsChecked { get; init; }
    public bool IsDanger { get; init; }
    public ICommand? Command { get; init; }
    public IReadOnlyList<MainMenuItem> Children { get; init; } = [];
}
```

---

# MainMenuViewModel

Responsável por:

- expor grupos do menu;
- controlar estado enabled/disabled;
- controlar itens checked;
- vincular comandos;
- reagir ao contexto da aplicação;
- expor atalhos;
- integrar com Command Palette.

Exemplo conceitual:

```csharp
public sealed class MainMenuViewModel
{
    public ObservableCollection<MainMenuItemViewModel> Items { get; } = new();
}
```

---

# Integração com Commands

Itens do menu devem executar Commands.

Exemplos:

- `ImportGameCommand`
- `ExportSelectedCommand`
- `OpenSettingsCommand`
- `RunDiagnosticsCommand`
- `ToggleThemeCommand`
- `ExitApplicationCommand`

O menu não deve chamar serviços diretamente pela View.

---

# Integração com Navigation System

Itens de navegação devem usar `INavigationService`.

Exemplos:

- Dashboard.
- Biblioteca.
- Dispositivos.
- Tarefas.
- Diagnósticos.
- Configurações.

---

# Integração com Command Palette

Comandos globais do Main Menu devem ser registrados na Command Palette.

Exemplos:

- Importar jogo.
- Abrir biblioteca.
- Executar diagnóstico.
- Alternar tema.
- Abrir configurações.
- Ver atalhos.

---

# Integração com Keyboard Shortcuts

O Main Menu deve exibir atalhos quando existirem.

Sugestões:

| Atalho | Ação |
|-------|------|
| Ctrl+I | Importar jogo |
| Ctrl+Shift+I | Importar pasta |
| Ctrl+E | Exportar selecionados |
| Ctrl+R | Atualizar biblioteca |
| Ctrl+F | Pesquisar |
| Ctrl+, | Configurações |
| Ctrl+Shift+P | Command Palette |
| F1 | Ajuda |
| Alt+F4 | Sair |

---

# Integração com Localization

Todos os textos devem usar chaves localizáveis.

Exemplos:

```text
Menu.File
Menu.File.ImportGame
Menu.File.ImportFolder
Menu.Edit.Copy
Menu.View.Dashboard
Menu.Tools.RunDiagnostics
Menu.Help.About
```

Nunca usar texto fixo diretamente na View.

---

# Estados dos Itens

Itens do menu devem suportar:

- Default
- Hover
- Focus
- Pressed
- Disabled
- Checked
- Danger
- Loading

---

# Ações Destrutivas

Ações destrutivas devem:

- usar estado Danger;
- estar separadas por divisor;
- exigir confirmação via Dialog;
- descrever claramente impacto.

Exemplos:

- Remover selecionados.
- Limpar cache.
- Cancelar tarefas.
- Excluir backup.

---

# Tokens Utilizados

O Main Menu deve consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Color.Primary`
- `Color.Error`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Small`
- `Elevation.Menu`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    ├── Shell/
    │   └── MainMenu/
    │       ├── MainMenuView.axaml
    │       ├── MainMenuViewModel.cs
    │       ├── MainMenuItemViewModel.cs
    │       └── MainMenuCommandRegistry.cs
```

---

# Acessibilidade

Obrigatório:

- navegação por teclado;
- suporte a Alt para abrir menus;
- setas para navegar;
- Enter/Space para executar;
- ESC para fechar;
- foco visível;
- item disabled anunciado;
- atalho anunciado;
- contraste WCAG AA.

---

# Regras de UX

- Menus devem ser previsíveis.
- Agrupar comandos por finalidade.
- Manter nomes curtos e claros.
- Exibir atalhos quando existirem.
- Evitar submenus profundos.
- Colocar ações destrutivas no final.
- Desabilitar comandos indisponíveis em vez de ocultar quando isso ajudar a compreensão.
- Não duplicar comandos desnecessariamente.
- Comandos frequentes podem aparecer também em Toolbar.

---

# Regras para o Codex

- Nunca implementar comandos diretamente no code-behind.
- Sempre usar Commands.
- Sempre usar chaves de localização.
- Sempre usar tokens do Design System.
- Sempre exibir atalhos quando registrados.
- Sempre integrar comandos globais à Command Palette.
- Sempre confirmar ações destrutivas.
- Sempre suportar Light/Dark.
- Sempre garantir navegação por teclado.
- Nunca acessar serviços de domínio diretamente pela View.

---

# Casos de Erro

- Comando não registrado.
- Atalho conflitante.
- Item sem localização.
- Ação indisponível.
- Falha ao executar comando.
- Menu de plugin inválido.
- Item checked inconsistente.
- Ação destrutiva sem confirmação.

---

# Critérios de Aceite

- [x] Grupos oficiais definidos.
- [x] Itens principais documentados.
- [x] MainMenuViewModel especificado.
- [x] Commands integrados.
- [x] Atalhos exibidos.
- [x] Textos localizáveis.
- [x] Integração com Command Palette.
- [x] Integração com Navigation System.
- [x] Ações destrutivas protegidas.
- [x] Compatível com Light/Dark.
- [x] Navegação por teclado funcional.
- [x] Sem lógica de negócio na View.
- [x] Sem valores hardcoded.

---

# Testes

## Unitários

- Menu carrega grupos oficiais.
- Item executa command.
- Item disabled não executa.
- Atalho exibido.
- Item checked alterna estado.
- Ação destrutiva solicita confirmação.
- Texto localizado.
- Comando ausente tratado.

## Integração

- Main Menu na MainWindow.
- Navegação pelo menu Exibir.
- Importação pelo menu Arquivo.
- Diagnóstico pelo menu Ferramentas.
- Command Palette aberta pelo menu.
- Alternância de tema.
- Navegação por teclado.
- ESC fecha menu.
- Atalhos exibidos corretamente.

---

# Dependências

- F040-CommandPalette.md
- F046-Buttons.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- F054-MenusAndContextMenus.md
- F061-AppShellStructure.md
- F069-KeyboardShortcuts.md
- 022-Localization.md

---

# Definition of Done

- Main Menu documentado.
- Grupos e itens oficiais definidos.
- Commands especificados.
- Atalhos documentados.
- Acessibilidade validada.
- Localization prevista.
- Testes definidos.
- Documentação atualizada.

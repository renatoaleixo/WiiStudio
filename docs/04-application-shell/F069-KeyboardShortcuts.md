# F069 - Keyboard Shortcuts

> Feature ID: F069
> Status: Ready for Development
> Priority: Critical
> Epic: Application Shell

# Objetivo

Definir o sistema oficial de atalhos de teclado do WiiStudio, incluindo comandos
globais, comandos contextuais, resolução de conflitos, exibição em menus e tooltips,
acessibilidade, localização e integração com Command Palette, Main Menu, Toolbars
e Navigation System.

Atalhos de teclado devem aumentar produtividade, acessibilidade e consistência da
experiência desktop.

---

# Requisitos Funcionais

## RF001
Definir atalhos globais oficiais.

## RF002
Definir atalhos contextuais por tela.

## RF003
Integrar atalhos com Commands.

## RF004
Exibir atalhos em menus e tooltips.

## RF005
Integrar com Command Palette.

## RF006
Evitar conflitos de atalhos.

## RF007
Permitir futura customização por usuário.

## RF008
Garantir acessibilidade por teclado.

## RF009
Garantir compatibilidade multiplataforma.

## RF010
Evitar lógica de negócio no code-behind.

---

# Atalhos Globais Oficiais

| Atalho | Ação |
|-------|------|
| Ctrl+Shift+P | Abrir Command Palette |
| Ctrl+, | Abrir Configurações |
| Ctrl+R | Atualizar tela atual |
| Ctrl+F | Focar pesquisa |
| F1 | Abrir Ajuda |
| Alt+Left | Voltar |
| Alt+Right | Avançar |
| Ctrl+B | Expandir/Recolher Sidebar |
| Ctrl+Q | Sair |
| Esc | Fechar painel/dialog quando seguro |

---

# Atalhos de Navegação

| Atalho | Área |
|-------|------|
| Ctrl+1 | Dashboard |
| Ctrl+2 | Biblioteca |
| Ctrl+3 | Dispositivos |
| Ctrl+4 | Importação |
| Ctrl+5 | Conversão |
| Ctrl+6 | Tarefas |
| Ctrl+7 | Metadados |
| Ctrl+8 | Backups |
| Ctrl+9 | Diagnósticos |

---

# Atalhos da Biblioteca

| Atalho | Ação |
|-------|------|
| Ctrl+I | Importar jogo |
| Ctrl+Shift+I | Importar pasta |
| Ctrl+E | Exportar selecionados |
| Delete | Remover selecionados |
| Ctrl+D | Detectar duplicados |
| Ctrl+M | Sincronizar metadados |
| Ctrl+L | Baixar capas |
| Space | Alternar seleção |
| Enter | Abrir detalhes |
| Ctrl+A | Selecionar todos |

---

# Atalhos de Tarefas

| Atalho | Ação |
|-------|------|
| Ctrl+Shift+C | Cancelar tarefa selecionada |
| Ctrl+Shift+L | Limpar tarefas concluídas |
| Enter | Abrir detalhes da tarefa |
| F5 | Atualizar lista |

---

# Atalhos de Logs e Diagnóstico

| Atalho | Ação |
|-------|------|
| Ctrl+Shift+D | Executar diagnóstico |
| Ctrl+Shift+G | Abrir logs |
| Ctrl+C | Copiar item selecionado |
| Ctrl+Shift+C | Copiar detalhes |
| Ctrl+S | Exportar logs |
| F5 | Atualizar |

---

# Atalhos de Forms

| Atalho | Ação |
|-------|------|
| Tab | Próximo campo |
| Shift+Tab | Campo anterior |
| Enter | Confirmar quando seguro |
| Esc | Cancelar/fechar |
| Ctrl+S | Salvar |
| Ctrl+Z | Desfazer quando suportado |

---

# Atalhos de Dialogs

| Atalho | Ação |
|-------|------|
| Enter | Ação principal quando seguro |
| Esc | Cancelar/fechar quando seguro |
| Tab | Próximo elemento |
| Shift+Tab | Elemento anterior |
| Space | Acionar botão focado |

---

# Modelo Conceitual

```csharp
public sealed class KeyboardShortcut
{
    public string Id { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string Gesture { get; init; } = string.Empty;
    public string DescriptionKey { get; init; } = string.Empty;
    public ShortcutScope Scope { get; init; }
    public bool IsUserConfigurable { get; init; }
}
```

---

# Escopos de Atalho

```csharp
public enum ShortcutScope
{
    Global,
    Shell,
    Workspace,
    Library,
    Devices,
    Tasks,
    Diagnostics,
    Dialog,
    Form
}
```

---

# Shortcut Registry

A aplicação deve possuir registro centralizado de atalhos.

```csharp
public interface IKeyboardShortcutRegistry
{
    void Register(KeyboardShortcut shortcut);
    bool TryGetByGesture(string gesture, out KeyboardShortcut shortcut);
    IReadOnlyList<KeyboardShortcut> GetByScope(ShortcutScope scope);
}
```

---

# Shortcut Service

```csharp
public interface IKeyboardShortcutService
{
    Task HandleAsync(string gesture, ShortcutContext context, CancellationToken cancellationToken = default);
    IReadOnlyList<KeyboardShortcut> GetActiveShortcuts();
}
```

---

# Resolução de Conflitos

Quando houver conflito:

1. contexto mais específico vence;
2. Dialog vence Workspace;
3. Form vence Shell quando campo está focado;
4. Global é fallback;
5. conflitos não resolvíveis devem ser registrados como erro de configuração.

Exemplo:

```text
Ctrl+S em Settings = salvar configurações
Ctrl+S em Logs = exportar logs
```

---

# Integração com Commands

Atalhos devem executar comandos registrados.

Fluxo:

```text
Tecla pressionada
    ↓
KeyboardShortcutService
    ↓
Resolve contexto
    ↓
Localiza Command
    ↓
Executa Command
```

---

# Integração com Command Palette

A Command Palette deve exibir atalhos associados aos comandos.

Exemplo:

```text
Importar jogo                         Ctrl+I
Abrir configurações                   Ctrl+,
Executar diagnóstico                  Ctrl+Shift+D
```

---

# Integração com Main Menu

O Main Menu deve exibir atalhos ao lado dos itens.

Exemplo:

```text
Importar Jogo...        Ctrl+I
Configurações           Ctrl+,
Sair                    Ctrl+Q
```

---

# Integração com Tooltips

Tooltips de botões com atalho devem incluir a combinação.

Exemplo:

```text
Atualizar biblioteca (Ctrl+R)
```

---

# Customização Futura

O sistema deve permitir futura customização pelo usuário.

Considerar:

- salvar atalhos por perfil;
- restaurar padrão;
- detectar conflitos;
- exportar/importar atalhos;
- desabilitar atalho específico.

A implementação inicial pode usar atalhos fixos, desde que a arquitetura permita evolução.

---

# Multiplataforma

Considerar diferenças entre sistemas:

- Windows/Linux: `Ctrl`
- macOS futuro: `Cmd`
- teclados internacionais;
- atalhos reservados pelo sistema;
- Alt/Meta podem variar.

O texto exibido deve ser gerado por serviço, não hardcoded.

---

# Estrutura Recomendada

```text
src/
└── WiiStudio.UI/
    └── Input/
        ├── KeyboardShortcut.cs
        ├── KeyboardShortcutRegistry.cs
        ├── KeyboardShortcutService.cs
        ├── ShortcutContext.cs
        ├── ShortcutFormatter.cs
        └── DefaultShortcuts.cs
```

---

# Acessibilidade

Obrigatório:

- todas as funções críticas acessíveis por teclado;
- foco visível;
- atalhos documentados;
- atalhos exibidos quando possível;
- dialogs navegáveis por teclado;
- menus navegáveis por teclado;
- evitar atalhos que conflitem com tecnologias assistivas;
- permitir uso sem mouse.

---

# Regras de UX

- Atalhos devem seguir convenções comuns.
- Não criar atalhos demais.
- Atalhos destrutivos devem exigir confirmação quando aplicável.
- Atalhos devem aparecer em menus e tooltips.
- Atalhos contextuais devem ser previsíveis.
- Esc deve fechar apenas quando seguro.
- Enter deve confirmar apenas quando não houver risco.
- Delete deve ser protegido por confirmação em ações destrutivas.

---

# Regras para o Codex

- Nunca executar ação de teclado diretamente no code-behind.
- Sempre mapear atalho para Command.
- Sempre registrar atalhos em registry central.
- Sempre considerar escopo.
- Sempre evitar conflitos.
- Sempre exibir atalho em menus/tooltips quando aplicável.
- Sempre usar textos localizáveis.
- Sempre garantir acessibilidade.
- Sempre proteger ações destrutivas.
- Nunca hardcodar texto de atalho em múltiplos lugares.

---

# Casos de Erro

- Atalho duplicado.
- Command não registrado.
- Escopo inválido.
- Atalho reservado pelo sistema.
- Atalho acionado em contexto errado.
- Ação destrutiva sem confirmação.
- Tooltip desatualizado.
- Command Palette não exibe atalho.
- Atalho não funciona em layout de teclado específico.

---

# Critérios de Aceite

- [ ] Atalhos globais definidos.
- [ ] Atalhos contextuais definidos.
- [ ] Shortcut Registry especificado.
- [ ] Shortcut Service especificado.
- [ ] Resolução de conflitos documentada.
- [ ] Integração com Commands definida.
- [ ] Integração com Command Palette definida.
- [ ] Integração com Main Menu definida.
- [ ] Integração com Tooltips definida.
- [ ] Acessibilidade validada.
- [ ] Multiplataforma considerado.
- [ ] Sem lógica de negócio no code-behind.

---

# Testes

## Unitários

- Registrar atalho.
- Detectar conflito.
- Resolver escopo específico.
- Executar Command.
- Ignorar Command disabled.
- Formatar atalho.
- Exibir atalho localizado.
- Proteger ação destrutiva.

## Integração

- Command Palette com atalhos.
- Main Menu com atalhos.
- Tooltips com atalhos.
- Navegação Ctrl+1.
- Abrir Settings Ctrl+,.
- Abrir Command Palette Ctrl+Shift+P.
- Esc fecha Dialog.
- Delete confirma remoção.
- Ctrl+S em Settings.
- Atalho conflitante em contexto específico.

---

# Dependências

- F040-CommandPalette.md
- F046-Buttons.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- F054-MenusAndContextMenus.md
- F055-TooltipsAndHelpHints.md
- F062-MainMenu.md
- F063-SidebarNavigation.md
- F070-AccessibilityNavigation.md
- 022-Localization.md

---

# Definition of Done

- Keyboard Shortcuts documentado.
- Atalhos globais e contextuais definidos.
- Registry e Service especificados.
- Integrações documentadas.
- Resolução de conflitos definida.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

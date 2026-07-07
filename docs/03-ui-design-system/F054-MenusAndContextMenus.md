# F054 - Menus & Context Menus

> Feature ID: F054
> Status: Ready for Development
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de Menus, Context Menus, Flyout Menus e menus de ações do
WiiStudio, garantindo consistência visual, acessibilidade, organização de comandos,
integração com MVVM, Command Palette, Keyboard Shortcuts e Design System.

Menus devem ser usados para agrupar comandos relacionados, oferecer ações
contextuais e reduzir a poluição visual da interface.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de menus.

## RF002
Padronizar itens, ícones, atalhos, separadores e grupos.

## RF003
Suportar menus globais e contextuais.

## RF004
Integrar com Commands da ViewModel.

## RF005
Integrar com Keyboard Shortcuts e Command Palette.

## RF006
Garantir compatibilidade com Light e Dark Theme.

## RF007
Garantir acessibilidade por teclado e leitor de tela.

## RF008
Suportar ações destrutivas com confirmação.

---

# Tipos Oficiais

## Main Menu

Menu principal da aplicação.

Exemplos:

- Arquivo.
- Editar.
- Exibir.
- Ferramentas.
- Ajuda.

## Context Menu

Menu exibido sobre item específico.

Exemplos:

- Jogo na biblioteca.
- Tarefa.
- Dispositivo.
- Backup.
- Log.
- Plugin.

## Action Menu

Menu de ações extras em botão de overflow.

Exemplos:

- Mais ações.
- Opções avançadas.
- Exportar como.
- Abrir com.

## Flyout Menu

Menu leve ligado a um botão ou área contextual.

Exemplos:

- Perfil ativo.
- Tema.
- Filtros rápidos.
- Ordenação.

---

# Estrutura Visual

```text
+-----------------------------+
| Ícone  Ação principal  Ctrl+X |
| Ícone  Outra ação            |
|-----------------------------|
| Ícone  Ação destrutiva       |
+-----------------------------+
```

---

# Anatomia

Um item de menu pode possuir:

- Ícone.
- Texto.
- Atalho.
- Badge.
- Estado checked.
- Estado disabled.
- Submenu.
- Separador.
- Tooltip opcional.
- Indicador de perigo.

---

# Menus Globais Recomendados

## Arquivo

- Importar jogo.
- Exportar jogo.
- Abrir pasta.
- Backup.
- Sair.

## Editar

- Copiar.
- Selecionar tudo.
- Limpar seleção.
- Preferências.

## Exibir

- Dashboard.
- Biblioteca.
- Tarefas.
- Diagnósticos.
- Alternar tema.

## Ferramentas

- Verificar integridade.
- Atualizar metadados.
- Gerenciar plugins.
- Executar diagnóstico.

## Ajuda

- Documentação.
- Sobre.
- Logs.
- Relatar problema.

---

# Context Menus por Área

## Biblioteca

- Ver detalhes.
- Favoritar.
- Converter.
- Exportar.
- Verificar integridade.
- Abrir pasta.
- Remover.

## Dispositivos

- Atualizar.
- Ver detalhes.
- Analisar partição.
- Abrir logs.
- Exportar relatório.

## Tarefas

- Ver detalhes.
- Pausar.
- Retomar.
- Cancelar.
- Copiar logs.

## Logs

- Copiar mensagem.
- Copiar detalhes.
- Exportar.
- Filtrar por origem.
- Abrir diagnóstico.

## Plugins

- Habilitar.
- Desabilitar.
- Configurar.
- Ver permissões.
- Remover.

---

# Estados Visuais

Itens de menu devem suportar:

- Default
- Hover
- Focus
- Pressed
- Active
- Checked
- Disabled
- Danger
- Loading

---

# Tokens Utilizados

Menus devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Error`
- `Color.Primary`
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
styles/
└── components/
    ├── Menus.axaml
    ├── ContextMenus.axaml
    ├── FlyoutMenus.axaml
    ├── MenuTokens.axaml
    └── MenuItems.axaml
```

---

# Modelo Conceitual

```csharp
public sealed class MenuItemDefinition
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string? IconKey { get; init; }
    public string? ShortcutText { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsDanger { get; init; }
    public ICommand? Command { get; init; }
    public IReadOnlyList<MenuItemDefinition> Children { get; init; } = [];
}
```

---

# Integração com MVVM

Menus devem ser gerados ou controlados por ViewModels.

A View não deve conter lógica de decisão sobre permissões, estados ou ações.

Exemplo conceitual:

```csharp
public ObservableCollection<MenuItemViewModel> ContextActions { get; } = new();
```

---

# Integração com Command Palette

Ações relevantes de menu devem também estar disponíveis na Command Palette.

Exemplos:

- Importar jogo.
- Abrir configurações.
- Executar diagnóstico.
- Atualizar metadados.
- Abrir logs.

---

# Integração com Keyboard Shortcuts

Menus devem exibir atalhos quando existirem.

Exemplo:

```text
Importar jogo        Ctrl+I
Configurações        Ctrl+,
Sair                 Alt+F4
```

---

# Ações Destrutivas

Ações destrutivas devem:

- usar estado visual Danger;
- ficar separadas de ações comuns;
- exigir confirmação via Dialog;
- informar claramente o impacto da ação.

Exemplos:

- Remover jogo.
- Excluir backup.
- Limpar cache.
- Cancelar todas as tarefas.

---

# Submenus

Submenus devem ser usados com moderação.

Permitido quando:

- há muitas opções relacionadas;
- a organização melhora a leitura;
- o submenu não esconde ação crítica.

Evitar mais de dois níveis de profundidade.

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Setas navegam entre itens.
- Enter ou Space executa item.
- ESC fecha o menu.
- Foco visível.
- Item disabled anunciado.
- Atalho anunciado quando aplicável.
- Contraste WCAG AA.
- Texto acessível em itens com ícone.

---

# Regras de UX

- Menus devem ser curtos.
- Separadores devem agrupar semanticamente.
- Ações mais usadas devem aparecer primeiro.
- Ações destrutivas devem ficar no final.
- Textos devem ser verbos de ação.
- Não duplicar ações sem necessidade.
- Não esconder ação principal apenas em menu.
- Context Menu deve depender do item selecionado.
- Item disabled deve ter motivo claro quando possível.

---

# Regras para o Codex

- Nunca criar menu com estilo inline.
- Nunca executar lógica de negócio na View.
- Sempre usar Commands.
- Sempre usar textos localizáveis.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre exibir atalhos quando existirem.
- Sempre usar confirmação para ação destrutiva.
- Sempre adicionar ícones apenas quando ajudarem a compreensão.
- Sempre manter contexto do item selecionado.
- Sempre tratar item disabled.

---

# Casos de Erro

- Comando não registrado.
- Item sem permissão.
- Ação falha.
- Contexto inexistente.
- Item selecionado removido.
- Plugin registra ação inválida.
- Atalho conflitante.
- Submenu vazio.

---

# Critérios de Aceite

- [ ] Tipos oficiais de menus definidos.
- [ ] Main Menu documentado.
- [ ] Context Menus documentados.
- [ ] Action Menu documentado.
- [ ] Flyout Menu documentado.
- [ ] Estados visuais definidos.
- [ ] Integração com Commands prevista.
- [ ] Integração com Command Palette prevista.
- [ ] Integração com Keyboard Shortcuts prevista.
- [ ] Ações destrutivas exigem confirmação.
- [ ] Compatível com Light/Dark.
- [ ] Navegação por teclado funcional.
- [ ] Textos localizáveis.
- [ ] Sem estilos inline.
- [ ] Sem lógica de negócio na View.

---

# Testes

## Unitários

- Item executa command.
- Item disabled não executa.
- Ação destrutiva solicita confirmação.
- Submenu carregado.
- Atalho exibido.
- Item contextual baseado em seleção.
- Plugin action validada.

## Integração

- Main Menu.
- Context Menu da LibraryView.
- Context Menu do TaskManager.
- Context Menu do LoggingCenter.
- Menu de Plugins.
- Flyout de perfil.
- Menu de filtros.
- Alternância Light/Dark.
- Navegação por teclado.
- ESC fecha menu.

---

# Dependências

- F025-NotificationCenter.md
- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F052-ToolbarSystem.md
- 022-Localization.md

---

# Definition of Done

- Menus oficiais documentados.
- Context Menus definidos.
- Estados visuais definidos.
- Integração com Commands especificada.
- Ações destrutivas protegidas.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

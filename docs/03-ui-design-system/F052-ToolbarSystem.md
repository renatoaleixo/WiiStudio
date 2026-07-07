# F052 - Toolbar System

> Feature ID: F052
> Status: Ready for Development
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de Toolbars do WiiStudio, incluindo barras globais,
barras contextuais, agrupamento de comandos, ações rápidas, overflow, atalhos,
ícones, estados visuais e integração com MVVM, Command Palette, Design System e
Application Shell.

Toolbars devem permitir que o usuário execute ações frequentes de forma rápida,
clara e consistente, sem poluir a interface.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de Toolbars.

## RF002
Padronizar botões, ícones, separadores e grupos de comandos.

## RF003
Suportar ações globais e contextuais.

## RF004
Suportar overflow quando não houver espaço suficiente.

## RF005
Integrar com Commands da ViewModel.

## RF006
Integrar com Keyboard Shortcuts e Command Palette.

## RF007
Garantir compatibilidade com Light e Dark Theme.

## RF008
Garantir acessibilidade por teclado e leitor de tela.

---

# Tipos Oficiais de Toolbar

## Main Toolbar

Toolbar principal da aplicação.

Exemplos de ações:

- Atualizar.
- Importar.
- Converter.
- Abrir configurações.
- Abrir Command Palette.

## Context Toolbar

Toolbar relacionada à tela atual.

Exemplos:

- Biblioteca: filtrar, ordenar, importar, exportar.
- Dispositivos: atualizar, montar, analisar.
- Tarefas: pausar, cancelar, limpar concluídas.
- Logs: limpar, exportar, copiar.

## Selection Toolbar

Toolbar exibida quando há itens selecionados.

Exemplos:

- Converter selecionados.
- Exportar selecionados.
- Remover selecionados.
- Limpar seleção.

## Inline Toolbar

Toolbar pequena dentro de Cards, Panels ou Headers.

Exemplos:

- Ações rápidas de um jogo.
- Ações de um backup.
- Ações de um provider.
- Ações de um painel de diagnóstico.

---

# Estrutura Visual

```text
+------------------------------------------------------------+
| [Grupo 1]  [Ação] [Ação] | [Grupo 2] [Ação] | [Overflow]   |
+------------------------------------------------------------+
```

---

# Anatomia

Uma Toolbar pode possuir:

- Botões com texto.
- IconButtons.
- Separadores.
- Grupos.
- SearchBox.
- Menu de overflow.
- Indicador de loading.
- Estado disabled.
- Atalhos visíveis em tooltip.
- Contador contextual.

---

# Agrupamento de Comandos

Comandos devem ser agrupados por finalidade.

## Exemplo Biblioteca

```text
[Atualizar] | [Importar] [Exportar] | [Filtros] [Ordenar] | [...]
```

## Exemplo Tarefas

```text
[Pausar] [Retomar] [Cancelar] | [Limpar concluídas] | [...]
```

---

# Estados Visuais

Toolbar e seus itens devem suportar:

- Default
- Hover
- Focus
- Pressed
- Active
- Disabled
- Loading
- Overflow

---

# Overflow

Quando não houver espaço suficiente:

- comandos menos importantes devem ir para menu de overflow;
- comandos críticos devem permanecer visíveis;
- ações destrutivas não devem ficar ocultas sem confirmação;
- tooltips devem continuar disponíveis.

---

# Prioridade de Comandos

| Prioridade | Comportamento |
|-----------|---------------|
| Primary | Sempre visível quando possível |
| Secondary | Pode ir para overflow |
| Tertiary | Preferencialmente no overflow |
| Destructive | Visível ou em menu com confirmação |
| Diagnostic | Pode ficar em overflow |

---

# Tokens Utilizados

Toolbars devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Subtle`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Layout.ToolbarHeight`
- `Radius.Small`
- `Typography.Body`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Toolbars.axaml
    ├── ToolbarTokens.axaml
    ├── ToolbarButtons.axaml
    └── ToolbarOverflow.axaml
```

---

# Integração com MVVM

Toolbars devem ser alimentadas por Commands da ViewModel.

Exemplo conceitual:

```csharp
public sealed partial class LibraryToolbarViewModel : ObservableObject
{
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ImportCommand { get; }
    public IAsyncRelayCommand ExportSelectedCommand { get; }
}
```

A View não deve conter lógica de negócio.

---

# Integração com Command Palette

Comandos da Toolbar devem ser registráveis na Command Palette quando forem ações
globais ou relevantes.

Exemplos:

- Atualizar biblioteca.
- Importar jogos.
- Exportar selecionados.
- Abrir diagnósticos.
- Cancelar tarefas.

---

# Integração com Keyboard Shortcuts

Tooltips devem exibir atalhos quando existirem.

Exemplo:

```text
Importar jogos (Ctrl+I)
```

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Foco visível.
- Tooltip em IconButtons.
- Nome acessível para cada ação.
- Ordem de tabulação previsível.
- Contraste WCAG AA.
- Não depender apenas de ícone para ações críticas.

---

# Regras de UX

- Evitar excesso de botões.
- Agrupar comandos por contexto.
- Manter ações principais visíveis.
- Usar separadores com moderação.
- Preferir IconButton para ações recorrentes e compactas.
- Usar botão com texto para ações críticas.
- Ações destrutivas devem exigir confirmação.
- A Toolbar deve refletir o estado atual da tela.

---

# Regras para o Codex

- Nunca criar Toolbar com estilo inline.
- Nunca executar ação diretamente no code-behind.
- Sempre usar Commands.
- Sempre usar tokens do Design System.
- Sempre adicionar tooltip em IconButton.
- Sempre respeitar Light/Dark.
- Sempre prever overflow.
- Sempre tratar estado Disabled.
- Sempre integrar ações longas ao Task Manager quando aplicável.
- Sempre enviar feedback ao Notification Center quando necessário.

---

# Casos de Erro

- Comando indisponível.
- Ação falha.
- Seleção inválida.
- Operação cancelada.
- Espaço insuficiente na Toolbar.
- Falha ao resolver atalho.
- Falha em ação em lote.

---

# Critérios de Aceite

- [ ] Tipos oficiais de Toolbar definidos.
- [ ] Agrupamento de comandos documentado.
- [ ] Overflow suportado.
- [ ] Tooltips definidos.
- [ ] Integração com Commands prevista.
- [ ] Integração com Command Palette prevista.
- [ ] Integração com Keyboard Shortcuts prevista.
- [ ] Compatível com Light/Dark.
- [ ] Navegação por teclado funcional.
- [ ] Sem estilos inline.
- [ ] Sem lógica de negócio na View.

---

# Testes

## Unitários

- Comando executado.
- Comando desabilitado.
- Estado loading.
- Tooltip obrigatório.
- Overflow por prioridade.
- Ação em lote com seleção.

## Integração

- Toolbar da LibraryView.
- Toolbar do DeviceManager.
- Toolbar do TaskManager.
- Toolbar do LoggingCenter.
- Toolbar do AdvancedDiagnostics.
- Alternância Light/Dark.
- Navegação por teclado.
- Redimensionamento da janela.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F051-NavigationSystem.md
- 022-Localization.md

---

# Definition of Done

- Toolbar System documentado.
- Tipos oficiais definidos.
- Estados visuais definidos.
- Overflow documentado.
- Integração com Commands especificada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

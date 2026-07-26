# F055 - Tooltips & Help Hints

> Feature ID: F055
> Status: Implemented and Homologated
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de Tooltips, Help Hints, textos auxiliares e dicas contextuais do WiiStudio, garantindo que o usuário receba orientação clara, acessível e consistente durante o uso da aplicação.

Tooltips e Help Hints devem reduzir dúvidas, explicar ações, reforçar consequências e melhorar a usabilidade sem poluir visualmente a interface.

---

# Requisitos Funcionais

## RF001
Definir os tipos oficiais de dicas e textos de ajuda.

## RF002
Padronizar Tooltips para botões, ícones, menus e ações compactas.

## RF003
Padronizar Help Hints para campos, seções e fluxos complexos.

## RF004
Garantir compatibilidade com Light e Dark Theme.

## RF005
Garantir acessibilidade por teclado e leitor de tela.

## RF006
Garantir textos localizáveis.

## RF007
Definir regras de uso para evitar excesso de dicas.

## RF008
Integrar com Design System, Localization e Accessibility.

---

# Tipos Oficiais

## Tooltip

Usado para explicar uma ação curta ou ícone.

Exemplos:

- Atualizar biblioteca.
- Favoritar jogo.
- Abrir pasta.
- Ver detalhes.
- Cancelar tarefa.

## Help Hint

Usado para explicar um campo ou configuração.

Exemplos:

- Diretório de cache.
- Caminho do Wiimms.
- Política de conversão.
- Retenção de logs.
- Limite de tarefas simultâneas.

## Inline Help

Texto auxiliar exibido diretamente abaixo de um campo ou seção.

Exemplos:

- "Este caminho será usado para armazenar arquivos temporários."
- "Alterar esta opção pode exigir reiniciar a aplicação."
- "A conversão pode demorar dependendo do tamanho do arquivo."

## Contextual Help

Ajuda contextual associada a uma tela ou fluxo.

Exemplos:

- Ajuda do Import Wizard.
- Ajuda do Conversion Wizard.
- Ajuda de Backup & Restore.
- Ajuda de Plugin Permissions.

## Technical Hint

Texto de ajuda para áreas técnicas.

Exemplos:

- Path do executável `wit`.
- Código de erro de processo externo.
- Permissões do plugin.
- Diagnóstico de banco SQLite.

---

# Estrutura Visual

## Tooltip

```text
[Ícone/Botão]
     ↓
+----------------------------+
| Texto curto explicativo    |
+----------------------------+
```

## Help Hint

```text
Label do Campo   [?]
[Input]
Texto auxiliar opcional
```

---

# Regras de Conteúdo

Tooltips devem ser:

- curtos;
- objetivos;
- escritos como ação ou explicação direta;
- localizáveis;
- sem parágrafos longos;
- sem jargão desnecessário.

Help Hints podem ser mais explicativos, mas ainda devem ser claros e diretos.

---

# Exemplos de Texto

## Correto

```text
Atualizar biblioteca
```

```text
Selecione a pasta onde os arquivos convertidos serão salvos.
```

```text
Esta ação removerá o jogo da biblioteca, mas não excluirá o arquivo original.
```

## Evitar

```text
Clique aqui
```

```text
Botão usado para executar uma ação.
```

```text
Este campo é muito importante para o funcionamento do sistema.
```

---

# Quando Usar Tooltip

Usar Tooltip em:

- IconButtons.
- Ações de toolbar.
- Ações de menu com ícone.
- Badges com significado técnico.
- Status compactos.
- Campos avançados.
- Botões com atalhos.

---

# Quando Não Usar Tooltip

Não usar Tooltip quando:

- o texto já está claro;
- o botão já possui label completo;
- a informação é crítica e precisa estar visível;
- substituiria uma mensagem de erro;
- substituiria um label;
- tornaria a interface dependente do hover.

---

# Estados

Tooltips e Help Hints devem considerar:

- Default
- Hover
- Focus
- Disabled
- Error
- Warning
- Info
- Success

---

# Delay

Comportamento recomendado:

| Tipo | Delay |
|-----|------:|
| Tooltip de botão | 400 ms |
| Tooltip técnico | 500 ms |
| Tooltip de erro | imediato ao foco |
| Help Hint | sempre visível ou acionado por ícone |

---

# Posicionamento

Tooltips devem tentar abrir:

1. acima do elemento;
2. abaixo do elemento;
3. à direita;
4. à esquerda.

Nunca devem sair da área visível da janela.

---

# Tokens Utilizados

Tooltips e Help Hints devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Color.Info`
- `Color.Warning`
- `Color.Error`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Small`
- `Elevation.Tooltip`
- `Typography.Caption`
- `Icon.Info`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Tooltips.axaml
    ├── HelpHints.axaml
    ├── InlineHelp.axaml
    └── HelpTokens.axaml
```

---

# Integração com Localization

Todo texto deve usar chave de localização.

Exemplos:

```text
Tooltip.RefreshLibrary
Tooltip.OpenSettings
Help.WiimmsPath
Help.CacheDirectory
Help.MaxParallelTasks
```

Não usar textos fixos diretamente na View.

---

# Integração com Keyboard Shortcuts

Tooltips de comandos com atalho devem exibir o atalho.

Exemplo:

```text
Importar jogos (Ctrl+I)
```

---

# Integração com Accessibility

Tooltips não devem ser a única forma de transmitir informação essencial.

Quando a informação for crítica:

- usar texto visível;
- usar mensagem inline;
- usar estado visual;
- disponibilizar descrição acessível.

---

# Acessibilidade

Obrigatório:

- Tooltip acionável por foco de teclado.
- Conteúdo anunciado quando apropriado.
- Help Hint acessível por leitor de tela.
- Contraste WCAG AA.
- Não depender exclusivamente de hover.
- Não esconder informação crítica apenas em tooltip.
- Suporte a escala de fonte.
- Tempo suficiente para leitura.

---

# Regras de UX

- Tooltips devem explicar, não ensinar fluxos longos.
- Help Hints devem ficar próximos ao campo ou seção.
- Ícone de ajuda deve ser consistente.
- Não usar múltiplos tooltips competindo na mesma área.
- Evitar excesso de textos auxiliares.
- Mensagens de erro têm prioridade sobre Help Hint.
- Help Hint técnico deve ser claro e objetivo.
- Textos devem ser revisados para tom amigável.

---

# Integração com MVVM

Tooltips podem ser estáticos via localização ou dinâmicos via ViewModel quando dependem de estado.

Exemplo conceitual:

```csharp
public string ImportTooltip =>
    CanImport
        ? Localizer["Tooltip.Import"]
        : Localizer["Tooltip.ImportDisabledNoDevice"];
```

---

# Casos de Erro

- Tooltip não aparece.
- Tooltip aparece fora da janela.
- Texto não localizado.
- Tooltip sem acessibilidade.
- Tooltip conflitante com validação.
- Conteúdo longo demais.
- Tooltip de IconButton ausente.

---

# Regras para o Codex

- Sempre adicionar Tooltip em IconButton.
- Nunca usar Tooltip como substituto de Label.
- Nunca colocar informação crítica apenas em Tooltip.
- Sempre usar texto localizável.
- Sempre consumir tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre garantir funcionamento por teclado.
- Sempre revisar se o texto é curto e claro.
- Sempre usar Help Hint em campos avançados.
- Nunca usar estilos inline.

---

# Critérios de Aceite

- [x] Tipos oficiais definidos.
- [x] Tooltips padronizados.
- [x] Help Hints padronizados.
- [x] Inline Help padronizado.
- [x] Textos localizáveis.
- [x] Tooltip obrigatório em IconButton.
- [x] Compatível com Light/Dark.
- [x] Navegação por teclado funcional.
- [x] Contraste validado.
- [x] Sem estilos inline.
- [x] Sem informação crítica apenas em Tooltip.

---

# Testes

## Unitários

- Texto localizado.
- Tooltip de IconButton obrigatório.
- Tooltip com atalho.
- Tooltip dinâmico por estado.
- Help Hint associado a campo.

## Integração

- Toolbars.
- Menus.
- Forms.
- Settings.
- Import Wizard.
- Conversion Wizard.
- DataGrid.
- Alternância Light/Dark.
- Navegação por teclado.
- Redimensionamento da janela.

---

# Dependências

- F040-CommandPalette.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F047-InputsAndForms.md
- F052-ToolbarSystem.md
- F054-MenusAndContextMenus.md
- 022-Localization.md

---

# Definition of Done

- Tooltips documentados.
- Help Hints documentados.
- Regras de uso definidas.
- Acessibilidade validada.
- Localization prevista.
- Testes definidos.
- Documentação atualizada.

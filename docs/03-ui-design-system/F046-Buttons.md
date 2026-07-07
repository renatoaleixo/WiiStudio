# F046 - Buttons

> Feature ID: F046
> Status: Ready for Development
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o componente oficial de botões do WiiStudio, garantindo consistência visual,
acessibilidade, comportamento previsível e reutilização em toda a aplicação.

---

# Requisitos Funcionais

## RF001
Definir variantes oficiais de botões.

## RF002
Definir tamanhos padronizados.

## RF003
Definir estados visuais obrigatórios.

## RF004
Permitir uso com texto, ícone ou texto + ícone.

## RF005
Garantir compatibilidade com temas Light e Dark.

## RF006
Garantir acessibilidade por teclado e leitor de tela.

---

# Variantes Oficiais

## Primary

Utilizado para a ação principal da tela ou fluxo.

Exemplos:

- Importar
- Converter
- Salvar
- Continuar
- Confirmar

## Secondary

Utilizado para ações alternativas ou complementares.

Exemplos:

- Cancelar
- Voltar
- Ver Detalhes
- Escolher Pasta

## Tertiary

Utilizado para ações discretas sem grande destaque visual.

Exemplos:

- Limpar filtros
- Ver logs
- Abrir pasta

## Danger

Utilizado para ações destrutivas.

Exemplos:

- Remover jogo
- Limpar cache
- Restaurar padrão
- Cancelar todas as tarefas

## Success

Utilizado para ações positivas ou conclusivas.

Exemplos:

- Concluir
- Validar
- Aplicar

## IconButton

Utilizado para ações compactas com ícone.

Exemplos:

- Atualizar
- Favoritar
- Fechar
- Abrir menu

---

# Tamanhos

| Tamanho | Altura | Uso |
|---------|--------|-----|
| Small | 28 px | Toolbars, grids e ações compactas |
| Medium | 36 px | Uso padrão |
| Large | 44 px | Dialogs, wizards e CTAs |
| ExtraLarge | 52 px | Telas de boas-vindas e onboarding |

---

# Estados Visuais

Todo botão deve possuir os seguintes estados:

- Default
- Hover
- Pressed
- Focus
- Disabled
- Loading
- Success
- Error

---

# Composição

Um botão pode possuir:

- Texto
- Ícone à esquerda
- Ícone à direita
- Apenas ícone
- Indicador de loading
- Tooltip
- Atalho de teclado

---

# Tokens Utilizados

Os botões devem consumir tokens definidos no Design System:

- `Color.Primary`
- `Color.Secondary`
- `Color.Text.Primary`
- `Color.Text.Inverted`
- `Color.Border.Default`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Medium`
- `Typography.Body`
- `Icon.Default`

Nunca utilizar valores hardcoded de cor, margem, padding, fonte ou borda.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Buttons.axaml
    ├── ButtonTokens.axaml
    └── IconButtons.axaml
```

---

# Comportamento

## Click

O botão deve executar apenas uma ação clara.

## Loading

Quando uma ação assíncrona estiver em execução:

- desabilitar múltiplos cliques;
- exibir indicador visual;
- preservar largura do botão quando possível;
- permitir cancelamento apenas quando a ação suportar cancelamento.

## Disabled

O botão desabilitado deve:

- ser visualmente distinto;
- não receber foco;
- exibir tooltip explicativo quando necessário.

## Focus

Todo botão deve possuir indicação clara de foco para navegação por teclado.

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Foco visível.
- Texto acessível.
- Tooltip quando o botão possuir apenas ícone.
- Contraste WCAG AA.
- Área mínima clicável de 36 px.

---

# Regras de UX

- Uma tela deve possuir no máximo uma ação Primary principal.
- Ações destrutivas devem usar variante Danger.
- Ações destrutivas críticas exigem confirmação.
- Botões em dialogs devem seguir ordem consistente:
  - Cancelar à esquerda
  - Confirmar à direita
- Botões de toolbar devem preferir IconButton com tooltip.

---

# Integração com Commands

Todo botão deve se conectar a comandos da ViewModel.

Exemplo conceitual:

```csharp
public IAsyncRelayCommand ImportCommand { get; }
```

A View nunca deve conter lógica de negócio no evento de clique.

---

# Regras para o Codex

- Nunca criar botão com estilo inline.
- Nunca usar cor hardcoded.
- Nunca implementar lógica de negócio no code-behind.
- Sempre usar Command Binding.
- Sempre usar variante oficial.
- Sempre adicionar tooltip em IconButton.
- Sempre respeitar estados Disabled e Loading.
- Sempre garantir compatibilidade com Light/Dark.

---

# Casos de Erro

- Comando falha.
- Ação cancelada.
- Validação impede execução.
- Dependência indisponível.

O botão deve refletir o estado quando aplicável e delegar mensagens ao Notification Center.

---

# Critérios de Aceite

- [ ] Variantes oficiais implementadas.
- [ ] Tamanhos oficiais implementados.
- [ ] Estados visuais implementados.
- [ ] Compatível com Light/Dark.
- [ ] Navegação por teclado funcional.
- [ ] IconButton com tooltip.
- [ ] Sem estilos inline.
- [ ] Sem cores hardcoded.
- [ ] Integração com Command Binding.
- [ ] Loading state disponível.

---

# Testes

## Unitários

- Command executado corretamente.
- Command desabilitado impede execução.
- Loading state bloqueia múltiplos cliques.
- Tooltip obrigatório para IconButton.

## Integração

- Botões na MainWindow.
- Botões em dialogs.
- Botões em wizards.
- Botões em toolbars.
- Alternância Light/Dark.
- Navegação por teclado.

---

# Dependências

- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F025-NotificationCenter.md
- F031-MainWindow.md

---

# Definition of Done

- Componente Button padronizado.
- IconButton padronizado.
- Variantes implementadas.
- Estados implementados.
- Testes aprovados.
- Acessibilidade validada.
- Documentação atualizada.

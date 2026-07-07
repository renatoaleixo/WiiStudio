# F059 - Toasts & Inline Feedback

> Feature ID: F059
> Status: Ready for Development
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de Toasts, banners, mensagens inline e feedback leve do
WiiStudio, garantindo comunicação clara, consistente, acessível e não intrusiva
para ações concluídas, avisos, falhas recuperáveis, progresso breve e resultados
operacionais.

Toasts e Inline Feedback devem complementar o Notification Center sem substituir
dialogs, validações de formulário ou estados de erro permanentes.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de feedback visual.

## RF002
Padronizar Toasts de sucesso, erro, aviso e informação.

## RF003
Padronizar mensagens inline para formulários, painéis e ações contextuais.

## RF004
Integrar com Notification Center.

## RF005
Garantir compatibilidade com Light e Dark Theme.

## RF006
Garantir acessibilidade por teclado e leitor de tela.

## RF007
Garantir textos localizáveis.

## RF008
Definir regras para duração, empilhamento e dismiss.

---

# Tipos Oficiais

## Toast

Mensagem temporária exibida em área global da aplicação.

Exemplos:

- Jogo importado com sucesso.
- Backup criado.
- Metadados sincronizados.
- Conversão iniciada.
- Configurações salvas.

## Inline Feedback

Mensagem exibida próxima ao contexto.

Exemplos:

- Campo salvo automaticamente.
- Caminho inválido.
- Provider indisponível.
- Permissão negada.
- Validação de formulário.

## Banner

Mensagem persistente em área de destaque.

Exemplos:

- Dispositivo sem permissão.
- Ferramenta externa não configurada.
- Atualização disponível.
- Operação parcial com avisos.

## Snackbar

Mensagem temporária com ação curta.

Exemplos:

- Jogo removido. Desfazer.
- Filtro aplicado. Limpar.
- Backup restaurado. Abrir pasta.

---

# Severidades

| Tipo | Uso |
|------|-----|
| Info | Informação neutra |
| Success | Ação concluída |
| Warning | Atenção ou risco |
| Error | Falha recuperável |
| Critical | Falha importante que pode exigir Dialog |

---

# Toast

## Estrutura Visual

```text
+------------------------------------------+
| Ícone | Título                           |
|       | Mensagem curta                   |
|       | [Ação opcional]        [Fechar]  |
+------------------------------------------+
```

## Duração Recomendada

| Severidade | Duração |
|-----------|--------:|
| Success | 4 s |
| Info | 5 s |
| Warning | 7 s |
| Error | persistente até fechar ou 10 s |
| Critical | usar Dialog ou Banner persistente |

---

# Inline Feedback

## Estrutura Visual

```text
Campo / Área
Mensagem contextual abaixo ou ao lado
```

## Exemplos

```text
Caminho inválido ou sem permissão.
```

```text
Configurações salvas automaticamente.
```

```text
Este provider está temporariamente indisponível.
```

---

# Banner

## Estrutura Visual

```text
+-------------------------------------------------------+
| Ícone | Mensagem importante              [Ação] [x]   |
+-------------------------------------------------------+
```

## Uso Recomendado

Usar Banner quando:

- a mensagem afeta uma área inteira;
- o problema persiste;
- o usuário precisa executar uma ação;
- a mensagem não deve desaparecer automaticamente.

---

# Snackbar

Snackbar deve ser usado para feedback temporário com ação curta.

Exemplos:

```text
Jogo removido. [Desfazer]
```

```text
Filtro aplicado. [Limpar]
```

---

# Posicionamento

## Toasts

Posição recomendada:

- canto superior direito da MainWindow;
- empilhamento vertical;
- respeitar margens do Design System;
- não cobrir Dialogs.

## Inline Feedback

Próximo ao campo, componente ou área afetada.

## Banner

Topo da área afetada ou topo do Workspace.

---

# Empilhamento

Quando múltiplos Toasts forem exibidos:

- limitar quantidade visível;
- priorizar erro e warning;
- agrupar mensagens repetidas;
- mover histórico para Notification Center;
- evitar excesso de notificações.

---

# Ações

Toasts e Snackbars podem possuir ações curtas:

- Desfazer.
- Abrir.
- Ver detalhes.
- Copiar.
- Tentar novamente.

Ações destrutivas não devem ser executadas diretamente via Toast sem confirmação.

---

# Tokens Utilizados

Feedback visual deve consumir tokens do Design System:

- `Color.Info`
- `Color.Success`
- `Color.Warning`
- `Color.Error`
- `Color.Surface`
- `Color.Background`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Radius.Medium`
- `Elevation.Toast`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Toasts.axaml
    ├── Snackbars.axaml
    ├── Banners.axaml
    ├── InlineFeedback.axaml
    └── FeedbackTokens.axaml
```

---

# Modelo Conceitual

```csharp
public sealed class FeedbackMessage
{
    public string Id { get; init; } = string.Empty;
    public FeedbackSeverity Severity { get; init; }
    public string TitleKey { get; init; } = string.Empty;
    public string MessageKey { get; init; } = string.Empty;
    public string? ActionKey { get; init; }
    public ICommand? ActionCommand { get; init; }
    public bool IsPersistent { get; init; }
}
```

---

# Integração com Notification Center

Todo Toast relevante deve ser registrado no Notification Center, especialmente:

- erros;
- warnings;
- resultados de operações;
- falhas parciais;
- mensagens com detalhes técnicos.

Mensagens puramente transitórias podem não ser persistidas, conforme regra do Notification Center.

---

# Integração com Dialogs

Usar Dialog quando:

- há decisão obrigatória;
- há ação destrutiva;
- erro bloqueia continuidade;
- confirmação é necessária;
- detalhes técnicos precisam de leitura longa.

Usar Toast quando:

- ação foi concluída;
- informação é breve;
- erro é recuperável;
- usuário não precisa decidir imediatamente.

---

# Integração com Forms

Erros de validação devem ser Inline Feedback, não Toast.

Exemplos:

- campo obrigatório;
- valor inválido;
- caminho inexistente;
- permissão insuficiente.

---

# Integração com Task Manager

Operações longas devem mostrar progresso no Task Manager.

Toasts devem ser usados para:

- tarefa iniciada;
- tarefa concluída;
- tarefa falhou;
- tarefa cancelada;
- tarefa concluída parcialmente.

---

# Acessibilidade

Obrigatório:

- Toast deve ser anunciado por leitor de tela quando relevante.
- Toast não deve roubar foco sem necessidade.
- Ação do Toast deve ser acessível por teclado.
- Fechar deve ser acessível.
- Contraste WCAG AA.
- Mensagem não deve depender apenas de cor.
- Usuário deve ter tempo suficiente para ler.
- Mensagens críticas não devem desaparecer rapidamente.

---

# Regras de UX

- Não usar Toast para tudo.
- Não mostrar Toast para cada item em lote.
- Agrupar mensagens repetitivas.
- Usar Inline Feedback para erro contextual.
- Usar Banner para problema persistente.
- Usar Dialog para decisão obrigatória.
- Mensagens devem ser curtas.
- Título deve ser claro.
- Ação deve ser opcional e direta.
- Evitar excesso de animação.

---

# Regras para o Codex

- Sempre usar Notification Center para registrar feedback relevante.
- Nunca usar Toast para validação de campo.
- Nunca usar Toast para confirmação destrutiva.
- Sempre usar textos localizáveis.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre garantir acessibilidade.
- Sempre agrupar mensagens repetidas quando aplicável.
- Sempre usar Banner para problemas persistentes.
- Sempre usar Dialog para decisão obrigatória.
- Nunca bloquear a UI.

---

# Casos de Erro

- Toast duplicado.
- Mensagem sem localização.
- Toast cobre Dialog.
- Toast desaparece rápido demais.
- Toast crítico não persistente.
- Ação falha.
- Notification Center indisponível.
- Feedback exibido no contexto errado.

---

# Critérios de Aceite

- [ ] Toasts definidos.
- [ ] Inline Feedback definido.
- [ ] Banners definidos.
- [ ] Snackbars definidos.
- [ ] Severidades definidas.
- [ ] Duração e persistência documentadas.
- [ ] Integração com Notification Center definida.
- [ ] Integração com Forms definida.
- [ ] Integração com Dialogs definida.
- [ ] Compatível com Light/Dark.
- [ ] Acessibilidade validada.
- [ ] Textos localizáveis.
- [ ] Sem estilos inline.

---

# Testes

## Unitários

- Toast de sucesso.
- Toast de erro.
- Toast persistente.
- Agrupamento de mensagens repetidas.
- Ação executada.
- Ação falha.
- Mensagem localizada.
- Registro no Notification Center.

## Integração

- Importação concluída.
- Conversão falhou.
- Configuração salva.
- Validação inline em formulário.
- Banner de ferramenta não configurada.
- Snackbar com desfazer.
- Alternância Light/Dark.
- Navegação por teclado.
- Leitor de tela.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F047-InputsAndForms.md
- F048-DialogsAndModals.md
- F057-ProgressIndicators.md
- F058-EmptyErrorAndLoadingStates.md
- 022-Localization.md

---

# Definition of Done

- Toasts documentados.
- Inline Feedback documentado.
- Banners e Snackbars definidos.
- Integração com Notification Center especificada.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

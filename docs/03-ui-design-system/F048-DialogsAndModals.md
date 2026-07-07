# F048 - Dialogs & Modals

> Feature ID: F048
> Status: Ready for Development
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o padrão oficial de Dialogs, Modals, Popups e janelas auxiliares do WiiStudio,
garantindo consistência visual, clareza de fluxo, acessibilidade, comportamento
previsível e integração com MVVM, Theme Engine e Notification Center.

Dialogs e Modals devem ser usados para confirmar ações, coletar informações,
exibir detalhes contextuais, orientar fluxos críticos e proteger o usuário contra
ações destrutivas ou irreversíveis.

---

# Requisitos Funcionais

## RF001
Definir os tipos oficiais de dialogs e modals da aplicação.

## RF002
Padronizar estrutura visual, espaçamento, botões e estados.

## RF003
Garantir compatibilidade com Light e Dark Theme.

## RF004
Garantir acessibilidade por teclado e leitor de tela.

## RF005
Integrar com MVVM sem lógica de negócio no code-behind.

## RF006
Padronizar confirmação de ações destrutivas.

## RF007
Permitir dialogs assíncronos com loading e cancelamento.

## RF008
Padronizar mensagens de erro, alerta, sucesso e informação.

---

# Tipos Oficiais

## Information Dialog

Usado para apresentar informação simples.

Exemplos:

- Versão do WiiStudio.
- Resultado de verificação.
- Informação sobre formato de arquivo.
- Mensagem explicativa de uma funcionalidade.

## Confirmation Dialog

Usado para confirmar uma decisão do usuário.

Exemplos:

- Confirmar importação.
- Confirmar conversão.
- Confirmar alteração de configuração.
- Confirmar restauração de backup.

## Danger Confirmation Dialog

Usado para ações destrutivas ou irreversíveis.

Exemplos:

- Remover jogo.
- Limpar cache.
- Excluir backup.
- Cancelar todas as tarefas.

## Input Dialog

Usado para coletar uma informação curta.

Exemplos:

- Nome do perfil.
- Nome da lista.
- Caminho manual.
- Título personalizado.

## Form Dialog

Usado para formulários menores.

Exemplos:

- Editar perfil.
- Configurar provider.
- Adicionar caminho externo.
- Editar metadados básicos.

## Progress Dialog

Usado para operações longas.

Exemplos:

- Importando jogos.
- Convertendo arquivos.
- Verificando integridade.
- Restaurando backup.

## Error Dialog

Usado para falhas que exigem atenção imediata.

Exemplos:

- Falha ao acessar dispositivo.
- Falha ao executar Wiimms.
- Falha ao abrir banco SQLite.
- Erro crítico da aplicação.

## Advanced Details Dialog

Usado para apresentar detalhes técnicos.

Exemplos:

- Logs de diagnóstico.
- Saída do processo `wit`.
- Stack trace tratado.
- Relatório de integridade.

---

# Estrutura Visual Padrão

```text
+--------------------------------------------------+
| Ícone | Título                                   |
|--------------------------------------------------|
| Mensagem principal                               |
| Descrição complementar opcional                  |
|                                                  |
| Conteúdo opcional                                |
|                                                  |
|--------------------------------------------------|
| [Ação Secundária]              [Ação Primária]   |
+--------------------------------------------------+
```

---

# Anatomia

Todo dialog deve possuir:

- Ícone opcional.
- Título claro.
- Mensagem principal.
- Conteúdo opcional.
- Área de ações.
- Botão de fechamento quando aplicável.
- Foco inicial definido.
- Ordem de tabulação previsível.

---

# Tamanhos Oficiais

| Tamanho | Largura Recomendada | Uso |
|--------|---------------------|-----|
| Small | 360 px | Mensagens simples |
| Medium | 520 px | Confirmações e inputs |
| Large | 720 px | Formulários e detalhes |
| ExtraLarge | 960 px | Logs, diagnósticos e conteúdo técnico |

---

# Estados

Dialogs podem estar nos estados:

- Idle
- Opening
- Ready
- Validating
- Loading
- Completed
- Failed
- Closing
- Cancelled

---

# Tokens Utilizados

Dialogs devem consumir tokens do Design System:

- `Color.Background`
- `Color.Surface`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Error`
- `Color.Warning`
- `Color.Success`
- `Color.Info`
- `Spacing.MD`
- `Spacing.LG`
- `Spacing.XL`
- `Radius.Large`
- `Typography.Title`
- `Typography.Body`
- `Typography.Caption`
- `Elevation.Modal`

Nunca utilizar valores hardcoded de cor, margem, padding, fonte, sombra ou borda.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Dialogs.axaml
    ├── Modals.axaml
    ├── DialogTokens.axaml
    ├── MessageDialog.axaml
    ├── ConfirmationDialog.axaml
    ├── ProgressDialog.axaml
    └── ErrorDialog.axaml
```

---

# Serviço de Dialog

A aplicação deve usar uma abstração para exibição de dialogs.

## Interface Conceitual

```csharp
public interface IDialogService
{
    Task<DialogResult> ShowInformationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    Task<ConfirmationResult> ShowConfirmationAsync(
        ConfirmationDialogRequest request,
        CancellationToken cancellationToken = default);

    Task<ConfirmationResult> ShowDangerConfirmationAsync(
        DangerConfirmationDialogRequest request,
        CancellationToken cancellationToken = default);

    Task<T?> ShowFormAsync<T>(
        FormDialogRequest<T> request,
        CancellationToken cancellationToken = default);
}
```

---

# Resultado de Dialog

```csharp
public enum DialogResult
{
    None,
    Confirmed,
    Cancelled,
    Closed,
    Failed
}
```

---

# Regras de Confirmação

## Ações Simples

Podem usar Confirmation Dialog comum.

Exemplo:

```text
Deseja iniciar a conversão agora?
```

## Ações Destrutivas

Devem usar Danger Confirmation Dialog.

Exemplo:

```text
Remover este jogo da biblioteca?
```

## Ações Irreversíveis

Devem exigir confirmação explícita.

Exemplo:

```text
Digite REMOVER para confirmar.
```

---

# Progress Dialog

Progress Dialog deve suportar:

- título;
- operação atual;
- percentual;
- progresso indeterminado;
- botão cancelar quando suportado;
- log resumido opcional;
- tempo decorrido;
- estimativa de tempo quando possível.

---

# Error Dialog

Error Dialog deve suportar:

- mensagem amigável;
- detalhes técnicos expansíveis;
- ação recomendada;
- opção de copiar detalhes;
- opção de abrir logs quando aplicável;
- integração com Diagnostics.

---

# Modal vs Dialog

## Dialog

Usado para decisões rápidas e mensagens contextuais.

## Modal

Usado para fluxos que bloqueiam temporariamente a tela principal.

## Popup

Usado para ações leves e contextuais.

## Flyout

Usado para painéis rápidos sem interromper o fluxo.

---

# Acessibilidade

Obrigatório:

- Foco preso dentro do dialog enquanto aberto.
- ESC fecha apenas quando seguro.
- ENTER executa ação principal quando apropriado.
- Botão Cancelar acessível por teclado.
- Título anunciado por leitor de tela.
- Mensagens de erro associadas aos campos.
- Contraste WCAG AA.
- Foco visível.
- Ordem de tabulação previsível.

---

# Regras de UX

- O título deve ser curto e objetivo.
- O texto deve explicar claramente o impacto da ação.
- Evitar dialogs desnecessários.
- Não usar dialog para mensagens que podem ser notificações.
- Ações destrutivas devem ser visualmente destacadas.
- O botão principal deve refletir a ação real.
- O botão de cancelar deve preservar estado e dados do usuário.
- Dialogs não devem abrir outros dialogs sem necessidade.
- Progress Dialog deve impedir duplo processamento.
- Mensagens técnicas devem ficar em área expansível.

---

# Botões em Dialogs

Ordem recomendada:

```text
[Cancelar] [Confirmar]
```

Para ações destrutivas:

```text
[Cancelar] [Remover]
```

Para erro:

```text
[Copiar Detalhes] [Fechar]
```

Para progresso:

```text
[Cancelar]
```

---

# Integração com MVVM

Dialogs devem ser acionados pela ViewModel via serviço abstrato.

A ViewModel não deve conhecer a implementação visual do dialog.

Exemplo conceitual:

```csharp
public sealed partial class RemoveGameViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    public RemoveGameViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
}
```

---

# Integração com Notification Center

Usar Notification Center para mensagens não bloqueantes.

Usar Dialog quando:

- ação exige decisão;
- erro impede continuidade;
- dado precisa ser informado;
- operação longa precisa ser acompanhada;
- ação destrutiva precisa ser confirmada.

---

# Casos de Erro

- Falha ao abrir dialog.
- Operação cancelada.
- Validação de formulário falha.
- Serviço indisponível.
- Exceção durante ação confirmada.
- Cancelamento não suportado.
- Timeout de operação.

---

# Regras para o Codex

- Nunca exibir dialog diretamente pelo code-behind.
- Nunca usar lógica de negócio dentro da View do dialog.
- Sempre usar `IDialogService`.
- Sempre usar textos localizáveis.
- Sempre usar tokens do Design System.
- Sempre garantir navegação por teclado.
- Sempre usar Danger Dialog para ação destrutiva.
- Sempre disponibilizar detalhes técnicos expansíveis em erros críticos.
- Nunca bloquear thread da UI.
- Sempre usar `async/await` e `CancellationToken` em operações longas.
- Sempre integrar Progress Dialog com Task Manager quando aplicável.

---

# Critérios de Aceite

- [ ] Tipos oficiais de dialogs definidos.
- [ ] Estrutura visual padronizada.
- [ ] Serviço `IDialogService` definido.
- [ ] Confirmation Dialog implementado.
- [ ] Danger Confirmation Dialog implementado.
- [ ] Progress Dialog implementado.
- [ ] Error Dialog implementado.
- [ ] Compatível com Light/Dark.
- [ ] Navegação por teclado funcional.
- [ ] Foco preso corretamente no dialog.
- [ ] Textos localizáveis.
- [ ] Sem estilos inline.
- [ ] Sem lógica de negócio na View.

---

# Testes

## Unitários

- Resultado confirmado.
- Resultado cancelado.
- Resultado fechado.
- Danger confirmation exige confirmação.
- Progress Dialog recebe atualização.
- Cancelamento dispara `CancellationToken`.
- Error Dialog recebe detalhes técnicos.
- ViewModel chama `IDialogService`.

## Integração

- Dialog aberto pela MainWindow.
- Dialog em Import Wizard.
- Dialog em Conversion Wizard.
- Dialog em Remove Game.
- Dialog em Backup & Restore.
- Dialog em Settings.
- Alternância Light/Dark.
- Navegação por teclado.
- Fechamento por ESC quando permitido.
- Foco inicial correto.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F030-AdvancedDiagnostics.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F047-InputsAndForms.md
- 022-Localization.md

---

# Definition of Done

- Dialog System padronizado.
- Serviço `IDialogService` definido.
- Dialogs principais implementados.
- Estados de loading e erro suportados.
- Acessibilidade validada.
- Light/Dark validado.
- Testes aprovados.
- Documentação atualizada.

# F047 - Inputs & Forms

> Feature ID: F047
> Status: Ready for Development
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o padrão oficial de campos de entrada e formulários do WiiStudio,
garantindo consistência visual, validação clara, acessibilidade, reutilização
e integração completa com MVVM, FluentValidation e o Design System.

---

# Requisitos Funcionais

## RF001
Definir componentes oficiais de entrada de dados.

## RF002
Padronizar validação visual e mensagens de erro.

## RF003
Garantir compatibilidade com Light e Dark Theme.

## RF004
Garantir navegação por teclado.

## RF005
Integrar inputs com ViewModels e Validators.

## RF006
Padronizar layout de formulários simples, compostos e avançados.

---

# Componentes Oficiais

## TextBox

Uso para texto simples.

Exemplos:

- Nome do perfil
- Caminho de arquivo
- Pesquisa
- Título personalizado

## PasswordBox

Uso para dados sensíveis.

Exemplos:

- Chaves de API
- Tokens
- Senhas futuras

## ComboBox

Uso para seleção única.

Exemplos:

- Tema
- Idioma
- Formato de destino
- Perfil ativo

## CheckBox

Uso para opções booleanas.

Exemplos:

- Manter arquivos temporários
- Executar diagnóstico automático
- Habilitar backup automático

## RadioButton

Uso para opções mutuamente exclusivas.

Exemplos:

- Estratégia de conflito
- Modo de importação
- Política de atualização

## NumericInput

Uso para valores numéricos.

Exemplos:

- Limite de tarefas simultâneas
- Timeout
- Retenção de logs
- Limite de cache

## FilePicker

Uso para seleção de arquivos.

Exemplos:

- Caminho do wit
- Caminho do NKit
- Arquivo de backup
- Jogo para importação

## DirectoryPicker

Uso para seleção de diretórios.

Exemplos:

- Pasta de downloads
- Pasta de cache
- Pasta de exportação
- Pasta de biblioteca local

## SearchBox

Uso para pesquisa incremental.

Exemplos:

- Biblioteca
- Command Palette
- Logs
- Configurações

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Inputs.axaml
    ├── Forms.axaml
    ├── Validation.axaml
    ├── SearchBox.axaml
    ├── FilePicker.axaml
    └── DirectoryPicker.axaml
```

---

# Tokens Utilizados

Todos os inputs devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Border.Default`
- `Color.Border.Focus`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Error`
- `Color.Success`
- `Spacing.SM`
- `Spacing.MD`
- `Radius.Small`
- `Radius.Medium`
- `Typography.Body`
- `Typography.Caption`

Nunca utilizar valores hardcoded de cor, margem, padding, fonte ou borda.

---

# Estados Visuais

Todo input deve suportar:

- Default
- Hover
- Focus
- Filled
- Disabled
- ReadOnly
- Error
- Warning
- Success
- Loading

---

# Validação

## Obrigatório

Toda validação deve ser feita na camada apropriada usando Validators.

A ViewModel deve expor:

- valor atual;
- estado de validação;
- mensagem de erro;
- estado de carregamento quando aplicável.

## Mensagens

Mensagens de erro devem ser:

- claras;
- curtas;
- traduzíveis;
- associadas visualmente ao campo;
- acessíveis por leitor de tela.

---

# Layout de Formulários

## Formulário Simples

```text
Label
[Input]
Mensagem de ajuda ou erro
```

## Formulário em Duas Colunas

```text
Label            [Input]
Label            [Input]
Label            [Input]
```

## Formulário Avançado

```text
Seção
  Campo 1
  Campo 2

Seção
  Campo 3
  Campo 4
```

---

# Labels

Todo campo deve possuir label clara.

Exceção permitida:

- SearchBox com placeholder acessível;
- IconButton associado a input com tooltip;
- campos internos de componentes compostos.

---

# Placeholders

Placeholders devem ser usados apenas como exemplo, nunca como substituto do label.

Exemplo correto:

```text
Label: Pasta de Exportação
Placeholder: /home/usuario/WiiExports
```

---

# Help Text

Campos complexos devem ter texto auxiliar.

Exemplos:

- "Este diretório será usado para arquivos temporários."
- "Aumentar este valor pode consumir mais CPU."
- "A alteração será aplicada após reiniciar a aplicação."

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Ordem de tabulação previsível.
- Foco visível.
- Label associado ao input.
- Mensagem de erro acessível.
- Contraste WCAG AA.
- Tamanho mínimo de área interativa.
- Compatibilidade com leitores de tela.

---

# Regras de UX

- Agrupar campos relacionados.
- Evitar formulários muito longos sem seções.
- Validar o quanto antes, mas sem interromper digitação.
- Nunca limpar dados do usuário sem confirmação.
- Preservar valores quando houver erro.
- Destacar erros sem excesso visual.
- Preferir seleção guiada a digitação livre quando possível.

---

# Integração com MVVM

Inputs devem utilizar binding para propriedades da ViewModel.

Exemplo conceitual:

```csharp
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _downloadPath = string.Empty;
}
```

A View nunca deve conter lógica de validação no code-behind.

---

# Integração com FluentValidation

Validações complexas devem ser definidas em Validators.

Exemplo conceitual:

```csharp
public sealed class SettingsValidator : AbstractValidator<AppSettings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.DownloadPath)
            .NotEmpty()
            .WithMessage("A pasta de downloads é obrigatória.");
    }
}
```

---

# Integração com Notification Center

Erros de formulário devem aparecer próximos ao campo.

Erros globais ou falhas de salvamento devem ser enviados ao Notification Center.

---

# Casos de Erro

- Campo obrigatório não informado.
- Caminho inválido.
- Arquivo inexistente.
- Permissão insuficiente.
- Valor fora do intervalo.
- Formato inválido.
- Falha ao salvar.

---

# Regras para o Codex

- Nunca criar input com estilo inline.
- Nunca usar texto fixo sem localização.
- Nunca validar apenas na View.
- Sempre usar Binding.
- Sempre associar label ao campo.
- Sempre expor mensagens de erro acessíveis.
- Sempre utilizar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre respeitar o padrão de espaçamento oficial.
- Sempre criar Validator para formulários com regra de negócio.

---

# Critérios de Aceite

- [ ] Componentes de input padronizados.
- [ ] Formulários com layout consistente.
- [ ] Validação visual implementada.
- [ ] Mensagens de erro localizáveis.
- [ ] Acessibilidade validada.
- [ ] Integração com MVVM.
- [ ] Integração com FluentValidation.
- [ ] Compatível com Light/Dark.
- [ ] Sem estilos inline.
- [ ] Sem valores hardcoded.

---

# Testes

## Unitários

- Binding de propriedades.
- Validação de campos obrigatórios.
- Validação de intervalos.
- Mensagens de erro.
- Estados Disabled e ReadOnly.

## Integração

- Formulário de Settings.
- Import Wizard.
- Conversion Wizard.
- Backup & Restore.
- Profile Editor.
- Alternância Light/Dark.
- Navegação por teclado.

---

# Dependências

- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F045-SpacingAndLayout.md
- F023-SettingsManager.md
- F025-NotificationCenter.md
- 022-Localization.md

---

# Definition of Done

- Inputs padronizados.
- Formulários padronizados.
- Validação integrada.
- Acessibilidade validada.
- Testes aprovados.
- Documentação atualizada.

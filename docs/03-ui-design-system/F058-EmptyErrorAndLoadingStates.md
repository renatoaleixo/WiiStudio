# F058 - Empty, Error & Loading States

> Feature ID: F058
> Status: Ready for Development
> Priority: Critical
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de estados vazios, estados de erro e estados de carregamento
do WiiStudio, garantindo uma experiência consistente, clara, acessível e previsível
em todas as telas, componentes, dialogs, cards, grids, painéis e fluxos da aplicação.

Esses estados são essenciais para orientar o usuário quando não há dados, quando
uma operação está em andamento ou quando algo falha.

---

# Requisitos Funcionais

## RF001
Padronizar estados Empty, Error e Loading em toda a aplicação.

## RF002
Definir mensagens, ícones, ações e comportamento visual para cada estado.

## RF003
Garantir compatibilidade com Light e Dark Theme.

## RF004
Garantir acessibilidade por teclado e leitor de tela.

## RF005
Integrar com Notification Center, Dialogs, Task Manager e Diagnostics.

## RF006
Evitar telas em branco sem orientação.

## RF007
Permitir retry quando o erro for recuperável.

## RF008
Garantir textos localizáveis.

---

# Tipos de Estado

## Empty State

Usado quando não existem dados para exibir.

Exemplos:

- Biblioteca sem jogos.
- Nenhum dispositivo conectado.
- Nenhum backup criado.
- Nenhum plugin instalado.
- Nenhum log disponível.
- Nenhuma tarefa em execução.

## No Results State

Usado quando existem dados, mas filtros ou pesquisa não retornaram resultados.

Exemplos:

- Nenhum jogo encontrado para o filtro atual.
- Nenhum log encontrado com esse termo.
- Nenhuma tarefa com esse status.

## Loading State

Usado quando dados estão sendo carregados.

Exemplos:

- Carregando biblioteca.
- Detectando dispositivos.
- Consultando metadados.
- Abrindo banco local.
- Lendo logs.

## Error State

Usado quando houve falha ao carregar ou processar algo.

Exemplos:

- Falha ao acessar dispositivo.
- Falha ao carregar biblioteca.
- Erro ao executar Wiimms.
- Banco SQLite indisponível.
- Provider remoto indisponível.

## Partial Error State

Usado quando parte da operação funcionou e parte falhou.

Exemplos:

- 10 capas baixadas, 2 falharam.
- 8 jogos convertidos, 1 com erro.
- Biblioteca carregada com avisos.
- Alguns metadados não foram sincronizados.

---

# Estrutura Visual Padrão

## Empty State

```text
+------------------------------------------+
| Ícone                                    |
| Título curto                             |
| Mensagem explicativa                     |
| [Ação principal opcional]                |
+------------------------------------------+
```

## Error State

```text
+------------------------------------------+
| Ícone de erro                            |
| Título do erro                           |
| Mensagem amigável                        |
| [Tentar novamente] [Ver detalhes]        |
+------------------------------------------+
```

## Loading State

```text
+------------------------------------------+
| Indicador visual                         |
| Mensagem de carregamento                 |
+------------------------------------------+
```

---

# Mensagens Recomendadas

## Empty State

```text
Nenhum jogo encontrado na biblioteca.
Importe seu primeiro jogo para começar.
[Importar jogo]
```

```text
Nenhum dispositivo conectado.
Conecte um dispositivo USB para continuar.
[Atualizar dispositivos]
```

## No Results State

```text
Nenhum resultado encontrado.
Tente alterar os filtros ou limpar a pesquisa.
[Limpar filtros]
```

## Loading State

```text
Carregando biblioteca...
```

```text
Detectando dispositivos USB...
```

## Error State

```text
Não foi possível carregar a biblioteca.
Verifique os logs ou tente novamente.
[Tentar novamente] [Ver detalhes]
```

## Partial Error State

```text
Operação concluída com avisos.
Alguns itens não puderam ser processados.
[Ver detalhes]
```

---

# Componentes Afetados

Estados Empty, Error e Loading devem ser suportados em:

- MainWindow.
- Dashboard.
- LibraryView.
- DeviceManager.
- Import Wizard.
- Conversion Wizard.
- Task Manager.
- Logging Center.
- Advanced Diagnostics.
- Backup & Restore.
- Settings.
- Plugins.
- DataGrid.
- Cards.
- Panels.
- Dialogs.

---

# Estados por Tela

## Biblioteca

- Empty: nenhum jogo importado.
- Loading: carregando lista de jogos.
- Error: falha ao consultar banco.
- NoResults: filtros sem resultado.

## Dispositivos

- Empty: nenhum dispositivo conectado.
- Loading: detectando dispositivos.
- Error: falha de permissão ou leitura.
- Partial: dispositivo detectado com partição não suportada.

## Tarefas

- Empty: nenhuma tarefa ativa.
- Loading: carregando histórico.
- Error: falha ao consultar tarefas.
- Partial: tarefas carregadas com inconsistências.

## Diagnósticos

- Empty: nenhum diagnóstico executado.
- Loading: executando diagnóstico.
- Error: falha ao executar verificação.
- Partial: diagnóstico parcial.

---

# Tokens Utilizados

Os estados devem consumir tokens do Design System:

- `Color.Background`
- `Color.Surface`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Border.Default`
- `Color.Info`
- `Color.Warning`
- `Color.Error`
- `Color.Success`
- `Spacing.MD`
- `Spacing.LG`
- `Spacing.XL`
- `Radius.Medium`
- `Typography.Title`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Info`
- `Icon.Warning`
- `Icon.Error`
- `Icon.Empty`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── EmptyStates.axaml
    ├── ErrorStates.axaml
    ├── LoadingStates.axaml
    ├── StateContainers.axaml
    └── StateTokens.axaml
```

---

# Modelo Conceitual

```csharp
public enum ViewState
{
    Idle,
    Loading,
    Empty,
    NoResults,
    Ready,
    Error,
    PartialError
}
```

```csharp
public sealed class StateViewModel
{
    public ViewState State { get; init; }
    public string TitleKey { get; init; } = string.Empty;
    public string MessageKey { get; init; } = string.Empty;
    public string? IconKey { get; init; }
    public ICommand? PrimaryAction { get; init; }
    public ICommand? SecondaryAction { get; init; }
}
```

---

# Integração com MVVM

Telas devem expor estado por ViewModel.

Exemplo conceitual:

```csharp
public sealed partial class LibraryViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewState _state = ViewState.Loading;

    public IAsyncRelayCommand RetryCommand { get; }
}
```

A View deve apenas reagir ao estado, sem conter regra de negócio.

---

# Integração com Notification Center

Usar Notification Center para mensagens complementares.

Exemplos:

- erro recuperável;
- operação concluída com avisos;
- retry executado;
- falha parcial.

Estados visuais devem permanecer na tela quando a área não puder exibir dados.

---

# Integração com Diagnostics

Error State deve permitir acesso a detalhes técnicos quando aplicável.

Exemplos:

- abrir logs;
- copiar erro;
- abrir relatório de diagnóstico;
- executar diagnóstico rápido.

---

# Integração com Task Manager

Loading State de operações longas deve ser integrado ao Task Manager quando a
operação for assíncrona e acompanhável.

Exemplos:

- importação;
- conversão;
- download de capas;
- verificação de integridade;
- backup.

---

# Acessibilidade

Obrigatório:

- Mensagens legíveis.
- Texto acessível para ícones.
- Foco em ação principal quando apropriado.
- Contraste WCAG AA.
- Não depender apenas de cor.
- Loading com texto descritivo.
- Error com ação clara.
- Retry acessível por teclado.
- Estados anunciáveis por leitor de tela.

---

# Regras de UX

- Nunca deixar uma área vazia sem explicação.
- Empty State deve orientar o próximo passo.
- Error State deve explicar o problema em linguagem amigável.
- Error State técnico deve ter detalhes expansíveis.
- Loading State deve aparecer para operações perceptíveis.
- Skeleton deve ser usado quando melhorar percepção de performance.
- NoResults deve sugerir limpar filtros.
- Partial Error deve informar o que deu certo e o que falhou.
- Evitar mensagens genéricas como "Erro inesperado" sem orientação.

---

# Regras para o Codex

- Sempre implementar Loading, Empty e Error em telas com dados.
- Sempre usar textos localizáveis.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre oferecer retry quando erro for recuperável.
- Sempre evitar tela em branco.
- Sempre separar estado na ViewModel.
- Nunca colocar regra de estado no code-behind.
- Sempre integrar detalhes técnicos com Diagnostics quando aplicável.
- Sempre usar Notification Center para feedback complementar.

---

# Casos de Erro

- Estado não definido.
- Mensagem não localizada.
- Retry falha.
- Dados retornam vazios.
- Filtro sem resultado.
- Falha parcial.
- Loading infinito.
- Erro técnico sem detalhes.
- Ícone sem acessibilidade.

---

# Critérios de Aceite

- [ ] Empty State definido.
- [ ] NoResults State definido.
- [ ] Loading State definido.
- [ ] Error State definido.
- [ ] Partial Error State definido.
- [ ] Estados aplicáveis a DataGrid.
- [ ] Estados aplicáveis a Cards.
- [ ] Estados aplicáveis a Panels.
- [ ] Retry documentado.
- [ ] Textos localizáveis.
- [ ] Compatível com Light/Dark.
- [ ] Acessibilidade validada.
- [ ] Sem telas em branco.
- [ ] Sem estilos inline.

---

# Testes

## Unitários

- Estado Loading.
- Estado Empty.
- Estado NoResults.
- Estado Error.
- Retry Command.
- Falha parcial.
- Texto localizado.
- Estado desconhecido tratado.

## Integração

- LibraryView vazia.
- LibraryView com filtro sem resultado.
- DeviceManager sem dispositivo.
- TaskManager sem tarefas.
- LoggingCenter sem logs.
- Error em carregamento de banco.
- Retry com sucesso.
- Retry com falha.
- Alternância Light/Dark.
- Navegação por teclado.

---

# Dependências

- F025-NotificationCenter.md
- F026-TaskManager.md
- F030-AdvancedDiagnostics.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F048-DialogsAndModals.md
- F049-DataGrid.md
- F050-CardsAndContainers.md
- F057-ProgressIndicators.md
- 022-Localization.md

---

# Definition of Done

- Estados Empty, NoResults, Loading, Error e Partial Error documentados.
- Componentes afetados definidos.
- Integrações especificadas.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

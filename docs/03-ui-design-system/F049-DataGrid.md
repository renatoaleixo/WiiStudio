# F049 - DataGrid

> Feature ID: F049
> Status: Ready for Development
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o padrão oficial de tabelas, grades e listagens estruturadas do WiiStudio,
garantindo consistência visual, performance, acessibilidade, ordenação, filtros,
seleção, ações em lote e integração completa com MVVM, Design System, Task Manager
e Notification Center.

O DataGrid será um dos componentes centrais da aplicação, utilizado principalmente
para exibir biblioteca de jogos, dispositivos USB, tarefas, logs, diagnósticos,
backups, resultados de importação e relatórios técnicos.

---

# Requisitos Funcionais

## RF001
Definir o componente oficial para exibição tabular de dados.

## RF002
Padronizar colunas, cabeçalhos, linhas, seleção e ações.

## RF003
Suportar ordenação por coluna.

## RF004
Suportar filtros e pesquisa.

## RF005
Suportar seleção única e múltipla.

## RF006
Suportar ações em linha e ações em lote.

## RF007
Suportar estados de loading, vazio, erro e sem resultado.

## RF008
Garantir performance para grandes volumes de dados.

## RF009
Garantir compatibilidade com Light e Dark Theme.

## RF010
Garantir acessibilidade por teclado e leitor de tela.

---

# Cenários de Uso

O DataGrid será utilizado em:

- Biblioteca de jogos.
- Lista de jogos no dispositivo USB/WBFS.
- Resultado de importação.
- Resultado de conversão.
- Histórico de tarefas.
- Logs da aplicação.
- Diagnóstico avançado.
- Backups disponíveis.
- Plugins instalados.
- Providers de metadados.
- Arquivos duplicados.
- Relatórios de integridade.

---

# Estrutura Visual

```text
+---------------------------------------------------------------------+
| Toolbar: [Pesquisar] [Filtros] [Ações em lote] [Atualizar]          |
+---------------------------------------------------------------------+
| Coluna 1       | Coluna 2       | Coluna 3       | Ações            |
+---------------------------------------------------------------------+
| Linha 1        | Valor          | Valor          | [...]            |
| Linha 2        | Valor          | Valor          | [...]            |
| Linha 3        | Valor          | Valor          | [...]            |
+---------------------------------------------------------------------+
| Paginação / Total / Status                                          |
+---------------------------------------------------------------------+
```

---

# Anatomia

Um DataGrid oficial deve possuir:

- Toolbar opcional.
- Campo de pesquisa opcional.
- Área de filtros opcional.
- Cabeçalho de colunas.
- Linhas de dados.
- Coluna de ações opcional.
- Seleção opcional.
- Rodapé opcional.
- Paginação opcional.
- Estado vazio.
- Estado de loading.
- Estado de erro.
- Estado sem resultados.

---

# Tipos de DataGrid

## SimpleGrid

Usado para listas pequenas e simples.

Exemplos:

- Configurações simples.
- Lista de perfis.
- Lista de providers.

## LibraryGrid

Usado para a biblioteca principal de jogos.

Exemplos:

- Título.
- Código do jogo.
- Região.
- Plataforma.
- Formato.
- Tamanho.
- Status.
- Favorito.
- Ações.

## TaskGrid

Usado para tarefas em execução ou histórico.

Exemplos:

- Nome da tarefa.
- Tipo.
- Progresso.
- Status.
- Tempo decorrido.
- Ações.

## LogGrid

Usado para logs e diagnóstico.

Exemplos:

- Data/hora.
- Nível.
- Origem.
- Mensagem.
- Correlação.
- Ações.

## DeviceGrid

Usado para dispositivos e partições.

Exemplos:

- Nome.
- Tipo.
- Sistema de arquivos.
- Capacidade.
- Espaço livre.
- Status.
- Ações.

---

# Colunas

## Coluna de Texto

Utilizada para valores simples.

Exemplos:

- Nome do jogo.
- Região.
- Formato.
- Status.

## Coluna Numérica

Utilizada para números, tamanhos e quantidades.

Exemplos:

- Tamanho.
- Progresso.
- Quantidade de arquivos.
- Tempo.

## Coluna de Status

Utilizada para badges e indicadores.

Exemplos:

- Importado.
- Convertido.
- Com erro.
- Pendente.
- Em execução.

## Coluna de Ícone

Utilizada para indicação visual curta.

Exemplos:

- Favorito.
- Integridade verificada.
- Backup disponível.
- Metadados completos.

## Coluna de Ações

Utilizada para ações contextuais da linha.

Exemplos:

- Detalhes.
- Converter.
- Exportar.
- Remover.
- Abrir pasta.

---

# Estados Visuais

Todo DataGrid deve suportar:

- Loading
- Empty
- NoResults
- Ready
- Error
- Refreshing
- Selecting
- BulkActionRunning

---

# Estado Loading

Utilizar quando os dados ainda estão sendo carregados.

Deve exibir:

- Indicador visual.
- Mensagem curta.
- Opcionalmente skeleton rows.

Exemplo:

```text
Carregando biblioteca...
```

---

# Estado Empty

Utilizar quando não há dados cadastrados.

Exemplo:

```text
Nenhum jogo encontrado na biblioteca.
```

Deve oferecer ação quando aplicável:

```text
[Importar primeiro jogo]
```

---

# Estado NoResults

Utilizar quando os filtros não retornam dados.

Exemplo:

```text
Nenhum resultado encontrado para os filtros atuais.
```

Deve oferecer ação:

```text
[Limpar filtros]
```

---

# Estado Error

Utilizar quando houve falha ao carregar os dados.

Deve exibir:

- mensagem amigável;
- ação para tentar novamente;
- acesso a detalhes técnicos quando aplicável.

Exemplo:

```text
Não foi possível carregar a biblioteca.
[Tentar novamente] [Ver detalhes]
```

---

# Toolbar

A toolbar do DataGrid pode conter:

- SearchBox.
- Filtros.
- Ordenação avançada.
- Refresh.
- Exportação.
- Ações em lote.
- Seleção rápida.
- Contador de itens.

---

# Pesquisa

A pesquisa deve suportar:

- busca incremental;
- debounce;
- limpeza rápida;
- integração com filtros;
- estado sem resultados;
- cancelamento de busca assíncrona.

Exemplo conceitual:

```csharp
public IAsyncRelayCommand SearchCommand { get; }
```

---

# Filtros

Filtros comuns:

- Região.
- Formato.
- Status.
- Plataforma.
- Favorito.
- Tamanho.
- Data de importação.
- Integridade.
- Provider de metadados.

Filtros devem ser visíveis, removíveis e persistíveis quando aplicável.

---

# Ordenação

A ordenação deve suportar:

- coluna única;
- direção ascendente;
- direção descendente;
- estado sem ordenação;
- indicação visual no cabeçalho;
- persistência opcional por perfil.

---

# Seleção

## Seleção Única

Usada para grids simples ou detalhes de item.

## Seleção Múltipla

Usada para ações em lote.

Exemplos:

- Converter vários jogos.
- Exportar vários jogos.
- Remover vários itens.
- Verificar integridade em lote.

---

# Ações em Linha

Ações em linha devem ser contextuais e discretas.

Exemplos:

- Ver detalhes.
- Converter.
- Exportar.
- Remover.
- Favoritar.
- Abrir pasta.

Quando houver muitas ações, usar menu contextual.

---

# Ações em Lote

Ações em lote devem aparecer apenas quando houver itens selecionados.

Exemplos:

```text
3 itens selecionados
[Converter] [Exportar] [Remover] [Limpar seleção]
```

Ações destrutivas em lote exigem confirmação.

---

# Paginação e Virtualização

Para grandes volumes, o DataGrid deve suportar:

- virtualização de linhas;
- paginação opcional;
- carregamento incremental;
- cancelamento de carregamento;
- indicador de total;
- performance aceitável para milhares de registros.

---

# Performance

Obrigatório considerar:

- virtualização;
- bindings leves;
- evitar conversores pesados;
- evitar lógica de negócio na View;
- evitar recomputar colunas a cada atualização;
- usar observable collections com cuidado;
- processar filtros na Application Layer quando necessário.

---

# Tokens Utilizados

O DataGrid deve consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Default`
- `Color.Border.Subtle`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Text.Disabled`
- `Color.Primary`
- `Color.Warning`
- `Color.Error`
- `Color.Success`
- `Spacing.XS`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Radius.Small`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded de cor, margem, padding, fonte ou borda.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── DataGrid.axaml
    ├── DataGridTokens.axaml
    ├── DataGridColumns.axaml
    ├── DataGridStates.axaml
    └── DataGridToolbar.axaml
```

---

# Integração com MVVM

O DataGrid deve consumir ViewModels específicos para listagem.

Exemplo conceitual:

```csharp
public sealed partial class LibraryViewModel : ObservableObject
{
    public ObservableCollection<GameListItemViewModel> Games { get; } = new();

    [ObservableProperty]
    private GameListItemViewModel? _selectedGame;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
}
```

A View não deve conter regra de filtro, ordenação ou negócio.

---

# Integração com Application Layer

Filtros e ordenações complexas devem ser tratados pela Application Layer.

Exemplo conceitual:

```csharp
public sealed record GameLibraryQuery(
    string? SearchTerm,
    string? Region,
    string? Format,
    bool? IsFavorite,
    int Page,
    int PageSize);
```

---

# Integração com Task Manager

Ações longas iniciadas a partir do DataGrid devem ser encaminhadas ao Task Manager.

Exemplos:

- Conversão em lote.
- Importação em lote.
- Verificação de integridade.
- Download de capas.
- Exportação.

---

# Integração com Notification Center

O DataGrid deve usar Notification Center para:

- falha ao carregar dados;
- ação concluída;
- ação parcial;
- erro em lote;
- confirmação de cancelamento;
- resultado de exportação.

---

# Acessibilidade

Obrigatório:

- Navegação por teclado.
- Foco visível.
- Leitura de cabeçalhos.
- Seleção acessível.
- Ordem de tabulação previsível.
- Indicação textual para status importantes.
- Tooltip em ações apenas com ícone.
- Contraste WCAG AA.
- Suporte a escalonamento de fonte.
- Não depender apenas de cor para status.

---

# Regras de UX

- Cabeçalhos devem ser curtos e claros.
- Colunas importantes devem ficar à esquerda.
- Ações devem ficar à direita.
- Status deve usar texto + indicador visual.
- Não sobrecarregar a tabela com muitas colunas.
- Permitir ocultar colunas quando aplicável.
- Estados vazio e erro devem orientar o usuário.
- Filtros aplicados devem ser visíveis.
- Ações destrutivas devem exigir confirmação.
- Seleção múltipla deve ser clara.

---

# Regras para o Codex

- Nunca criar grid com estilo inline.
- Nunca colocar lógica de negócio na View.
- Nunca filtrar grandes listas diretamente na View.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre usar ViewModel para estado.
- Sempre usar Commands para ações.
- Sempre usar tooltip em ações por ícone.
- Sempre tratar Empty, Loading e Error.
- Sempre usar confirmação para ação destrutiva.
- Sempre considerar virtualização em listas grandes.
- Sempre integrar ações longas ao Task Manager.

---

# Casos de Erro

- Falha ao carregar dados.
- Banco indisponível.
- Dispositivo removido durante leitura.
- Filtro inválido.
- Falha ao executar ação em lote.
- Cancelamento de operação.
- Timeout.
- Dados inconsistentes.

---

# Critérios de Aceite

- [ ] DataGrid padronizado.
- [ ] Toolbar padronizada.
- [ ] Estados Loading, Empty, NoResults e Error implementados.
- [ ] Ordenação suportada.
- [ ] Pesquisa suportada.
- [ ] Filtros suportados.
- [ ] Seleção única suportada.
- [ ] Seleção múltipla suportada.
- [ ] Ações em linha suportadas.
- [ ] Ações em lote suportadas.
- [ ] Compatível com Light/Dark.
- [ ] Navegação por teclado funcional.
- [ ] Acessibilidade validada.
- [ ] Sem estilos inline.
- [ ] Sem lógica de negócio na View.
- [ ] Performance adequada para grandes volumes.

---

# Testes

## Unitários

- Query de listagem.
- Filtro por texto.
- Filtro por status.
- Ordenação ascendente.
- Ordenação descendente.
- Seleção única.
- Seleção múltipla.
- Ações em lote.
- Estado Empty.
- Estado Error.

## Integração

- LibraryView.
- DeviceManager.
- TaskManager.
- LoggingCenter.
- AdvancedDiagnostics.
- BackupAndRestore.
- Alternância Light/Dark.
- Navegação por teclado.
- Virtualização com grande volume.
- Ações destrutivas com confirmação.

---

# Dependências

- F015-SearchAndFilter.md
- F016-DuplicateDetection.md
- F025-NotificationCenter.md
- F026-TaskManager.md
- F029-LoggingCenter.md
- F030-AdvancedDiagnostics.md
- F032-LibraryView.md
- F035-DeviceManager.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F047-InputsAndForms.md
- F048-DialogsAndModals.md
- 006-ApplicationLayer.md
- 008-UI-UX.md
- 022-Localization.md

---

# Definition of Done

- DataGrid padronizado.
- Estados visuais implementados.
- Pesquisa, filtros e ordenação documentados.
- Seleção e ações em lote documentadas.
- Acessibilidade validada.
- Performance considerada.
- Light/Dark validado.
- Testes aprovados.
- Documentação atualizada.

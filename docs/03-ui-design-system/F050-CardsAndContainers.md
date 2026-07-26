# F050 - Cards & Containers

> Feature ID: F050
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o padrão oficial de Cards, Containers, Panels e áreas estruturais do WiiStudio,
garantindo consistência visual, organização das informações, reutilização,
responsividade, acessibilidade e integração completa com o Design System.

Cards e Containers serão utilizados para organizar conteúdos visuais, métricas,
atalhos, detalhes de jogos, tarefas, dispositivos, configurações, diagnósticos e
áreas de composição da interface.

---

# Requisitos Funcionais

## RF001
Definir os componentes oficiais de containerização visual.

## RF002
Padronizar Cards, Panels, Sections e Content Blocks.

## RF003
Garantir compatibilidade com Light e Dark Theme.

## RF004
Garantir uso consistente de espaçamentos, bordas, sombras e tipografia.

## RF005
Permitir composição reutilizável em telas, dashboards, dialogs e wizards.

## RF006
Suportar estados visuais de loading, empty, error, selected e disabled.

## RF007
Garantir acessibilidade e navegação por teclado quando o container for interativo.

## RF008
Eliminar estilos inline e duplicação visual.

---

# Componentes Oficiais

## Card

Componente visual para agrupar conteúdo relacionado.

Exemplos:

- Card de jogo.
- Card de tarefa.
- Card de estatística.
- Card de dispositivo.
- Card de backup.
- Card de provider.

## Container

Estrutura base para organizar blocos maiores de layout.

Exemplos:

- Área principal.
- Área lateral.
- Área de conteúdo.
- Área de configuração.
- Área de detalhes.

## Panel

Área visual com comportamento estrutural.

Exemplos:

- Painel de detalhes.
- Painel de filtros.
- Painel de logs.
- Painel de ações rápidas.
- Painel de diagnóstico.

## Section

Agrupamento semântico dentro de uma tela ou formulário.

Exemplos:

- Seção de metadados.
- Seção de caminhos.
- Seção de preferências.
- Seção de dispositivos.
- Seção de tarefas.

## Tile

Card compacto para dashboard ou navegação rápida.

Exemplos:

- Total de jogos.
- Jogos favoritos.
- Espaço usado.
- Tarefas em execução.
- Último backup.

## EmptyStateContainer

Container para ausência de dados.

Exemplos:

- Biblioteca vazia.
- Nenhum backup encontrado.
- Nenhum dispositivo conectado.
- Nenhum log disponível.

## ErrorStateContainer

Container para falhas de carregamento ou erro contextual.

Exemplos:

- Falha ao carregar biblioteca.
- Falha ao acessar dispositivo.
- Falha ao consultar provider.
- Falha ao abrir banco.

---

# Estrutura Visual Padrão

## Card

```text
+------------------------------------------+
| Header                                   |
|------------------------------------------|
| Content                                  |
|                                          |
|------------------------------------------|
| Footer / Actions                         |
+------------------------------------------+
```

## Panel

```text
+------------------------------------------+
| Título                         [Ações]   |
|------------------------------------------|
| Conteúdo                                  |
|                                          |
+------------------------------------------+
```

## Section

```text
Título da Seção
Descrição opcional

[Conteúdo]
```

---

# Anatomia do Card

Um Card pode possuir:

- Header.
- Ícone.
- Título.
- Subtítulo.
- Imagem ou capa.
- Conteúdo principal.
- Badges.
- Status.
- Ações rápidas.
- Footer.
- Menu contextual.
- Estado visual.

---

# Tipos de Cards

## GameCard

Exibe informações resumidas de um jogo.

Conteúdo sugerido:

- Capa.
- Título.
- Código do jogo.
- Região.
- Formato.
- Tamanho.
- Status de integridade.
- Favorito.
- Ações rápidas.

## TaskCard

Exibe uma tarefa em execução ou concluída.

Conteúdo sugerido:

- Tipo da tarefa.
- Status.
- Progresso.
- Tempo decorrido.
- Mensagem atual.
- Ação de cancelar quando suportada.

## DeviceCard

Exibe um dispositivo detectado.

Conteúdo sugerido:

- Nome do dispositivo.
- Sistema de arquivos.
- Capacidade.
- Espaço livre.
- Status.
- Ações disponíveis.

## MetricCard

Exibe indicadores no Dashboard.

Conteúdo sugerido:

- Ícone.
- Valor principal.
- Descrição.
- Variação.
- Status.

## SettingsCard

Agrupa configurações relacionadas.

Conteúdo sugerido:

- Título.
- Descrição.
- Campos.
- Ações.
- Status de salvamento.

## DiagnosticCard

Exibe informações técnicas ou de saúde do sistema.

Conteúdo sugerido:

- Nome do diagnóstico.
- Resultado.
- Severidade.
- Detalhes.
- Ação recomendada.

---

# Tamanhos Oficiais

| Tamanho | Uso |
|--------|-----|
| Small | Tiles, métricas e atalhos |
| Medium | Cards padrão |
| Large | Cards com imagem ou conteúdo detalhado |
| FullWidth | Seções e containers principais |

---

# Estados Visuais

Cards e Containers devem suportar:

- Default
- Hover
- Focus
- Selected
- Disabled
- Loading
- Empty
- Error
- Warning
- Success

---

# Estado Loading

Utilizado quando o conteúdo ainda está sendo carregado.

Pode exibir:

- Skeleton.
- Spinner.
- Mensagem curta.
- Placeholder visual.

Exemplo:

```text
Carregando detalhes do jogo...
```

---

# Estado Empty

Utilizado quando não há conteúdo para exibir.

Exemplo:

```text
Nenhum backup encontrado.
```

Deve oferecer ação quando aplicável:

```text
[Criar backup agora]
```

---

# Estado Error

Utilizado quando houve falha contextual.

Exemplo:

```text
Não foi possível carregar este painel.
[Tentar novamente]
```

---

# Tokens Utilizados

Cards e Containers devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Border.Default`
- `Color.Border.Subtle`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Error`
- `Color.Warning`
- `Color.Success`
- `Color.Info`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Spacing.XL`
- `Radius.Medium`
- `Radius.Large`
- `Elevation.Card`
- `Elevation.Panel`
- `Typography.Title`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded de cor, margem, padding, fonte, borda ou sombra.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Cards.axaml
    ├── Containers.axaml
    ├── Panels.axaml
    ├── Sections.axaml
    ├── Tiles.axaml
    ├── StateContainers.axaml
    └── CardTokens.axaml
```

---

# Regras de Layout

## Padding

Valores recomendados:

- Small Card: `Spacing.SM`
- Medium Card: `Spacing.MD`
- Large Card: `Spacing.LG`
- Panel: `Spacing.LG`
- Section: `Spacing.XL`

## Gap Interno

Valores recomendados:

- Conteúdo compacto: `Spacing.SM`
- Conteúdo padrão: `Spacing.MD`
- Conteúdo complexo: `Spacing.LG`

## Border Radius

Valores recomendados:

- Cards: `Radius.Medium`
- Panels: `Radius.Large`
- Tiles: `Radius.Medium`

---

# Cards Interativos

Um Card interativo deve:

- possuir foco visível;
- ter estado hover;
- suportar navegação por teclado;
- possuir ação clara;
- não depender apenas de cor;
- evitar múltiplas ações concorrentes sem hierarquia.

Exemplos:

- abrir detalhes do jogo;
- selecionar dispositivo;
- abrir tarefa;
- acessar configuração.

---

# Cards Não Interativos

Um Card não interativo deve ser usado apenas como agrupamento visual.

Exemplos:

- resumo de estatística;
- bloco de descrição;
- seção de metadados;
- conteúdo informativo.

---

# Ações em Cards

Ações podem aparecer como:

- botão primário;
- botão secundário;
- icon button;
- menu contextual;
- link interno;
- ação de footer.

Ações destrutivas devem ser confirmadas via Dialog.

---

# Containers de Estado

## EmptyStateContainer

Deve conter:

- ícone;
- título;
- mensagem;
- ação opcional;
- link para ajuda quando aplicável.

## ErrorStateContainer

Deve conter:

- ícone de erro;
- mensagem amigável;
- ação de retry;
- opção de detalhes técnicos quando aplicável.

## LoadingStateContainer

Deve conter:

- indicador visual;
- mensagem;
- placeholder quando aplicável.

---

# Responsividade

Cards e Containers devem suportar:

- layout em coluna única;
- layout em grid;
- quebra de linha;
- largura fluida;
- altura mínima;
- overflow controlado;
- adaptação para telas menores.

---

# Integração com Dashboard

MetricCards e Tiles devem ser utilizados no Dashboard.

Exemplos:

- Total de jogos.
- Total de favoritos.
- Espaço utilizado.
- Dispositivos conectados.
- Tarefas em execução.
- Último backup.
- Erros recentes.

---

# Integração com Game Details

GameCard e Panels devem ser utilizados para detalhes de jogos.

Exemplos:

- capa;
- metadados;
- arquivos;
- ações;
- integridade;
- histórico;
- assets.

---

# Integração com Task Manager

TaskCard deve refletir:

- progresso;
- status;
- mensagens;
- cancelamento;
- falha;
- conclusão;
- detalhes.

---

# Integração com Notification Center

Cards podem disparar notificações quando:

- uma ação é concluída;
- uma ação falha;
- uma tarefa é iniciada;
- um retry é executado;
- um item é removido;
- uma operação parcial ocorre.

---

# Acessibilidade

Obrigatório:

- Contraste WCAG AA.
- Foco visível em Cards interativos.
- Ordem de tabulação previsível.
- Texto alternativo para imagens quando aplicável.
- Não depender apenas de cor para status.
- Tooltip para ações apenas com ícone.
- Área mínima clicável.
- Compatibilidade com escala de fonte.

---

# Regras de UX

- Cards devem agrupar apenas informações relacionadas.
- Evitar excesso de ações no mesmo Card.
- A ação principal deve ser visualmente clara.
- Badges devem ser curtos e consistentes.
- Cards de mesmo tipo devem ter mesma estrutura.
- Containers não devem ser usados apenas para decoração.
- Estados vazio e erro devem orientar o usuário.
- Evitar sombras exageradas.
- Evitar bordas e backgrounds conflitantes.
- Priorizar legibilidade.

---

# Integração com MVVM

Cards e Containers devem consumir ViewModels.

Exemplo conceitual:

```csharp
public sealed partial class GameCardViewModel : ObservableObject
{
    public string Title { get; init; } = string.Empty;
    public string GameCode { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string CoverPath { get; init; } = string.Empty;
    public bool IsFavorite { get; init; }

    public IRelayCommand OpenDetailsCommand { get; init; }
}
```

A View não deve conter lógica de negócio.

---

# Regras para o Codex

- Nunca criar Card com estilo inline.
- Nunca usar cor hardcoded.
- Nunca usar margem ou padding hardcoded.
- Sempre consumir tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre separar View e ViewModel.
- Sempre usar Commands para ações.
- Sempre considerar estados Loading, Empty e Error.
- Sempre garantir foco visível em Cards interativos.
- Sempre usar tooltip em ações por ícone.
- Sempre confirmar ações destrutivas.
- Sempre manter consistência entre Cards do mesmo tipo.

---

# Casos de Erro

- Falha ao carregar dados do Card.
- Falha ao carregar imagem.
- Falha ao executar ação.
- Dados incompletos.
- Estado inconsistente.
- Operação cancelada.
- Permissão insuficiente.
- Dispositivo desconectado.

---

# Critérios de Aceite

- [x] Card padrão definido.
- [x] Container padrão definido.
- [x] Panel padrão definido.
- [x] Section padrão definida.
- [x] Tile padrão definido.
- [x] EmptyStateContainer definido.
- [x] ErrorStateContainer definido.
- [x] LoadingStateContainer definido.
- [x] Estados visuais implementados.
- [x] Compatível com Light/Dark.
- [x] Responsividade considerada.
- [x] Acessibilidade validada.
- [x] Sem estilos inline.
- [x] Sem cores hardcoded.
- [x] Sem lógica de negócio na View.

---

# Testes

## Unitários

- ViewModel de Card.
- Estado Loading.
- Estado Empty.
- Estado Error.
- Comando principal.
- Ação desabilitada.
- Badge de status.
- Dados incompletos.

## Integração

- Dashboard.
- LibraryView.
- GameDetails.
- DeviceManager.
- TaskManager.
- BackupAndRestore.
- Settings.
- AdvancedDiagnostics.
- Alternância Light/Dark.
- Navegação por teclado.
- Responsividade.

---

# Dependências

- F013-GameDetails.md
- F025-NotificationCenter.md
- F026-TaskManager.md
- F031-MainWindow.md
- F032-LibraryView.md
- F036-Dashboard.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F046-Buttons.md
- F048-DialogsAndModals.md
- 008-UI-UX.md
- 021-ThemeSystem.md
- 022-Localization.md

---

# Definition of Done

- Cards padronizados.
- Containers padronizados.
- Panels e Sections documentados.
- Estados Loading, Empty e Error definidos.
- Responsividade considerada.
- Acessibilidade validada.
- Light/Dark validado.
- Testes aprovados.
- Documentação atualizada.

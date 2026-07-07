# F056 - Badges & Status Indicators

> Feature ID: F056
> Status: Ready for Development
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de Badges, Chips, Labels, Pills e Status Indicators do WiiStudio, garantindo comunicação visual consistente para estados, categorias, severidades, progresso, formatos, regiões, tarefas e resultados operacionais.

Badges e indicadores devem ajudar o usuário a entender rapidamente o estado de jogos, tarefas, dispositivos, metadados, backups, plugins e diagnósticos.

---

# Requisitos Funcionais

## RF001
Definir tipos oficiais de Badges e Status Indicators.

## RF002
Padronizar cores, ícones, tamanhos e textos.

## RF003
Suportar severidades e estados de domínio.

## RF004
Garantir compatibilidade com Light e Dark Theme.

## RF005
Garantir acessibilidade e não depender apenas de cor.

## RF006
Integrar com Design System, Localization e DataGrid.

## RF007
Permitir uso em Cards, Tables, Toolbars, Dialogs e Panels.

## RF008
Definir regras para status técnicos e operacionais.

---

# Tipos Oficiais

## Badge

Indicador compacto de categoria ou estado.

Exemplos:

- WBFS
- ISO
- RVZ
- NKit
- USA
- EUR
- JAP

## Status Badge

Indica status operacional.

Exemplos:

- Importado
- Convertendo
- Verificado
- Com erro
- Pendente
- Cancelado

## Severity Badge

Indica severidade de mensagem ou diagnóstico.

Exemplos:

- Info
- Warning
- Error
- Critical
- Success

## Count Badge

Exibe contadores compactos.

Exemplos:

- tarefas pendentes;
- notificações;
- plugins com atualização;
- erros recentes.

## Chip

Elemento removível ou selecionável.

Exemplos:

- filtros aplicados;
- tags de gênero;
- regiões;
- formatos selecionados.

## Status Dot

Indicador visual mínimo.

Exemplos:

- online/offline;
- ativo/inativo;
- conectado/desconectado;
- tarefa em execução.

---

# Estrutura Visual

## Badge

```text
[ WBFS ]
```

## Status Badge

```text
[✓ Verificado]
```

## Chip

```text
[Região: USA  x]
```

## Status Dot

```text
● Conectado
```

---

# Categorias de Status

## Jogos

- Disponível
- Importado
- Convertido
- Duplicado
- Inválido
- Incompleto
- Verificado
- Com erro

## Tarefas

- Pendente
- Em execução
- Pausado
- Concluído
- Cancelado
- Falhou
- Parcial

## Dispositivos

- Conectado
- Desconectado
- Montado
- Somente leitura
- Sem permissão
- Não suportado
- Em análise

## Metadados

- Completo
- Parcial
- Ausente
- Desatualizado
- Sincronizado
- Falha na sincronização

## Plugins

- Ativo
- Inativo
- Bloqueado
- Sem permissão
- Atualização disponível
- Incompatível

## Diagnósticos

- OK
- Atenção
- Erro
- Crítico
- Ignorado

---

# Severidades Oficiais

| Severidade | Uso |
|-----------|-----|
| Info | Informação neutra |
| Success | Resultado positivo |
| Warning | Atenção ou risco moderado |
| Error | Falha recuperável |
| Critical | Falha grave ou bloqueante |
| Neutral | Informação categórica |

---

# Tamanhos Oficiais

| Tamanho | Uso |
|--------|-----|
| Small | DataGrid, listas compactas |
| Medium | Cards e Panels |
| Large | Estados importantes e detalhes |

---

# Estados Visuais

Badges e indicadores devem suportar:

- Default
- Hover
- Focus
- Selected
- Disabled
- Loading
- Error
- Warning
- Success

---

# Tokens Utilizados

Badges devem consumir tokens do Design System:

- `Color.Info`
- `Color.Success`
- `Color.Warning`
- `Color.Error`
- `Color.Text.Primary`
- `Color.Text.Inverted`
- `Color.Border.Default`
- `Color.Surface`
- `Spacing.XS`
- `Spacing.SM`
- `Radius.Pill`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Badges.axaml
    ├── Chips.axaml
    ├── StatusIndicators.axaml
    ├── SeverityBadges.axaml
    └── BadgeTokens.axaml
```

---

# Regras de Texto

Textos devem ser curtos e localizáveis.

Exemplos:

```text
Status.Imported
Status.Converting
Status.Failed
Severity.Warning
Format.WBFS
Region.USA
```

Evitar textos longos dentro de Badges.

---

# Ícones

Ícones podem ser usados para reforçar significado, mas nunca como único indicador.

Exemplos:

- ✓ para sucesso;
- ! para alerta;
- x para erro;
- ⟳ para em execução;
- ● para conectado.

O significado textual deve continuar disponível.

---

# Uso em DataGrid

Badges devem ser utilizados em colunas de status, formato, região e severidade.

Exemplos:

```text
Título                Formato     Região     Status
Mario Kart Wii        [WBFS]      [USA]      [✓ Verificado]
```

---

# Uso em Cards

Cards podem usar Badges para:

- formato do jogo;
- região;
- favorito;
- status de integridade;
- status da tarefa;
- severidade de diagnóstico.

---

# Uso em Filters

Chips removíveis devem representar filtros aplicados.

Exemplo:

```text
[Formato: WBFS x] [Região: USA x] [Favoritos x]
```

---

# Acessibilidade

Obrigatório:

- Não depender apenas de cor.
- Texto legível.
- Contraste WCAG AA.
- Tooltip quando o significado for técnico.
- Leitor de tela deve anunciar o status.
- Ícones devem ter descrição acessível.
- Badges interativos devem ter foco visível.
- Chips removíveis devem ser acionáveis por teclado.

---

# Regras de UX

- Usar Badges com moderação.
- Status crítico deve ser visualmente claro.
- Evitar muitas cores diferentes na mesma área.
- Manter nomenclatura consistente.
- Não misturar categoria e status no mesmo badge quando isso causar ambiguidade.
- Usar Chips para filtros removíveis.
- Usar Status Dot apenas quando o texto próximo explicar o significado.
- Badges técnicos devem ter tooltip.

---

# Integração com MVVM

Status deve ser derivado do estado do ViewModel ou DTO.

Exemplo conceitual:

```csharp
public sealed class GameListItemViewModel
{
    public string FormatBadgeKey { get; init; } = "Format.WBFS";
    public string RegionBadgeKey { get; init; } = "Region.USA";
    public GameStatus Status { get; init; }
}
```

---

# Enums Recomendados

```csharp
public enum BadgeSeverity
{
    Neutral,
    Info,
    Success,
    Warning,
    Error,
    Critical
}
```

```csharp
public enum OperationStatus
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed,
    Partial
}
```

---

# Regras para o Codex

- Nunca usar cor como único indicador.
- Nunca usar Badge com texto fixo sem localização.
- Sempre usar tokens do Design System.
- Sempre suportar Light/Dark.
- Sempre usar severidades oficiais.
- Sempre usar tooltip para status técnico.
- Sempre garantir contraste.
- Sempre manter badges curtos.
- Sempre preferir enum para estados de domínio.
- Nunca criar variante visual sem documentar.

---

# Casos de Erro

- Status desconhecido.
- Severidade inválida.
- Texto não localizado.
- Badge com contraste insuficiente.
- Badge sem descrição acessível.
- Chip removível sem comando.
- Estado técnico sem tooltip.

---

# Critérios de Aceite

- [ ] Tipos oficiais definidos.
- [ ] Severidades oficiais definidas.
- [ ] Status por domínio documentados.
- [ ] Badges compatíveis com Light/Dark.
- [ ] Chips removíveis definidos.
- [ ] Status Dot definido.
- [ ] Uso em DataGrid documentado.
- [ ] Uso em Cards documentado.
- [ ] Textos localizáveis.
- [ ] Acessibilidade validada.
- [ ] Sem cores hardcoded.
- [ ] Não depende apenas de cor.

---

# Testes

## Unitários

- Mapeamento de status para severidade.
- Texto localizado.
- Status desconhecido tratado.
- Chip remove filtro.
- Badge técnico possui tooltip.
- Badge interativo possui comando.

## Integração

- DataGrid.
- LibraryView.
- TaskManager.
- DeviceManager.
- Diagnostics.
- Plugins.
- Filters.
- Cards.
- Alternância Light/Dark.
- Navegação por teclado.

---

# Dependências

- F013-GameDetails.md
- F025-NotificationCenter.md
- F026-TaskManager.md
- F030-AdvancedDiagnostics.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F049-DataGrid.md
- F050-CardsAndContainers.md
- 022-Localization.md

---

# Definition of Done

- Badges oficiais documentados.
- Status Indicators definidos.
- Severidades definidas.
- Chips definidos.
- Uso em DataGrid e Cards documentado.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

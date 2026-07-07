# F053 - Tabs System

> Feature ID: F053
> Status: Ready for Development
> Priority: High
> Epic: Advanced UI Components

# Objetivo

Definir o padrão oficial de abas do WiiStudio, incluindo tabs horizontais,
tabs verticais, tabs internas, estados visuais, navegação por teclado,
persistência de seleção e integração com MVVM, Design System e Application Shell.

Tabs devem ser utilizadas para organizar conteúdo relacionado dentro de uma mesma
área, evitando telas longas e mantendo o contexto do usuário.

---

# Requisitos Funcionais

## RF001
Definir os tipos oficiais de Tabs.

## RF002
Padronizar estados visuais e comportamento de seleção.

## RF003
Suportar navegação por teclado.

## RF004
Suportar tabs com ícones, badges e status.

## RF005
Permitir persistência opcional da tab ativa.

## RF006
Integrar com ViewModels e Commands.

## RF007
Garantir compatibilidade com Light e Dark Theme.

## RF008
Garantir acessibilidade por teclado e leitor de tela.

---

# Tipos Oficiais de Tabs

## Primary Tabs

Tabs principais dentro de uma tela.

Exemplos:

- Detalhes do jogo.
- Configurações.
- Diagnósticos.
- Plugins.

## Secondary Tabs

Tabs internas dentro de uma seção.

Exemplos:

- Metadados / Assets / Arquivos.
- Geral / Avançado / Segurança.
- Logs / Métricas / Eventos.

## Vertical Tabs

Tabs laterais para seções numerosas.

Exemplos:

- Settings.
- Advanced Diagnostics.
- Plugin Configuration.

## Compact Tabs

Tabs pequenas para áreas limitadas.

Exemplos:

- Painéis laterais.
- Cards avançados.
- Dialogs grandes.

---

# Estrutura Visual

```text
+--------------------------------------------------+
| [Tab 1] [Tab 2] [Tab 3] [Tab 4]                 |
+--------------------------------------------------+
| Conteúdo da tab ativa                            |
|                                                  |
+--------------------------------------------------+
```

---

# Anatomia

Uma Tab pode possuir:

- Título.
- Ícone opcional.
- Badge opcional.
- Indicador de status.
- Tooltip.
- Estado ativo.
- Estado disabled.
- Conteúdo associado.

---

# Estados Visuais

Cada Tab deve suportar:

- Default
- Hover
- Focus
- Active
- Disabled
- Loading
- Error
- Warning
- Success

---

# Uso Recomendado

Tabs devem ser usadas quando:

- conteúdos pertencem ao mesmo contexto;
- usuário precisa alternar entre seções relacionadas;
- dividir a tela melhora a compreensão;
- a navegação não representa mudança principal de área.

Tabs não devem substituir a navegação principal.

---

# Exemplos no WiiStudio

## Game Details

```text
[Resumo] [Arquivos] [Metadados] [Assets] [Integridade] [Histórico]
```

## Settings

```text
[Geral] [Aparência] [Ferramentas] [Plugins] [Backups] [Avançado]
```

## Diagnostics

```text
[Visão Geral] [Logs] [Processos] [Banco] [Dispositivos]
```

## Plugins

```text
[Instalados] [Disponíveis] [Permissões] [Configurações]
```

---

# Persistência

A aplicação pode persistir a última tab ativa por contexto.

Exemplos:

- última aba aberta em Settings;
- última aba aberta em Diagnostics;
- última aba de Game Details não precisa necessariamente persistir.

Persistência deve respeitar perfil do usuário quando aplicável.

---

# Tokens Utilizados

Tabs devem consumir tokens do Design System:

- `Color.Surface`
- `Color.Background`
- `Color.Primary`
- `Color.Border.Default`
- `Color.Text.Primary`
- `Color.Text.Secondary`
- `Color.Warning`
- `Color.Error`
- `Color.Success`
- `Spacing.SM`
- `Spacing.MD`
- `Spacing.LG`
- `Radius.Small`
- `Typography.Body`
- `Typography.Caption`
- `Icon.Default`

Nunca utilizar valores hardcoded.

---

# Estrutura Recomendada

```text
styles/
└── components/
    ├── Tabs.axaml
    ├── TabTokens.axaml
    ├── HorizontalTabs.axaml
    ├── VerticalTabs.axaml
    └── CompactTabs.axaml
```

---

# Integração com MVVM

Tabs devem ser controladas por ViewModel.

Exemplo conceitual:

```csharp
public sealed partial class TabHostViewModel : ObservableObject
{
    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

    [ObservableProperty]
    private TabItemViewModel? _selectedTab;
}
```

Conteúdo da tab deve ser carregado de forma preguiçosa quando necessário.

---

# Lazy Loading

Tabs com conteúdo pesado devem suportar lazy loading.

Exemplos:

- logs extensos;
- diagnóstico avançado;
- lista de arquivos;
- assets remotos;
- plugins disponíveis.

---

# Acessibilidade

Obrigatório:

- Navegação com teclado.
- Setas devem alternar entre tabs.
- Enter ou Space ativa a tab.
- Foco visível.
- Tab ativa anunciada.
- Badge/status com texto acessível.
- Contraste WCAG AA.
- Ordem de tabulação previsível.

---

# Regras de UX

- Evitar mais de 7 tabs horizontais.
- Usar Vertical Tabs quando houver muitas seções.
- Títulos devem ser curtos.
- Tabs devem manter o contexto atual.
- Não usar tabs para fluxos sequenciais; usar Wizard.
- Tabs com erro devem indicar status.
- Tabs disabled devem explicar motivo via tooltip.
- Conteúdo não deve mudar inesperadamente.

---

# Regras para o Codex

- Nunca criar Tab com estilo inline.
- Nunca usar string fixa sem localização.
- Sempre usar tokens do Design System.
- Sempre usar ViewModel para estado selecionado.
- Sempre suportar navegação por teclado.
- Sempre suportar Light/Dark.
- Sempre considerar lazy loading para conteúdo pesado.
- Sempre usar tooltip quando houver status ou disabled.
- Nunca usar Tabs para substituir Navigation System.
- Nunca colocar lógica de negócio na View.

---

# Casos de Erro

- Tab não encontrada.
- Conteúdo falha ao carregar.
- ViewModel não registrada.
- Tab disabled acionada.
- Lazy loading falha.
- Estado persistido inválido.
- Plugin registra tab inválida.

---

# Critérios de Aceite

- [ ] Tipos oficiais de Tabs definidos.
- [ ] Estados visuais documentados.
- [ ] Navegação por teclado definida.
- [ ] Persistência opcional definida.
- [ ] Lazy loading previsto.
- [ ] Compatível com Light/Dark.
- [ ] Textos localizáveis.
- [ ] Sem estilos inline.
- [ ] Sem lógica de negócio na View.
- [ ] Acessibilidade validada.

---

# Testes

## Unitários

- Seleção de tab.
- Tab disabled não ativa.
- Estado ativo atualizado.
- Persistência de tab ativa.
- Lazy loading executado uma vez.
- Falha de carregamento tratada.

## Integração

- Tabs em GameDetails.
- Tabs em Settings.
- Tabs em Diagnostics.
- Tabs em Plugins.
- Navegação por teclado.
- Alternância Light/Dark.
- Tabs com badge.
- Tabs com erro.

---

# Dependências

- F013-GameDetails.md
- F023-SettingsManager.md
- F030-AdvancedDiagnostics.md
- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md
- F044-Iconography.md
- F045-SpacingAndLayout.md
- F051-NavigationSystem.md
- 022-Localization.md

---

# Definition of Done

- Tabs System documentado.
- Tipos oficiais definidos.
- Estados visuais definidos.
- Navegação por teclado definida.
- Persistência opcional documentada.
- Lazy loading previsto.
- Acessibilidade validada.
- Testes definidos.
- Documentação atualizada.

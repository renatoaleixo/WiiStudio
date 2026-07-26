# F094 - UI Testing Strategy

> Feature ID: F094
> Status: Implemented and Homologated
> Priority: High
> Epic: Testing & Quality

# Objetivo

Definir a estratégia oficial de testes de UI do WiiStudio, cobrindo ViewModels,
componentes Avalonia, navegação, dialogs, wizards, DataGrid, temas, responsividade,
acessibilidade básica e fluxos críticos da interface.

A estratégia deve priorizar testes confiáveis, rápidos e sustentáveis, evitando
testes frágeis baseados em detalhes visuais desnecessários.

---

# Requisitos Funcionais

## RF001
Definir níveis de testes de UI.

## RF002
Separar testes de ViewModel de testes visuais.

## RF003
Testar navegação principal.

## RF004
Testar dialogs e modals.

## RF005
Testar wizards críticos.

## RF006
Testar estados Empty, Loading e Error.

## RF007
Testar Light/Dark Theme.

## RF008
Testar responsividade básica.

## RF009
Testar acessibilidade por teclado.

## RF010
Evitar testes frágeis por layout exato.

---

# Níveis de Teste

## ViewModel Tests

Testam lógica de apresentação sem abrir janela.

Cobrem:

- commands;
- loading;
- error;
- seleção;
- validação;
- navegação;
- notificações.

## Component Tests

Testam componentes isolados.

Cobrem:

- botões;
- inputs;
- dialogs;
- cards;
- badges;
- progress;
- tabs.

## Shell Tests

Testam estrutura principal.

Cobrem:

- App Shell;
- Sidebar;
- Header;
- Workspace;
- Status Bar;
- Navigation.

## Flow Tests

Testam fluxos principais.

Cobrem:

- importar jogo;
- converter jogo;
- abrir detalhes;
- executar diagnóstico;
- abrir configurações.

---

# Ferramentas

Usar conforme viabilidade:

- testes de ViewModel com xUnit;
- utilitários de teste do Avalonia;
- fakes para serviços;
- snapshots controlados quando úteis;
- testes manuais documentados para cenários visuais complexos.

---

# ViewModel First

A maior parte da lógica de UI deve estar em ViewModels testáveis.

Exemplo:

```csharp
[Fact]
public async Task ImportCommand_ShouldShowError_WhenFileIsInvalid()
{
    // Arrange
    // Act
    // Assert
}
```

A View deve conter o mínimo de lógica.

---

# Navegação

Testar:

- Dashboard abre;
- Library abre;
- Settings abre;
- rota inválida mostra erro;
- GoBack;
- Sidebar atualiza item ativo;
- Header atualiza título;
- Workspace troca ViewModel.

---

# Dialogs

Testar:

- abertura;
- fechamento;
- ação principal;
- cancelamento;
- foco inicial;
- confirmação destrutiva;
- retorno de resultado;
- erro de validação.

---

# Wizards

Testar:

- etapa inicial;
- avançar;
- voltar;
- validação por etapa;
- cancelar;
- conclusão;
- erro;
- progresso.

Wizards importantes:

- Import Wizard;
- Conversion Wizard;
- Backup Restore.

---

# DataGrid

Testar:

- carregamento de dados;
- seleção;
- ordenação;
- filtros;
- estado vazio;
- erro;
- ações de linha;
- keyboard navigation básica.

---

# Temas

Testar:

- troca Light/Dark;
- recursos carregados;
- componentes principais renderizam;
- tokens obrigatórios resolvem;
- não há recursos ausentes.

Não testar cor exata salvo tokens críticos.

---

# Responsividade

Testar breakpoints:

- Compact;
- Standard;
- Wide.

Validar:

- Sidebar recolhe;
- Toolbar usa overflow;
- Right Panel vira overlay;
- Dialog cabe na janela;
- Workspace não fica em branco.

---

# Acessibilidade

Testar:

- foco visível;
- navegação por Tab;
- ESC fecha Dialog quando seguro;
- Enter executa ação principal;
- IconButton possui tooltip/label;
- campos possuem labels;
- erro associado ao campo.

---

# Estados de Tela

Toda tela com dados deve testar:

- Loading;
- Empty;
- Ready;
- Error;
- NoResults quando aplicável.

---

# O que Evitar

Evitar:

- testar pixels exatos;
- snapshots grandes demais;
- sleeps;
- depender de timing de animação;
- depender de ordem visual frágil;
- abrir ferramenta externa real;
- depender de dados reais.

---

# Testes Manuais Documentados

Para cenários difíceis de automatizar inicialmente:

- DPI scaling;
- múltiplos monitores;
- screen reader real;
- temas em monitores diferentes;
- comportamento nativo de janela.

Esses testes devem ter checklist.

---

# Regras para o Codex

- Sempre testar ViewModel antes de testar View.
- Sempre testar estados de UI.
- Sempre usar fakes para serviços.
- Sempre evitar sleep.
- Sempre testar teclado em dialogs e menus.
- Sempre testar Light/Dark em componente novo.
- Sempre testar erro e loading.
- Nunca colocar regra de negócio na View para facilitar teste.
- Nunca depender de pixel perfeito.
- Nunca usar dados reais do usuário.

---

# Casos de Erro

- Teste frágil por animação.
- Teste depende de resolução local.
- ViewModel não testável.
- Command sem teste.
- Dialog não retorna foco.
- Tema com recurso ausente.
- Estado Error não renderiza.
- Foco perdido após navegação.

---

# Critérios de Aceite

- [x] Níveis de UI test definidos.
- [x] ViewModel tests priorizados.
- [x] Navigation tests definidos.
- [x] Dialog tests definidos.
- [x] Wizard tests definidos.
- [x] DataGrid tests definidos.
- [x] Theme tests definidos.
- [x] Responsive tests definidos.
- [x] Accessibility tests definidos.
- [x] Testes frágeis evitados.

---

# Definition of Done

- UI Testing Strategy documentada.
- Testes por componente definidos.
- Fluxos críticos mapeados.
- Acessibilidade básica considerada.
- Documentação atualizada.

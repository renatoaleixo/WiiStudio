# F076 - Plugin UI Extensions

> Feature ID: F076
> Status: Ready for Development
> Priority: High
> Epic: Plugin SDK

# Objetivo

Definir o modelo oficial para extensões de interface criadas por plugins no WiiStudio,
permitindo que plugins adicionem elementos visuais controlados em regiões permitidas,
sem comprometer estabilidade, acessibilidade, performance, segurança e consistência
do Design System.

---

# Requisitos Funcionais

## RF001
Definir regiões permitidas para UI Extensions.

## RF002
Definir contrato para registro de extensões visuais.

## RF003
Garantir que extensões respeitem Design System.

## RF004
Garantir compatibilidade com Light e Dark Theme.

## RF005
Garantir acessibilidade.

## RF006
Controlar permissões de UI.

## RF007
Impedir que plugin substitua UI core sem autorização.

## RF008
Permitir desativação de extensão problemática.

## RF009
Registrar diagnósticos de UI Extensions.

## RF010
Evitar bloqueio da thread da UI.

---

# Regiões Permitidas

## Dashboard Cards

Plugins podem adicionar cards controlados ao Dashboard.

## Game Details Tabs

Plugins podem adicionar tabs em detalhes do jogo.

## Settings Sections

Plugins podem adicionar seções de configuração próprias.

## Diagnostics Panels

Plugins podem adicionar painéis de diagnóstico.

## Toolbar Actions

Plugins podem adicionar ações controladas em toolbars específicas.

## Context Menu Actions

Plugins podem adicionar ações contextuais em itens permitidos.

---

# Regiões Proibidas

Plugins não devem substituir:

- App Shell principal;
- Login/segurança futura;
- Dialog Host;
- Notification Host;
- Navigation Core;
- Theme Engine;
- serviços internos;
- Status Bar global sem contrato específico.

---

# Contrato Base

```csharp
public interface IUiExtensionPlugin : IWiiStudioPlugin
{
    Task RegisterUiExtensionsAsync(
        IUiExtensionRegistry registry,
        CancellationToken cancellationToken = default);
}
```

---

# UI Extension Registry

```csharp
public interface IUiExtensionRegistry
{
    void RegisterDashboardCard(DashboardCardExtension extension);
    void RegisterGameDetailsTab(GameDetailsTabExtension extension);
    void RegisterSettingsSection(SettingsSectionExtension extension);
    void RegisterDiagnosticsPanel(DiagnosticsPanelExtension extension);
    void RegisterToolbarAction(ToolbarActionExtension extension);
    void RegisterContextMenuAction(ContextMenuActionExtension extension);
}
```

---

# Modelo de Extensão

```csharp
public abstract class UiExtension
{
    public string PluginId { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string? IconKey { get; init; }
    public int Order { get; init; }
    public bool IsEnabled { get; init; } = true;
}
```

---

# Dashboard Card Extension

Usada para adicionar cards ao Dashboard.

Exemplos:

- status de provider externo;
- métrica de plugin;
- tarefa customizada;
- integração externa.

---

# Game Details Tab Extension

Usada para adicionar tabs em detalhes de jogo.

Exemplos:

- informações de compatibilidade;
- dados externos;
- assets customizados;
- notas do usuário.

---

# Settings Section Extension

Usada para configurações do próprio plugin.

Regras:

- seção deve ser isolada;
- campos devem usar padrões de Forms;
- validação deve ser clara;
- settings devem ser armazenadas em storage do plugin.

---

# Diagnostics Panel Extension

Usada para diagnóstico técnico.

Exemplos:

- status de API externa;
- validação de configuração;
- teste de conexão;
- relatório customizado.

---

# Toolbar Action Extension

Permite ação visual em toolbar específica.

Regras:

- ação deve ser contextual;
- comando deve ser seguro;
- ação destrutiva exige confirmação;
- tooltip obrigatório.

---

# Context Menu Action Extension

Permite adicionar ação em menu contextual.

Exemplos:

- ação sobre jogo;
- ação sobre asset;
- ação sobre tarefa;
- ação sobre log.

---

# Permissão Necessária

Toda UI Extension exige permissão:

```text
ui-extension
```

Extensões com ações sensíveis podem exigir permissões adicionais.

---

# Design System

Extensões visuais devem usar:

- tokens de cor;
- tokens de tipografia;
- tokens de espaçamento;
- componentes oficiais;
- estados oficiais;
- Light/Dark;
- acessibilidade.

Plugins não devem usar estilos inline que quebrem a identidade visual.

---

# Theme Engine

UI Extensions devem reagir a mudança de tema.

Regras:

- não fixar cor;
- não carregar tema próprio sobre o core;
- usar recursos fornecidos pelo contexto;
- validar Light/Dark.

---

# Localization

Textos devem ser localizáveis.

Plugins podem fornecer recursos próprios, mas devem usar sistema oficial.

---

# Acessibilidade

Obrigatório:

- foco visível;
- navegação por teclado;
- descrição acessível em ícones;
- contraste adequado;
- não depender apenas de cor;
- não prender foco indevidamente;
- compatibilidade com leitores de tela.

---

# Performance

UI Extensions devem:

- carregar sob demanda quando possível;
- evitar bloqueio da UI;
- evitar operações pesadas em binding;
- usar async/await;
- cancelar operações quando removidas;
- liberar recursos no unload.

---

# Falha Segura

Se uma UI Extension falhar:

- remover apenas a extensão problemática;
- registrar diagnóstico;
- não derrubar a tela inteira;
- notificar quando relevante;
- permitir desativar plugin.

---

# Regras para o Codex

- Nunca permitir UI Extension sem permissão.
- Sempre registrar extensão em região permitida.
- Sempre validar PluginId.
- Sempre usar Design System.
- Sempre suportar Light/Dark.
- Sempre garantir acessibilidade.
- Sempre isolar falha da extensão.
- Sempre remover extensão ao descarregar plugin.
- Nunca permitir substituir App Shell.
- Nunca bloquear UI thread.
- Nunca usar estilos inline críticos.

---

# Casos de Erro

- Região não permitida.
- Permissão ausente.
- Extensão com ID duplicado.
- ViewModel inválido.
- View falha ao renderizar.
- Tema não suportado.
- Texto não localizado.
- Ação sem Command.
- Ação destrutiva sem confirmação.
- Extensão trava UI.

---

# Critérios de Aceite

- [ ] Regiões permitidas definidas.
- [ ] Regiões proibidas definidas.
- [ ] `IUiExtensionPlugin` definido.
- [ ] `IUiExtensionRegistry` definido.
- [ ] Modelos de extensão definidos.
- [ ] Permissão `ui-extension` exigida.
- [ ] Design System obrigatório.
- [ ] Acessibilidade obrigatória.
- [ ] Falha segura documentada.
- [ ] Remoção no unload definida.

---

# Testes

## Unitários

- Registrar dashboard card.
- Registrar game details tab.
- Bloquear região proibida.
- Bloquear sem permissão.
- Detectar ID duplicado.
- Remover extensão no unload.
- Falha de renderização isolada.

## Integração

- Plugin adiciona card no Dashboard.
- Plugin adiciona tab em Game Details.
- Plugin adiciona Settings Section.
- Plugin adiciona Diagnostics Panel.
- Alternância Light/Dark.
- Navegação por teclado.
- Plugin unload remove UI.

---

# Dependências

- F041-DesignSystemFoundation.md
- F048-DialogsAndModals.md
- F051-NavigationSystem.md
- F053-TabsSystem.md
- F067-PanelSystem.md
- F070-AccessibilityNavigation.md
- F071-PluginSDKFoundation.md
- F075-PluginPermissions.md
- F079-PluginDiagnostics.md
- F084-PluginSecurity.md

---

# Definition of Done

- Plugin UI Extensions documentado.
- Regiões permitidas definidas.
- Contratos especificados.
- Segurança e permissões definidas.
- Acessibilidade definida.
- Testes definidos.
- Documentação atualizada.

# F040 - Command Palette

> Feature ID: F040 Status: Implemented and Homologated Priority: Critical
> Epic: User Interface

# Objetivo

Disponibilizar uma Paleta de Comandos inspirada em Visual Studio Code,
JetBrains Rider e Visual Studio, permitindo executar qualquer
funcionalidade do WiiStudio através de pesquisa inteligente, atalhos e
navegação por teclado.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Abrir a paleta através de atalho configurável (padrão: Ctrl+Shift+P).

## RF002

Pesquisar comandos em tempo real.

## RF003

Executar comandos sem utilizar menus.

## RF004

Pesquisar configurações, jogos e dispositivos.

## RF005

Exibir comandos recentes e favoritos.

## RF006

Permitir extensão por plugins.

------------------------------------------------------------------------

# Tipos de Comandos

-   Navegação
-   Biblioteca
-   Importação
-   Conversão
-   Exportação
-   Dispositivos
-   Configurações
-   Diagnóstico
-   Atualizações
-   Plugins

------------------------------------------------------------------------

# Layout

``` text
+--------------------------------------------------------------+
| > Pesquise um comando...                                     |
+--------------------------------------------------------------+
| ▶ Importar Jogos                                             |
| ▶ Converter para WBFS                                        |
| ▶ Abrir Device Manager                                       |
| ▶ Verificar Integridade                                      |
| ▶ Atualizar GameTDB                                          |
| ▶ Configurações                                              |
+--------------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Ctrl+Shift+P
      │
Abrir Paleta
      │
Pesquisar
      │
Selecionar
      │
Executar
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
MainWindow
      │
CommandPaletteView
      │
CommandPaletteViewModel
      │
ICommandPaletteService
      │
CommandRegistry
      │
Application Modules
```

------------------------------------------------------------------------

# Máquina de Estados

-   Hidden
-   Opening
-   Searching
-   Executing
-   Completed
-   Failed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface ICommandPaletteService
{
    Task<IReadOnlyList<CommandItem>> SearchAsync(
        string query,
        CancellationToken cancellationToken);

    Task ExecuteAsync(
        string commandId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CommandItem>> GetRecentCommandsAsync(
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Pesquisa fuzzy.
-   Suporte a aliases.
-   Ordenação por relevância.
-   Histórico persistente.
-   Favoritos.
-   Extensível por plugins.

------------------------------------------------------------------------

# Atalhos

-   Ctrl+Shift+P → Abrir Paleta
-   Enter → Executar
-   Esc → Fechar
-   ↑ ↓ → Navegação
-   Tab → Autocompletar

------------------------------------------------------------------------

# Eventos

-   PaletteOpened
-   CommandSearched
-   CommandExecuted
-   RecentCommandsUpdated

------------------------------------------------------------------------

# Interface

-   Caixa de pesquisa
-   Lista dinâmica de comandos
-   Ícones por categoria
-   Histórico
-   Favoritos
-   Ajuda contextual

------------------------------------------------------------------------

# Casos de Erro

-   Comando inexistente.
-   Módulo indisponível.
-   Falha durante execução.

Todos os erros devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Pesquisa em menos de 100 ms.
-   [x] Pesquisa fuzzy.
-   [x] Histórico persistente.
-   [x] Favoritos.
-   [x] Extensibilidade por plugins.

------------------------------------------------------------------------

# Testes

## Unitários

-   Pesquisa.
-   Execução.
-   Histórico.
-   Favoritos.

## Integração

-   Executar importação.
-   Executar conversão.
-   Abrir configurações.
-   Abrir diagnóstico.
-   Plugins registrando novos comandos.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F026-TaskManager.md
-   F031-MainWindow.md
-   F039-ThemeEngine.md

------------------------------------------------------------------------

# Definition of Done

-   Paleta implementada.
-   Pesquisa otimizada.
-   Histórico funcional.
-   Testes aprovados.
-   Documentação atualizada.

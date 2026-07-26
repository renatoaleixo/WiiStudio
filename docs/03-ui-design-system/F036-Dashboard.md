# F036 - Dashboard

> Feature ID: F036 Status: Implemented and Homologated Priority: High Epic:
> User Interface

# Objetivo

Disponibilizar um painel inicial inteligente que apresente uma visão
consolidada do ambiente do WiiStudio, reunindo indicadores da
biblioteca, dispositivos, tarefas, atualizações e ações rápidas.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exibir resumo da biblioteca.

## RF002

Exibir dispositivos conectados.

## RF003

Exibir tarefas em andamento.

## RF004

Exibir notificações recentes.

## RF005

Exibir estatísticas de uso.

## RF006

Disponibilizar atalhos rápidos para operações frequentes.

------------------------------------------------------------------------

# Widgets

-   Biblioteca
-   Dispositivos
-   Conversões
-   Downloads
-   Atualizações
-   Integridade
-   Favoritos
-   Últimas atividades

------------------------------------------------------------------------

# Layout

``` text
+----------------------------------------------------------+
| Toolbar                                                  |
+----------------------+----------------------+------------+
| Biblioteca           | Dispositivos         | Tarefas    |
+----------------------+----------------------+------------+
| Estatísticas         | Atividades           | Alertas    |
+----------------------+----------------------+------------+
| Atalhos Rápidos                                     |
+----------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
DashboardView
      │
DashboardViewModel
      │
IDashboardService
      │
StatisticsService
TaskManager
NotificationCenter
DeviceManager
```

------------------------------------------------------------------------

# Máquina de Estados

-   Loading
-   Refreshing
-   Ready
-   Error

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IDashboardService
{
    Task<DashboardData> LoadAsync(CancellationToken cancellationToken);
    Task RefreshAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Indicadores

-   Total de jogos
-   Jogos por formato
-   Espaço ocupado
-   Espaço livre
-   Dispositivos conectados
-   Conversões ativas
-   Downloads ativos
-   Última sincronização
-   Integridade da biblioteca

------------------------------------------------------------------------

# Eventos

-   DashboardLoaded
-   DashboardRefreshed
-   WidgetUpdated
-   QuickActionExecuted

------------------------------------------------------------------------

# Regras de Negócio

-   Atualização automática configurável.
-   Widgets podem ser ocultados.
-   Ordem dos widgets persistida por perfil.
-   Dados obtidos de serviços existentes.

------------------------------------------------------------------------

# Casos de Erro

-   Serviço indisponível.
-   Widget falhou.
-   Dados inconsistentes.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Carregamento inferior a 2 segundos.
-   [x] Atualização automática.
-   [x] Widgets configuráveis.
-   [x] Atalhos rápidos funcionais.
-   [x] Integração com todos os módulos.

------------------------------------------------------------------------

# Testes

## Unitários

-   Carregamento.
-   Atualização.
-   Widgets.
-   Atalhos.

## Integração

-   Biblioteca grande.
-   Dispositivos conectados.
-   Conversões em andamento.
-   Atualizações pendentes.

------------------------------------------------------------------------

# Dependências

-   F025-NotificationCenter.md
-   F026-TaskManager.md
-   F031-MainWindow.md
-   F035-DeviceManager.md

------------------------------------------------------------------------

# Definition of Done

-   Dashboard implementado.
-   Widgets funcionais.
-   Testes aprovados.
-   Documentação atualizada.

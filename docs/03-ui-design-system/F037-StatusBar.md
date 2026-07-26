# F037 - Status Bar

> Feature ID: F037 Status: Implemented and Homologated Priority: Medium Epic:
> User Interface

# Objetivo

Implementar uma barra de status inteligente que apresente continuamente
o estado da aplicação, dos dispositivos conectados e das tarefas em
execução, fornecendo feedback imediato ao usuário.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exibir dispositivo ativo.

## RF002

Exibir espaço livre do destino atual.

## RF003

Exibir progresso resumido das tarefas.

## RF004

Exibir quantidade de notificações pendentes.

## RF005

Exibir versão da aplicação.

## RF006

Permitir atalhos rápidos para módulos relacionados.

------------------------------------------------------------------------

# Informações Exibidas

-   Dispositivo ativo
-   Sistema de arquivos
-   Espaço livre
-   Jogos selecionados
-   Tarefas ativas
-   Uso de memória (opcional)
-   Status da conexão
-   Versão do WiiStudio

------------------------------------------------------------------------

# Layout

``` text
+------------------------------------------------------------------+
| Device | Free Space | Tasks | Notifications | Memory | Version   |
+------------------------------------------------------------------+
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
StatusBarView
      │
StatusBarViewModel
      │
IStatusBarService
      │
TaskManager
NotificationCenter
DeviceManager
```

------------------------------------------------------------------------

# Máquina de Estados

-   Initializing
-   Updating
-   Ready
-   Warning
-   Error

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IStatusBarService
{
    Task<StatusBarInfo> GetAsync(CancellationToken cancellationToken);
    Task RefreshAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Atualização periódica configurável.
-   Atualização imediata após eventos relevantes.
-   Indicadores visuais para estados críticos.
-   Baixo consumo de recursos.

------------------------------------------------------------------------

# Eventos

-   StatusUpdated
-   DeviceChanged
-   TaskSummaryChanged
-   NotificationsChanged

------------------------------------------------------------------------

# Interface

-   Indicadores compactos.
-   Ícones de status.
-   Tooltips detalhados.
-   Clique para abrir módulo correspondente.

------------------------------------------------------------------------

# Casos de Erro

-   Serviço indisponível.
-   Dados inconsistentes.
-   Falha ao atualizar indicadores.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Atualização em tempo real.
-   [x] Indicadores corretos.
-   [x] Atalhos funcionais.
-   [x] Baixo impacto na performance.

------------------------------------------------------------------------

# Testes

## Unitários

-   Atualização.
-   Indicadores.
-   Eventos.

## Integração

-   Conectar dispositivo.
-   Iniciar conversão.
-   Receber notificações.
-   Alterar versão/configuração.

------------------------------------------------------------------------

# Dependências

-   F025-NotificationCenter.md
-   F026-TaskManager.md
-   F035-DeviceManager.md
-   F036-Dashboard.md

------------------------------------------------------------------------

# Definition of Done

-   Barra implementada.
-   Indicadores funcionais.
-   Testes aprovados.
-   Documentação atualizada.

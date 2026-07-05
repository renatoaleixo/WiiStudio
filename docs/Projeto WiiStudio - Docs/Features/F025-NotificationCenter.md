# F025 - Notification Center

> Feature ID: F025 Status: Ready for Development Priority: High Epic:
> Settings & User Experience

# Objetivo

Centralizar todas as notificações do WiiStudio em um único serviço,
oferecendo mensagens consistentes para operações em andamento, sucesso,
aviso, erro e informações persistentes.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Exibir notificações Toast.

## RF002

Exibir notificações persistentes.

## RF003

Exibir progresso de tarefas longas.

## RF004

Agrupar notificações relacionadas.

## RF005

Permitir histórico de notificações.

## RF006

Permitir ações rápidas diretamente na notificação.

------------------------------------------------------------------------

# Tipos

-   Information
-   Success
-   Warning
-   Error
-   Progress
-   Persistent

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Operação iniciada
      │
Evento publicado
      │
Notification Center
      │
Toast / Painel
      │
Ação do usuário (opcional)
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
Application
     │
INotificationService
     │
NotificationCenter
     │
EventBus
     │
Presentation
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Showing
-   Updating
-   Dismissed
-   Archived

------------------------------------------------------------------------

# Contrato

``` csharp
public interface INotificationService
{
    Task PublishAsync(NotificationMessage message,
        CancellationToken cancellationToken);

    Task DismissAsync(Guid notificationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationMessage>> GetHistoryAsync(
        CancellationToken cancellationToken);

    Task ClearHistoryAsync(
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Toasts desaparecem automaticamente (configurável).
-   Erros críticos permanecem até confirmação.
-   Progresso deve ser atualizado em tempo real.
-   Histórico limitado por política configurável.

------------------------------------------------------------------------

# Eventos

-   NotificationPublished
-   NotificationUpdated
-   NotificationDismissed
-   NotificationArchived

------------------------------------------------------------------------

# Interface

Painel contendo:

-   Lista de notificações
-   Histórico
-   Barra de progresso
-   Filtro por tipo
-   Limpar histórico

------------------------------------------------------------------------

# Casos de Erro

-   Falha ao persistir histórico.
-   Evento inválido.
-   Excesso de notificações.

Todos devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Toasts funcionais.
-   [ ] Histórico persistente.
-   [ ] Progresso em tempo real.
-   [ ] Agrupamento.
-   [ ] Ações rápidas.

------------------------------------------------------------------------

# Testes

## Unitários

-   Publicação.
-   Atualização.
-   Dismiss.
-   Histórico.

## Integração

-   Conversão em lote.
-   Download de capas.
-   Atualização GameTDB.
-   Múltiplas notificações simultâneas.

------------------------------------------------------------------------

# Dependências

-   F008-BatchConversion.md
-   F017-DownloadCovers.md
-   F023-SettingsManager.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Histórico funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

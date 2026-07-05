# F009 - Remove Game

> Feature ID: F009\
> Status: Ready for Development\
> Priority: High\
> Epic: Library Management

# Objetivo

Permitir a remoção segura de um ou mais jogos da biblioteca WiiStudio,
preservando a integridade do dispositivo e fornecendo confirmação
explícita ao usuário antes de qualquer operação destrutiva.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Permitir remover um ou vários jogos.

## RF002

Solicitar confirmação antes da exclusão.

## RF003

Exibir informações do(s) jogo(s) selecionado(s).

## RF004

Executar a remoção utilizando o Wiimms.

## RF005

Atualizar automaticamente a biblioteca após a conclusão.

## RF006

Registrar a operação no histórico.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Operação assíncrona.
-   Suporte a cancelamento antes da execução.
-   Não bloquear a interface.
-   Atualização em tempo real da biblioteca.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
      ▼
Remover
      │
      ▼
Confirmar exclusão
      │
      ▼
Executar remoção
      │
      ▼
Atualizar biblioteca
      │
      ▼
Notificar conclusão
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
GameLibraryView
      │
RemoveGameViewModel
      │
IRemoveGameService
      │
IWiimmsService
      │
ProcessRunner
      │
wwt remove / wit remove
      │
Refresh Library
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Selecting
-   Confirming
-   Removing
-   Refreshing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Regras de Negócio

-   Nunca remover sem confirmação.
-   Jogos em conversão não podem ser removidos.
-   Remoção em lote deve continuar mesmo que um item falhe
    (configurável).
-   Sempre atualizar o histórico.

------------------------------------------------------------------------

# Validações

Antes da remoção:

-   Dispositivo conectado.
-   Jogo existente.
-   Permissão de escrita.
-   Nenhuma conversão ativa para o jogo.

------------------------------------------------------------------------

# Eventos

-   RemoveRequested
-   RemoveConfirmed
-   RemoveStarted
-   RemoveCompleted
-   RemoveFailed
-   LibraryRefreshed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IRemoveGameService
{
    Task<RemovePreview> AnalyzeAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);

    Task<RemoveResult> RemoveAsync(
        RemoveRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Interface do Usuário

A janela de confirmação deverá exibir:

-   Quantidade de jogos.
-   Lista dos títulos.
-   Espaço que será liberado.
-   Aviso de operação irreversível.

Botões:

-   Remover
-   Cancelar

------------------------------------------------------------------------

# Casos de Erro

-   Dispositivo desconectado.
-   Jogo inexistente.
-   Falha do Wiimms.
-   Permissão negada.
-   Arquivo bloqueado.

Todos os erros devem ser registrados em log estruturado.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Remoção individual.
-   [ ] Remoção em lote.
-   [ ] Confirmação obrigatória.
-   [ ] Atualização automática da biblioteca.
-   [ ] Histórico atualizado.
-   [ ] Logs registrados.

------------------------------------------------------------------------

# Testes

## Unitários

-   Validação.
-   Confirmação.
-   Remoção.
-   Cancelamento.

## Integração

-   Remover um jogo.
-   Remover múltiplos jogos.
-   Falha parcial.
-   Dispositivo removido durante operação.

------------------------------------------------------------------------

# Dependências

-   011-WBFS.md
-   012-WiimmsIntegration.md
-   016-GameLibrary.md
-   F002-ScanGameLibrary.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Testes aprovados.
-   Biblioteca atualizada.
-   Histórico registrado.
-   Documentação revisada.

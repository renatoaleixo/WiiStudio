# F022 - GameTDB Update Manager

> Feature ID: F022 Status: Implemented and Homologated Priority: High Epic:
> Metadata & Assets

# Objetivo

Gerenciar o ciclo de vida das atualizações do banco de metadados
GameTDB, garantindo sincronizações seguras, incrementais e versionadas.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Verificar periodicamente novas versões do GameTDB.

## RF002

Executar atualização manual.

## RF003

Executar atualização incremental quando suportada.

## RF004

Validar integridade dos arquivos baixados.

## RF005

Realizar rollback em caso de falha.

## RF006

Notificar o usuário sobre novas atualizações.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Atualização em segundo plano.
-   Não bloquear a interface.
-   Downloads retomáveis.
-   Compatível com múltiplos mirrors.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Verificar Atualizações
        │
Nova versão?
   ├── Não → Encerrar
   └── Sim
        │
Baixar
        │
Validar
        │
Aplicar
        │
Atualizar Cache
        │
Concluir
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
UpdateManager
      │
IGameTdbUpdateService
      │
MirrorProvider
      │
Downloader
      │
Validator
      │
MetadataCache
      │
AssetManager
```

------------------------------------------------------------------------

# Estratégia de Atualização

1.  Consultar versão local.
2.  Consultar mirrors.
3.  Selecionar melhor origem.
4.  Baixar pacote.
5.  Validar checksum.
6.  Aplicar atualização.
7.  Atualizar índice.
8.  Publicar eventos.

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Checking
-   Downloading
-   Validating
-   Applying
-   Refreshing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IGameTdbUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(
        CancellationToken cancellationToken);

    Task<UpdateResult> UpdateAsync(
        UpdateOptions options,
        CancellationToken cancellationToken);

    Task RollbackAsync(
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   UpdateCheckStarted
-   UpdateAvailable
-   DownloadStarted
-   ValidationCompleted
-   UpdateApplied
-   UpdateFailed
-   RollbackCompleted

------------------------------------------------------------------------

# Interface

Tela "Atualizações":

-   Versão instalada
-   Última sincronização
-   Última verificação
-   Mirror utilizado
-   Barra de progresso
-   Botão Verificar
-   Botão Atualizar
-   Botão Restaurar versão anterior

------------------------------------------------------------------------

# Casos de Erro

-   Mirror indisponível.
-   Checksum inválido.
-   Download interrompido.
-   Banco bloqueado.
-   Espaço insuficiente.

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Verificação automática.
-   [x] Atualização manual.
-   [x] Atualização incremental.
-   [x] Rollback funcional.
-   [x] Integração com Metadata Cache.
-   [x] Logs completos.

------------------------------------------------------------------------

# Testes

## Unitários

-   Verificação de versão.
-   Seleção de mirror.
-   Checksum.
-   Rollback.
-   Cancelamento.

## Integração

-   Atualização completa.
-   Falha de download.
-   Mirror indisponível.
-   Rollback após erro.
-   Atualização de grande base.

------------------------------------------------------------------------

# Dependências

-   014-GameTDB.md
-   F017-DownloadCovers.md
-   F018-MetadataSynchronization.md
-   F019-OfflineMetadataCache.md
-   F020-AssetManager.md
-   F021-BannerAndDiscArt.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Atualização validada.
-   Rollback funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

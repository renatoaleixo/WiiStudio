# F021 - Banner & Disc Art

> Feature ID: F021 Status: Implemented and Homologated Priority: Medium Epic:
> Metadata & Assets

# Objetivo

Gerenciar o download, armazenamento, atualização e exibição de
**Banners** e **Disc Art** dos jogos, proporcionando uma experiência
visual rica e consistente.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Baixar banners automaticamente.

## RF002

Baixar Disc Art automaticamente.

## RF003

Atualizar apenas recursos ausentes.

## RF004

Permitir atualização forçada.

## RF005

Exibir pré-visualização.

## RF006

Integrar com o Asset Manager.

------------------------------------------------------------------------

# Recursos

-   Banner
-   Disc Art
-   Miniaturas derivadas

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
Atualizar Recursos
      │
Consultar Cache
      │
Consultar Provider
      │
Persistir Assets
      │
Atualizar Biblioteca
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
GameDetails
     │
BannerDiscArtViewModel
     │
IBannerAssetService
     │
IAssetManager
     │
ICoverProvider
     │
Asset Cache
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   CheckingCache
-   Downloading
-   Saving
-   Refreshing
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Estrutura

``` text
assets/
├── banners/
├── discs/
└── thumbnails/
```

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IBannerAssetService
{
    Task<AssetDownloadResult> DownloadAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);

    Task<bool> HasAssetsAsync(
        string gameId,
        CancellationToken cancellationToken);

    Task RefreshAsync(
        string gameId,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Utilizar cache antes do Provider.
-   Não baixar novamente assets válidos.
-   Validar integridade dos arquivos.
-   Manter compatibilidade com múltiplos Providers.

------------------------------------------------------------------------

# Eventos

-   BannerDownloadStarted
-   BannerDownloaded
-   DiscArtDownloaded
-   AssetRefreshCompleted
-   AssetDownloadFailed

------------------------------------------------------------------------

# Interface

Exibir:

-   Banner atual
-   Disc Art
-   Origem (Cache/Provider)
-   Última atualização
-   Botão Atualizar

------------------------------------------------------------------------

# Casos de Erro

-   Provider indisponível.
-   Arquivo inválido.
-   Cache corrompido.
-   Falha de gravação.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Download individual.
-   [x] Download em lote.
-   [x] Cache reutilizado.
-   [x] Atualização incremental.
-   [x] Integração com Asset Manager.

------------------------------------------------------------------------

# Testes

## Unitários

-   Cache hit.
-   Cache miss.
-   Atualização.
-   Integridade.

## Integração

-   Biblioteca sem assets.
-   Atualização parcial.
-   Provider offline.
-   Grande volume.

------------------------------------------------------------------------

# Dependências

-   F017-DownloadCovers.md
-   F019-OfflineMetadataCache.md
-   F020-AssetManager.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Cache funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# F020 - Asset Manager

> Feature ID: F020 Status: Ready for Development Priority: Critical
> Epic: Metadata & Assets

# Objetivo

Centralizar o gerenciamento de todos os recursos estáticos utilizados
pelo WiiStudio, incluindo capas, banners, disc art, miniaturas, ícones e
demais assets, fornecendo uma API única para armazenamento, cache,
limpeza e acesso.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Gerenciar todos os assets através de um único serviço.

## RF002

Resolver automaticamente caminhos físicos dos recursos.

## RF003

Controlar cache e versionamento.

## RF004

Detectar assets ausentes ou corrompidos.

## RF005

Executar limpeza seletiva.

## RF006

Reconstruir o índice de assets.

------------------------------------------------------------------------

# Tipos de Assets

-   Cover Front
-   Cover Back
-   Disc Art
-   Banner
-   Thumbnail
-   Ícones
-   Temas
-   Imagens temporárias

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
IAssetManager
      │
AssetManager
      │
├── Cache
├── Metadata Index
├── Providers
└── File Storage
```

------------------------------------------------------------------------

# Estrutura Física

``` text
assets/
├── covers/
├── discs/
├── banners/
├── thumbnails/
├── icons/
├── themes/
└── temp/
```

------------------------------------------------------------------------

# Fluxo

``` text
Solicitação
    │
Consultar Cache
    │
Encontrou?
 ├── Sim → Retornar
 └── Não
        │
Consultar Provider
        │
Persistir
        │
Retornar
```

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IAssetManager
{
    Task<AssetInfo?> GetAsync(
        AssetRequest request,
        CancellationToken cancellationToken);

    Task SaveAsync(
        AssetSaveRequest request,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        AssetKey key,
        CancellationToken cancellationToken);

    Task RebuildIndexAsync(
        CancellationToken cancellationToken);

    Task<AssetStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Nunca duplicar assets.
-   Utilizar hash para identificar conteúdo idêntico.
-   Persistir metadados do asset.
-   Permitir múltiplos providers.

------------------------------------------------------------------------

# Eventos

-   AssetLoaded
-   AssetSaved
-   AssetRemoved
-   AssetCacheHit
-   AssetCacheMiss
-   AssetIndexRebuilt

------------------------------------------------------------------------

# Interface

Tela "Gerenciador de Assets":

-   Espaço utilizado
-   Quantidade por categoria
-   Assets órfãos
-   Botão Limpar
-   Botão Reconstruir Índice
-   Botão Validar Assets

------------------------------------------------------------------------

# Casos de Erro

-   Asset corrompido.
-   Provider indisponível.
-   Permissão negada.
-   Espaço insuficiente.

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] API única para assets.
-   [ ] Cache transparente.
-   [ ] Reconstrução de índice.
-   [ ] Limpeza seletiva.
-   [ ] Estatísticas disponíveis.

------------------------------------------------------------------------

# Testes

## Unitários

-   Get
-   Save
-   Remove
-   Cache
-   Reconstrução

## Integração

-   Download de capas
-   Cache reutilizado
-   Provider offline
-   Reconstrução completa
-   Biblioteca \>10.000 assets

------------------------------------------------------------------------

# Dependências

-   F017-DownloadCovers.md
-   F018-MetadataSynchronization.md
-   F019-OfflineMetadataCache.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Índice funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

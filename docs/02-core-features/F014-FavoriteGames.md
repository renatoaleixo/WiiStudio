# F014 - Favorite Games

> Feature ID: F014 Status: Implemented and Homologated Priority: Medium Epic:
> Library Management

# Objetivo

Permitir que o usuário marque jogos como favoritos para acesso rápido,
organização personalizada e sincronização com a biblioteca do WiiStudio.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Marcar um jogo como favorito.

## RF002

Remover um jogo dos favoritos.

## RF003

Favoritar múltiplos jogos.

## RF004

Filtrar apenas favoritos.

## RF005

Ordenar favoritos por diferentes critérios.

## RF006

Persistir favoritos entre sessões.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Atualização instantânea da interface.
-   Persistência local.
-   Operação assíncrona.
-   Compatível com futuras sincronizações em nuvem.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
Marcar Favorito
      │
Atualizar Biblioteca
      │
Visualizar Lista de Favoritos
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
GameLibrary
      │
FavoriteGamesViewModel
      │
IFavoriteGameService
      │
LibraryRepository
      │
SQLite
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Updating
-   Completed
-   Failed

------------------------------------------------------------------------

# Regras de Negócio

-   Favoritar não altera os arquivos do jogo.
-   O mesmo jogo não pode ser favoritado duas vezes.
-   Favoritos devem acompanhar atualizações de metadados.

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IFavoriteGameService
{
    Task AddAsync(string gameId, CancellationToken cancellationToken);
    Task RemoveAsync(string gameId, CancellationToken cancellationToken);
    Task<bool> IsFavoriteAsync(string gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GameSummaryDto>> GetFavoritesAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   FavoriteAdded
-   FavoriteRemoved
-   FavoritesUpdated

------------------------------------------------------------------------

# Interface do Usuário

Adicionar:

-   Ícone ⭐ na biblioteca.
-   Filtro "Somente Favoritos".
-   Ordenação por favoritos.
-   Indicador visual permanente.

------------------------------------------------------------------------

# Casos de Erro

-   Jogo inexistente.
-   Banco indisponível.
-   Persistência falhou.

Todos devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Favoritar/desfavoritar instantaneamente.
-   [x] Persistência entre reinicializações.
-   [x] Suporte a múltiplos jogos.
-   [x] Integração com pesquisa e filtros.

------------------------------------------------------------------------

# Testes

## Unitários

-   Adicionar favorito.
-   Remover favorito.
-   Consultar favoritos.
-   Persistência.

## Integração

-   Reiniciar aplicação mantendo favoritos.
-   Favoritos após atualização da biblioteca.

------------------------------------------------------------------------

# Dependências

-   016-GameLibrary.md
-   019-FeatureDevelopmentGuide.md
-   F013-GameDetails.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Persistência funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

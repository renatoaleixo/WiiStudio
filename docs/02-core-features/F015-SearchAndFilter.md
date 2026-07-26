# F015 - Search & Filter

> Feature ID: F015 Status: Implemented and Homologated Priority: Critical
> Epic: Library Management

# Objetivo

Disponibilizar um mecanismo de pesquisa e filtros avançados para
localizar jogos rapidamente, mesmo em bibliotecas com milhares de
títulos.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Pesquisa incremental em tempo real.

## RF002

Pesquisa por:

-   Título
-   Game ID
-   Desenvolvedor
-   Publicadora
-   Gênero
-   Região

## RF003

Filtros combináveis:

-   Região
-   Formato
-   Integridade
-   Favoritos
-   Número de jogadores
-   Ano
-   Tamanho

## RF004

Ordenação dinâmica.

## RF005

Salvar filtros favoritos.

## RF006

Limpar todos os filtros com um clique.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Resultado em menos de 200 ms para até 5.000 jogos.
-   Não bloquear a UI.
-   Pesquisa sem diferenciar maiúsculas, minúsculas e acentos.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Digitar pesquisa
      │
Aplicar filtros
      │
Atualizar resultados
      │
Abrir jogo
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
SearchBar
    │
SearchFilterViewModel
    │
IGameSearchService
    │
Library Index
    │
SQLite + Cache
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Searching
-   Filtering
-   Completed
-   Empty
-   Failed

------------------------------------------------------------------------

# Regras de Negócio

-   Pesquisa e filtros funcionam simultaneamente.
-   Alterações atualizam os resultados automaticamente.
-   Ordenação preserva filtros ativos.

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IGameSearchService
{
    Task<SearchResult> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Ordenações

-   Título
-   Data de importação
-   Tamanho
-   Região
-   Game ID
-   Favoritos
-   Última utilização

------------------------------------------------------------------------

# Eventos

-   SearchStarted
-   SearchCompleted
-   FilterChanged
-   SortChanged

------------------------------------------------------------------------

# Interface do Usuário

-   Caixa de pesquisa.
-   Painel lateral de filtros.
-   Chips com filtros ativos.
-   Botão "Limpar Filtros".
-   Contador de resultados.

------------------------------------------------------------------------

# Casos de Erro

-   Índice indisponível.
-   Banco inacessível.
-   Consulta cancelada.

Todos devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Pesquisa incremental.
-   [x] Filtros combináveis.
-   [x] Ordenação dinâmica.
-   [x] Persistência de filtros favoritos.
-   [x] Performance dentro da meta.

------------------------------------------------------------------------

# Testes

## Unitários

-   Pesquisa.
-   Combinação de filtros.
-   Ordenação.
-   Persistência.

## Integração

-   Biblioteca \>5.000 jogos.
-   Pesquisa com acentos.
-   Atualização após importação.

------------------------------------------------------------------------

# Dependências

-   016-GameLibrary.md
-   F013-GameDetails.md
-   F014-FavoriteGames.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Índice otimizado.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# F019 - Offline Metadata Cache

> Feature ID: F019\
> Status: Ready for Development\
> Priority: High\
> Epic: Metadata & Assets

# Objetivo

Implementar um sistema robusto de cache offline para metadados e
recursos do WiiStudio, permitindo funcionamento completo da biblioteca
mesmo sem conexão com a internet, reduzindo chamadas aos Providers e
melhorando significativamente o desempenho.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Armazenar localmente todos os metadados baixados.

## RF002

Armazenar localmente: - Capas - Disc Art - Banner - Ícones - Metadados

## RF003

Permitir funcionamento totalmente offline.

## RF004

Atualizar automaticamente somente dados expirados.

## RF005

Permitir limpeza seletiva do cache.

## RF006

Exibir estatísticas do cache.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Cache transparente ao usuário.
-   Alta velocidade de leitura.
-   Compressão opcional.
-   Integridade garantida.
-   Baixo consumo de memória.

------------------------------------------------------------------------

# Arquitetura

``` text
Application
      │
IMetadataCacheService
      │
Metadata Cache Manager
      │
├── SQLite
├── Assets Cache
└── File Index
```

------------------------------------------------------------------------

# Estrutura Física

``` text
cache/
├── metadata/
│   └── games.db
├── covers/
├── discs/
├── banners/
└── thumbnails/
```

------------------------------------------------------------------------

# Política de Expiração

  Recurso     Expiração
  ----------- -------------
  Metadados   30 dias
  Capas       Nunca
  Disc Art    Nunca
  Banner      Nunca
  Índice      Incremental

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IMetadataCacheService
{
    Task<GameMetadata?> GetAsync(
        string gameId,
        CancellationToken cancellationToken);

    Task SaveAsync(
        GameMetadata metadata,
        CancellationToken cancellationToken);

    Task InvalidateAsync(
        string gameId,
        CancellationToken cancellationToken);

    Task<CacheStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken);

    Task CleanupAsync(
        CleanupOptions options,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Estatísticas

Exibir:

-   Total de jogos
-   Espaço utilizado
-   Última atualização
-   Cache Hit Rate
-   Downloads evitados
-   Economia estimada

------------------------------------------------------------------------

# Eventos

-   CacheInitialized
-   CacheHit
-   CacheMiss
-   CacheUpdated
-   CacheCleaned

------------------------------------------------------------------------

# Interface

Tela "Cache":

-   Espaço utilizado
-   Quantidade de arquivos
-   Última sincronização
-   Atualizar
-   Limpar
-   Reconstruir Índice

------------------------------------------------------------------------

# Casos de Erro

-   Cache corrompido
-   Espaço insuficiente
-   Permissão negada
-   Banco bloqueado

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Funciona offline
-   [ ] Cache reutilizado automaticamente
-   [ ] Limpeza seletiva
-   [ ] Estatísticas disponíveis
-   [ ] Reconstrução do cache

------------------------------------------------------------------------

# Testes

## Unitários

-   Cache Hit
-   Cache Miss
-   Expiração
-   Limpeza
-   Reconstrução

## Integração

-   Biblioteca offline
-   Cache corrompido
-   Atualização incremental
-   Limpeza completa
-   Biblioteca \>10.000 jogos

------------------------------------------------------------------------

# Dependências

-   014-GameTDB.md
-   F017-DownloadCovers.md
-   F018-MetadataSynchronization.md

------------------------------------------------------------------------

# Definition of Done

-   Cache implementado.
-   Estatísticas funcionais.
-   Limpeza implementada.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

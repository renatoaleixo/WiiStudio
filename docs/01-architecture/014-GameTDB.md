# 014 - GameTDB Provider Specification

> Status: Accepted Version: 1.0.0

# Objetivo

Definir a integração oficial do WiiStudio com o GameTDB utilizando uma
arquitetura baseada em Providers.

------------------------------------------------------------------------

# Princípios

-   O GameTDB é uma fonte de metadados.
-   O WiiStudio nunca dependerá diretamente de uma implementação
    específica.
-   Toda comunicação ocorrerá através de contratos.

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
Application
      │
IGameMetadataProvider
      │
┌──────────────┬──────────────────┐
│              │                  │
GameTDB   LocalCacheProvider  FutureProvider
```

------------------------------------------------------------------------

# Responsabilidades

O Provider deve fornecer:

-   Título oficial
-   Região
-   Desenvolvedor
-   Publicadora
-   Ano
-   Gênero
-   Número de jogadores
-   Classificação
-   Sinopse
-   Capas (Front/Back/Disc)
-   Banner (quando disponível)

------------------------------------------------------------------------

# Interfaces

``` csharp
public interface IGameMetadataProvider
{
    Task<GameMetadata> GetByGameIdAsync(
        string gameId,
        CancellationToken cancellationToken);

    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken);
}
```

``` csharp
public interface ICoverProvider
{
    Task<CoverResult> DownloadCoverAsync(
        string gameId,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Cache

Todo conteúdo baixado deverá ser armazenado localmente.

Objetivos:

-   reduzir chamadas;
-   permitir uso offline;
-   melhorar desempenho.

Nunca sobrescrever arquivos válidos sem confirmação.

------------------------------------------------------------------------

# Fluxo

1.  Usuário seleciona um jogo.
2.  Verificar cache.
3.  Se existir, utilizar cache.
4.  Caso contrário:
    -   consultar Provider;
    -   validar resposta;
    -   persistir cache;
    -   atualizar UI.

------------------------------------------------------------------------

# Falhas

Situações tratadas:

-   Sem internet
-   GameId inexistente
-   Timeout
-   Rate limit
-   Arquivo inválido

Cada falha deve possuir mensagem amigável.

------------------------------------------------------------------------

# Logging

Registrar:

-   início
-   fim
-   duração
-   provider utilizado
-   cache hit/miss
-   erros

------------------------------------------------------------------------

# Providers Futuros

A arquitetura deve permitir novos provedores:

-   OpenVGDB
-   Local XML
-   JSON Offline
-   Community Provider

Sem alterar a camada Application.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca acessar GameTDB diretamente pela UI.
-   Toda consulta passa por IGameMetadataProvider.
-   Toda capa passa por ICoverProvider.
-   Sempre consultar o cache antes da internet.

------------------------------------------------------------------------

# Critérios de Aceite

-   Provider desacoplado.
-   Cache funcional.
-   Suporte offline.
-   Cobertura de testes.
-   Preparado para múltiplos provedores.

------------------------------------------------------------------------

# Checklist

-   [ ] Interface definida
-   [ ] Cache especificado
-   [ ] Fluxo documentado
-   [ ] Logging definido
-   [ ] Extensibilidade garantida

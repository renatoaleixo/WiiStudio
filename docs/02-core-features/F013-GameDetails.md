# F013 - Game Details

> Feature ID: F013\
> Status: Implemented and Homologated\
> Priority: Medium\
> Epic: Library Management

# Objetivo

Exibir uma visão completa e unificada de todas as informações de um
jogo, permitindo ao usuário consultar metadados, arquivos, integridade,
histórico, capas e ações disponíveis em um único painel.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Abrir detalhes a partir da biblioteca.

## RF002

Exibir metadados completos.

## RF003

Exibir informações técnicas do arquivo.

## RF004

Exibir histórico de operações.

## RF005

Permitir ações rápidas.

## RF006

Atualizar informações automaticamente após alterações.

------------------------------------------------------------------------

# Informações Exibidas

## Gerais

-   Título
-   Game ID
-   Região
-   Desenvolvedor
-   Publicadora
-   Ano
-   Gênero
-   Número de jogadores
-   Classificação

## Arquivo

-   Formato
-   Tamanho
-   Caminho
-   Data de importação
-   Última verificação
-   Status de integridade

## Recursos

-   Capa frontal
-   Capa traseira
-   Disco
-   Banner (quando disponível)

------------------------------------------------------------------------

# Ações Disponíveis

-   Converter
-   Exportar
-   Verificar Integridade
-   Reparar
-   Atualizar Capas
-   Favoritar
-   Remover

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo
      │
Abrir detalhes
      │
Consultar informações
      │
Executar ação (opcional)
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
GameLibrary
    │
GameDetailsViewModel
    │
IGameDetailsService
    │
Metadata Provider
History Service
Diagnostics Service
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Loading
-   Loaded
-   Refreshing
-   Failed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IGameDetailsService
{
    Task<GameDetailsDto> GetAsync(
        string gameId,
        CancellationToken cancellationToken);

    Task RefreshAsync(
        string gameId,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   GameDetailsOpened
-   GameDetailsLoaded
-   MetadataUpdated
-   CoversUpdated
-   DetailsRefreshCompleted

------------------------------------------------------------------------

# Interface

O painel deverá conter:

-   Aba Geral
-   Aba Arquivo
-   Aba Metadados
-   Aba Histórico
-   Aba Diagnóstico

------------------------------------------------------------------------

# Casos de Erro

-   Jogo inexistente
-   Metadados indisponíveis
-   Provider offline
-   Arquivo removido

Todos devem gerar mensagens amigáveis e log estruturado.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Informações carregadas em menos de 2 segundos (cache).
-   [x] Atualização dinâmica.
-   [x] Todas as ações disponíveis.
-   [x] Histórico integrado.
-   [x] Compatível com múltiplos Providers.

------------------------------------------------------------------------

# Testes

## Unitários

-   Carregamento de detalhes.
-   Atualização.
-   Falhas de Provider.
-   Cache.

## Integração

-   Abrir jogo válido.
-   Abrir jogo sem metadados.
-   Atualizar informações.
-   Provider indisponível.

------------------------------------------------------------------------

# Dependências

-   014-GameTDB.md
-   016-GameLibrary.md
-   020-Diagnostics.md
-   F010-VerifyGameIntegrity.md
-   F011-RepairGame.md
-   F012-ExportGame.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Painel funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# F017 - Download Covers

> Feature ID: F017 Status: Ready for Development Priority: High Epic:
> Metadata & Assets

# Objetivo

Baixar, atualizar e gerenciar automaticamente capas, artes de disco,
banners e demais recursos visuais dos jogos utilizando provedores de
metadados, com cache local e suporte à atualização em lote.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Baixar capas de um jogo.

## RF002

Baixar capas em lote.

## RF003

Atualizar somente recursos ausentes.

## RF004

Forçar atualização quando solicitado.

## RF005

Permitir múltiplos provedores.

## RF006

Operar mesmo sem internet utilizando cache local.

------------------------------------------------------------------------

# Recursos Suportados

-   Capa frontal
-   Capa traseira
-   Arte do disco
-   Banner
-   Ícone (quando disponível)

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
Download Assets
      │
Consultar Cache
      │
Consultar Provider
      │
Salvar Assets
      │
Atualizar Biblioteca
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
Library
   │
CoverManagerViewModel
   │
ICoverDownloadService
   │
ICoverProvider
   │
GameTDB / Future Providers
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

# Estratégia de Cache

Diretório padrão:

``` text
assets/cache/
├── covers/
├── discs/
├── banners/
└── thumbnails/
```

Regras:

-   Priorizar cache válido.
-   Downloads incrementais.
-   Permitir limpeza seletiva.

------------------------------------------------------------------------

# Contrato

``` csharp
public interface ICoverDownloadService
{
    Task<DownloadPreview> AnalyzeAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);

    Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Eventos

-   CoverDownloadStarted
-   CoverDownloaded
-   CoverUpdated
-   CoverDownloadCompleted
-   CoverDownloadFailed

------------------------------------------------------------------------

# Interface

Exibir:

-   Prévia das imagens
-   Progresso
-   Recursos encontrados
-   Recursos ausentes
-   Origem (Cache/Provider)

------------------------------------------------------------------------

# Casos de Erro

-   Sem conexão.
-   Provider indisponível.
-   Arquivo inválido.
-   Falha ao gravar.
-   Cancelamento.

Registrar logs estruturados e manter cache consistente.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Download individual.
-   [ ] Download em lote.
-   [ ] Cache reutilizado.
-   [ ] Atualização incremental.
-   [ ] Suporte a múltiplos Providers.

------------------------------------------------------------------------

# Testes

## Unitários

-   Cache hit.
-   Cache miss.
-   Download.
-   Atualização.
-   Cancelamento.

## Integração

-   Biblioteca vazia.
-   Biblioteca com assets parciais.
-   Provider indisponível.
-   Atualização em lote.

------------------------------------------------------------------------

# Dependências

-   014-GameTDB.md
-   018-ConversionEngine.md
-   F013-GameDetails.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Cache funcional.
-   Downloads validados.
-   Testes aprovados.
-   Documentação atualizada.

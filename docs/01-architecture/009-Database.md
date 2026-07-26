# 009 - Database Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a estratégia de persistência do WiiStudio.

> **Importante:** O WiiStudio **não** utilizará banco de dados para
> armazenar a biblioteca de jogos do HD WBFS. O banco existe apenas para
> configurações, cache, histórico e recursos da aplicação.

------------------------------------------------------------------------

# Tecnologia

-   SQLite
-   Um único arquivo local.
-   Migrações versionadas.
-   Acesso por Entity Framework Core.

------------------------------------------------------------------------

# Responsabilidades do Banco

## Configurações

-   Tema
-   Idioma
-   Caminho do wit
-   Caminho do wwt
-   Caminho do NKit
-   Diretório padrão
-   Atualizações

## Cache

-   Metadados do GameTDB
-   Capas baixadas
-   Banners
-   Informações já consultadas

## Histórico

-   Conversões
-   Importações
-   Remoções
-   Downloads

## Favoritos

Lista de jogos favoritos identificados pelo GameId.

------------------------------------------------------------------------

# Tabelas

## Settings

  Campo   Tipo
  ------- --------
  Id      Guid
  Key     string
  Value   string

## GameCache

  Campo       Tipo
  ----------- ----------
  GameId      string
  Title       string
  Region      string
  Genre       string
  Developer   string
  Players     int
  CachedAt    DateTime

## CoverCache

  Campo          Tipo
  -------------- ----------
  GameId         string
  ImagePath      string
  DownloadedAt   DateTime

## History

  Campo        Tipo
  ------------ ----------
  Id           Guid
  Action       string
  GameId       string
  ExecutedAt   DateTime
  Success      bool

------------------------------------------------------------------------

# O que NÃO armazenar

-   Arquivos ISO
-   Arquivos WBFS
-   Conteúdo do HD
-   Partições WBFS

Essas informações sempre serão lidas diretamente do dispositivo.

------------------------------------------------------------------------

# Convenções

-   GUID como chave primária.
-   UTC para datas.
-   Soft Delete apenas quando necessário.
-   Índices em GameId e Key.

------------------------------------------------------------------------

# Migrações

Toda alteração estrutural deve gerar nova migration.

Padrão:

``` text
20260705_InitialSchema
20260710_AddHistoryTable
```

------------------------------------------------------------------------

# Backup

O banco deve permitir exportação e importação.

Local padrão:

``` text
~/.config/WiiStudio/
```

(ou equivalente por sistema operacional).

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca acessar SQLite diretamente pela UI.
-   Toda consulta via repositórios/serviços.
-   Nunca persistir informações disponíveis no HD WBFS.
-   Evitar duplicação entre cache e origem.

------------------------------------------------------------------------

# Critérios de Aceite

-   Banco pequeno (\<100 MB em uso normal).
-   Migrações reproduzíveis.
-   Sem dependência do banco para listar jogos do HD.

------------------------------------------------------------------------

# Checklist

-   [x] Schema definido
-   [x] Migrações documentadas
-   [x] Índices definidos
-   [x] Estratégia de backup definida
-   [x] Cache documentado

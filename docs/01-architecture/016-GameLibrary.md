# 016 - Game Library Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

A Biblioteca de Jogos é o principal módulo do WiiStudio. Ela centraliza
a visualização, organização e gerenciamento dos jogos detectados em
dispositivos WBFS e em diretórios compatíveis (`wbfs/`).

------------------------------------------------------------------------

# Objetivos Funcionais

-   Listar jogos rapidamente.
-   Pesquisar instantaneamente.
-   Filtrar por múltiplos critérios.
-   Exibir metadados e capas.
-   Executar ações sobre um ou mais jogos.

------------------------------------------------------------------------

# Fontes de Dados

A biblioteca pode ser alimentada por:

-   HD WBFS legado
-   FAT32 + pasta wbfs
-   NTFS + pasta wbfs
-   exFAT + pasta wbfs

A listagem nunca dependerá exclusivamente do SQLite.

------------------------------------------------------------------------

# Layout

``` text
┌───────────────────────────────────────────────┐
│ Pesquisa............................. [🔍]    │
├───────────────────────────────────────────────┤
│ Filtros │ Biblioteca                         │
│ Região  │ ┌──────┐ ┌──────┐ ┌──────┐         │
│ Formato │ │Capa  │ │Capa  │ │Capa  │         │
│ Gênero  │ │Título│ │Título│ │Título│         │
│         │ └──────┘ └──────┘ └──────┘         │
└───────────────────────────────────────────────┘
```

------------------------------------------------------------------------

# Modos de Visualização

-   Grade (padrão)
-   Lista detalhada
-   Compacto

------------------------------------------------------------------------

# Pesquisa

A pesquisa deve ser incremental e ignorar maiúsculas/minúsculas.

Campos pesquisáveis:

-   Título
-   Game ID
-   Desenvolvedor
-   Gênero

------------------------------------------------------------------------

# Filtros

-   Região
-   Formato
-   Tamanho
-   Número de jogadores
-   Favoritos

Filtros combináveis.

------------------------------------------------------------------------

# Ações

Individual:

-   Abrir detalhes
-   Converter
-   Atualizar capa
-   Remover
-   Verificar integridade

Em lote:

-   Converter
-   Exportar
-   Remover
-   Atualizar capas

------------------------------------------------------------------------

# Ordenação

-   Título
-   Data de adição
-   Tamanho
-   Região
-   Game ID

Persistir última ordenação.

------------------------------------------------------------------------

# Estados

## Loading

Skeleton.

## Empty

Mensagem + botão "Importar jogo".

## Error

Mensagem amigável + "Tentar novamente".

------------------------------------------------------------------------

# Performance

-   Virtualização da lista.
-   Carregamento progressivo de capas.
-   Cache local de imagens.

Meta:

Até 2.000 jogos sem degradação perceptível.

------------------------------------------------------------------------

# Eventos

-   LibraryLoaded
-   LibraryRefreshed
-   GameSelected
-   GameRemoved

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca acessar Wiimms diretamente pela UI.
-   Toda atualização passa pela Application.
-   Componentes reutilizáveis.
-   Biblioteca sempre sincronizada com eventos.

------------------------------------------------------------------------

# Critérios de Aceite

-   Pesquisa instantânea.
-   Filtros combináveis.
-   Virtualização habilitada.
-   Suporte a ações em lote.
-   Atualização automática após importação.

------------------------------------------------------------------------

# Checklist

-   [x] Layout definido
-   [x] Pesquisa especificada
-   [x] Filtros definidos
-   [x] Ações em lote
-   [x] Estados documentados
-   [x] Regras de performance

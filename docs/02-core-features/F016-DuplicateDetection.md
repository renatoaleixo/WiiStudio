# F016 - Duplicate Detection

> Feature ID: F016 Status: Ready for Development Priority: High Epic:
> Library Management

# Objetivo

Detectar jogos duplicados na biblioteca considerando Game ID, conteúdo,
formato, dispositivo e metadados, oferecendo ações inteligentes para
consolidação da coleção.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Detectar duplicidade pelo Game ID.

## RF002

Detectar o mesmo jogo em formatos diferentes.

## RF003

Detectar duplicatas entre múltiplos dispositivos.

## RF004

Agrupar duplicatas automaticamente.

## RF005

Sugerir qual cópia manter.

## RF006

Executar ações em lote.

------------------------------------------------------------------------

# Critérios de Comparação

-   Game ID
-   CRC/Hash (quando disponível)
-   Título
-   Região
-   Tamanho
-   Formato
-   Dispositivo
-   Caminho

------------------------------------------------------------------------

# Estratégia

Níveis:

  Nível      Descrição
  ---------- ---------------------
  Exact      Mesmo conteúdo
  Probable   Mesmo Game ID
  Similar    Mesmo título/região

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Abrir Duplicatas
      │
Escanear Biblioteca
      │
Agrupar Jogos
      │
Selecionar Ação
      │
Executar
      │
Atualizar Biblioteca
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
Library
   │
DuplicateDetectionViewModel
   │
IDuplicateDetectionService
   │
DuplicateAnalyzer
   │
LibraryRepository
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Scanning
-   Comparing
-   Grouping
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Ações

-   Remover duplicatas
-   Exportar relatório
-   Ignorar grupo
-   Marcar favorito
-   Abrir detalhes
-   Converter formato

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IDuplicateDetectionService
{
    Task<DuplicateDetectionResult> ScanAsync(
        CancellationToken cancellationToken);

    Task<MergePreview> AnalyzeGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Interface

Exibir:

-   Grupos de duplicatas
-   Jogo recomendado
-   Economia de espaço
-   Filtros
-   Ações em lote

------------------------------------------------------------------------

# Eventos

-   DuplicateScanStarted
-   DuplicateFound
-   DuplicateGroupCreated
-   DuplicateResolutionCompleted

------------------------------------------------------------------------

# Casos de Erro

-   Biblioteca indisponível.
-   Índice inconsistente.
-   Cancelamento.
-   Falha de leitura.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Detecta duplicatas em todos os formatos suportados.
-   [ ] Agrupa automaticamente.
-   [ ] Sugere melhor cópia.
-   [ ] Suporta ações em lote.
-   [ ] Gera relatório.

------------------------------------------------------------------------

# Testes

## Unitários

-   Comparação por Game ID.
-   Comparação por hash.
-   Agrupamento.
-   Recomendação.

## Integração

-   Biblioteca com duplicatas.
-   Múltiplos dispositivos.
-   Formatos mistos.
-   Grande volume (\>5000 jogos).

------------------------------------------------------------------------

# Dependências

-   016-GameLibrary.md
-   F002-ScanGameLibrary.md
-   F013-GameDetails.md
-   F015-SearchAndFilter.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Agrupamento validado.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

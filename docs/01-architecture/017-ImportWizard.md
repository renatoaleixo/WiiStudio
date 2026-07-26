# 017 - Import Wizard Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir o assistente oficial de importação do WiiStudio. Toda entrada de
jogos no sistema deverá passar por este fluxo, garantindo validação,
segurança, rastreabilidade e excelente experiência do usuário.

------------------------------------------------------------------------

# Objetivos

-   Guiar o usuário passo a passo.
-   Detectar automaticamente o formato do arquivo.
-   Validar antes de qualquer escrita.
-   Minimizar riscos de corrupção.
-   Permitir cancelamento em qualquer etapa.

------------------------------------------------------------------------

# Formatos Suportados

  Formato   Suporte
  --------- -------------------
  ISO       ✅
  WBFS      ✅
  RVZ       ✅ (via provider)
  NKIT      ✅ (restauração)

------------------------------------------------------------------------

# Fluxo Geral

``` text
Selecionar Arquivos
        │
        ▼
Detectar Formato
        │
        ▼
Validar Arquivos
        │
        ▼
Selecionar Destino
        │
        ▼
Resumo
        │
        ▼
Importação
        │
        ▼
Conclusão
```

------------------------------------------------------------------------

# Etapas

## 1. Seleção

Permitir:

-   Arquivos
-   Pastas
-   Drag & Drop

Detectar múltiplos jogos automaticamente.

------------------------------------------------------------------------

## 2. Validação

Verificar:

-   Formato suportado
-   Integridade básica
-   Espaço disponível
-   Duplicidade
-   Acesso ao dispositivo

Falhas impedem o avanço.

------------------------------------------------------------------------

## 3. Destino

Selecionar:

-   HD WBFS
-   Pasta `wbfs`
-   Diretório temporário (quando necessário)

Exibir:

-   Espaço total
-   Espaço livre
-   Espaço necessário

------------------------------------------------------------------------

## 4. Resumo

Exibir:

-   Jogos encontrados
-   Formato de origem
-   Conversões necessárias
-   Espaço estimado
-   Tempo estimado

------------------------------------------------------------------------

## 5. Importação

Exibir:

-   Barra de progresso
-   Arquivo atual
-   Velocidade
-   Tempo restante
-   Botão Cancelar

Operação totalmente assíncrona.

------------------------------------------------------------------------

## 6. Conclusão

Resumo:

-   Jogos importados
-   Jogos ignorados
-   Falhas
-   Tempo total
-   Link para logs

------------------------------------------------------------------------

# Máquina de Estados

``` text
Idle
 ↓
Selecting
 ↓
Validating
 ↓
Ready
 ↓
Importing
 ↓
Completed
  └── Failed
  └── Cancelled
```

------------------------------------------------------------------------

# Rollback

Caso uma operação falhe:

-   preservar biblioteca consistente;
-   remover arquivos temporários;
-   registrar histórico;
-   exibir causa ao usuário.

------------------------------------------------------------------------

# Eventos

-   ImportStarted
-   ImportProgressChanged
-   ImportCompleted
-   ImportCancelled
-   ImportFailed

------------------------------------------------------------------------

# Interfaces

``` csharp
public interface IImportWizardService
{
    Task<ImportPreview> AnalyzeAsync(
        IReadOnlyList<string> files,
        CancellationToken cancellationToken);

    Task<ImportResult> ExecuteAsync(
        ImportRequest request,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca iniciar importação sem validação.
-   Nunca bloquear a UI.
-   Sempre permitir cancelamento.
-   Sempre registrar histórico.
-   Toda escrita passa pela camada Application.

------------------------------------------------------------------------

# Critérios de Aceite

-   Assistente em etapas.
-   Suporte a múltiplos arquivos.
-   Rollback documentado.
-   Progresso em tempo real.
-   Cancelamento funcional.

------------------------------------------------------------------------

# Checklist

-   [x] Fluxo definido
-   [x] Máquina de estados
-   [x] Rollback
-   [x] Eventos
-   [x] Interfaces
-   [x] Critérios de aceite

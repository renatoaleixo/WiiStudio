# F011 - Repair Game

> Feature ID: F011\
> Status: Implemented and Homologated\
> Priority: High\
> Epic: Library Management

# Objetivo

Detectar e reparar automaticamente problemas recuperáveis em jogos da
biblioteca, reduzindo a necessidade de intervenção manual e preservando
a integridade dos dados.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Executar reparo em um único jogo.

## RF002

Executar reparo em lote.

## RF003

Exibir prévia das ações antes da execução.

## RF004

Registrar todas as alterações realizadas.

## RF005

Atualizar automaticamente a biblioteca após o reparo.

------------------------------------------------------------------------

# Escopo de Reparo

-   Reconstrução de metadados.
-   Atualização de capas.
-   Limpeza de cache inconsistente.
-   Revalidação do Game ID.
-   Reimportação quando recomendada.
-   Revalidação via Wiimms.

Não tenta reparar mídias fisicamente corrompidas.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
      ▼
Analisar problemas
      │
      ▼
Exibir plano de reparo
      │
      ▼
Confirmar
      │
      ▼
Executar reparo
      │
      ▼
Validar resultado
      │
      ▼
Atualizar biblioteca
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
VerifyGameService
      │
RepairPlanner
      │
RepairGameService
      │
Wiimms / Providers
      │
Library Refresh
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Analyzing
-   Planning
-   Repairing
-   Validating
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Estratégias de Reparo

  Problema                   Estratégia
  -------------------------- ------------------------
  Metadados inconsistentes   Atualizar Provider
  Capa ausente               Download
  Cache inválido             Reconstruir
  ISO inválida               Solicitar reimportação
  WBFS inconsistente         Validar via Wiimms

------------------------------------------------------------------------

# Eventos

-   RepairRequested
-   RepairPlanGenerated
-   RepairStarted
-   RepairProgressChanged
-   RepairCompleted
-   RepairFailed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IRepairGameService
{
    Task<RepairPlan> AnalyzeAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);

    Task<RepairResult> ExecuteAsync(
        RepairPlan plan,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Interface do Usuário

Exibir:

-   Problemas encontrados
-   Ações propostas
-   Riscos
-   Tempo estimado
-   Barra de progresso
-   Resultado final

------------------------------------------------------------------------

# Casos de Erro

-   Provider indisponível.
-   Falha do Wiimms.
-   Dispositivo removido.
-   Arquivo irrecuperável.
-   Cancelamento.

Registrar logs estruturados e manter histórico.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Plano de reparo gerado.
-   [x] Reparo executado com confirmação.
-   [x] Biblioteca atualizada.
-   [x] Relatório final disponível.
-   [x] Histórico registrado.

------------------------------------------------------------------------

# Testes

## Unitários

-   Geração do plano.
-   Estratégias de reparo.
-   Cancelamento.
-   Resultado.

## Integração

-   Reparo de metadados.
-   Download de capas.
-   Cache corrompido.
-   Falha parcial.
-   Dispositivo removido.

------------------------------------------------------------------------

# Dependências

-   F010-VerifyGameIntegrity.md
-   012-WiimmsIntegration.md
-   014-GameTDB.md
-   020-Diagnostics.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Plano de reparo funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

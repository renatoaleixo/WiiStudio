# F010 - Verify Game Integrity

> Feature ID: F010\
> Status: Implemented and Homologated\
> Priority: High\
> Epic: Library Management

# Objetivo

Verificar a integridade dos jogos armazenados na biblioteca do
WiiStudio, identificando arquivos corrompidos, incompletos,
incompatíveis ou inconsistentes antes de operações como conversão,
exportação ou execução.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Verificar um jogo individual.

## RF002

Verificar múltiplos jogos em lote.

## RF003

Executar validação utilizando o Wiimms sempre que possível.

## RF004

Detectar arquivos incompletos ou corrompidos.

## RF005

Gerar relatório detalhado.

## RF006

Sugerir ações corretivas quando aplicável.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Operação assíncrona.
-   Cancelável.
-   Não bloquear a interface.
-   Escalável para grandes bibliotecas.

------------------------------------------------------------------------

# Escopo das Verificações

-   Header do disco
-   Game ID
-   Região
-   Estrutura do arquivo
-   Tamanho esperado
-   Integridade via Wiimms
-   Leitura básica de partições
-   Compatibilidade do formato

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Selecionar jogo(s)
      │
      ▼
Executar verificação
      │
      ▼
Analisar resultados
      │
      ▼
Gerar relatório
      │
      ▼
Oferecer ações corretivas
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
GameLibrary
     │
VerifyGameViewModel
     │
IVerifyGameService
     │
IWiimmsService
     │
ProcessRunner
     │
IntegrityReport
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Preparing
-   Verifying
-   GeneratingReport
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Níveis de Resultado

  Status        Descrição
  ------------- ------------------------
  Healthy       Arquivo íntegro
  Warning       Pequena inconsistência
  Corrupted     Arquivo inválido
  Unsupported   Formato não suportado

------------------------------------------------------------------------

# Eventos

-   VerificationStarted
-   VerificationProgressChanged
-   VerificationCompleted
-   VerificationFailed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IVerifyGameService
{
    Task<VerificationReport> VerifyAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Interface do Usuário

Exibir:

-   Barra de progresso
-   Lista de verificações
-   Status por jogo
-   Relatório detalhado
-   Botão "Exportar Relatório"
-   Botão "Reparar" (quando disponível)

------------------------------------------------------------------------

# Casos de Erro

-   Arquivo inacessível.
-   Wiimms indisponível.
-   Dispositivo removido.
-   Leitura interrompida.

Todos os erros devem ser registrados em log estruturado.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Verificação individual.
-   [x] Verificação em lote.
-   [x] Relatório exportável.
-   [x] Integração com Wiimms.
-   [x] Sugestões de reparo.

------------------------------------------------------------------------

# Testes

## Unitários

-   Arquivo válido.
-   Arquivo corrompido.
-   Cancelamento.
-   Relatório.

## Integração

-   Biblioteca saudável.
-   Biblioteca com arquivos inválidos.
-   Dispositivo removido durante verificação.

------------------------------------------------------------------------

# Dependências

-   011-WBFS.md
-   012-WiimmsIntegration.md
-   020-Diagnostics.md
-   F002-ScanGameLibrary.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Relatórios gerados.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

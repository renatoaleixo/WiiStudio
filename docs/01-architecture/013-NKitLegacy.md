# 013 - NKit Legacy Support

> Status: Accepted Version: 1.0.0

# Objetivo

Definir a estratégia oficial para suporte ao formato **NKit** no
WiiStudio.

## Princípios

-   NKit é um formato legado.
-   O WiiStudio não implementará o algoritmo NKit.
-   A restauração será feita por uma ferramenta externa configurável.
-   Todo o fluxo será transparente para o usuário.

------------------------------------------------------------------------

# Escopo

Suportar:

-   `.nkit.iso`
-   `.nkit.gcz` (quando suportado pela ferramenta externa)

Fluxo oficial:

``` text
NKit
   │
   ▼
ISO Restaurada
   │
   ▼
Wiimms
   │
   ▼
WBFS / HD
```

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
Application
      │
INKitService
      │
NKitService
      │
ProcessRunner
      │
NKit CLI
```

------------------------------------------------------------------------

# Descoberta da Ferramenta

Ordem:

1.  Caminho configurado pelo usuário.
2.  Diretório tools/nkit.
3.  PATH do sistema.

Na inicialização:

-   verificar existência;
-   validar versão (quando disponível);
-   registrar em log.

------------------------------------------------------------------------

# Interface

``` csharp
public interface INKitService
{
    Task<bool> IsAvailableAsync(CancellationToken ct);
    Task<RestoreResult> RestoreAsync(
        string source,
        string destination,
        CancellationToken ct);
}
```

------------------------------------------------------------------------

# Fluxo de Importação

1.  Usuário seleciona arquivo `.nkit.iso`.
2.  Detectar formato.
3.  Criar diretório temporário.
4.  Restaurar para ISO.
5.  Validar ISO.
6.  Enviar ISO ao Wiimms.
7.  Limpar arquivos temporários.
8.  Atualizar biblioteca.

------------------------------------------------------------------------

# Arquivos Temporários

Utilizar diretório específico:

``` text
temp/nkit/
```

Remover automaticamente após sucesso.

Em caso de falha, permitir manter os arquivos para diagnóstico.

------------------------------------------------------------------------

# Tratamento de Erros

Mapear:

  Situação               Exceção
  ---------------------- -----------------------------
  Executável ausente     NKitNotInstalledException
  ISO inválida           InvalidRestoredIsoException
  Falha na restauração   NKitRestoreFailedException
  Cancelamento           OperationCanceledException

------------------------------------------------------------------------

# UX

Ao importar um NKit:

-   informar que será restaurado;
-   exibir progresso;
-   permitir cancelamento;
-   mostrar tempo estimado.

------------------------------------------------------------------------

# Logging

Registrar:

-   arquivo de origem;
-   duração;
-   tamanho restaurado;
-   código de saída;
-   mensagens da ferramenta.

------------------------------------------------------------------------

# Segurança

-   Nunca sobrescrever ISO existente sem confirmação.
-   Validar espaço livre antes da restauração.
-   Limpar arquivos temporários ao finalizar.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca implementar algoritmo NKit.
-   Sempre utilizar ProcessRunner.
-   Nunca chamar a ferramenta diretamente da UI.
-   Toda restauração deve passar pelo INKitService.

------------------------------------------------------------------------

# Critérios de Aceite

-   Fluxo transparente ao usuário.
-   Cancelamento suportado.
-   Limpeza automática de temporários.
-   Integração desacoplada.
-   Testável com FakeProcessRunner.

------------------------------------------------------------------------

# Checklist

-   [x] Interface definida
-   [x] Fluxo documentado
-   [x] Tratamento de erros
-   [x] Logging
-   [x] UX definida

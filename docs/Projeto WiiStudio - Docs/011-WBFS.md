# 011 - WBFS Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir como o WiiStudio trata dispositivos e arquivos WBFS.

------------------------------------------------------------------------

# Escopo

O WiiStudio deverá suportar dois cenários:

1.  Partição WBFS (legado)
2.  FAT32/NTFS/exFAT contendo pasta `wbfs/`

Ambos devem ser tratados como cidadãos de primeira classe.

------------------------------------------------------------------------

# Filosofia

O WiiStudio **não implementará o formato WBFS**.

Toda operação será delegada ao Wiimms ISO Tools (`wit`/`wwt`).

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
Application
      │
IWbfsService
      │
WbfsService
      │
ProcessRunner
      │
wit / wwt
```

------------------------------------------------------------------------

# Operações

## Leitura

-   Detectar formato
-   Validar integridade
-   Listar jogos
-   Calcular espaço livre

## Escrita

-   Adicionar jogo
-   Remover jogo
-   Verificar duplicidade
-   Atualizar índice

## Conversão

Suportar:

-   ISO → WBFS
-   WBFS → ISO
-   RVZ → WBFS (via fluxo dedicado)
-   NKIT → ISO → WBFS

------------------------------------------------------------------------

# Regras

-   Nunca sobrescrever jogo existente sem confirmação.
-   Validar espaço antes de copiar.
-   Operações devem ser canceláveis.
-   Nunca bloquear a UI.

------------------------------------------------------------------------

# Integridade

Antes de qualquer gravação:

1.  Detectar dispositivo.
2.  Validar formato.
3.  Verificar escrita.
4.  Verificar espaço.
5.  Executar operação.

Após:

1.  Validar retorno.
2.  Atualizar biblioteca.
3.  Registrar histórico.

------------------------------------------------------------------------

# Compatibilidade

## Obrigatória

-   HD WBFS legado
-   Pendrive WBFS
-   FAT32 + wbfs/
-   NTFS + wbfs/
-   exFAT + wbfs/

------------------------------------------------------------------------

# Erros

Criar exceções específicas:

-   InvalidWbfsException
-   UnsupportedFileSystemException
-   DuplicateGameException
-   NotEnoughSpaceException

------------------------------------------------------------------------

# Segurança

Nunca executar operações destrutivas automaticamente.

Remoções exigem confirmação.

------------------------------------------------------------------------

# Interface

``` csharp
public interface IWbfsService
{
    Task ScanAsync(CancellationToken cancellationToken);
    Task ImportAsync(string path, CancellationToken cancellationToken);
    Task RemoveAsync(string gameId, CancellationToken cancellationToken);
    Task VerifyAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca manipular WBFS diretamente.
-   Sempre utilizar Wiimms.
-   Toda operação deve passar pelo ProcessRunner.
-   Toda escrita deve gerar log.

------------------------------------------------------------------------

# Critérios de Aceite

-   Compatível com HD legado.
-   Compatível com pasta wbfs.
-   Operações seguras.
-   Cancelamento suportado.
-   Logs completos.

------------------------------------------------------------------------

# Checklist

-   [ ] Fluxo de leitura
-   [ ] Fluxo de escrita
-   [ ] Estratégia de validação
-   [ ] Compatibilidade documentada
-   [ ] Interface definida

# 012 - Wiimms Integration Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a integração oficial entre o WiiStudio e o **Wiimms ISO Tools**
(`wit` e `wwt`).

O Wiimms será a engine oficial para manipulação de jogos e dispositivos
Wii. O WiiStudio nunca implementará o formato WBFS nem executará
operações diretamente sobre o disco.

------------------------------------------------------------------------

# Escopo

O módulo deve abstrair completamente o uso das ferramentas externas.

A UI e a Application nunca conhecerão comandos como:

``` bash
wit list
wwt add
wit copy
```

Toda comunicação ocorrerá através de interfaces.

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
Application
      │
IWiimmsService
      │
WiimmsService
      │
ProcessRunner
      │
wit / wwt
```

------------------------------------------------------------------------

# Descoberta dos Executáveis

Ordem de busca:

1.  Caminho configurado pelo usuário.
2.  PATH do sistema.
3.  Diretório tools/.
4.  Diretório da aplicação.

Ao iniciar o WiiStudio:

-   Validar existência.
-   Validar versão.
-   Registrar no log.

------------------------------------------------------------------------

# Interfaces

``` csharp
public interface IWiimmsService
{
    Task<Version> GetVersionAsync(CancellationToken ct);
    Task<IReadOnlyList<GameDto>> ListGamesAsync(string device, CancellationToken ct);
    Task ImportAsync(string source, string device, CancellationToken ct);
    Task RemoveAsync(string gameId, string device, CancellationToken ct);
    Task VerifyAsync(string device, CancellationToken ct);
}
```

------------------------------------------------------------------------

# ProcessRunner

Todas as chamadas externas passam pelo ProcessRunner.

Responsabilidades:

-   Escape de argumentos.
-   Timeout.
-   CancellationToken.
-   Captura de stdout.
-   Captura de stderr.
-   ExitCode.
-   Tempo de execução.

------------------------------------------------------------------------

# Parsing

Nunca utilizar parsing baseado em posição.

Preferir:

-   regex nomeadas;
-   formatos estruturados quando disponíveis;
-   adaptadores separados por comando.

------------------------------------------------------------------------

# Tratamento de Erros

Mapear erros do Wiimms para exceções da aplicação.

  Erro Wiimms           Exceção
  --------------------- -----------------------------
  Disco inexistente     DeviceNotFoundException
  Espaço insuficiente   NotEnoughSpaceException
  Formato inválido      InvalidGameFormatException
  Executável ausente    WiimmsNotInstalledException

------------------------------------------------------------------------

# Logging

Registrar:

-   comando executado (sem dados sensíveis);
-   duração;
-   código de saída;
-   quantidade de jogos retornados;
-   erros.

------------------------------------------------------------------------

# Performance

-   Operações assíncronas.
-   Nunca bloquear a UI.
-   Atualização de progresso quando disponível.
-   Suporte a cancelamento.

------------------------------------------------------------------------

# Testabilidade

Criar abstração para ProcessRunner.

Nos testes unitários utilizar FakeProcessRunner.

Nunca depender do wit instalado para testes da camada Application.

------------------------------------------------------------------------

# Compatibilidade

Sistemas:

-   Linux
-   Windows
-   macOS

Versões do Wiimms devem ser verificadas na inicialização.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca chamar Process diretamente.
-   Nunca montar comandos por concatenação insegura.
-   Sempre registrar logs.
-   Sempre utilizar CancellationToken.
-   Toda integração deve ser mockável.

------------------------------------------------------------------------

# Critérios de Aceite

-   Descoberta automática do wit.
-   Execução segura.
-   Parse consistente.
-   Tratamento de falhas.
-   Cobertura de testes para adaptadores.

------------------------------------------------------------------------

# Checklist

-   [ ] Descoberta de executáveis
-   [ ] ProcessRunner reutilizado
-   [ ] Parsing documentado
-   [ ] Exceções mapeadas
-   [ ] Logging definido
-   [ ] Testabilidade garantida

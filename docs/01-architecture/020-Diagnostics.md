# 020 - Diagnostics & Health Monitoring Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir o subsistema de diagnóstico do WiiStudio responsável por
monitorar a saúde da aplicação, integrações, dispositivos e ferramentas
externas.

------------------------------------------------------------------------

# Objetivos

-   Facilitar suporte técnico.
-   Detectar problemas antes que afetem o usuário.
-   Centralizar informações de diagnóstico.
-   Gerar relatórios para depuração.

------------------------------------------------------------------------

# Dashboard de Saúde

O sistema deverá apresentar um painel contendo:

  Item              Status
  ----------------- ---------
  Wiimms            ✅ / ❌
  NKit              ✅ / ❌
  GameTDB           ✅ / ❌
  SQLite            ✅ / ❌
  USB               ✅ / ❌
  Espaço em Disco   ✅ / ⚠️

------------------------------------------------------------------------

# Verificações

## Ambiente

-   Versão do WiiStudio
-   Sistema operacional
-   Arquitetura
-   Runtime .NET
-   Memória disponível

## Ferramentas

-   wit
-   wwt
-   NKit

Validar:

-   instalação
-   versão
-   permissões
-   localização

## Dispositivos

-   dispositivos encontrados
-   sistema de arquivos
-   capacidade
-   espaço livre
-   status de montagem

------------------------------------------------------------------------

# Auto Diagnostics

Na inicialização executar:

1.  Validar banco.
2.  Validar providers.
3.  Validar ferramentas.
4.  Validar permissões.
5.  Publicar relatório.

------------------------------------------------------------------------

# Relatório

Formato JSON:

``` json
{
  "applicationVersion": "1.0.0",
  "platform": "Linux",
  "dotnet": "9.0",
  "providers": [],
  "devices": [],
  "errors": []
}
```

Também permitir exportação em texto.

------------------------------------------------------------------------

# Logging

Registrar:

-   duração dos testes
-   falhas
-   exceções
-   ambiente
-   versões

------------------------------------------------------------------------

# Interface

``` csharp
public interface IDiagnosticsService
{
    Task<DiagnosticsReport> RunAsync(
        CancellationToken cancellationToken);

    Task ExportAsync(
        string path,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# UX

Tela "Diagnóstico":

-   Executar diagnóstico
-   Exportar relatório
-   Copiar para área de transferência
-   Abrir pasta de logs

------------------------------------------------------------------------

# Casos de Erro

-   Wiimms não encontrado
-   Banco inacessível
-   USB indisponível
-   Cache corrompido
-   Provider indisponível

Cada erro deve sugerir uma ação corretiva.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca ocultar erros críticos.
-   Sempre registrar informações suficientes para suporte.
-   Não expor caminhos sensíveis sem consentimento do usuário.
-   Diagnósticos devem ser independentes da UI.

------------------------------------------------------------------------

# Critérios de Aceite

-   Diagnóstico executado em menos de 5 segundos em ambiente normal.
-   Relatório exportável.
-   Estado de todas as dependências visível.
-   Cobertura de testes para cenários de falha.

------------------------------------------------------------------------

# Checklist

-   [x] Dashboard definido
-   [x] Relatório estruturado
-   [x] Interface especificada
-   [x] Exportação documentada
-   [x] Casos de erro definidos

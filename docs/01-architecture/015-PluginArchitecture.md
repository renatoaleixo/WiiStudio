# 015 - Plugin Architecture Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a arquitetura oficial de plugins do WiiStudio para permitir
evolução do produto sem alterar o núcleo da aplicação.

------------------------------------------------------------------------

# Princípios

-   Núcleo pequeno.
-   Extensibilidade por contratos.
-   Plugins isolados.
-   Carregamento dinâmico.
-   Compatibilidade entre versões.

------------------------------------------------------------------------

# Casos de Uso

Os plugins poderão adicionar:

-   Novos conversores.
-   Novos provedores de metadados.
-   Novos provedores de capas.
-   Ferramentas de diagnóstico.
-   Exportadores.
-   Importadores.
-   Temas.

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
      │
Application
      │
PluginManager
      │
┌───────────────┬───────────────┬───────────────┐
│ Conversion    │ Metadata      │ UI            │
│ Plugin        │ Plugin        │ Plugin        │
└───────────────┴───────────────┴───────────────┘
```

------------------------------------------------------------------------

# Estrutura

``` text
plugins/
    PluginA/
    PluginB/
    PluginC/
```

Cada plugin deve possuir:

``` text
plugin.json
Plugin.dll
Assets/
```

------------------------------------------------------------------------

# Manifesto

Exemplo:

``` json
{
  "id": "wiistudio.plugin.example",
  "name": "Example Plugin",
  "version": "1.0.0",
  "author": "Open Source",
  "minimumVersion": "1.0.0"
}
```

------------------------------------------------------------------------

# Contratos

Todos os plugins devem implementar:

``` csharp
public interface IPlugin
{
    string Name { get; }
    Version Version { get; }
    Task InitializeAsync(CancellationToken cancellationToken);
}
```

Interfaces especializadas:

-   IConversionProvider
-   IGameMetadataProvider
-   ICoverProvider
-   IThemeProvider
-   IDiagnosticProvider

------------------------------------------------------------------------

# Ciclo de Vida

1.  Descobrir plugins.
2.  Validar manifesto.
3.  Verificar compatibilidade.
4.  Carregar.
5.  Inicializar.
6.  Registrar serviços.
7.  Disponibilizar recursos.

------------------------------------------------------------------------

# Segurança

-   Não carregar plugins incompatíveis.
-   Isolar falhas.
-   Registrar todas as exceções.
-   Nunca impedir a inicialização do WiiStudio por erro de plugin.

------------------------------------------------------------------------

# Atualizações

Cada plugin possui versão independente.

Compatibilidade baseada em Semantic Versioning.

------------------------------------------------------------------------

# Logging

Registrar:

-   plugin carregado
-   plugin ignorado
-   incompatibilidades
-   tempo de carregamento

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca depender de um plugin específico.
-   Programar sempre contra interfaces.
-   O núcleo não referencia plugins.
-   Plugins nunca alteram entidades de domínio diretamente.

------------------------------------------------------------------------

# Critérios de Aceite

-   Descoberta automática.
-   Carregamento dinâmico.
-   Compatibilidade validada.
-   Falhas isoladas.
-   Extensível sem recompilar o núcleo.

------------------------------------------------------------------------

# Checklist

-   [x] Manifesto definido
-   [x] Interfaces documentadas
-   [x] Ciclo de vida especificado
-   [x] Estratégia de segurança
-   [x] Logging definido

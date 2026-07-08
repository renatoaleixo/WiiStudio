# Plugin SDK Reference

Implementação inicial dos documentos `F071` até `F080`.

## Projetos

- `WiiStudio.Plugin.Abstractions`: contratos públicos do SDK, manifesto, permissões, providers, UI extensions, diagnostics e marketplace metadata.
- `WiiStudio.Plugin.Hosting`: leitura e validação de manifesto, discovery, registry, storage isolado, contexto e loader.
- `WiiStudio.Plugin.Security`: política padrão de risco e serviço de permissões sem concessão implícita para permissões sensíveis.
- `WiiStudio.Plugin.Diagnostics`: eventos e relatório de diagnóstico de plugins.

## Fluxo Implementado

1. Discovery procura `plugin.json` em `installed`, `disabled` e `quarantine`.
2. Manifestos são lidos e validados antes de qualquer assembly.
3. Duplicidades são marcadas como `Duplicate`.
4. Somente plugins `ReadyForLoad` chegam ao loader.
5. O loader valida permissões antes de carregar DLL.
6. O plugin é inicializado com `PluginContext` isolado.
7. Falhas viram `PluginLoadResult` e eventos de diagnóstico.

## Segurança Inicial

- `entryAssembly` precisa ser relativo, `.dll` e permanecer dentro da pasta do plugin.
- Permissões sensíveis retornam `RequiresUserApproval` por padrão.
- `FileSystemPluginStorage` bloqueia path traversal.
- Discovery nunca carrega assemblies.

## Escopo Futuro

- UI do gerenciador de plugins.
- Persistência de aprovações/revogações de permissões.
- Assinatura e verificação de pacote de marketplace.
- Registro visual de providers e extensões no shell.

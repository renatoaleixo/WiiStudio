# F117 - Plugin Marketplace UI

> Feature ID: F117  
> Status: Ready for Development  
> Priority: High  
> Epic: Plugin SDK / Marketplace Preparation  
> Area: UI, Plugin Hosting, Plugin Security, Diagnostics, Settings  
> Depends on: F071-F080, F084

# Objetivo

Criar uma tela completa de gerenciamento de plugins do WiiStudio, cobrindo instalação,
habilitar/desabilitar, permissões, diagnóstico, logs, atualização e remoção.

A base do Plugin SDK já existe nos documentos, mas falta uma experiência visual para o usuário
gerenciar plugins com segurança.

---

# Problema

Sem uma UI de gerenciamento, plugins ficam difíceis de:

- instalar;
- validar;
- habilitar/desabilitar;
- entender permissões;
- diagnosticar falhas;
- remover;
- atualizar;
- auditar logs.

---

# Resultado Esperado

Criar tela:

```text
Plugins
├── Instalados
├── Disponíveis, futuro
├── Permissões
├── Logs
├── Diagnóstico
└── Quarentena
```

---

# Escopo

## Incluído

- listar plugins instalados;
- instalar plugin local;
- validar manifest;
- habilitar/desabilitar plugin;
- exibir permissões;
- aprovar/revogar permissões;
- diagnóstico por plugin;
- logs por plugin;
- remover plugin;
- quarentena de plugin inválido;
- integração com Notification Center.

## Fora do Escopo Inicial

- marketplace remoto público obrigatório;
- pagamento/licenciamento;
- ranking/reviews;
- execução de plugin não confiável sem validação;
- auto-update silencioso de plugin.

---

# Tela de Plugins

Informações por plugin:

- nome;
- versão;
- autor;
- descrição;
- status;
- permissões solicitadas;
- permissões concedidas;
- capabilities;
- data de instalação;
- último erro;
- ações.

Status:

```text
Installed
Enabled
Disabled
Invalid
Quarantined
UpdateAvailable
FailedToLoad
PermissionRequired
```

---

# Instalar Plugin Local

Fluxo:

```text
Selecionar pacote/pasta
    ↓
Validar manifest
    ↓
Validar assinatura/checksum, quando disponível
    ↓
Exibir permissões solicitadas
    ↓
Usuário confirma
    ↓
Instalar em pasta de plugins
    ↓
Registrar plugin
    ↓
Carregar se habilitado
```

---

# Permissões

Exibir permissões solicitadas:

```text
FileSystem.Read
FileSystem.Write
MetadataProvider
ConversionProvider
NetworkAccess
UIExtension
ExternalProcess
```

Cada permissão deve ter:

- descrição;
- risco;
- status;
- botão conceder/revogar quando permitido.

---

# Logs por Plugin

Tela deve permitir filtrar logs por:

- PluginId;
- severidade;
- período;
- evento;
- erro.

Exemplo:

```text
Plugin: MyMetadataProvider
Último erro: Timeout ao consultar provider
```

---

# Diagnóstico por Plugin

Mostrar:

- manifest válido;
- permissões válidas;
- assembly carregado;
- capabilities registradas;
- falhas de inicialização;
- tempo de loading;
- eventos recentes;
- status de unload.

---

# Quarentena

Plugin deve ir para quarentena quando:

- manifest inválido;
- permissão proibida;
- falha crítica no loading;
- exceções repetidas;
- assinatura inválida quando exigida.

A UI deve permitir:

- ver motivo;
- remover;
- tentar reparar/revalidar;
- abrir pasta;
- copiar diagnóstico.

---

# Contratos

## IPluginManagementService

```csharp
public interface IPluginManagementService
{
    Task<IReadOnlyList<PluginManagementInfo>> GetInstalledAsync(
        CancellationToken cancellationToken = default);

    Task<PluginInstallResult> InstallFromPathAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task EnableAsync(string pluginId, CancellationToken cancellationToken = default);

    Task DisableAsync(string pluginId, CancellationToken cancellationToken = default);

    Task RemoveAsync(string pluginId, CancellationToken cancellationToken = default);
}
```

---

# UI Actions

- Instalar plugin;
- Habilitar;
- Desabilitar;
- Remover;
- Ver permissões;
- Ver logs;
- Ver diagnóstico;
- Abrir pasta;
- Revalidar;
- Colocar em quarentena.

---

# Segurança

Obrigatório:

- validar manifest;
- não carregar plugin durante discovery simples;
- não conceder permissão automaticamente;
- não permitir plugin elevar privilégio;
- isolar falhas;
- logs com PluginId;
- remover capabilities no disable/unload;
- confirmar remoção;
- bloquear path traversal;
- não instalar de origem remota sem provider seguro.

---

# Integração com Marketplace Futuro

A UI deve estar preparada para fonte remota futura:

```text
PluginMarketplaceProvider
```

Mas a primeira versão pode suportar somente instalação local.

---

# Critérios de Aceite

- [ ] Tela lista plugins instalados.
- [ ] Plugin válido pode ser instalado localmente.
- [ ] Manifest inválido é bloqueado.
- [ ] Usuário vê permissões solicitadas.
- [ ] Plugin pode ser habilitado/desabilitado.
- [ ] Plugin pode ser removido.
- [ ] Logs por plugin são exibidos.
- [ ] Diagnóstico por plugin é exibido.
- [ ] Plugin inválido vai para quarentena.
- [ ] Testes usam plugins fake.

---

# Testes Esperados

- listar plugins;
- instalar plugin válido;
- bloquear manifest inválido;
- habilitar plugin;
- desabilitar plugin;
- remover plugin;
- revogar permissão;
- plugin falha no load;
- plugin em quarentena;
- logs filtrados por PluginId;
- diagnóstico preenchido.

---

# Regras para o Codex

- Não carregar assembly durante discovery.
- Não conceder permissão automaticamente.
- Não permitir plugin executar processo elevado.
- Sempre validar manifest.
- Sempre capturar falhas.
- Sempre registrar PluginId.
- Nunca instalar plugin remoto sem provider seguro.
- Criar testes com plugins fake.

---

# Dependências

- `F071-PluginSDKFoundation.md`
- `F072-PluginManifest.md`
- `F073-PluginDiscovery.md`
- `F074-PluginLoading.md`
- `F075-PluginPermissions.md`
- `F079-PluginDiagnostics.md`
- `F080-PluginMarketplacePreparation.md`
- `F084-PluginSecurity.md`

---

# Definition of Done

- Tela de plugins definida.
- Instalação local definida.
- Permissões e diagnóstico definidos.
- Logs por plugin definidos.
- Quarentena definida.
- Testes com fakes definidos.

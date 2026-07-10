# F123 - Remote Plugin Marketplace and Trust Policy

> Feature ID: F123  
> Status: Ready for Development  
> Priority: High  
> Epic: Plugin SDK / Marketplace / Security  
> Area: Plugins, Marketplace, Trust, Security, Update, Quarantine  
> Depends on: F117, F084

# Objetivo

Definir o marketplace remoto de plugins do WiiStudio e a política de confiança para
instalação, atualização, bloqueio, revogação e quarentena.

Esta feature expande `F117-PluginMarketplaceUI.md`, que cobre a UI local de plugins.

---

# Princípios

- Plugin remoto nunca é confiável por padrão.
- Manifest e pacote devem ser validados.
- Permissões devem ser explícitas.
- Usuário deve confirmar instalação.
- Plugin pode ser bloqueado/revogado.
- Falhas devem isolar o plugin.
- Plugins não podem elevar privilégio diretamente.
- Plugin não pode executar comando arbitrário.

---

# Fonte do Catálogo Remoto

URL recomendada:

```text
https://github.com/<ORG_OR_USER>/WiiStudio-Plugins/releases/latest/download/plugin-catalog.json
```

Ou domínio oficial futuro:

```text
https://plugins.wiistudio.app/catalog.json
```

Regras:

- HTTPS obrigatório;
- host allowlist;
- schema validado;
- assinatura do catálogo recomendada;
- cache local;
- fallback offline;
- não carregar plugin apenas por ele aparecer no catálogo.

---

# Formato do Catálogo

```json
{
  "schemaVersion": 1,
  "publishedAt": "2026-01-01T10:00:00Z",
  "catalogVersion": "1.0.0",
  "plugins": [
    {
      "pluginId": "com.example.metadata",
      "name": "Example Metadata Provider",
      "description": "Adds metadata from Example source.",
      "author": "Example",
      "version": "1.0.0",
      "minWiiStudioVersion": "1.0.0",
      "packageUrl": "https://...",
      "sha256": "...",
      "signatureUrl": "https://...",
      "permissions": [
        "NetworkAccess",
        "MetadataProvider"
      ],
      "riskLevel": "Medium",
      "status": "Active"
    }
  ],
  "revocations": [
    {
      "pluginId": "com.bad.plugin",
      "reason": "Unsafe behavior",
      "revokedAt": "2026-01-01T10:00:00Z"
    }
  ]
}
```

---

# Assinatura de Plugins

Cada plugin remoto deve ter:

- checksum SHA-256;
- assinatura do pacote quando possível;
- manifest interno;
- compatibilidade de versão;
- permissões declaradas.

Campos:

```text
sha256
signatureUrl
signingKeyId
signatureAlgorithm
```

---

# Instalação Segura

Fluxo:

```text
User selects plugin
    ↓
Show details and permissions
    ↓
User confirms
    ↓
Download package via HTTPS
    ↓
Validate checksum
    ↓
Validate signature
    ↓
Extract to temp
    ↓
Validate manifest
    ↓
Install disabled by default or ask enable
    ↓
Load in isolated flow
```

---

# Permissões

Permissões devem ser mostradas antes da instalação:

```text
NetworkAccess
FileSystemRead
FileSystemWrite
MetadataProvider
ConversionProvider
UIExtension
ExternalToolAccess
DiagnosticsAccess
```

Regras:

- nenhuma permissão é concedida implicitamente;
- permissões perigosas exigem aviso;
- revogação deve desabilitar capability;
- plugin não pode solicitar privilégio elevado diretamente;
- plugin não pode acessar formatação USB;
- plugin não pode executar `wwt`/`wit` privilegiado.

---

# Quarentena

Plugin deve ir para quarentena se:

- checksum inválido;
- assinatura inválida;
- manifest inválido;
- plugin revogado;
- permissões proibidas;
- falha crítica repetida;
- comportamento inseguro detectado.

Na quarentena:

- não carregar;
- não executar;
- mostrar motivo;
- permitir remover;
- permitir copiar diagnóstico.

---

# Bloqueio e Revogação

Catálogo remoto pode conter lista de revogações.

Ao atualizar catálogo:

```text
Check installed plugins
    ↓
If plugin is revoked
    ↓
Disable plugin
    ↓
Move to quarantine
    ↓
Notify user
```

Revogação deve ganhar prioridade sobre preferência local.

---

# Atualização de Plugins

Fluxo:

```text
Check catalog
    ↓
Compare versions
    ↓
Show update available
    ↓
User confirms
    ↓
Download/validate
    ↓
Disable old plugin
    ↓
Install new plugin
    ↓
Rollback if load fails
```

---

# Política de Confiança

Níveis:

```text
Official
Verified
Community
Unknown
Blocked
Revoked
```

## Official

Plugins mantidos pelo projeto WiiStudio.

## Verified

Plugins de terceiros validados manualmente.

## Community

Plugins listados, mas com menor garantia.

## Unknown

Instalação local ou fonte não reconhecida.

## Blocked / Revoked

Não podem ser carregados.

---

# Diagnóstico

Adicionar por plugin:

```text
Trust level
Catalog source
Checksum status
Signature status
Permission status
Revocation status
Quarantine status
Last load error
Last update check
```

---

# Segurança

Obrigatório:

- HTTPS;
- checksum obrigatório;
- assinatura para plugins oficiais/verificados;
- manifest obrigatório;
- permissões explícitas;
- quarentena;
- revogação;
- path traversal bloqueado;
- extração em temp;
- não carregar antes de validar;
- logs por PluginId;
- isolamento de falhas;
- bloqueio de operações privilegiadas por plugin.

---

# Testes Esperados

- catálogo válido;
- catálogo inválido;
- plugin válido;
- checksum inválido;
- assinatura inválida;
- plugin revogado;
- plugin em quarentena;
- permissões exibidas;
- permissões revogadas;
- update de plugin;
- rollback após falha;
- fallback offline.

---

# Critérios de Aceite

- [ ] Catálogo remoto definido.
- [ ] Formato JSON definido.
- [ ] Download de plugin exige HTTPS.
- [ ] Checksum obrigatório.
- [ ] Assinatura definida.
- [ ] Permissões exibidas.
- [ ] Plugin revogado é bloqueado.
- [ ] Quarentena funciona.
- [ ] Atualização de plugin definida.
- [ ] Política de confiança definida.
- [ ] Testes com fakes definidos.

---

# Regras para o Codex

- Nunca carregar plugin antes de validar.
- Nunca conceder permissão automaticamente.
- Nunca ignorar revogação.
- Nunca instalar plugin remoto sem checksum.
- Nunca aceitar HTTP.
- Sempre isolar falhas.
- Sempre registrar PluginId.
- Sempre usar quarentena para inseguro.
- Nunca permitir plugin acionar formatação, elevação ou comando arbitrário.

---

# Definition of Done

- Catálogo definido.
- Trust policy definida.
- Instalação/update definidos.
- Quarentena/revogação definidas.
- Testes definidos.

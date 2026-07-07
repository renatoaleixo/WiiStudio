# F080 - Plugin Marketplace Preparation

> Feature ID: F080
> Status: Ready for Development
> Priority: Medium
> Epic: Plugin SDK

# Objetivo

Preparar a arquitetura do WiiStudio para um futuro marketplace ou catálogo de plugins, sem implementar obrigatoriamente uma loja completa na primeira versão.

Este documento define metadados, requisitos de listagem, validação, segurança, atualização, instalação e apresentação de plugins para permitir evolução futura.

---

# Requisitos Funcionais

## RF001
Definir metadados necessários para catálogo de plugins.

## RF002
Preparar estrutura para listagem de plugins disponíveis.

## RF003
Preparar fluxo de instalação futura.

## RF004
Preparar fluxo de atualização futura.

## RF005
Definir requisitos de segurança para marketplace.

## RF006
Definir exibição de permissões antes da instalação.

## RF007
Definir compatibilidade por versão.

## RF008
Definir modelo de assinatura/verificação futura.

## RF009
Integrar com Plugin Manager.

## RF010
Não depender de marketplace para instalação manual.

---

# Escopo Inicial

A primeira versão pode suportar apenas:

- instalação manual de plugins;
- leitura de manifesto;
- exibição de plugins instalados;
- validação de permissões;
- diagnóstico;
- preparação de campos para catálogo futuro.

Marketplace online pode ser implementado depois.

---

# Metadados de Catálogo

```csharp
public sealed class PluginCatalogItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? IconUrl { get; init; }
    public string? DownloadUrl { get; init; }
    public string? RepositoryUrl { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = [];
    public IReadOnlyList<string> Capabilities { get; init; } = [];
}
```

---

# Categorias Futuras

- Metadata Providers
- Asset Providers
- Conversion Providers
- Diagnostics
- UI Extensions
- Automation
- Tools
- Integrations

---

# Estados de Marketplace

```csharp
public enum PluginCatalogState
{
    NotAvailable,
    Available,
    Installed,
    UpdateAvailable,
    Incompatible,
    Blocked,
    Deprecated
}
```

---

# Plugin Manager

A interface de plugins deve preparar áreas para:

- plugins instalados;
- plugins disponíveis;
- atualizações;
- permissões;
- diagnóstico;
- instalação manual;
- remoção;
- ativação/desativação.

---

# Instalação Manual

Fluxo recomendado:

```text
Selecionar pacote/pasta
    ↓
Validar manifesto
    ↓
Validar compatibilidade
    ↓
Exibir permissões
    ↓
Confirmar instalação
    ↓
Copiar para plugins/installed
    ↓
Executar discovery
```

---

# Atualização Futura

Fluxo futuro:

```text
Consultar catálogo
    ↓
Comparar versões
    ↓
Exibir changelog
    ↓
Exibir novas permissões
    ↓
Confirmar atualização
    ↓
Backup do plugin atual
    ↓
Instalar nova versão
    ↓
Validar
```

---

# Segurança

Marketplace futuro deve considerar:

- assinatura de plugin;
- checksum;
- origem confiável;
- revisão de permissões;
- bloqueio de versões vulneráveis;
- lista de plugins banidos;
- transporte HTTPS;
- validação de pacote.

---

# Assinatura Futura

Campos previstos:

```json
{
  "signature": {
    "algorithm": "ed25519",
    "signatureFile": "plugin.sig",
    "publicKeyId": "author-key-id"
  }
}
```

A assinatura não precisa ser implementada na primeira versão, mas a arquitetura deve permitir.

---

# Compatibilidade

Antes de exibir como instalável:

- validar versão mínima do WiiStudio;
- validar versão do Plugin SDK;
- validar plataforma;
- validar arquitetura;
- validar permissões;
- verificar se está bloqueado.

---

# Permissões

Marketplace deve exibir permissões antes da instalação.

Exemplo:

```text
Este plugin solicita:
- Acesso à rede
- Leitura da biblioteca de jogos
- Consulta de metadados
```

Permissões críticas devem ter destaque.

---

# Offline

A aplicação deve continuar funcionando sem marketplace.

Regras:

- plugins instalados continuam disponíveis;
- instalação manual continua possível;
- catálogo remoto é opcional;
- falha de rede não deve afetar app.

---

# Regras para o Codex

- Nunca tornar marketplace obrigatório.
- Sempre manter instalação manual.
- Sempre exibir permissões antes de instalar.
- Sempre validar manifesto antes de copiar/carregar.
- Sempre validar compatibilidade.
- Sempre preparar arquitetura para assinatura futura.
- Nunca baixar e executar plugin sem validação.
- Sempre registrar diagnóstico de instalação.
- Sempre preservar plugin anterior durante atualização quando possível.

---

# Casos de Erro

- Catálogo indisponível.
- Download falha.
- Checksum inválido.
- Plugin incompatível.
- Permissão crítica não aprovada.
- Instalação interrompida.
- Atualização falha.
- Plugin anterior não restaurado.
- Pacote inválido.
- Assinatura inválida futura.

---

# Critérios de Aceite

- [ ] Metadados de catálogo definidos.
- [ ] Categorias futuras definidas.
- [ ] Estados de catálogo definidos.
- [ ] Instalação manual documentada.
- [ ] Atualização futura documentada.
- [ ] Segurança futura definida.
- [ ] Assinatura futura prevista.
- [ ] Permissões antes da instalação definidas.
- [ ] Marketplace não é obrigatório.
- [ ] Integração com Plugin Manager prevista.

---

# Testes

## Unitários

- Catalog item válido.
- Plugin instalado detectado.
- Update available.
- Plugin incompatível.
- Permissão crítica identificada.
- Marketplace indisponível não quebra app.

## Integração

- Instalação manual.
- Plugin inválido bloqueado.
- Plugin instalado aparece no manager.
- Catálogo mockado.
- Atualização mockada.
- Falha de rede tratada.

---

# Dependências

- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F073-PluginDiscovery.md
- F075-PluginPermissions.md
- F079-PluginDiagnostics.md
- F084-PluginSecurity.md
- F089-UpdateSecurity.md

---

# Definition of Done

- Preparação para Marketplace documentada.
- Metadados definidos.
- Segurança futura prevista.
- Instalação manual preservada.
- Testes definidos.
- Documentação atualizada.

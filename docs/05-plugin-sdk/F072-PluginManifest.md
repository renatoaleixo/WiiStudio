# F072 - Plugin Manifest

> Feature ID: F072
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Plugin SDK

# Objetivo

Definir o formato oficial do manifesto de plugins do WiiStudio (`plugin.json`),
incluindo metadados, compatibilidade, permissões, capacidades, dependências,
pontos de extensão, localização, assets e regras de validação.

O manifesto é o primeiro contrato de confiança entre o WiiStudio e qualquer plugin.
Nenhum plugin deve ser carregado antes da validação do manifesto.

---

# Requisitos Funcionais

## RF001
Definir estrutura oficial do arquivo `plugin.json`.

## RF002
Validar campos obrigatórios.

## RF003
Validar compatibilidade com WiiStudio e Plugin SDK.

## RF004
Declarar permissões solicitadas.

## RF005
Declarar capacidades oferecidas.

## RF006
Declarar entry point do plugin.

## RF007
Declarar dependências opcionais.

## RF008
Declarar assets e recursos localizáveis.

## RF009
Bloquear plugin com manifesto inválido.

## RF010
Registrar diagnóstico detalhado de validação.

---

# Localização do Manifesto

Cada plugin deve possuir um arquivo:

```text
plugin.json
```

na raiz da pasta do plugin.

Exemplo:

```text
PluginFolder/
├── plugin.json
├── MyPlugin.dll
├── assets/
└── resources/
```

---

# Estrutura Oficial

```json
{
  "id": "com.example.metadata.provider",
  "name": "Example Metadata Provider",
  "description": "Provides metadata from an external source.",
  "version": "1.0.0",
  "author": "Example Author",
  "website": "https://example.com",
  "entryAssembly": "Example.Metadata.Provider.dll",
  "entryType": "Example.Metadata.Provider.Plugin",
  "pluginSdkVersion": "1.0.0",
  "minimumWiiStudioVersion": "1.0.0",
  "supportedPlatforms": ["linux", "windows"],
  "capabilities": ["metadata-provider"],
  "permissions": ["network-access", "access-game-library"],
  "dependencies": [],
  "resources": {
    "icon": "assets/icon.png",
    "localization": "resources/i18n"
  }
}
```

---

# Campos Obrigatórios

| Campo | Obrigatório | Descrição |
|------|-------------|-----------|
| id | Sim | Identificador único do plugin |
| name | Sim | Nome exibido |
| version | Sim | Versão semântica do plugin |
| entryAssembly | Sim | Assembly principal |
| entryType | Sim | Tipo que implementa `IWiiStudioPlugin` |
| pluginSdkVersion | Sim | Versão do SDK usada |
| minimumWiiStudioVersion | Sim | Versão mínima do WiiStudio |
| capabilities | Sim | Capacidades oferecidas |
| permissions | Sim | Permissões solicitadas |

---

# Campos Opcionais

| Campo | Descrição |
|------|-----------|
| description | Descrição do plugin |
| author | Autor |
| website | Site |
| repository | Repositório |
| license | Licença |
| supportedPlatforms | Plataformas suportadas |
| dependencies | Dependências de outros plugins |
| resources | Recursos adicionais |
| tags | Tags |
| category | Categoria |
| releaseNotes | Notas da versão |

---

# ID do Plugin

O ID deve ser único e estável.

Formato recomendado:

```text
com.author.plugin-name
```

Exemplos:

```text
com.wiistudio.gametdb-provider
com.community.rvzintegration
com.example.diagnostics
```

Regras:

- usar letras minúsculas;
- usar pontos como namespace;
- não usar espaços;
- não mudar entre versões;
- não duplicar plugin existente.

---

# Versionamento

O campo `version` deve seguir Semantic Versioning.

Exemplos:

```text
1.0.0
1.2.3
2.0.0-beta.1
```

Regras:

- versão inválida bloqueia plugin;
- downgrade deve ser tratado com cuidado;
- conflitos de versão devem ser diagnosticados.

---

# Entry Assembly

O campo `entryAssembly` indica o arquivo DLL principal.

Regras:

- deve existir na pasta do plugin;
- deve ser caminho relativo;
- não pode escapar da pasta do plugin;
- deve possuir extensão permitida;
- deve ser validado antes do carregamento.

---

# Entry Type

O campo `entryType` indica o tipo que implementa `IWiiStudioPlugin`.

Regras:

- deve existir no assembly;
- deve implementar contrato base;
- deve possuir construtor compatível;
- falha deve gerar diagnóstico claro.

---

# Compatibilidade

Campos de compatibilidade:

```json
{
  "pluginSdkVersion": "1.0.0",
  "minimumWiiStudioVersion": "1.0.0",
  "maximumWiiStudioVersion": "2.0.0",
  "supportedPlatforms": ["linux", "windows"]
}
```

Validações:

- SDK incompatível bloqueia plugin;
- versão mínima não atendida bloqueia plugin;
- plataforma não suportada bloqueia plugin;
- versão máxima pode gerar warning ou bloqueio conforme política.

---

# Capacidades

Capacidades oficiais iniciais:

```text
metadata-provider
asset-provider
conversion-provider
diagnostics-provider
ui-extension
automation
```

Um plugin pode declarar múltiplas capacidades.

---

# Permissões

Permissões devem ser declaradas explicitamente.

Exemplos:

```text
network-access
file-system-read
file-system-write
execute-process
access-game-library
access-metadata
access-assets
access-diagnostics
ui-extension
```

Permissões serão detalhadas em `F075-PluginPermissions.md`.

---

# Dependências

Plugins podem declarar dependências.

Exemplo:

```json
"dependencies": [
  {
    "id": "com.example.base",
    "minimumVersion": "1.0.0",
    "required": true
  }
]
```

Regras:

- dependência obrigatória ausente bloqueia plugin;
- dependência opcional ausente gera warning;
- ciclos de dependência devem ser rejeitados.

---

# Recursos

Recursos podem incluir:

- ícone;
- imagens;
- arquivos de localização;
- documentação;
- templates;
- schema adicional.

Exemplo:

```json
"resources": {
  "icon": "assets/icon.png",
  "localization": "resources/i18n",
  "documentation": "README.md"
}
```

Paths devem ser relativos e seguros.

---

# Localização

Plugins podem fornecer arquivos de localização.

Estrutura recomendada:

```text
resources/
└── i18n/
    ├── pt-BR.json
    ├── en-US.json
    └── es-ES.json
```

O plugin deve ter fallback para texto padrão.

---

# Schema JSON

Deve existir um schema oficial para validação.

Arquivo recomendado:

```text
plugin-manifest.schema.json
```

Esse schema deve validar:

- campos obrigatórios;
- tipos;
- formatos;
- enums;
- paths relativos;
- permissões conhecidas;
- capacidades conhecidas.

---

# Validação

Fluxo recomendado:

```text
Read plugin.json
    ↓
Parse JSON
    ↓
Validate Schema
    ↓
Validate Required Fields
    ↓
Validate Paths
    ↓
Validate Version
    ↓
Validate Compatibility
    ↓
Validate Permissions
    ↓
Validate Capabilities
    ↓
Return ManifestResult
```

---

# Resultado de Validação

```csharp
public sealed class PluginManifestValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

---

# Modelo Conceitual

```csharp
public sealed class PluginManifest
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Version { get; init; } = string.Empty;
    public string EntryAssembly { get; init; } = string.Empty;
    public string EntryType { get; init; } = string.Empty;
    public string PluginSdkVersion { get; init; } = string.Empty;
    public string MinimumWiiStudioVersion { get; init; } = string.Empty;
    public IReadOnlyList<string> Capabilities { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
```

---

# Segurança de Paths

Todos os paths do manifesto devem:

- ser relativos;
- permanecer dentro da pasta do plugin;
- não conter `..` perigoso;
- não apontar para paths absolutos;
- ser normalizados antes de uso.

---

# Logging e Diagnóstico

Falhas de manifesto devem registrar:

- PluginId quando disponível;
- path do manifesto;
- campo inválido;
- mensagem amigável;
- mensagem técnica;
- severidade;
- sugestão de correção quando possível.

---

# Regras para o Codex

- Nunca carregar assembly antes de validar o manifesto.
- Sempre validar schema.
- Sempre validar paths.
- Sempre validar compatibilidade.
- Sempre validar permissões.
- Sempre bloquear manifesto inválido.
- Sempre registrar erros detalhados.
- Sempre tratar campos opcionais com fallback.
- Nunca confiar em path do manifesto sem normalização.
- Nunca permitir path absoluto.
- Nunca permitir escapar da pasta do plugin.

---

# Casos de Erro

- `plugin.json` ausente.
- JSON inválido.
- Campo obrigatório ausente.
- ID inválido.
- Versão inválida.
- Entry assembly ausente.
- Entry type ausente.
- SDK incompatível.
- Plataforma não suportada.
- Permissão desconhecida.
- Capacidade desconhecida.
- Path inseguro.
- Dependência ausente.
- Manifesto duplicado.

---

# Critérios de Aceite

- [x] Estrutura oficial do manifesto definida.
- [x] Campos obrigatórios definidos.
- [x] Campos opcionais definidos.
- [x] ID do plugin padronizado.
- [x] Versionamento definido.
- [x] Compatibilidade definida.
- [x] Capacidades oficiais definidas.
- [x] Permissões declaradas.
- [x] Dependências documentadas.
- [x] Recursos documentados.
- [x] Schema JSON previsto.
- [x] Validação documentada.
- [x] Segurança de paths definida.
- [x] Regras para o Codex documentadas.

---

# Testes

## Unitários

- Manifesto válido.
- Manifesto ausente.
- JSON inválido.
- Campo obrigatório ausente.
- Versão inválida.
- SDK incompatível.
- Entry assembly inexistente.
- Path absoluto rejeitado.
- Path com `..` rejeitado.
- Permissão desconhecida.
- Capacidade desconhecida.
- Dependência ausente.

## Integração

- Descoberta de plugin com manifesto válido.
- Plugin bloqueado por manifesto inválido.
- Diagnóstico de manifesto exibido.
- Plugin com recursos localizados.
- Plugin com ícone.
- Plugin com dependência opcional ausente.
- Plugin duplicado rejeitado.

---

# Dependências

- F071-PluginSDKFoundation.md
- F073-PluginDiscovery.md
- F074-PluginLoading.md
- F075-PluginPermissions.md
- F079-PluginDiagnostics.md
- F090-SecurityChecklist.md
- 003-CodingStandards.md
- 007-Infrastructure.md

---

# Definition of Done

- Plugin Manifest documentado.
- Estrutura `plugin.json` definida.
- Validação especificada.
- Segurança de paths definida.
- Testes definidos.
- Documentação atualizada.

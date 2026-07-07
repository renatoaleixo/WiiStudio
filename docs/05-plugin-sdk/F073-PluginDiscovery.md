# F073 - Plugin Discovery

> Feature ID: F073
> Status: Ready for Development
> Priority: Critical
> Epic: Plugin SDK

# Objetivo

Definir o processo oficial de descoberta de plugins do WiiStudio, incluindo diretórios
de busca, leitura de manifestos, validação inicial, detecção de duplicidade,
classificação de estado e preparação para carregamento seguro.

A descoberta de plugins deve ser executada antes de qualquer carregamento de assembly,
garantindo que somente plugins válidos e compatíveis avancem no ciclo de vida.

---

# Requisitos Funcionais

## RF001
Definir diretórios oficiais para descoberta de plugins.

## RF002
Localizar arquivos `plugin.json`.

## RF003
Ler e validar manifestos antes do carregamento.

## RF004
Detectar plugins duplicados.

## RF005
Detectar plugins incompatíveis.

## RF006
Detectar plugins desativados.

## RF007
Registrar resultados de descoberta.

## RF008
Gerar diagnósticos amigáveis para falhas.

## RF009
Permitir atualização da lista de plugins sem reiniciar quando seguro.

## RF010
Nunca carregar assembly durante a etapa de descoberta.

---

# Diretórios Oficiais

Diretórios recomendados:

```text
WiiStudio/
├── plugins/
│   ├── installed/
│   ├── disabled/
│   └── quarantine/
```

## installed

Plugins disponíveis para validação e carregamento.

## disabled

Plugins instalados, mas desativados pelo usuário ou pelo sistema.

## quarantine

Plugins bloqueados por falha crítica, manifesto inválido ou risco de segurança.

---

# Fluxo de Descoberta

```text
Start Discovery
    ↓
Resolve Plugin Directories
    ↓
Find plugin.json Files
    ↓
Read Manifest
    ↓
Validate Manifest
    ↓
Check Duplicate Ids
    ↓
Check Compatibility
    ↓
Check Disabled/Blocked State
    ↓
Create PluginDescriptor
    ↓
Publish Discovery Result
```

---

# Plugin Descriptor

```csharp
public sealed class PluginDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string DirectoryPath { get; init; } = string.Empty;
    public string ManifestPath { get; init; } = string.Empty;
    public PluginManifest Manifest { get; init; } = default!;
    public PluginDiscoveryState State { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
}
```

---

# Estados de Descoberta

```csharp
public enum PluginDiscoveryState
{
    Found,
    ManifestMissing,
    ManifestInvalid,
    Duplicate,
    Incompatible,
    Disabled,
    Blocked,
    ReadyForLoad
}
```

---

# Serviço de Descoberta

```csharp
public interface IPluginDiscoveryService
{
    Task<IReadOnlyList<PluginDescriptor>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}
```

---

# Validações Obrigatórias

A descoberta deve validar:

- existência de `plugin.json`;
- JSON válido;
- campos obrigatórios;
- ID válido;
- versão válida;
- paths seguros;
- compatibilidade com SDK;
- compatibilidade com WiiStudio;
- plataforma suportada;
- duplicidade de ID;
- estado desativado/bloqueado.

---

# Duplicidade

Quando dois plugins tiverem o mesmo ID:

- bloquear ambos ou manter apenas o mais seguro conforme política;
- registrar erro detalhado;
- não carregar automaticamente nenhum plugin ambíguo;
- orientar o usuário a remover duplicados.

---

# Plugins Desativados

Plugins desativados:

- devem aparecer na lista de plugins;
- não devem ser carregados;
- devem preservar metadados;
- podem ser reativados pelo usuário quando seguros.

---

# Quarentena

Plugins em quarentena:

- não devem ser carregados;
- devem exibir motivo;
- podem exigir ação explícita do usuário;
- devem gerar diagnóstico.

---

# Integração com Plugin Manifest

A descoberta depende de:

- `F072-PluginManifest.md`

Nenhum plugin deve avançar sem manifesto válido.

---

# Integração com Plugin Loading

A descoberta entrega somente plugins em estado `ReadyForLoad` para o carregador.

---

# Integração com Diagnostics

Todos os problemas devem ser reportáveis no diagnóstico:

- manifesto inválido;
- plugin duplicado;
- SDK incompatível;
- path inseguro;
- dependência ausente;
- plugin bloqueado.

---

# Regras para o Codex

- Nunca carregar assembly durante discovery.
- Sempre validar manifesto antes de qualquer outra ação.
- Sempre normalizar paths.
- Sempre rejeitar paths inseguros.
- Sempre detectar duplicidade.
- Sempre registrar warnings e errors.
- Sempre retornar resultado completo, não apenas plugins válidos.
- Sempre tratar cancellation token.
- Nunca derrubar a aplicação por plugin inválido.

---

# Casos de Erro

- Diretório de plugins inexistente.
- Permissão negada.
- Manifesto ausente.
- Manifesto inválido.
- JSON malformado.
- Plugin duplicado.
- SDK incompatível.
- Plataforma incompatível.
- Path inseguro.
- Falha de leitura.
- Operação cancelada.

---

# Critérios de Aceite

- [ ] Diretórios oficiais definidos.
- [ ] Discovery Service especificado.
- [ ] PluginDescriptor definido.
- [ ] Estados de descoberta definidos.
- [ ] Manifestos são validados.
- [ ] Duplicidades são detectadas.
- [ ] Plugins desativados são reconhecidos.
- [ ] Plugins em quarentena são bloqueados.
- [ ] Diagnóstico é gerado.
- [ ] Nenhum assembly é carregado durante discovery.

---

# Testes

## Unitários

- Descobrir plugin válido.
- Manifesto ausente.
- Manifesto inválido.
- Plugin duplicado.
- Plugin incompatível.
- Plugin desativado.
- Plugin em quarentena.
- Path inseguro.
- CancellationToken.

## Integração

- Descoberta em pasta real.
- Múltiplos plugins.
- Plugin válido + inválido.
- Plugin duplicado.
- Plugin bloqueado.
- Diagnóstico exibido.

---

# Dependências

- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F074-PluginLoading.md
- F079-PluginDiagnostics.md
- F084-PluginSecurity.md
- 007-Infrastructure.md

---

# Definition of Done

- Plugin Discovery documentado.
- Serviço de descoberta especificado.
- Estados definidos.
- Validações definidas.
- Diagnósticos definidos.
- Testes definidos.
- Documentação atualizada.

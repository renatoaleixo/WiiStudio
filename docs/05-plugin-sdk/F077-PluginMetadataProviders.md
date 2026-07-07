# F077 - Plugin Metadata Providers

> Feature ID: F077
> Status: Ready for Development
> Priority: High
> Epic: Plugin SDK

# Objetivo

Definir o modelo oficial para plugins provedores de metadados no WiiStudio,
permitindo que fontes externas ou customizadas forneçam informações de jogos,
sem acoplar a aplicação principal a provedores específicos.

Metadata Provider Plugins devem respeitar cache, permissões, prioridade,
fallback, rate limit, localização, diagnóstico e regras de preservação de dados
customizados pelo usuário.

---

# Requisitos Funcionais

## RF001
Definir contrato oficial para Metadata Provider Plugin.

## RF002
Permitir consulta por GameId, título, região e hash quando disponível.

## RF003
Permitir retorno padronizado de metadados.

## RF004
Integrar com Metadata Synchronization.

## RF005
Integrar com Offline Metadata Cache.

## RF006
Respeitar permissões declaradas.

## RF007
Permitir múltiplos providers com prioridade.

## RF008
Suportar fallback entre providers.

## RF009
Registrar diagnóstico de falhas e latência.

## RF010
Nunca sobrescrever dados customizados sem política explícita.

---

# Contrato Base

```csharp
public interface IMetadataProviderPlugin : IWiiStudioPlugin
{
    string ProviderId { get; }

    string DisplayName { get; }

    int Priority { get; }

    Task<MetadataProviderResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<MetadataProviderResult> GetByGameIdAsync(
        string gameId,
        CancellationToken cancellationToken = default);
}
```

---

# Search Request

```csharp
public sealed class MetadataSearchRequest
{
    public string? GameId { get; init; }
    public string? Title { get; init; }
    public string? Region { get; init; }
    public string? Platform { get; init; }
    public string? FileHash { get; init; }
    public string? Language { get; init; }
}
```

---

# Resultado

```csharp
public sealed class MetadataProviderResult
{
    public bool Success { get; init; }
    public string ProviderId { get; init; } = string.Empty;
    public IReadOnlyList<GameMetadata> Items { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
    public bool FromCache { get; init; }
}
```

---

# Game Metadata

```csharp
public sealed class GameMetadata
{
    public string GameId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? Developer { get; init; }
    public string? Publisher { get; init; }
    public string? Genre { get; init; }
    public string? Region { get; init; }
    public int? ReleaseYear { get; init; }
    public int? PlayerCount { get; init; }
    public string? Rating { get; init; }
    public IReadOnlyList<string> Languages { get; init; } = [];
}
```

---

# Permissões Necessárias

Metadata Provider pode exigir:

- `access-metadata`
- `modify-metadata` quando alterar dados
- `network-access` para provider remoto
- `file-system-read` para provider local
- `access-game-library` para cruzar dados da biblioteca

---

# Prioridade

Providers devem possuir prioridade.

Exemplo:

| Provider | Prioridade |
|---------|-----------:|
| GameTDB | 100 |
| Local Cache | 90 |
| Plugin Externo | configurável |
| Fallback Manual | 10 |

Maior prioridade deve ser consultada primeiro, salvo preferência do usuário.

---

# Fallback

Fluxo recomendado:

```text
Local Cache
    ↓
Primary Provider
    ↓
Secondary Provider
    ↓
Fallback Provider
    ↓
Manual Metadata
```

---

# Política de Atualização

Nunca sobrescrever:

- título customizado pelo usuário;
- capa customizada;
- descrição editada manualmente;
- tags pessoais;
- favoritos;
- observações locais.

Atualizar apenas quando:

- campo está vazio;
- cache expirou;
- provider tem versão mais recente;
- usuário solicitou atualização forçada;
- política permite mesclagem.

---

# Cache

Metadata Provider deve integrar com cache.

Regras:

- respeitar TTL;
- marcar origem do dado;
- registrar provider;
- armazenar data de sincronização;
- permitir invalidação;
- permitir modo offline.

---

# Rate Limit

Providers remotos devem respeitar rate limit.

Regras:

- não bombardear API;
- usar retry controlado;
- usar backoff;
- registrar throttling;
- permitir cancelamento.

---

# Idioma

Provider deve considerar idioma preferido.

Exemplos:

- pt-BR;
- en-US;
- es-ES.

Fallback de idioma deve ser previsível.

---

# Diagnóstico

Registrar:

- provider consultado;
- duração;
- sucesso/falha;
- quantidade de resultados;
- cache hit/miss;
- erros de rede;
- rate limit;
- fallback usado.

---

# Integração com Metadata Synchronization

O provider deve ser usado por:

- `F018-MetadataSynchronization.md`
- `F019-OfflineMetadataCache.md`
- `F022-GameTDBUpdateManager.md`

---

# Integração com UI

Provider pode aparecer em:

- Settings;
- Metadata Sync;
- Game Details;
- Diagnostics;
- Plugin Manager.

---

# Regras para o Codex

- Sempre usar contrato do provider.
- Sempre respeitar CancellationToken.
- Sempre registrar ProviderId.
- Sempre tratar falha sem quebrar sincronização inteira.
- Sempre respeitar cache.
- Sempre respeitar rate limit.
- Sempre preservar dados customizados do usuário.
- Sempre validar permissões antes de consulta.
- Sempre usar fallback quando configurado.
- Nunca acoplar provider específico ao Application Core.
- Nunca sobrescrever dados manualmente editados sem política explícita.

---

# Casos de Erro

- Provider indisponível.
- Sem permissão de rede.
- Timeout.
- Rate limit.
- Resultado vazio.
- Dados inválidos.
- JSON inválido.
- Cache corrompido.
- Provider retorna GameId inconsistente.
- Cancelamento pelo usuário.

---

# Critérios de Aceite

- [ ] Contrato `IMetadataProviderPlugin` definido.
- [ ] Request definido.
- [ ] Result definido.
- [ ] GameMetadata definido.
- [ ] Permissões documentadas.
- [ ] Prioridade documentada.
- [ ] Fallback documentado.
- [ ] Cache documentado.
- [ ] Rate limit documentado.
- [ ] Política de preservação definida.
- [ ] Diagnóstico definido.
- [ ] Integração com Metadata Synchronization prevista.

---

# Testes

## Unitários

- Consulta por GameId.
- Consulta por título.
- Provider sem permissão.
- Timeout.
- Resultado vazio.
- Cache hit.
- Cache miss.
- Fallback para segundo provider.
- Preservar campo customizado.
- Rate limit tratado.

## Integração

- Plugin provider remoto.
- Plugin provider local.
- Sincronização de metadados.
- Modo offline.
- Provider falha e fallback funciona.
- Diagnostics mostra latência.
- Settings exibe provider.

---

# Dependências

- F014-GameTDB.md
- F018-MetadataSynchronization.md
- F019-OfflineMetadataCache.md
- F022-GameTDBUpdateManager.md
- F071-PluginSDKFoundation.md
- F075-PluginPermissions.md
- F079-PluginDiagnostics.md
- F084-PluginSecurity.md

---

# Definition of Done

- Plugin Metadata Providers documentado.
- Contratos definidos.
- Permissões definidas.
- Cache, fallback e prioridade definidos.
- Política de atualização definida.
- Testes definidos.
- Documentação atualizada.

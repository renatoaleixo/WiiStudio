# F102 - GameTDB Cover Provider

> Feature ID: F102  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: Metadata & Assets  
> Area: Infrastructure, Application, UI, Cache, Diagnostics  
> Related Problem: `NullCoverProvider` está registrado na infraestrutura, então o fluxo atual não baixa assets reais.

# Objetivo

Implementar um `ICoverProvider` real para o WiiStudio, substituindo o `NullCoverProvider`
por um provedor funcional baseado preferencialmente no **GameTDB/WiiTDB**.

O provedor deve baixar assets por **Game ID Wii**, por exemplo:

```text
RMCP01
RSPE01
RMGP01
```

Assets suportados no primeiro ciclo:

- Front Cover;
- Back/Full Cover;
- Disc Art;
- Banner, quando fonte suportar;
- Icon/Thumbnail, preferencialmente gerado localmente.

---

# Estado Atual do Código

O fluxo de download já existe parcialmente:

```text
UI
    ↓
DownloadSelectedCoversCommand
    ↓
Application
    ↓
ICoverDownloadService
    ↓
ICoverProvider
    ↓
ICoverCache / FileSystemCoverCache
```

Mas a infraestrutura registra:

```text
NullCoverProvider
```

Resultado atual:

- comando executa;
- serviço é chamado;
- cache existe;
- mas nada real é baixado.

---

# Resultado Esperado

Substituir o provider nulo por:

```csharp
GameTdbCoverProvider : ICoverProvider
```

O fluxo esperado:

```text
Selecionar jogo(s)
    ↓
Verificar cache
    ↓
Baixar asset ausente
    ↓
Validar imagem
    ↓
Salvar cache
    ↓
Atualizar biblioteca
    ↓
Exibir status ao usuário
```

---

# Fonte Recomendada dos Assets

## Fonte Principal

GameTDB / WiiTDB.

Páginas oficiais:

```text
https://www.gametdb.com/
https://www.gametdb.com/Wii
https://www.gametdb.com/Wii/downloads
```

Base recomendada para assets diretos:

```text
https://art.gametdb.com/wii/
```

O GameTDB informa que não é necessário cadastro para baixar o banco de dados e os
packs de capas. A área Wii/GameCube também informa suporte a informações e artwork
em múltiplos idiomas.

---

# URL Templates

O provider deve usar templates configuráveis, não strings espalhadas pelo código.

Templates iniciais recomendados:

```text
Front Cover:
https://art.gametdb.com/wii/cover/{LANG}/{GAME_ID}.png

3D Cover, opcional/futuro:
https://art.gametdb.com/wii/cover3D/{LANG}/{GAME_ID}.png

Full Cover:
https://art.gametdb.com/wii/coverfull/{LANG}/{GAME_ID}.png

Disc Art:
https://art.gametdb.com/wii/disc/{LANG}/{GAME_ID}.png
```

Exemplos:

```text
https://art.gametdb.com/wii/cover/EN/RMCP01.png
https://art.gametdb.com/wii/cover/PT/RMCP01.png
https://art.gametdb.com/wii/disc/EN/RMCP01.png
https://art.gametdb.com/wii/coverfull/EN/RMCP01.png
```

Observação:

- `cover`, `cover3D`, `coverfull` e `disc` são os tipos mais conhecidos e usados por loaders Wii.
- `banner` e `icon` não devem ser assumidos como sempre disponíveis no GameTDB.
- Banner e Icon devem ter fallback seguro.

---

# Idiomas e Regiões

## Idioma Preferencial

Configuração recomendada:

```text
CoverPreferredLanguage = pt-BR
```

Mapeamento para GameTDB:

```text
pt-BR -> PT
en-US -> EN
es-ES -> ES
fr-FR -> FR
de-DE -> DE
it-IT -> IT
ja-JP -> JA
ko-KR -> KO
zh-CN -> ZH
```

## Fallback de Idioma

Ordem recomendada para usuário brasileiro:

```text
PT
EN
US
ORIG
```

Ordem geral:

```text
PreferredLanguage
EN
US
ORIG
```

---

# Mapeamento dos Assets

```csharp
public enum CoverAssetKind
{
    FrontCover,
    BackCover,
    DiscArt,
    Banner,
    Icon
}
```

| CoverAssetKind | Fonte GameTDB | Diretório de Cache | Observação |
|---|---|---|---|
| FrontCover | `cover/{LANG}/{GAME_ID}.png` | `covers/` | Capa 2D frontal |
| BackCover | `coverfull/{LANG}/{GAME_ID}.png` | `covers-back/` | GameTDB geralmente fornece full cover, não back-only |
| DiscArt | `disc/{LANG}/{GAME_ID}.png` | `discs/` | Arte do disco |
| Banner | fonte opcional/configurável | `banners/` | Não assumir disponibilidade no GameTDB |
| Icon | gerado localmente | `thumbnails/` | Thumbnail derivado da capa frontal |

---

# Regra para Banner

Como o GameTDB não deve ser assumido como fonte universal de banners para Wii, o
primeiro ciclo deve tratar `Banner` como opcional.

Estratégia:

1. se houver URL template configurado para banner, tentar baixar;
2. se não houver, tentar gerar/obter por fonte futura;
3. se indisponível, retornar `Missing`;
4. não falhar o download inteiro por falta de banner.

Resultado:

```text
Banner Missing não é erro fatal.
```

---

# Regra para Icon / Thumbnail

`Icon` deve ser gerado localmente a partir de `FrontCover` quando possível.

```text
FrontCover baixada
    ↓
Validar imagem
    ↓
Salvar covers/{GAME_ID}.png
    ↓
Gerar thumbnails/{GAME_ID}.png
```

Tamanho sugerido:

```text
160x224
```

---

# Diretórios de Cache

Diretórios oficiais:

```text
covers/
covers-back/
discs/
banners/
thumbnails/
```

Estrutura sugerida:

```text
AppData/
└── WiiStudio/
    └── assets/
        ├── covers/
        ├── covers-back/
        ├── discs/
        ├── banners/
        └── thumbnails/
```

---

# Extensão dos Arquivos

## Decisão

Trocar o cache final de `.bin` para extensão real de imagem.

Extensões permitidas:

```text
.png
.jpg
.jpeg
.webp
```

Para GameTDB, o padrão inicial deve ser:

```text
.png
```

## Compatibilidade

Se já existir cache legado `.bin`, o sistema pode:

- ler como fallback;
- migrar para extensão real quando asset for atualizado;
- parar de escrever `.bin` para novos downloads.

## Regra de Escrita

Novo arquivo deve ser salvo como:

```text
covers/RMCP01.png
covers-back/RMCP01.png
discs/RMCP01.png
banners/RMCP01.png
thumbnails/RMCP01.png
```

---

# Validação de Imagem

Após download:

- HTTP status deve ser 200;
- `Content-Type` deve começar com `image/`;
- tamanho máximo deve ser respeitado;
- stream deve ser decodificável como imagem;
- dimensões devem ser maiores que zero;
- arquivo não pode ser HTML, JSON ou texto de erro;
- hash opcional pode ser calculado para auditoria/cache;
- salvar primeiro em arquivo temporário;
- mover para cache final apenas após validação.

---

# Tamanho Máximo

Configuração recomendada:

```text
MaxCoverDownloadBytes = 5 MB
MaxFullCoverDownloadBytes = 10 MB
```

Se exceder:

```text
CoverDownloadResult = InvalidImageTooLarge
```

---

# Cache e ForceRefresh

## Cache Hit

Se o asset já existir e `ForceRefresh = false`:

```text
retornar FromCache
não baixar novamente
```

## Cache Miss

Se não existir:

```text
baixar
validar
salvar
retornar Downloaded
```

## ForceRefresh

Se `ForceRefresh = true`:

```text
ignorar cache
baixar novamente
validar
substituir arquivo existente com escrita temporária
```

## Sobrescrita Segura

Nunca sobrescrever diretamente.

```text
download para temp
    ↓
validar imagem
    ↓
salvar temp final
    ↓
replace/move atômico quando possível
```

---

# Contratos Sugeridos

## ICoverProvider

```csharp
public interface ICoverProvider
{
    Task<CoverProviderResult> GetCoverAsync(
        CoverProviderRequest request,
        CancellationToken cancellationToken = default);
}
```

## CoverProviderRequest

```csharp
public sealed class CoverProviderRequest
{
    public string GameId { get; init; } = string.Empty;
    public CoverAssetKind Kind { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? PreferredRegion { get; init; }
    public bool ForceRefresh { get; init; }
}
```

## CoverProviderResult

```csharp
public sealed class CoverProviderResult
{
    public string GameId { get; init; } = string.Empty;
    public CoverAssetKind Kind { get; init; }
    public CoverProviderStatus Status { get; init; }
    public Stream? Content { get; init; }
    public string? ContentType { get; init; }
    public string? SourceUrl { get; init; }
    public string? ErrorMessage { get; init; }
}
```

## CoverProviderStatus

```csharp
public enum CoverProviderStatus
{
    Downloaded,
    FromCache,
    Missing,
    InvalidImage,
    NetworkError,
    Timeout,
    ProviderUnavailable,
    Cancelled
}
```

---

# GameTdbCoverProvider

Classe principal:

```csharp
public sealed class GameTdbCoverProvider : ICoverProvider
{
    public Task<CoverProviderResult> GetCoverAsync(
        CoverProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Build fallback URLs
        // Try each URL
        // Validate HTTP response
        // Validate image
        // Return stream/result
    }
}
```

Dependências recomendadas:

```csharp
IHttpClientFactory
IGameTdbCoverUrlBuilder
IImageValidator
ILogger<GameTdbCoverProvider>
IOptions<GameTdbCoverProviderOptions>
```

---

# GameTdbCoverUrlBuilder

```csharp
public interface IGameTdbCoverUrlBuilder
{
    IReadOnlyList<Uri> BuildCandidateUrls(
        string gameId,
        CoverAssetKind kind,
        string? preferredLanguage,
        string? preferredRegion);
}
```

Regras:

- validar Game ID;
- normalizar para uppercase;
- bloquear path traversal;
- usar apenas HTTPS;
- montar fallback de idiomas;
- retornar lista ordenada de candidatos.

---

# Configurações

## GameTdbCoverProviderOptions

```csharp
public sealed class GameTdbCoverProviderOptions
{
    public string BaseUrl { get; init; } = "https://art.gametdb.com/wii";
    public string PreferredLanguage { get; init; } = "PT";
    public IReadOnlyList<string> FallbackLanguages { get; init; } =
        ["PT", "EN", "US", "ORIG"];

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
    public long MaxImageBytes { get; init; } = 5 * 1024 * 1024;
    public long MaxFullCoverBytes { get; init; } = 10 * 1024 * 1024;
    public bool AllowAutomaticCoverDownload { get; init; } = true;
}
```

## Settings de Usuário

```text
Assets.Covers.Provider = GameTDB
Assets.Covers.PreferredLanguage = PT
Assets.Covers.PreferredRegion = Auto
Assets.Covers.TimeoutSeconds = 15
Assets.Covers.AllowAutomaticDownload = true
Assets.Covers.ForceRefresh = false
Assets.Covers.GenerateThumbnails = true
Assets.Covers.MaxImageBytes = 5242880
```

---

# Fluxo Técnico Completo

```text
User selects game(s)
    ↓
DownloadSelectedCoversCommand
    ↓
ICoverDownloadService
    ↓
For each selected game
    ↓
For each requested CoverAssetKind
    ↓
Check ICoverCache
    ↓
If cache hit and !ForceRefresh
    ↓
Return FromCache
    ↓
If cache miss or ForceRefresh
    ↓
GameTdbCoverProvider builds candidate URLs
    ↓
HTTP GET with timeout
    ↓
Validate status/content-type/size/image
    ↓
Save to FileSystemCoverCache
    ↓
Generate thumbnail if configured
    ↓
Update library asset references
    ↓
Notify UI with counts
```

---

# Resultado em Lote

```csharp
public sealed class CoverBatchDownloadSummary
{
    public int Requested { get; init; }
    public int Downloaded { get; init; }
    public int FromCache { get; init; }
    public int Missing { get; init; }
    public int Failed { get; init; }
    public int Invalid { get; init; }
}
```

UI deve exibir exemplo:

```text
Capas baixadas: 8
Do cache: 12
Ausentes: 2
Falhas: 0
```

---

# Tratamento de Erro

## Sem Internet

Status:

```text
NetworkError
```

Ação:

- não quebrar lote;
- informar usuário;
- manter cache existente;
- permitir tentar novamente.

## Jogo Não Encontrado

Status:

```text
Missing
```

## Asset Indisponível

Status:

```text
Missing
```

## HTTP 404

Status:

```text
Missing
```

## Timeout

Status:

```text
Timeout
```

## Imagem Inválida

Status:

```text
InvalidImage
```

## Provider Fora do Ar

Status:

```text
ProviderUnavailable
```

---

# Segurança

Obrigatório:

- permitir apenas HTTPS;
- validar `GameId` com allowlist;
- bloquear path traversal;
- validar tamanho máximo;
- validar `Content-Type`;
- validar imagem decodificável;
- nunca executar conteúdo baixado;
- nunca interpretar resposta como script;
- salvar em diretório controlado;
- usar escrita temporária;
- redigir logs;
- respeitar timeout;
- respeitar CancellationToken.

## Validação de Game ID

Regex recomendada:

```text
^[A-Z0-9]{6}$
```

Se inválido:

```text
CoverProviderStatus.InvalidGameId
```

ou erro de validação antes de chamar provider.

---

# Logging

Registrar:

- início do download;
- Game ID;
- asset kind;
- URL base sem query sensível;
- idioma tentado;
- cache hit;
- cache miss;
- status HTTP;
- resultado;
- duração;
- erros controlados.

Não registrar:

- paths sensíveis completos desnecessariamente;
- payload binário;
- conteúdo da imagem.

---

# Diagnóstico

Advanced Diagnostics deve mostrar:

```text
Cover Provider: GameTDB
Base URL: https://art.gametdb.com/wii
Preferred language: PT
Fallback languages: PT, EN, US, ORIG
Last check: ...
Last error: ...
Downloaded assets: ...
Cache hits: ...
Missing assets: ...
```

---

# Impacto no Código Atual

## Criar

```text
GameTdbCoverProvider
GameTdbCoverUrlBuilder
GameTdbCoverProviderOptions
ImageValidator
CoverAssetPathResolver
CoverBatchDownloadSummary
```

## Alterar DI

Substituir:

```csharp
services.AddSingleton<ICoverProvider, NullCoverProvider>();
```

por:

```csharp
services.AddHttpClient<GameTdbCoverProvider>();
services.AddSingleton<ICoverProvider, GameTdbCoverProvider>();
```

ou factory equivalente.

## Ajustar FileSystemCoverCache

Ajustar para:

- salvar extensão real;
- ler legado `.bin` como fallback;
- resolver path por `CoverAssetKind`;
- usar escrita temporária;
- validar path.

## Atualizar UI

Possíveis ajustes:

- exibir preview após download;
- atualizar capa sem reiniciar tela;
- exibir contadores de lote;
- exibir Missing sem erro crítico;
- botão Force Refresh.

---

# Integração com ICoverDownloadService

Regra:

```text
ICoverProvider baixa.
ICoverCache salva/lê.
ICoverDownloadService orquestra.
UI apenas aciona e exibe estado.
```

---

# Critérios de Aceite

- [x] `GameTdbCoverProvider` implementado.
- [x] `NullCoverProvider` deixa de ser o provider padrão.
- [x] Capa frontal real baixa para jogo conhecido, exemplo `RMCP01`.
- [x] Capa frontal real baixa para jogo conhecido, exemplo `RSPE01`.
- [x] Capas em lote funcionam.
- [x] Cache é reutilizado quando arquivo já existe.
- [x] `ForceRefresh` baixa novamente.
- [x] Contagem correta: baixadas, cache, ausentes, falhas.
- [x] HTTP 404 retorna `Missing`.
- [x] Erro de rede não quebra lote.
- [x] Imagem inválida é rejeitada.
- [x] `CoverAssetKind` mapeia para diretórios corretos.
- [x] Thumbnails são gerados quando configurado.
- [x] Logs e Diagnostics mostram provider real.
- [x] Segurança aplicada.

---

# Testes Esperados

## Unitários

- `GameTdbCoverUrlBuilder` monta URL de FrontCover.
- `GameTdbCoverUrlBuilder` monta URL de BackCover/full cover.
- `GameTdbCoverUrlBuilder` monta URL de DiscArt.
- Fallback PT -> EN -> US -> ORIG.
- Game ID inválido é rejeitado.
- `CoverAssetKind` resolve diretório correto.
- `ForceRefresh` ignora cache.
- Cache hit não chama HTTP.
- Cache miss chama provider.
- 404 retorna `Missing`.
- Timeout retorna `Timeout`.
- Erro de rede retorna `NetworkError`.
- Content-Type inválido rejeita.
- Imagem inválida rejeita.
- Arquivo grande demais rejeita.

## Integração

- Download fake salva em `covers/`.
- Download fake salva em `covers-back/`.
- Download fake salva em `discs/`.
- Thumbnail é gerado.
- Cache legado `.bin` é lido como fallback.
- Escrita temporária substitui arquivo com segurança.
- Lote parcial continua após 404.
- UI/ViewModel recebe resumo correto.

## Segurança

- URL HTTP é bloqueada.
- Path traversal em Game ID é bloqueado.
- Content-Type `text/html` é rejeitado.
- Resposta maior que limite é rejeitada.
- Cache não escreve fora da pasta.
- Logs não incluem payload.

---

# Regras para o Codex

- Não manter `NullCoverProvider` como provider padrão.
- Implementar `GameTdbCoverProvider` na Infrastructure.
- Não colocar download HTTP na UI.
- Não salvar cache como `.bin` para novos downloads.
- Usar extensão real de imagem.
- Validar imagem antes de salvar.
- Usar HTTPS.
- Usar timeout e CancellationToken.
- Não quebrar lote por asset ausente.
- Não tratar 404 como exceção fatal.
- Gerar thumbnail localmente.
- Atualizar DI.
- Criar testes com HTTP fake.
- Não depender de internet real no pipeline de testes.
- Não executar conteúdo baixado.

---

# Dependências

- `F017-DownloadCovers.md`
- `F019-OfflineMetadataCache.md`
- `F020-AssetManager.md`
- `F021-BannerAndDiscArt.md`
- `F023-SettingsManager.md`
- `F025-NotificationCenter.md`
- `F030-AdvancedDiagnostics.md`
- `F077-PluginMetadataProviders.md`
- `F081-SecurityFoundation.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F091-TestingStrategy.md`
- `F093-IntegrationTestingStandards.md`

---

# Definition of Done

- Provider real implementado.
- DI atualizado.
- Cache usa extensão real.
- Download real funciona manualmente.
- Testes automatizados usam fake HTTP.
- Segurança aplicada.
- Diagnostics atualizado.
- UI exibe status correto.
- Documentação atualizada.

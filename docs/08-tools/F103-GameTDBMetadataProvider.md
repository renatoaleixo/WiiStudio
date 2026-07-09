# F103 - GameTDB Metadata Provider

> Feature ID: F103  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Metadata & Assets  
> Area: Infrastructure, Application, Cache, Diagnostics, UI  
> Related Problem: `NullGameMetadataProvider` está registrado na infraestrutura, então o botão de sincronização executa, mas nenhum metadado real é baixado.

# Objetivo

Implementar um provider real de metadados baseado em **GameTDB/WiiTDB**, substituindo
o `NullGameMetadataProvider` como provider padrão do WiiStudio.

O provider deve consultar metadados reais de jogos Wii por **Game ID**, por exemplo:

```text
RMCP01
RSPE01
RMGP01
```

O fluxo já existente deve continuar sendo respeitado:

```text
UI
    ↓
SynchronizeSelectedMetadataCommand
    ↓
Application
    ↓
IMetadataSynchronizationService
    ↓
IGameMetadataProvider
    ↓
IMetadataCacheService / JsonMetadataCacheService
```

---

# Estado Atual do Código

Hoje o WiiStudio já possui parte do fluxo:

- UI chama `SynchronizeSelectedMetadataCommand`;
- Application usa `IMetadataSynchronizationService`;
- existe cache local via `IMetadataCacheService`;
- existe implementação de cache `JsonMetadataCacheService`;
- mas a Infrastructure registra `NullGameMetadataProvider`.

Resultado atual:

```text
O botão executa, mas nenhum metadado real é baixado.
```

---

# Resultado Esperado

Criar:

```csharp
GameTdbMetadataProvider : IGameMetadataProvider
```

E alterar o DI para substituir:

```csharp
NullGameMetadataProvider
```

por:

```csharp
GameTdbMetadataProvider
```

---

# Fonte dos Metadados

## Fonte Recomendada

GameTDB / WiiTDB.

URLs oficiais:

```text
https://www.gametdb.com/
https://www.gametdb.com/Wii
https://www.gametdb.com/Wii/downloads
```

Arquivo principal recomendado:

```text
https://www.gametdb.com/wiitdb.zip
```

Arquivo esperado dentro do ZIP:

```text
wiitdb.xml
```

Formato:

```text
ZIP contendo XML
```

---

# Banco Completo vs Consulta por Game ID

## Decisão Recomendada

Usar **download do banco completo `wiitdb.zip`** e consultar localmente por Game ID.

Motivos:

- GameTDB/WiiTDB oferece o banco de dados Wii para download;
- evita muitas requisições individuais;
- melhora uso offline;
- permite cache local robusto;
- reduz impacto no provider;
- facilita sincronização em lote;
- permite atualização incremental por data/versão quando suportado.

## Consulta por Game ID

O provider público deve expor consulta por Game ID através do contrato existente:

```csharp
GetByGameIdAsync(string gameId, CancellationToken cancellationToken)
```

Mas internamente o provider pode:

1. verificar cache;
2. garantir banco `wiitdb.xml` local atualizado;
3. procurar o Game ID no XML local;
4. mapear para `GameMetadataDto`.

---

# Contrato Atual

Usar o contrato existente:

```csharp
public interface IGameMetadataProvider
{
    Task<GameMetadataDto?> GetByGameIdAsync(
        string gameId,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default);
}
```

## Regras

- Não alterar o contrato sem necessidade.
- Implementar o provider real na Infrastructure.
- Manter Application dependendo apenas da interface.
- UI não deve conhecer GameTDB diretamente.

---

# Provider Proposto

```csharp
public sealed class GameTdbMetadataProvider : IGameMetadataProvider
{
    public Task<GameMetadataDto?> GetByGameIdAsync(
        string gameId,
        CancellationToken cancellationToken = default)
    {
        // Validate Game ID
        // Ensure local GameTDB database
        // Parse/query XML
        // Map to GameMetadataDto
    }

    public Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        // Check local cache/database or remote availability
    }
}
```

Dependências recomendadas:

```csharp
IHttpClientFactory
IGameTdbDatabaseService
IGameTdbXmlParser
IGameTdbMetadataMapper
IMetadataCacheService
ILogger<GameTdbMetadataProvider>
IOptions<GameTdbMetadataProviderOptions>
```

---

# Serviços Auxiliares

## IGameTdbDatabaseService

Responsável por baixar, validar, extrair e armazenar o `wiitdb.xml`.

```csharp
public interface IGameTdbDatabaseService
{
    Task<GameTdbDatabaseStatus> EnsureDatabaseAsync(
        bool forceRefresh,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenDatabaseXmlAsync(
        CancellationToken cancellationToken = default);
}
```

## IGameTdbXmlParser

Responsável por ler o XML e localizar jogo por ID.

```csharp
public interface IGameTdbXmlParser
{
    Task<GameTdbGameXmlEntry?> FindByGameIdAsync(
        Stream xmlStream,
        string gameId,
        CancellationToken cancellationToken = default);
}
```

## IGameTdbMetadataMapper

Responsável por mapear XML para DTO da aplicação.

```csharp
public interface IGameTdbMetadataMapper
{
    GameMetadataDto Map(
        GameTdbGameXmlEntry source,
        GameTdbMetadataMappingContext context);
}
```

---

# Configurações

## GameTdbMetadataProviderOptions

```csharp
public sealed class GameTdbMetadataProviderOptions
{
    public string DatabaseUrl { get; init; } = "https://www.gametdb.com/wiitdb.zip";
    public string PreferredLanguage { get; init; } = "PT";
    public IReadOnlyList<string> FallbackLanguages { get; init; } =
        ["PT", "EN", "US", "ORIG"];

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public long MaxDatabaseZipBytes { get; init; } = 50 * 1024 * 1024;
    public bool AllowAutomaticMetadataDownload { get; init; } = true;
    public TimeSpan DatabaseTtl { get; init; } = TimeSpan.FromDays(7);
}
```

## Settings de Usuário

```text
Metadata.Provider = GameTDB
Metadata.PreferredLanguage = PT
Metadata.FallbackLanguages = PT,EN,US,ORIG
Metadata.TimeoutSeconds = 30
Metadata.AllowAutomaticDownload = true
Metadata.ForceRefresh = false
Metadata.DatabaseTtlDays = 7
Metadata.LastGameTdbUpdateAt
Metadata.GameTdbVersion
```

---

# Idiomas Disponíveis e Fallback

O GameTDB/WiiTDB possui dados em múltiplos idiomas. O provider deve trabalhar com
idioma preferencial e fallback.

## Mapeamento de Idioma

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

## Fallback Recomendado para Brasil

```text
PT
EN
US
ORIG
```

## Fallback Geral

```text
PreferredLanguage
EN
US
ORIG
```

Regras:

- se título em PT não existir, tentar EN;
- se descrição em PT não existir, tentar EN;
- se campo localizado não existir, usar campo neutro/original quando disponível;
- não falhar a sincronização inteira por ausência de tradução.

---

# Mapeamento para GameMetadataDto

Mapear os campos abaixo.

## GameId

Origem:

```text
game.id
```

Regra:

- normalizar uppercase;
- validar regex `^[A-Z0-9]{6}$`.

---

## Title

Origem recomendada:

```text
locale/title
```

Fallback:

```text
PT -> EN -> US -> ORIG -> nome interno quando disponível
```

---

## Region

Origem possível:

```text
region
```

ou inferência pelo Game ID.

Exemplos de inferência:

```text
E = USA/NTSC-U
P = PAL/Europe
J = Japan
K = Korea
```

---

## Subtitle

Origem:

```text
locale/synopsis
```

ou outro campo curto quando disponível.

Regra:

- se não houver subtitle explícito, deixar nulo;
- não copiar descrição longa inteira para subtitle.

---

## Developer

Origem:

```text
developer
```

---

## Publisher

Origem:

```text
publisher
```

---

## Year

Origem:

```text
date
release date
year
```

Regra:

- extrair ano quando data completa existir;
- manter nulo se inválido.

---

## Languages

Origem:

```text
languages
```

Regra:

- mapear para lista normalizada;
- manter códigos originais quando não houver mapeamento.

---

## Genres

Origem:

```text
genre
```

Regra:

- separar múltiplos gêneros quando o XML indicar lista/texto composto;
- normalizar strings vazias.

---

## Players

Origem:

```text
players
```

Regra:

- converter para inteiro quando possível;
- se houver intervalo, preservar representação ou extrair mínimo/máximo conforme DTO suportar.

---

## Rating

Origem possível:

```text
rating
ratings
```

Regra:

- preservar sistema quando disponível: ESRB, PEGI, CERO etc.;
- não inventar rating.

---

## Description

Origem:

```text
locale/synopsis
```

Fallback:

```text
PT -> EN -> US -> ORIG
```

---

## OnlineFeatures

Origem possível:

```text
wi-fi
online
features
```

Regra:

- mapear quando existir;
- manter vazio quando não existir.

---

## Compatibility

Origem:

```text
não obrigatória no GameTDB
```

Regra:

- manter nulo/desconhecido quando o GameTDB não fornecer;
- não inventar compatibilidade.

---

## GameTdbVersion

Origem:

```text
versão/data do arquivo wiitdb.xml ou data de download
```

Regra:

- se XML possuir versão/data, usar;
- caso contrário, usar timestamp de atualização do banco local.

---

## UpdatedAt

Origem:

```text
data de atualização local
```

Regra:

- usar `DateTimeOffset.UtcNow` no momento em que o DTO foi sincronizado;
- se o XML oferecer data de modificação do jogo, guardar como origem adicional quando suportado.

---

# Estratégia de Cache

Usar:

```text
IMetadataCacheService
JsonMetadataCacheService
```

## Cache de Metadados por Jogo

Chave recomendada:

```text
{GAME_ID}-{LANGUAGE}
```

Exemplo:

```text
RMCP01-PT
RMCP01-EN
```

## Cache do Banco GameTDB

Além do cache por jogo, manter cache do banco completo:

```text
metadata/
└── gametdb/
    ├── wiitdb.zip
    ├── wiitdb.xml
    ├── manifest.json
    └── index.json
```

## Manifesto Local

```json
{
  "provider": "GameTDB",
  "databaseUrl": "https://www.gametdb.com/wiitdb.zip",
  "downloadedAt": "2026-01-01T10:00:00Z",
  "preferredLanguage": "PT",
  "gameCount": 12345,
  "sourceVersion": "unknown-or-detected"
}
```

---

# Quando Consultar Cache

## ForceRefresh = false

```text
1. verificar cache por jogo
2. se existir e válido, retornar cache
3. se não existir, consultar banco local GameTDB
4. se banco local expirado/ausente, baixar wiitdb.zip
5. salvar cache por jogo
```

## ForceRefresh = true

```text
1. ignorar cache por jogo
2. atualizar banco GameTDB se permitido
3. consultar XML
4. salvar cache por jogo novamente
```

---

# Atualização Incremental

## Primeira Versão

Implementar atualização por TTL do banco completo.

Exemplo:

```text
DatabaseTtl = 7 dias
```

## Futuro

Se o GameTDB oferecer XML com timestamp por jogo ou parâmetro de atualização,
permitir atualização incremental.

Até lá:

- evitar baixar o banco em toda sincronização;
- usar TTL;
- permitir ForceRefresh manual;
- registrar última atualização.

---

# Preservar Dados Personalizados

Nunca sobrescrever dados editados manualmente pelo usuário sem política explícita.

Preservar:

- título customizado;
- descrição customizada;
- tags;
- favoritos;
- notas pessoais;
- capa customizada;
- classificação local;
- qualquer campo marcado como `UserOverride`.

Estratégia:

```text
GameTDB metadata
    ↓
Merge policy
    ↓
Preserve user overrides
    ↓
Update only provider-owned fields
```

---

# Fluxo Esperado

```text
Selecionar jogo(s)
    ↓
SynchronizeSelectedMetadataCommand
    ↓
IMetadataSynchronizationService
    ↓
Validar Game ID
    ↓
Verificar JsonMetadataCacheService
    ↓
Se cache hit e !ForceRefresh: usar cache
    ↓
Se cache miss ou ForceRefresh:
    ↓
GameTdbMetadataProvider
    ↓
Ensure wiitdb.xml local
    ↓
Parse/query XML
    ↓
Mapear para GameMetadataDto
    ↓
Salvar metadados no cache
    ↓
Aplicar merge preservando dados do usuário
    ↓
Atualizar biblioteca/tela
    ↓
Mostrar contadores
```

---

# Resultado em Lote

Criar/usar resumo semelhante:

```csharp
public sealed class MetadataSynchronizationSummary
{
    public int Requested { get; init; }
    public int Updated { get; init; }
    public int FromCache { get; init; }
    public int Missing { get; init; }
    public int Failed { get; init; }
    public int InvalidGameIds { get; init; }
}
```

Mensagem de UI:

```text
Metadados atualizados: 8
Do cache: 12
Não encontrados: 2
Falhas: 0
```

---

# Validação e Segurança

Obrigatório:

- validar Game ID com regex `^[A-Z0-9]{6}$`;
- permitir apenas HTTPS;
- aplicar timeout;
- limitar tamanho máximo do ZIP;
- validar ZIP antes de extrair;
- bloquear zip slip/path traversal;
- validar XML antes de processar;
- não executar nenhum conteúdo baixado;
- não interpretar XML como script;
- desabilitar DTD/external entities no parser XML;
- tratar XML inválido;
- tratar provider fora do ar;
- respeitar CancellationToken;
- usar escrita temporária;
- redigir logs.

---

# Segurança no XML Parser

O parser XML deve:

- desabilitar DTD;
- bloquear external entities;
- limitar profundidade/tamanho quando possível;
- lidar com campos ausentes;
- não confiar em strings do XML como path;
- normalizar texto;
- tratar caracteres inválidos.

---

# Tamanho Máximo

Configuração inicial:

```text
MaxDatabaseZipBytes = 50 MB
```

Se exceder:

```text
ProviderUnavailable ou InvalidDatabase
```

A decisão exata depende dos tipos existentes.

---

# Erros Esperados

## Sem Internet

Resultado:

```text
NetworkError
```

Ação:

- usar cache existente se possível;
- informar usuário;
- não quebrar UI.

## Game ID Não Encontrado

Resultado:

```text
Missing
```

Ação:

- não tratar como erro fatal;
- contar como `Missing`.

## GameTDB Indisponível

Resultado:

```text
ProviderUnavailable
```

Ação:

- usar cache se possível;
- exibir diagnóstico;
- permitir tentar depois.

## Timeout

Resultado:

```text
Timeout
```

## XML Inválido

Resultado:

```text
InvalidProviderData
```

Ação:

- não substituir cache anterior;
- registrar warning/error;
- manter banco anterior quando possível.

## Cache Corrompido

Ação:

- tentar reconstruir cache;
- remover item corrompido quando seguro;
- baixar novamente se permitido;
- registrar diagnóstico.

## Resposta Vazia

Resultado:

```text
ProviderUnavailable
```

ou:

```text
InvalidProviderData
```

---

# Diagnóstico

Adicionar seção em Advanced Diagnostics:

```text
Metadata Provider: GameTDB
Base URL: https://www.gametdb.com/wiitdb.zip
Preferred language: PT
Fallback languages: PT, EN, US, ORIG
Local database: metadata/gametdb/wiitdb.xml
Total metadata cached: ...
Last update: ...
Last successful sync: ...
Last error: ...
Provider available: true/false
```

Ações de diagnóstico:

- verificar disponibilidade;
- limpar cache de metadados;
- forçar atualização do banco;
- abrir pasta do cache;
- copiar detalhes técnicos.

---

# Logging

Registrar:

- início da sincronização;
- Game ID;
- cache hit/miss;
- download do banco;
- tamanho baixado;
- XML validado;
- jogo encontrado/não encontrado;
- campos mapeados;
- merge aplicado;
- dados preservados;
- resultado final.

Não registrar:

- XML completo;
- payload completo;
- paths sensíveis desnecessários;
- dados pessoais.

---

# Impacto no Código Atual

## Criar

```text
GameTdbMetadataProvider
GameTdbDatabaseService
GameTdbXmlParser
GameTdbMetadataMapper
GameTdbMetadataProviderOptions
GameTdbDatabaseManifest
```

## Alterar DI

Substituir:

```csharp
services.AddSingleton<IGameMetadataProvider, NullGameMetadataProvider>();
```

por:

```csharp
services.AddHttpClient<GameTdbMetadataProvider>();
services.AddSingleton<IGameMetadataProvider, GameTdbMetadataProvider>();
```

ou factory equivalente.

## Ajustar Cache

Usar:

```text
JsonMetadataCacheService
```

Garantir:

- cache por Game ID;
- cache por idioma;
- leitura de cache corrompido com falha segura;
- ForceRefresh;
- TTL do banco GameTDB.

## Atualizar UI

A UI deve:

- exibir contadores claros;
- atualizar detalhes do jogo após sincronização;
- mostrar “não encontrado” sem erro fatal;
- indicar quando dados vieram do cache;
- permitir ForceRefresh.

---

# Critérios de Aceite

- [ ] `NullGameMetadataProvider` deixa de ser provider padrão.
- [ ] `GameTdbMetadataProvider` implementado.
- [ ] `IsAvailableAsync` funciona.
- [ ] Sincroniza metadados de jogo conhecido, exemplo `RMCP01`.
- [ ] Sincroniza em lote.
- [ ] Reutiliza cache quando `ForceRefresh = false`.
- [ ] `ForceRefresh` busca novamente.
- [ ] Botão mostra contagem correta.
- [ ] Metadados aparecem na UI ou ficam acessíveis na biblioteca.
- [ ] Game ID inválido é rejeitado.
- [ ] XML inválido é tratado.
- [ ] Provider indisponível não quebra UI.
- [ ] Dados personalizados do usuário são preservados.
- [ ] Testes não dependem de internet real.

---

# Testes Esperados

## Unitários

- provider disponível.
- provider indisponível.
- cache hit.
- cache miss.
- ForceRefresh ignora cache.
- Game ID inválido.
- XML válido.
- XML inválido.
- jogo não encontrado.
- erro de rede.
- timeout.
- resposta vazia.
- cache corrompido.
- mapeamento correto para `GameMetadataDto`.

## Mapeamento

Validar campos:

- GameId;
- Title;
- Region;
- Subtitle;
- Developer;
- Publisher;
- Year;
- Languages;
- Genres;
- Players;
- Rating;
- Description;
- OnlineFeatures;
- Compatibility;
- GameTdbVersion;
- UpdatedAt.

## Integração

- baixar ZIP fake;
- extrair XML fake;
- consultar jogo existente;
- consultar jogo ausente;
- salvar cache JSON;
- ler cache JSON;
- ForceRefresh baixa novamente;
- lote parcial continua após erro;
- UI/ViewModel recebe resumo correto.

## Segurança

- URL HTTP bloqueada.
- Game ID com traversal bloqueado.
- ZIP inválido rejeitado.
- ZIP slip bloqueado.
- XML com DTD/external entity bloqueado.
- XML grande demais rejeitado.
- Cache corrompido tratado.
- Logs não incluem XML completo.

---

# Regras para o Codex

- Não manter `NullGameMetadataProvider` como provider padrão.
- Implementar `GameTdbMetadataProvider` na Infrastructure.
- Não baixar metadados na UI.
- Não fazer HTTP na Application.
- Usar `JsonMetadataCacheService`.
- Usar HTTPS.
- Validar Game ID.
- Usar timeout e CancellationToken.
- Validar ZIP e XML.
- Bloquear XXE/DTD.
- Não executar conteúdo baixado.
- Não quebrar lote por jogo ausente.
- Preservar dados personalizados do usuário.
- Criar testes com HTTP fake.
- Não depender de internet real no pipeline.
- Atualizar DI.
- Atualizar Diagnostics.

---

# Dependências

- `F019-OfflineMetadataCache.md`
- `F022-GameTDBUpdateManager.md`
- `F023-SettingsManager.md`
- `F030-AdvancedDiagnostics.md`
- `F077-PluginMetadataProviders.md`
- `F081-SecurityFoundation.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F091-TestingStrategy.md`
- `F093-IntegrationTestingStandards.md`
- `F102-GameTdbCoverProvider.md`

---

# Definition of Done

- Provider real implementado.
- DI atualizado.
- Cache JSON integrado.
- Banco GameTDB baixado/validado com segurança.
- XML parseado com segurança.
- Mapeamento para `GameMetadataDto` completo.
- Dados personalizados preservados.
- Diagnostics atualizado.
- UI mostra resumo correto.
- Testes automatizados sem internet real.
- Documentação atualizada.

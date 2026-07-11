# F116 - Cover Asset Bulk Actions

> Feature ID: F116  
> Status: Ready for Development  
> Priority: High  
> Epic: Metadata & Assets / Library UX  
> Area: UI, Application, Cache, GameTDB, Batch Operations  
> Depends on: F102, F104

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 3.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: F102, F104, F113, F114.
- Reforço de escopo: menu de ações de assets, seleção de tipos de asset, ForceRefresh, limpeza de cache selecionado, troca de idioma, rebaixar assets, validar cache, progresso por lote, resumo final e atualização automática da UI.
- Fora de escopo reforçado: edição de imagem, upload para GameTDB, geração de imagem por IA, múltiplos providers remotos além do configurado e sincronização cloud.
- Regras adicionais: ações em lote devem respeitar seleção atual, confirmar operações destrutivas de cache, usar tarefas/notificações e não bloquear a UI.
- Impacto de UI: menu "Ações de Assets", seleção de frente/verso/disco/banner/icon, progresso por jogo e relatório final de sucesso/falha.

# Objetivo

Criar ações em lote para gerenciamento de capas e assets dos jogos, permitindo que o usuário
controle cache, idioma, atualização forçada e tipos de assets baixados.

A feature complementa:

- `F102-GameTdbCoverProvider.md`
- `F104-CoverAssetGalleryAndBatchSelection.md`

---

# Problema

Hoje o WiiStudio pode baixar assets via provider real e visualizar galeria, mas ainda falta
um conjunto de ações avançadas para múltiplos jogos:

- rebaixar capa;
- limpar cache;
- trocar idioma;
- forçar refresh;
- escolher quais tipos de assets baixar;
- corrigir assets inválidos;
- atualizar somente jogos selecionados.

---

# Resultado Esperado

Na biblioteca, ao selecionar um ou mais jogos, o usuário deve poder abrir um menu:

```text
Assets
├── Baixar capas ausentes
├── Forçar atualização
├── Rebaixar capa frontal
├── Baixar somente disc art
├── Limpar cache dos selecionados
├── Trocar idioma dos assets
└── Validar assets
```

---

# Escopo

## Incluído

- ações em lote para assets;
- seleção de tipos de assets;
- ForceRefresh por lote;
- limpar cache por jogo/asset;
- trocar idioma preferencial;
- rebaixar assets;
- validar assets existentes;
- contadores claros;
- integração com Task Manager;
- integração com Notification Center;
- testes com provider/cache fake.

## Fora do Escopo Inicial

- edição manual de imagem;
- upload para GameTDB;
- IA para gerar capas;
- busca em múltiplos providers sem plugin;
- sincronização em nuvem.

---

# Ações em Lote

## Baixar Capas Ausentes

Baixa somente assets que não existem no cache.

```text
Cache hit -> não baixar
Cache miss -> baixar
```

## Forçar Refresh

Ignora cache e baixa novamente.

```text
ForceRefresh = true
```

## Rebaixar Capa

Rebaixa asset específico, mesmo que já exista.

Uso:

- capa corrompida;
- idioma trocado;
- imagem antiga;
- usuário quer atualizar.

## Limpar Cache

Remove assets dos jogos selecionados.

Deve pedir confirmação:

```text
Você está prestes a remover os assets em cache dos jogos selecionados.
Eles poderão ser baixados novamente depois.
Deseja continuar?
```

## Trocar Idioma

Permite baixar assets usando outro idioma.

Exemplo:

```text
PT -> EN
EN -> PT
```

Regras:

- idioma afeta URLs do GameTDB;
- manter assets anteriores se opção “substituir” não estiver marcada;
- salvar preferência se usuário escolher.

## Escolher Tipo de Asset

Permitir escolher:

- FrontCover;
- BackCover / FullCover;
- DiscArt;
- Banner;
- Thumbnail/Icon.

---

# UI Sugerida

```text
[Assets em lote]
  Idioma: [PT v]
  Tipos:
    [x] Frente
    [x] Verso / Full Cover
    [x] Disco
    [ ] Banner
    [x] Thumbnail
  Opções:
    [ ] Forçar atualização
    [ ] Limpar antes de baixar
  [Executar]
```

---

# Fluxo Técnico

```text
Selecionar jogos
    ↓
Abrir ações de assets
    ↓
Escolher tipos/idioma/opções
    ↓
Confirmar
    ↓
Criar tarefa
    ↓
Para cada jogo e asset kind:
        verificar cache
        limpar se pedido
        baixar se ausente ou ForceRefresh
        validar imagem
        salvar cache
        atualizar UI
    ↓
Mostrar resumo
```

---

# Contratos

## ICoverAssetBulkActionService

```csharp
public interface ICoverAssetBulkActionService
{
    Task<CoverAssetBulkActionResult> ExecuteAsync(
        CoverAssetBulkActionRequest request,
        IProgress<CoverAssetBulkActionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

## CoverAssetBulkActionRequest

```csharp
public sealed class CoverAssetBulkActionRequest
{
    public IReadOnlyList<string> GameIds { get; init; } = [];
    public IReadOnlyList<CoverAssetKind> AssetKinds { get; init; } = [];
    public string? Language { get; init; }
    public bool ForceRefresh { get; init; }
    public bool ClearBeforeDownload { get; init; }
    public CoverAssetBulkActionMode Mode { get; init; }
}
```

## CoverAssetBulkActionMode

```csharp
public enum CoverAssetBulkActionMode
{
    DownloadMissing,
    ForceRefresh,
    ClearCache,
    ValidateCache,
    ReDownload,
    ChangeLanguage
}
```

---

# Resultado

```csharp
public sealed class CoverAssetBulkActionResult
{
    public int RequestedGames { get; init; }
    public int RequestedAssets { get; init; }
    public int Downloaded { get; init; }
    public int FromCache { get; init; }
    public int Cleared { get; init; }
    public int Missing { get; init; }
    public int Failed { get; init; }
    public int Invalid { get; init; }
    public bool Cancelled { get; init; }
}
```

---

# Integração com Cache

Usar:

```text
ICoverCache
CoverAssetKind
```

Mapeamento:

```text
FrontCover -> covers/
BackCover -> covers-back/
DiscArt -> discs/
Banner -> banners/
Icon -> thumbnails/
```

A UI não deve montar paths manualmente.

---

# Atualização da UI

Após cada asset alterado:

- atualizar miniatura;
- atualizar galeria;
- atualizar painel de detalhes;
- atualizar status;
- atualizar contadores.

Pode usar:

```text
CoverAssetUpdatedEvent
```

---

# Tratamento de Erros

## Asset Ausente

Conta como `Missing`, não erro fatal.

## Provider Fora do Ar

Conta como `Failed`, lote continua.

## Imagem Inválida

Conta como `Invalid`, não salva cache.

## Cache Corrompido

Marca como inválido e oferece rebaixar.

## Cancelamento

Interrompe próximos itens, mantém assets já processados.

---

# Segurança

- validar Game ID;
- validar asset kind;
- validar idioma;
- usar HTTPS via provider;
- não executar conteúdo baixado;
- validar imagem;
- bloquear path traversal;
- apagar somente dentro do diretório de cache;
- pedir confirmação antes de limpar cache;
- logs sem payload binário.

---

# Critérios de Aceite

- [ ] Usuário consegue escolher tipos de assets em lote.
- [ ] Usuário consegue forçar refresh.
- [ ] Usuário consegue limpar cache dos selecionados.
- [ ] Usuário consegue trocar idioma e baixar novamente.
- [ ] Lote mostra progresso.
- [ ] Lote mostra resumo correto.
- [ ] Falha parcial não interrompe todos os jogos.
- [ ] UI atualiza após cada asset processado.
- [ ] Testes usam provider/cache fake.

---

# Testes Esperados

- baixar apenas FrontCover;
- baixar FrontCover + DiscArt;
- ForceRefresh ignora cache;
- limpar cache remove somente assets selecionados;
- trocar idioma altera request;
- asset ausente contabiliza Missing;
- provider falha e lote continua;
- cancelamento gera resumo parcial;
- path traversal bloqueado;
- UI recebe eventos de atualização.

---

# Regras para o Codex

- Não acessar filesystem direto pela UI.
- Usar `ICoverCache`.
- Usar `CoverAssetKind`.
- Sempre pedir confirmação para limpar cache.
- Não quebrar lote por asset ausente.
- Sempre atualizar Task Manager.
- Sempre emitir resumo.
- Não depender de internet real nos testes.

---

# Dependências

- `F102-GameTdbCoverProvider.md`
- `F104-CoverAssetGalleryAndBatchSelection.md`
- `F113-TaskAndNotificationCenterIntegration.md`
- `F086-SafePathHandling.md`
- `F091-TestingStrategy.md`

---

# Definition of Done

- Serviço de bulk actions definido.
- UI de opções definida.
- Cache integrado.
- Task/Notification integrados.
- Testes com fakes definidos.

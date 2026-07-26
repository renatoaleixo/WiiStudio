# F104 - Cover Asset Gallery and Batch Selection

> Feature ID: F104  
> Status: Implemented and Homologated  
> Priority: High  
> Epic: Metadata & Assets / Library UX  
> Area: UI, Application, Cache, Asset Manager, Batch Operations  
> Depends on: F102 - GameTDB Cover Provider

# Objetivo

Criar uma experiência completa para **seleção em lote** e **visualização de capas/assets**
dos jogos no WiiStudio.

A feature complementa a `F102-GameTdbCoverProvider.md`, que implementa o download real
de capas/assets via GameTDB, permitindo que o usuário:

- selecione múltiplos jogos na biblioteca;
- baixe capas/assets em lote;
- acompanhe progresso por jogo;
- visualize capa frontal ampliada;
- navegue por uma galeria de assets do jogo;
- veja FrontCover, BackCover/FullCover, DiscArt, Banner e Thumbnail/Icon;
- atualize a UI imediatamente após o download, sem reiniciar a aplicação.

---

# Contexto

O WiiStudio já possui o fluxo de download de capas/assets definido na `F102`:

- `FrontCover`;
- `BackCover` / `FullCover`;
- `DiscArt`;
- `Thumbnail` / `Icon`;
- `Banner` opcional.

Porém, a experiência atual da UI ainda é limitada:

- não há seleção múltipla eficiente;
- não há ações em lote claras;
- a biblioteca mostra apenas uma miniatura simples;
- não existe galeria de assets do jogo;
- não é possível visualizar capa ampliada;
- não é possível ver verso/full cover e disc art de forma organizada.

---

# Resultado Esperado

A biblioteca deve permitir:

```text
Selecionar múltiplos jogos
    ↓
Executar ação em lote
    ↓
Baixar capas/assets
    ↓
Ver progresso e resumo
    ↓
Atualizar miniaturas
    ↓
Abrir detalhes do jogo
    ↓
Visualizar galeria de assets
```

---

# Seleção Múltipla na Biblioteca

## Requisitos

A tela de biblioteca deve permitir selecionar mais de um jogo.

Suportes obrigatórios:

- clique simples para selecionar um jogo;
- `Ctrl + Click` para adicionar/remover item da seleção;
- `Shift + Click` para selecionar intervalo;
- botão **Selecionar tudo**;
- botão **Limpar seleção**;
- contador de selecionados;
- ações em lote baseadas na seleção.

---

# Comportamento de Seleção

## Clique Simples

```text
Clique em jogo
    ↓
Seleciona somente aquele jogo
    ↓
Atualiza painel de detalhes
```

## Ctrl + Click

```text
Ctrl + Click em jogo
    ↓
Alterna seleção daquele jogo
    ↓
Mantém os demais selecionados
```

## Shift + Click

```text
Selecionar jogo inicial
    ↓
Shift + Click em outro jogo
    ↓
Seleciona intervalo entre os dois
```

## Selecionar Tudo

```text
Seleciona todos os jogos visíveis no filtro atual
```

## Limpar Seleção

```text
Remove todos os jogos da seleção
```

---

# Contador de Seleção

A UI deve mostrar:

```text
1 jogo selecionado
```

ou:

```text
12 jogos selecionados
```

Quando não houver seleção:

```text
Nenhum jogo selecionado
```

---

# Ações em Lote

Ações disponíveis para seleção múltipla:

- baixar capas;
- sincronizar metadados;
- exportar;
- remover;
- favoritar/desfavoritar;
- verificar integridade, quando aplicável.

Ações destrutivas, como remover, devem exigir confirmação.

---

# Download de Capas em Lote

## Requisito Principal

O botão **Baixar capas** deve atuar sobre todos os jogos selecionados.

Fluxo:

```text
Usuário seleciona 2+ jogos
    ↓
Clica em Baixar capas
    ↓
Sistema cria tarefa em lote
    ↓
Para cada jogo:
        verificar cache
        baixar assets ausentes
        validar imagem
        salvar cache
        atualizar UI
    ↓
Mostrar resumo final
```

---

# Assets Baixados no Lote

A ação deve permitir baixar:

- FrontCover;
- BackCover / FullCover;
- DiscArt;
- Banner, se configurado;
- Thumbnail/Icon, gerado quando possível.

A primeira versão pode oferecer uma ação simples:

```text
Baixar capas e artes
```

E futuramente permitir seleção granular:

```text
[x] Frente
[x] Verso / Full Cover
[x] Disco
[x] Banner
[x] Thumbnail
```

---

# Progresso por Jogo

Durante o lote, exibir:

```text
Baixando capas 3/20
Atual: Mario Kart Wii
```

Status por jogo:

```text
RMCP01 - baixando capa frontal
RMCP01 - capa encontrada no cache
RMCP01 - disc art indisponível
RMCP01 - concluído
```

---

# Resultado do Lote

Exibir resumo claro:

```text
Capas baixadas: 18
Do cache: 34
Ausentes: 5
Falhas: 2
Jogos processados: 20
```

O lote não deve parar se um jogo falhar.

---

# Cancelamento

O usuário deve poder cancelar a operação.

Regras:

- respeitar `CancellationToken`;
- finalizar item atual de forma segura quando possível;
- não corromper cache;
- manter assets já baixados;
- exibir resumo parcial;
- registrar cancelamento.

---

# Visualização Ampliada da Capa

## Painel de Detalhes

Ao clicar em um jogo, mostrar painel de detalhes lateral ou inferior.

O painel deve exibir:

- capa frontal grande;
- título do jogo;
- Game ID;
- região;
- status dos assets;
- botões de ação;
- galeria de assets.

---

# Capa Frontal Grande

A capa frontal deve ser exibida em tamanho maior que a miniatura da lista.

Regras:

- usar `FrontCover` do cache;
- mostrar placeholder se ausente;
- atualizar automaticamente após download;
- manter proporção;
- evitar distorção;
- carregar de forma assíncrona.

---

# Modal / Lightbox

Ao clicar na capa grande:

```text
Abrir modal/lightbox
    ↓
Mostrar imagem ampliada
    ↓
Permitir fechar com ESC
    ↓
Permitir fechar clicando fora, se padrão da UI permitir
```

Funcionalidades desejadas:

- zoom básico;
- ajustar à tela;
- visualizar tamanho original;
- copiar caminho do asset, opcional;
- abrir pasta do asset, opcional.

---

# Placeholder

Quando não houver capa:

```text
Imagem não disponível
```

O placeholder deve:

- seguir o Design System;
- usar ícone adequado;
- não parecer erro crítico;
- oferecer ação para baixar asset quando aplicável.

---

# Galeria de Assets do Jogo

## Objetivo

Exibir todos os assets baixados para o jogo selecionado.

A galeria pode usar abas, cards ou seletor.

Assets exibidos:

- Frente / FrontCover;
- Verso ou FullCover / BackCover;
- Disco / DiscArt;
- Banner, se existir;
- Thumbnail/Icon.

---

# Layout de Galeria

## Opção 1 - Abas

```text
[Frente] [Verso] [Disco] [Banner] [Icon]
```

## Opção 2 - Cards

```text
┌────────────┐ ┌────────────┐ ┌────────────┐
│ Frente     │ │ Verso      │ │ Disco      │
│ imagem     │ │ imagem     │ │ imagem     │
└────────────┘ └────────────┘ └────────────┘
```

## Opção Recomendada

Usar cards para visão geral e lightbox para ampliação.

---

# Estados Visuais dos Assets

Cada asset deve apresentar um estado.

```csharp
public enum CoverAssetViewState
{
    Available,
    Missing,
    FromCache,
    Downloading,
    Failed,
    InvalidImage
}
```

## Available

Asset disponível e válido.

## Missing

Asset não existe no cache e provider retornou ausente.

## FromCache

Asset encontrado no cache.

## Downloading

Download em andamento.

## Failed

Falha de rede, timeout ou provider indisponível.

## InvalidImage

Arquivo baixado ou cache existente é inválido.

---

# Integração com Cache

Usar:

```text
ICoverCache
CoverAssetKind
```

Mapeamento oficial:

| CoverAssetKind | Diretório |
|---|---|
| FrontCover | `covers/` |
| BackCover | `covers-back/` |
| DiscArt | `discs/` |
| Banner | `banners/` |
| Icon | `thumbnails/` |

A UI não deve montar paths manualmente. Deve consumir serviços/view models que resolvem
os assets com segurança.

---

# Atualização Automática da UI

Após baixar capas:

- atualizar miniatura da biblioteca;
- atualizar painel de detalhes;
- atualizar galeria;
- atualizar status do asset;
- atualizar contadores;
- não exigir reiniciar o WiiStudio.

Fluxo:

```text
Download concluído
    ↓
ICoverDownloadService publica resultado
    ↓
ViewModel atualiza asset paths/states
    ↓
Bindings atualizam imagens
```

---

# Eventos / Notificações Internas

Pode ser criado um evento:

```csharp
public sealed class CoverAssetUpdatedEvent
{
    public string GameId { get; init; } = string.Empty;
    public CoverAssetKind Kind { get; init; }
    public string? AssetPath { get; init; }
    public CoverAssetViewState State { get; init; }
}
```

Uso:

- LibraryViewModel atualiza miniatura;
- GameDetailsViewModel atualiza galeria;
- NotificationCenter exibe resumo.

---

# Layout Sugerido da Biblioteca

```text
┌──────────────────────────────────────────────────────────────┐
│ Toolbar                                                      │
│ [Selecionar tudo] [Limpar] [Baixar capas] [Metadados]         │
│ 12 jogos selecionados                                        │
├──────────────────────────────────────┬───────────────────────┤
│ Lista/Tabela de Jogos                │ Painel de Detalhes     │
│                                      │                       │
│ [thumb] Mario Kart Wii  RMCP01       │ [Capa grande]          │
│ [thumb] Wii Sports      RSPE01       │ Título                 │
│ [thumb] ...                         │ Game ID                │
│                                      │                       │
│                                      │ Galeria:               │
│                                      │ [Frente][Verso][Disco] │
└──────────────────────────────────────┴───────────────────────┘
```

---

# Colunas da Biblioteca

A lista/tabela deve conter:

- checkbox de seleção, opcional;
- miniatura;
- título;
- Game ID;
- região;
- formato;
- status de assets, opcional.

---

# Painel Lateral ou Inferior

## Desktop Wide

Usar painel lateral direito.

## Janela Menor

Usar painel inferior ou drawer.

## Responsividade

Seguir `F060-ResponsiveBehaviors.md`.

---

# Acessibilidade e UX

Obrigatório:

- imagens com descrição/título do jogo;
- botões com texto claro;
- botões com tooltip;
- navegação por teclado;
- foco visível;
- feedback visual durante download;
- leitura clara de status;
- não depender apenas de cor;
- placeholder acessível;
- não travar UI.

Exemplo de descrição da imagem:

```text
Capa frontal de Mario Kart Wii
```

---

# Commands Recomendados

```csharp
DownloadSelectedCoversCommand
SynchronizeSelectedMetadataCommand
SelectAllGamesCommand
ClearSelectionCommand
OpenCoverLightboxCommand
RefreshSelectedAssetsCommand
CancelBatchOperationCommand
```

---

# ViewModels Sugeridos

## LibraryViewModel

Responsável por:

- lista de jogos;
- seleção múltipla;
- ações em lote;
- contador de selecionados;
- status da operação.

## GameAssetGalleryViewModel

Responsável por:

- assets do jogo selecionado;
- estados dos assets;
- abrir lightbox;
- refresh de assets.

## CoverAssetViewModel

```csharp
public sealed partial class CoverAssetViewModel : ObservableObject
{
    public CoverAssetKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ImagePath { get; set; }
    public CoverAssetViewState State { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

# Application Services

A UI deve chamar serviços da Application.

Exemplo:

```csharp
public interface IBatchCoverDownloadService
{
    Task<CoverBatchDownloadSummary> DownloadAsync(
        IReadOnlyList<string> gameIds,
        IReadOnlyList<CoverAssetKind> assetKinds,
        bool forceRefresh,
        IProgress<CoverBatchProgress> progress,
        CancellationToken cancellationToken = default);
}
```

---

# Progresso

```csharp
public sealed class CoverBatchProgress
{
    public int TotalGames { get; init; }
    public int ProcessedGames { get; init; }
    public string? CurrentGameId { get; init; }
    public string? CurrentTitle { get; init; }
    public CoverAssetKind? CurrentAssetKind { get; init; }
    public CoverAssetViewState CurrentState { get; init; }
}
```

---

# Integração com Task Manager

Downloads em lote devem aparecer no Task Manager.

Exemplo:

```text
Baixando capas
Status: Em andamento
Progresso: 8/40 assets
Cancelar
```

---

# Integração com Notification Center

Ao concluir:

```text
Download de capas concluído: 18 baixadas, 34 do cache, 5 ausentes, 2 falhas.
```

Em cancelamento:

```text
Download de capas cancelado. 8 jogos processados.
```

---

# Tratamento de Erro

## Falha Parcial

Se um jogo falhar:

- registrar erro;
- marcar asset como `Failed`;
- continuar próximo jogo;
- incluir em resumo final.

## Asset Ausente

Se provider retorna 404/Missing:

- marcar como `Missing`;
- não tratar como erro fatal.

## Imagem Inválida

Se o cache ou download contém imagem inválida:

- marcar como `InvalidImage`;
- permitir ForceRefresh;
- não quebrar galeria.

## Cache Corrompido

Se asset local não carregar:

- marcar `InvalidImage`;
- permitir baixar novamente;
- registrar warning.

---

# Segurança

Obrigatório:

- UI não monta path manualmente;
- usar `ICoverCache`;
- validar paths no cache;
- bloquear path traversal;
- não executar conteúdo baixado;
- validar imagem;
- carregar imagem de forma segura;
- limitar tamanho;
- não travar UI;
- logs sem payload binário.

---

# Critérios de Aceite

- [x] Selecionar múltiplos jogos na biblioteca.
- [x] `Ctrl + Click` altera seleção.
- [x] `Shift + Click` seleciona intervalo.
- [x] Botão **Selecionar tudo** funciona.
- [x] Botão **Limpar seleção** funciona.
- [x] Contador de seleção está correto.
- [x] Baixar capas para múltiplos jogos selecionados.
- [x] Progresso por jogo é exibido.
- [x] Lote continua mesmo com falhas parciais.
- [x] Operação em lote pode ser cancelada.
- [x] Visualizar capa frontal ampliada.
- [x] Visualizar BackCover/FullCover.
- [x] Visualizar DiscArt.
- [x] Visualizar Banner quando existir.
- [x] Mostrar placeholder quando asset não existir.
- [x] Atualizar UI logo após download.
- [x] Miniatura da biblioteca atualiza após download.
- [x] Painel de detalhes atualiza após download.
- [x] Galeria atualiza após download.
- [x] Testes não dependem de internet real.

---

# Testes Esperados

## Seleção

- seleção simples;
- seleção múltipla com Ctrl;
- seleção por intervalo com Shift;
- selecionar tudo;
- limpar seleção;
- contador correto;
- seleção respeita filtro atual.

## Batch Download

- baixar capas para 2+ jogos;
- progresso por jogo;
- cancelamento;
- falha parcial não quebra lote;
- resumo final correto;
- asset ausente contabilizado como Missing;
- cache contabilizado como FromCache.

## Cache e Galeria

- carregar FrontCover do cache;
- carregar BackCover do cache;
- carregar DiscArt do cache;
- carregar Banner do cache quando existir;
- carregar Thumbnail/Icon do cache;
- asset ausente mostra placeholder;
- imagem inválida mostra estado correto;
- após download, galeria atualiza.

## UI

- abrir lightbox;
- fechar lightbox com ESC;
- placeholder acessível;
- botão baixar desabilitado sem seleção;
- botão cancelar visível durante operação;
- UI não bloqueia durante download.

## Serviços

- `IBatchCoverDownloadService` processa múltiplos IDs;
- `CoverBatchProgress` publica progresso;
- `CoverAssetUpdatedEvent` atualiza ViewModel;
- `ICoverCache` resolve diretórios corretos.

---

# Impacto no Código Atual

## Criar

```text
GameAssetGalleryViewModel
CoverAssetViewModel
CoverAssetViewState
CoverAssetUpdatedEvent
CoverBatchProgress
IBatchCoverDownloadService
BatchCoverDownloadService
CoverLightboxDialog
GameAssetGalleryView
```

## Alterar

```text
LibraryViewModel
LibraryView
LibraryDataGrid/List
DownloadSelectedCoversCommand
FileSystemCoverCache, se necessário
NotificationCenter integration
TaskManager integration
```

## Atualizar UI

- adicionar seleção múltipla;
- adicionar toolbar de ações em lote;
- adicionar contador de selecionados;
- adicionar painel de detalhes;
- adicionar galeria;
- adicionar lightbox;
- atualizar miniatura após download.

---

# Regras para o Codex

- Não colocar regra de batch download na View.
- Usar ViewModel e Application Services.
- Não montar path de asset diretamente na UI.
- Usar `ICoverCache`.
- Usar `CoverAssetKind`.
- Não travar UI durante downloads.
- Usar `CancellationToken`.
- Atualizar UI via binding/evento.
- Não falhar lote inteiro por um asset ausente.
- Mostrar contadores claros.
- Implementar placeholders.
- Garantir acessibilidade básica.
- Criar testes com provider/cache fake.
- Não depender de internet real.

---

# Dependências

- `F017-DownloadCovers.md`
- `F020-AssetManager.md`
- `F021-BannerAndDiscArt.md`
- `F025-NotificationCenter.md`
- `F026-TaskManager.md`
- `F032-LibraryView.md`
- `F049-DataGrid.md`
- `F057-ProgressIndicators.md`
- `F058-EmptyErrorAndLoadingStates.md`
- `F060-ResponsiveBehaviors.md`
- `F070-AccessibilityNavigation.md`
- `F086-SafePathHandling.md`
- `F091-TestingStrategy.md`
- `F094-UITestingStrategy.md`
- `F102-GameTdbCoverProvider.md`

---

# Definition of Done

- Seleção múltipla implementada.
- Ações em lote disponíveis.
- Download de capas em lote funcionando.
- Progresso e cancelamento funcionando.
- Galeria de assets implementada.
- Lightbox de imagem implementado.
- Placeholders implementados.
- UI atualiza após download.
- Testes automatizados com fakes.
- Documentação atualizada.

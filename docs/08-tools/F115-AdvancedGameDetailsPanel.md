# F115 - Advanced Game Details Panel

> Feature ID: F115  
> Status: Ready for Development  
> Priority: High  
> Epic: Library UX / Game Details  
> Area: UI, Metadata, Assets, Library, Operations  
> Depends on: F104 - Cover Asset Gallery and Batch Selection

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 3.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: F013, F102, F103, F104, F110, F114.
- Reforço de escopo: painel avançado, capa grande, galeria de assets, metadados completos, dados técnicos, informações de arquivo, ações contextuais, estados vazio/loading/error e integração com SQLite quando F114 estiver ativo.
- Fora de escopo reforçado: edição avançada de imagens, edição massiva de metadados, marketplace de assets, scraping adicional fora dos providers existentes e operação direta sem Application Service.
- Regras adicionais: sem jogo selecionado deve mostrar placeholder, dados ausentes não podem quebrar o painel, ações indisponíveis devem explicar motivo e assets devem vir de cache/repositories.
- Impacto de UI: layout responsivo com detalhes, galeria/lightbox, ações contextuais e mensagens claras para metadados/capas ausentes.

# Objetivo

Melhorar a visualização do jogo selecionado na biblioteca com um painel avançado de detalhes,
exibindo capas, disco, metadados completos, dados técnicos, caminho do arquivo e operações disponíveis.

---

# Problema Atual

A biblioteca exibe informações básicas e miniatura, mas o usuário não tem uma visão completa do jogo.

Falta mostrar de forma organizada:

- capa frente;
- capa verso/full cover;
- disco/DVD;
- metadados completos;
- ID6/Game ID;
- região;
- formato;
- tamanho;
- caminho;
- data de importação;
- operações disponíveis.

---

# Resultado Esperado

Ao selecionar um jogo:

```text
Painel de Detalhes
    ├── Capa grande
    ├── Galeria de assets
    ├── Metadados
    ├── Informações técnicas
    ├── Arquivos/caminhos
    └── Ações disponíveis
```

---

# Layout Sugerido

```text
┌───────────────────────────────────────────────┐
│ Mario Kart Wii                                │
│ RMCP01 • PAL • WBFS                           │
├───────────────┬───────────────────────────────┤
│ Capa grande   │ Metadados                     │
│               │ Developer, Publisher, Year    │
│ [Frente]      │ Genres, Players, Rating        │
├───────────────┴───────────────────────────────┤
│ Galeria: Frente | Verso | Disco | Banner       │
├───────────────────────────────────────────────┤
│ Técnico: formato, tamanho, caminho, importado  │
├───────────────────────────────────────────────┤
│ [Baixar capas] [Sincronizar metadados] [...]   │
└───────────────────────────────────────────────┘
```

---

# Dados Exibidos

## Identificação

- título;
- Game ID / ID6;
- região;
- subtitle;
- favorito;
- status da biblioteca.

## Metadados

- developer;
- publisher;
- year;
- languages;
- genres;
- players;
- rating;
- description;
- online features;
- compatibility;
- GameTDB version;
- updated at.

## Dados Técnicos

- formato;
- tamanho;
- caminho;
- hash, se disponível;
- data de importação;
- última verificação;
- origem;
- duplicidade;
- integridade.

## Assets

- FrontCover;
- BackCover/FullCover;
- DiscArt;
- Banner;
- Thumbnail/Icon.

---

# Ações Disponíveis

Botões:

- Baixar/atualizar capas;
- Sincronizar metadados;
- Abrir pasta;
- Converter;
- Exportar;
- Enviar para USB;
- Verificar integridade;
- Remover;
- Favoritar;
- Abrir logs/diagnóstico do jogo.

Ações indisponíveis devem explicar motivo.

---

# Estados Visuais

- carregando;
- sem jogo selecionado;
- metadados ausentes;
- capas ausentes;
- erro ao carregar asset;
- arquivo não encontrado;
- jogo removido;
- operação em andamento.

---

# Integração com F104

A galeria deve reutilizar:

- `CoverAssetKind`;
- `ICoverCache`;
- `GameAssetGalleryViewModel`;
- lightbox;
- placeholders;
- atualização automática após download.

---

# Integração com F103/F114

Metadados vêm de:

```text
GameMetadataDto
SQLite repository
Json cache, quando aplicável
```

Quando F114 estiver ativa, o painel deve carregar do SQLite via Application Service.

---

# ViewModel

## AdvancedGameDetailsViewModel

```csharp
public sealed partial class AdvancedGameDetailsViewModel : ObservableObject
{
    public string? GameId { get; }
    public string? Title { get; }
    public string? Region { get; }
    public string? Format { get; }
    public string? FilePath { get; }
    public long? SizeBytes { get; }
    public GameAssetGalleryViewModel Assets { get; }
    public IReadOnlyList<GameDetailActionViewModel> Actions { get; }
}
```

---

# Serviços

```csharp
public interface IGameDetailsService
{
    Task<GameDetailsDto?> GetDetailsAsync(
        string gameId,
        CancellationToken cancellationToken = default);
}
```

---

# UX

## Sem Jogo Selecionado

```text
Selecione um jogo para ver detalhes.
```

## Metadados Ausentes

```text
Metadados ainda não sincronizados.
[Sincronizar metadados]
```

## Capas Ausentes

```text
Capas ainda não baixadas.
[Baixar capas]
```

## Arquivo Ausente

```text
O arquivo deste jogo não foi encontrado no caminho original.
```

---

# Acessibilidade

Obrigatório:

- imagens com texto alternativo;
- botões com descrição;
- navegação por teclado;
- foco visível;
- não depender apenas de cor;
- área rolável acessível;
- descrição clara de status.

---

# Responsividade

Desktop amplo:

```text
painel lateral direito
```

Janela menor:

```text
painel inferior ou drawer
```

Tela estreita:

```text
abas/seções empilhadas
```

---

# Performance

- carregar assets de forma assíncrona;
- não travar UI;
- usar thumbnails na lista;
- carregar imagem grande sob demanda;
- virtualizar quando necessário;
- cache de ViewModels quando seguro.

---

# Diagnóstico

Por jogo, permitir copiar detalhes:

```text
Game ID
Title
File path
Format
Size
Metadata status
Asset status
Last operations
```

---

# Testes Esperados

## Unitários

- carrega detalhes completos;
- jogo sem metadados;
- jogo sem assets;
- arquivo ausente;
- ações disponíveis;
- ações indisponíveis com motivo;
- formatação de tamanho;
- região exibida;
- descrição longa exibida.

## UI/ViewModel

- seleção de jogo atualiza painel;
- capa frontal carregada;
- back/full cover carregada;
- disc art carregada;
- placeholder aparece quando ausente;
- botão baixar capas chama command;
- botão metadados chama command;
- lightbox abre;
- estados Loading/Error.

---

# Critérios de Aceite

- [ ] Painel avançado aparece ao selecionar jogo.
- [ ] Exibe FrontCover grande.
- [ ] Exibe BackCover/FullCover.
- [ ] Exibe DiscArt.
- [ ] Exibe metadados completos.
- [ ] Exibe ID6/Game ID.
- [ ] Exibe região.
- [ ] Exibe formato.
- [ ] Exibe tamanho.
- [ ] Exibe caminho.
- [ ] Exibe data de importação.
- [ ] Ações disponíveis aparecem.
- [ ] Placeholders funcionam.
- [ ] UI atualiza após download/sync.
- [ ] Testes com fakes passam.

---

# Regras para o Codex

- Não acessar SQLite diretamente na View.
- Não montar path de asset na UI.
- Reutilizar F104.
- Usar Application Service.
- Não bloquear UI.
- Tratar dados ausentes.
- Criar placeholders.
- Garantir acessibilidade.
- Não duplicar lógica de metadados.

---

# Dependências

- `F013-GameDetails.md`
- `F032-LibraryView.md`
- `F102-GameTdbCoverProvider.md`
- `F103-GameTDBMetadataProvider.md`
- `F104-CoverAssetGalleryAndBatchSelection.md`
- `F114-LibraryPersistenceWithSQLite.md`
- `F060-ResponsiveBehaviors.md`
- `F070-AccessibilityNavigation.md`

---

# Definition of Done

- Painel avançado definido.
- Dados e ações definidos.
- Integrações com assets/metadados definidas.
- Estados visuais definidos.
- Testes definidos.

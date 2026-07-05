# 008 - UI & UX Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a experiência do usuário e os padrões visuais do WiiStudio.

------------------------------------------------------------------------

# Princípios

-   Interface limpa.
-   Máximo de 3 cliques para operações frequentes.
-   Feedback visual para todas as operações.
-   Tema Dark como padrão.
-   Totalmente responsiva.

------------------------------------------------------------------------

# Navegação

``` text
Dashboard
 ├── Biblioteca
 ├── Dispositivos
 ├── Conversões
 ├── Capas
 ├── Configurações
 └── Logs
```

------------------------------------------------------------------------

# Dashboard

Exibir:

-   HD conectado
-   Espaço livre
-   Quantidade de jogos
-   Conversões em andamento
-   Últimos jogos adicionados

------------------------------------------------------------------------

# Biblioteca

Colunas:

-   Capa
-   Título
-   ID
-   Região
-   Formato
-   Tamanho

Ações:

-   Abrir
-   Converter
-   Remover
-   Atualizar capa

------------------------------------------------------------------------

# Conversões

Cada tarefa deve exibir:

-   Barra de progresso
-   Tempo restante
-   Velocidade
-   Cancelar

Nunca bloquear a interface.

------------------------------------------------------------------------

# Componentes

Utilizar componentes reutilizáveis:

-   GameCard
-   ProgressCard
-   UsbStatus
-   NotificationBar
-   SearchBox

------------------------------------------------------------------------

# Estados da UI

## Carregando

Skeleton loading.

## Sem dados

Mensagem amigável com CTA.

## Erro

Mensagem clara + botão "Tentar novamente".

## Sucesso

Toast discreto.

------------------------------------------------------------------------

# Drag & Drop

Suportar:

-   ISO
-   WBFS
-   RVZ
-   NKIT

Ao soltar:

1.  Validar.
2.  Detectar formato.
3.  Abrir assistente de importação.

------------------------------------------------------------------------

# Pesquisa

Pesquisa instantânea.

Filtros:

-   Região
-   Formato
-   Tamanho
-   Favoritos

------------------------------------------------------------------------

# Acessibilidade

-   Navegação por teclado.
-   Alto contraste.
-   Tooltips.
-   Fontes escaláveis.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca criar telas sem ViewModel.
-   Não duplicar componentes.
-   Utilizar estilos compartilhados.
-   Separar Resources por responsabilidade.

------------------------------------------------------------------------

# Critérios de Aceite

-   Layout consistente.
-   Responsivo.
-   Componentes reutilizados.
-   Estados de tela implementados.
-   UX uniforme.

------------------------------------------------------------------------

# Checklist

-   [ ] Dashboard
-   [ ] Biblioteca
-   [ ] Conversões
-   [ ] Configurações
-   [ ] Drag & Drop
-   [ ] Pesquisa
-   [ ] Acessibilidade

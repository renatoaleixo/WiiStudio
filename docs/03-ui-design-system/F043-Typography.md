# F043 - Typography

> Feature ID: F043
> Status: Implemented and Homologated
> Priority: High
> Epic: Design System

# Objetivo

Definir o sistema tipográfico oficial do WiiStudio, garantindo legibilidade,
consistência visual, acessibilidade e reutilização em toda a aplicação.

## Requisitos Funcionais

- RF001 Definir famílias tipográficas oficiais.
- RF002 Padronizar tamanhos e pesos.
- RF003 Definir alturas de linha e espaçamentos.
- RF004 Suportar temas Light/Dark.
- RF005 Garantir acessibilidade WCAG.
- RF006 Disponibilizar tokens reutilizáveis.

## Categorias

- Display
- Heading
- Title
- Subtitle
- Body
- Caption
- Code
- Monospace

## Estrutura

styles/
└── tokens/
    ├── Typography.axaml
    ├── FontSizes.axaml
    ├── FontWeights.axaml
    └── TextStyles.axaml

## Convenções

- Typography.Display
- Typography.Title
- Typography.Body
- Typography.Caption
- Typography.Code

Nunca definir FontSize diretamente nas Views.

## Regras para o Codex

- Utilizar apenas TextStyles oficiais.
- Não criar tamanhos arbitrários.
- Componentes devem consumir tokens tipográficos.
- Toda alteração deve preservar a hierarquia visual.

## Acessibilidade

- Escalável por DPI.
- Compatível com aumento de fonte.
- Contraste adequado com o Color System.
- Leitura confortável em telas grandes e pequenas.

## Critérios de Aceite

- [x] Hierarquia tipográfica definida.
- [x] Tokens reutilizáveis.
- [x] Sem FontSize hardcoded.
- [x] Compatível com Light/Dark.
- [x] Acessibilidade validada.

## Testes

### Unitários
- Carregamento dos estilos.
- Aplicação dos tokens.

### Integração
- MainWindow
- Dashboard
- DataGrid
- Dialogs
- Wizards

## Dependências

- F041-DesignSystemFoundation.md
- F042-ColorSystem.md

## Definition of Done

- Sistema tipográfico implementado.
- Tokens documentados.
- Testes aprovados.
- Documentação atualizada.

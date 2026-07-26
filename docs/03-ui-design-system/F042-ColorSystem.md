# F042 - Color System

> Feature ID: F042
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o sistema oficial de cores do WiiStudio utilizando Design Tokens, garantindo consistência visual, acessibilidade, temas Light/Dark e facilidade de manutenção.

## Requisitos Funcionais

- RF001 Centralizar todas as cores em Design Tokens.
- RF002 Suportar temas Light e Dark.
- RF003 Permitir extensão por temas personalizados.
- RF004 Eliminar cores hardcoded.
- RF005 Garantir contraste WCAG AA.
- RF006 Disponibilizar paleta reutilizável.

## Categorias

- Primary
- Secondary
- Accent
- Background
- Surface
- Border
- Text
- Success
- Warning
- Error
- Info
- Disabled

## Estrutura

styles/
└── tokens/
    ├── Colors.axaml
    ├── LightColors.axaml
    └── DarkColors.axaml

## Regras para o Codex

- Nunca usar cores HEX diretamente nas Views.
- Sempre utilizar Design Tokens.
- Todo componente deve herdar o tema ativo.
- Novas cores exigem documentação.

## Critérios de Aceite

- Tokens definidos.
- Light/Dark funcionando.
- Sem cores hardcoded.
- Contraste validado.
- Documentação completa.

## Dependências

- F039-ThemeEngine.md
- F041-DesignSystemFoundation.md

## Definition of Done

- Paleta implementada.
- Tokens documentados.
- Testes aprovados.
- Documentação atualizada.

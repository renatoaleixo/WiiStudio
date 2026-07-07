# F044 - Iconography

> Feature ID: F044
> Status: Ready for Development
> Priority: High
> Epic: Design System

# Objetivo

Definir o sistema oficial de iconografia do WiiStudio, padronizando a utilização
de ícones em toda a aplicação para garantir consistência visual, acessibilidade,
clareza e facilidade de manutenção.

## Requisitos Funcionais

- RF001 Definir biblioteca oficial de ícones (Fluent UI Icons).
- RF002 Padronizar tamanhos e categorias.
- RF003 Definir estados visuais dos ícones.
- RF004 Integrar com Light e Dark Theme.
- RF005 Disponibilizar ícones reutilizáveis.
- RF006 Permitir futura extensão por plugins.

## Biblioteca Oficial

- Fluent UI System Icons
- SVG vetorial
- Escalável
- Compatível com Avalonia

## Categorias

- Navigation
- Devices
- Library
- Import
- Export
- Conversion
- Settings
- Notifications
- Diagnostics
- Favorites
- Search
- Actions

## Tamanhos

- Small (16px)
- Medium (20px)
- Default (24px)
- Large (32px)
- Extra Large (48px)

## Estados

- Default
- Hover
- Pressed
- Disabled
- Selected
- Error
- Success
- Warning

## Estrutura

styles/
└── icons/
    ├── FluentIcons.axaml
    ├── IconTokens.axaml
    └── IconStyles.axaml

## Convenções

- Icon.Library
- Icon.Import
- Icon.Convert
- Icon.Settings
- Icon.Warning
- Icon.Error

Nunca utilizar imagens bitmap para ícones da interface.

## Regras para o Codex

- Utilizar apenas ícones registrados.
- Não duplicar ícones semanticamente iguais.
- Manter consistência entre telas.
- Preferir SVG vetorial.

## Critérios de Aceite

- [ ] Biblioteca oficial definida.
- [ ] Tamanhos padronizados.
- [ ] Estados documentados.
- [ ] Compatível com temas.
- [ ] Tokens reutilizáveis.

## Testes

### Unitários
- Carregamento dos recursos.
- Resolução dos ícones.

### Integração
- MainWindow
- Toolbar
- Navigation
- Dialogs
- Dashboard

## Dependências

- F041-DesignSystemFoundation.md
- F042-ColorSystem.md
- F043-Typography.md

## Definition of Done

- Biblioteca integrada.
- Tokens documentados.
- Testes aprovados.
- Documentação atualizada.

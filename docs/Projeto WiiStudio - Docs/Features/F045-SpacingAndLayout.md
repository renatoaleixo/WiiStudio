# F045 - Spacing & Layout

> Feature ID: F045
> Status: Ready for Development
> Priority: Critical
> Epic: Design System

# Objetivo

Definir o sistema oficial de espaçamentos, alinhamentos, grids e composição
visual do WiiStudio, garantindo consistência entre todas as telas,
componentes e futuras extensões da aplicação.

---

# Requisitos Funcionais

## RF001
Padronizar todos os espaçamentos da aplicação.

## RF002
Definir Grid System reutilizável.

## RF003
Padronizar alinhamentos.

## RF004
Definir margens e paddings oficiais.

## RF005
Definir largura máxima de conteúdo.

## RF006
Garantir responsividade.

---

# Escala Oficial

Utilizar sempre múltiplos de 4.

| Token | Valor |
|--------|------:|
| XS | 4 px |
| SM | 8 px |
| MD | 12 px |
| LG | 16 px |
| XL | 24 px |
| XXL | 32 px |
| XXXL | 48 px |
| HUGE | 64 px |

---

# Grid System

Base:

- 12 colunas
- Espaçamento entre colunas
- Layout fluido
- Containers centralizados

---

# Layout

Toda tela deverá possuir:

- Header
- Toolbar
- Workspace
- Side Panel
- Status Bar

---

# Padding Oficial

Cards

16 px

Dialogs

24 px

Buttons

12 x 16 px

Forms

16 px

Dashboard

24 px

---

# Margens

Componentes irmãos:

16 px

Seções:

32 px

Grandes blocos:

48 px

---

# Alinhamentos

Sempre utilizar:

- Left
- Center
- Right

Nunca posicionamento absoluto sem necessidade.

---

# Responsividade

Breakpoints

Small

Tablet

Desktop

UltraWide

---

# Estrutura

styles/

tokens/

Spacing.axaml

Layout.axaml

Grid.axaml

---

# Regras para o Codex

Nunca utilizar Margin hardcoded.

Nunca utilizar Padding hardcoded.

Sempre consumir tokens.

Toda tela deve respeitar Grid System.

---

# Critérios de Aceite

- [ ] Grid implementado.
- [ ] Espaçamentos padronizados.
- [ ] Layout consistente.
- [ ] Responsividade.
- [ ] Tokens reutilizáveis.

---

# Testes

## Unitários

- Carregamento dos tokens.

- Grid.

## Integração

- MainWindow.

- Dashboard.

- Dialogs.

- Wizards.

- DataGrid.

---

# Dependências

F041-DesignSystemFoundation.md

F042-ColorSystem.md

F043-Typography.md

F044-Iconography.md

---

# Definition of Done

- Grid implementado.
- Tokens documentados.
- Testes aprovados.
- Documentação atualizada.

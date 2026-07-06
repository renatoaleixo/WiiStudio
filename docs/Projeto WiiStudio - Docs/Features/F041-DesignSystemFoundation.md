
# F041 - Design System Foundation

> Feature ID: F041  
> Status: Ready for Development  
> Priority: Critical  
> Epic: Design System

# Objetivo

Definir a fundação visual do WiiStudio, criando um Design System consistente,
reutilizável e extensível para orientar todas as telas, componentes, temas,
ícones, espaçamentos e padrões de interação da aplicação.

---

# Requisitos Funcionais

## RF001
Definir tokens visuais globais.

## RF002
Padronizar cores, tipografia, espaçamentos, bordas e elevações.

## RF003
Garantir compatibilidade com temas claro e escuro.

## RF004
Disponibilizar padrões reutilizáveis para todos os componentes.

## RF005
Documentar regras de uso para o Codex.

## RF006
Permitir evolução futura sem quebrar telas existentes.

---

# Escopo

O Design System deve cobrir:

- Cores
- Tipografia
- Espaçamentos
- Bordas
- Elevação
- Ícones
- Estados visuais
- Feedback
- Componentes base
- Acessibilidade
- Responsividade

---

# Estrutura Sugerida

```text
styles/
├── tokens/
│   ├── Colors.axaml
│   ├── Typography.axaml
│   ├── Spacing.axaml
│   ├── Radius.axaml
│   └── Elevation.axaml
│
├── components/
│   ├── Buttons.axaml
│   ├── Cards.axaml
│   ├── Inputs.axaml
│   ├── Dialogs.axaml
│   └── DataGrid.axaml
│
└── themes/
    ├── Dark.axaml
    └── Light.axaml
```

---

# Tokens

## Cores

Categorias obrigatórias:

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

## Tipografia

Categorias:

- Display
- Title
- Subtitle
- Body
- Caption
- Code

## Espaçamentos

Escala recomendada:

```text
4, 8, 12, 16, 24, 32, 48, 64
```

## Bordas

- Small
- Medium
- Large
- Pill

---

# Estados Visuais

Todo componente deve suportar:

- Default
- Hover
- Focus
- Pressed
- Disabled
- Loading
- Error
- Success

---

# Acessibilidade

Obrigatório:

- Contraste mínimo WCAG AA.
- Navegação por teclado.
- Foco visível.
- Tamanho mínimo de alvo clicável.
- Compatibilidade com escala de fonte.

---

# Regras para o Codex

- Nunca usar cores hardcoded.
- Nunca criar estilos isolados sem verificar o Design System.
- Todo componente novo deve consumir tokens existentes.
- Toda tela deve funcionar em tema claro e escuro.
- Todo estado interativo deve possuir estilo visual.

---

# Critérios de Aceite

- [ ] Tokens globais definidos.
- [ ] Temas claro e escuro suportados.
- [ ] Componentes base consomem tokens.
- [ ] Acessibilidade considerada.
- [ ] Documentação de uso criada.
- [ ] Sem estilos duplicados.

---

# Testes

## Unitários

- Carregamento de recursos.
- Alternância de tema.
- Validação de tokens obrigatórios.

## Integração

- Aplicação do tema na janela principal.
- Componentes base com tokens.
- Troca dinâmica entre Light/Dark.

---

# Dependências

- F031-MainWindow.md
- F039-ThemeEngine.md
- 008-UI-UX.md
- 021-ThemeSystem.md

---

# Definition of Done

- Tokens criados.
- Temas integrados.
- Componentes base conectados.
- Testes aprovados.
- Documentação atualizada.

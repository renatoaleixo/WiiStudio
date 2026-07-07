# CONTRIBUTING

# Objetivo

Definir regras para contribuição no projeto WiiStudio, incluindo padrões de código,
arquitetura, testes, segurança, documentação, pull requests, commits e revisão.

Este documento deve ser seguido por desenvolvedores humanos e agentes de IA.

---

# Antes de Contribuir

Leia:

1. `README.md`
2. `AI_CONTEXT.md`
3. `DOCUMENT_INDEX.md`
4. `001-Architecture.md`
5. `003-CodingStandards.md`
6. `F090-SecurityChecklist.md`
7. `F100-QualityGates.md`

---

# Regras de Arquitetura

Obrigatório:

- respeitar Clean Architecture;
- Domain sem dependência externa;
- UI sem regra de negócio;
- Infrastructure implementa interfaces;
- Application orquestra casos de uso;
- usar Dependency Injection;
- evitar Service Locator.

---

# Regras de UI

Obrigatório:

- MVVM;
- CommunityToolkit.Mvvm;
- textos localizáveis;
- tokens do Design System;
- Light/Dark;
- estados Loading/Empty/Error;
- acessibilidade por teclado;
- sem lógica de negócio no code-behind.

---

# Regras de Segurança

Obrigatório:

- validar entrada externa;
- normalizar paths;
- bloquear path traversal;
- usar ProcessRunner;
- redigir logs;
- confirmar ações destrutivas;
- validar plugins;
- verificar permissões;
- aplicar Security Checklist.

---

# Processos Externos

Proibido:

```csharp
Process.Start(...)
```

Use:

```text
IProcessRunner
```

Exceções precisam ser documentadas e aprovadas.

---

# Plugins

Toda alteração em plugins deve garantir:

- manifesto validado;
- permissões explícitas;
- contexto isolado;
- falha segura;
- logs com PluginId;
- unload limpa recursos;
- diagnóstico disponível.

---

# Testes

Toda contribuição deve incluir testes adequados.

## Obrigatório

- teste de sucesso;
- teste de erro;
- teste de cancelamento quando async;
- teste de permissão quando sensível;
- teste de regressão para bug corrigido.

## Ferramentas

- xUnit;
- FluentAssertions;
- Moq ou NSubstitute;
- fakes manuais;
- banco temporário;
- diretórios temporários.

---

# Commits

Use mensagens claras.

Formato recomendado:

```text
feat: add library scan use case
fix: prevent path traversal in plugin manifest
test: add regression for conversion cancel
docs: update plugin loading rules
```

Tipos recomendados:

- feat
- fix
- test
- docs
- refactor
- chore
- perf
- security

---

# Pull Requests

Um PR deve conter:

- resumo;
- motivação;
- documentos relacionados;
- screenshots quando UI;
- testes executados;
- riscos;
- checklist de segurança quando aplicável.

---

# Template de PR

```text
## Resumo

## Documentos relacionados

## Alterações

## Testes

## Segurança

## Screenshots

## Checklist
- [ ] Build passa
- [ ] Testes passam
- [ ] Documentação atualizada
- [ ] Security Checklist aplicado
- [ ] Quality Gates atendidos
```

---

# Revisão de Código

Revisar:

- arquitetura;
- segurança;
- testes;
- logs;
- tratamento de erro;
- performance;
- UI;
- documentação.

Bloquear se:

- `Process.Start` direto;
- path traversal;
- secret em log;
- plugin sem permissão;
- UI com regra de negócio;
- teste crítico ausente;
- documentação desatualizada.

---

# Documentação

Atualizar documentação quando:

- nova feature;
- mudança de arquitetura;
- mudança de regra de segurança;
- novo provider;
- novo plugin contract;
- nova migration;
- mudança de fluxo de UI;
- bug crítico corrigido.

---

# Qualidade

Aplicar:

- `F091-TestingStrategy.md`
- `F092-UnitTestingStandards.md`
- `F093-IntegrationTestingStandards.md`
- `F100-QualityGates.md`

---

# Estilo de Código

Seguir:

- nomes claros;
- métodos pequenos;
- async/await;
- CancellationToken;
- nullability;
- interfaces para infraestrutura;
- logs estruturados;
- exceptions controladas;
- validações explícitas.

---

# O que Evitar

- lógica na View;
- acoplamento direto;
- strings fixas na UI;
- paths hardcoded;
- logs com secrets;
- testes frágeis;
- sleeps em testes;
- plugin com acesso irrestrito;
- shell arbitrário;
- operação destrutiva sem confirmação.

---

# Checklist Final do Contribuidor

Antes de abrir PR:

- [ ] Li o documento da feature.
- [ ] Respeitei a arquitetura.
- [ ] Criei testes.
- [ ] Testei erro e sucesso.
- [ ] Validei segurança.
- [ ] Atualizei documentação.
- [ ] Não usei Process.Start direto.
- [ ] Não deixei secrets em logs.
- [ ] UI tem estados adequados.
- [ ] Quality Gates passam.

---

# Para Agentes de IA

Ao implementar:

1. leia o contexto;
2. cite documentos usados no plano interno;
3. modifique o mínimo necessário;
4. crie testes;
5. não invente APIs inexistentes sem verificar projeto;
6. mantenha arquitetura;
7. atualize documentação;
8. nunca ignore segurança.

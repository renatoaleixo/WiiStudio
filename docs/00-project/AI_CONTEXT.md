# AI_CONTEXT

# Objetivo

Este documento fornece contexto consolidado para agentes de IA, como Codex, entenderem o projeto WiiStudio, suas decisões arquiteturais, regras obrigatórias, stack, limites técnicos e prioridades de desenvolvimento.

O conteúdo deste arquivo deve ser lido antes de qualquer implementação relevante.

---

# Projeto

Nome: WiiStudio

Objetivo: aplicação desktop cross-platform para gerenciamento de biblioteca de jogos Wii, dispositivos, importação, conversão, metadados, assets, diagnósticos, plugins e operações relacionadas.

---

# Stack Principal

- .NET 10
- C# 13
- Avalonia UI
- MVVM
- CommunityToolkit.Mvvm
- Clean Architecture
- Dependency Injection
- SQLite
- Serilog
- xUnit
- FluentAssertions
- Moq ou NSubstitute
- Wiimms ISO Tools
- NKit Legacy como ferramenta externa
- Plugin SDK

---

# Arquitetura

A arquitetura segue:

```text
UI
 ↓
Application
 ↓
Domain
 ↓
Infrastructure
```

Regras:

- Domain não depende de UI, Infrastructure ou frameworks externos.
- Application define casos de uso e interfaces.
- Infrastructure implementa integrações externas.
- UI usa MVVM e não contém regra de negócio.
- Serviços são registrados via DI.
- Não usar Service Locator.
- Não acoplar View a banco, filesystem, processo externo ou provider.

---

# Regras Obrigatórias

## Processos Externos

- Nunca usar `Process.Start` diretamente.
- Sempre usar `IProcessRunner`.
- Sempre passar argumentos estruturados.
- Sempre aplicar timeout.
- Sempre aceitar CancellationToken.
- Sempre capturar stdout/stderr.
- Sempre aplicar log redaction.

## Wiimms

- Wiimms é a engine oficial para operações WBFS.
- Não implementar WBFS manualmente.
- `wit` e `wwt` devem ser chamados via ProcessRunner.

## NKit

- NKit é integração externa legado.
- Não implementar algoritmo NKit internamente.
- Tratar NKit como ferramenta externa opcional.

## Plugins

- Plugins exigem manifesto.
- Plugins exigem permissões explícitas.
- Plugins não são confiáveis por padrão.
- Plugins devem falhar de forma isolada.
- Plugin não pode derrubar aplicação principal.

## Segurança

- Validar toda entrada externa.
- Normalizar paths.
- Bloquear path traversal.
- Redigir secrets em logs.
- Confirmar ações destrutivas.
- Nunca salvar segredo sem política.
- Nunca executar shell arbitrário.

## UI

- Usar Avalonia + MVVM.
- Não colocar regra de negócio no code-behind.
- Usar tokens do Design System.
- Suportar Light/Dark.
- Suportar localization.
- Suportar acessibilidade por teclado.
- Sempre tratar Loading, Empty e Error.

## Operações Longas

- Usar async/await.
- Usar CancellationToken.
- Integrar com Task Manager.
- Exibir progresso quando possível.
- Não bloquear UI thread.

---

# Principais Módulos

- App Shell
- Dashboard
- Game Library
- Device Manager
- Import Wizard
- Conversion Wizard
- Metadata System
- Asset Manager
- Task Manager
- Notification Center
- Logging Center
- Advanced Diagnostics
- Plugin SDK
- Security & Permissions
- Settings
- Backup & Restore
- Update Manager

---

# Diretórios Esperados

```text
src/
├── WiiStudio.Domain/
├── WiiStudio.Application/
├── WiiStudio.Infrastructure/
├── WiiStudio.UI/
├── WiiStudio.Plugin.Abstractions/
├── WiiStudio.Plugin.Hosting/
├── WiiStudio.Plugin.Security/
└── WiiStudio.Plugin.Diagnostics/

tests/
├── WiiStudio.Domain.Tests/
├── WiiStudio.Application.Tests/
├── WiiStudio.Infrastructure.Tests/
├── WiiStudio.UI.Tests/
├── WiiStudio.Plugin.Tests/
├── WiiStudio.Integration.Tests/
└── WiiStudio.Performance.Tests/
```

---

# Padrões de Teste

- xUnit
- FluentAssertions
- Arrange/Act/Assert
- fakes para ProcessRunner
- diretórios temporários para filesystem
- banco SQLite temporário
- sem ferramenta externa real em teste unitário
- regressão para bug corrigido
- testes de segurança para feature sensível

---

# Prioridades de Implementação

1. Arquitetura base.
2. App Shell.
3. Design System.
4. Biblioteca de jogos.
5. Device Manager.
6. Wiimms Integration.
7. Import/Conversion.
8. Metadata/Assets.
9. Task Manager e Diagnostics.
10. Plugin SDK.
11. Security.
12. Testes e Quality Gates.

---

# Instrução para o Codex

Antes de implementar qualquer feature:

1. Ler `AI_CONTEXT.md`.
2. Ler `DOCUMENT_INDEX.md`.
3. Ler o documento específico da feature.
4. Verificar dependências.
5. Implementar seguindo Clean Architecture.
6. Criar testes.
7. Aplicar Security Checklist quando aplicável.
8. Atualizar documentação se necessário.

---

# Proibições Importantes

- Não implementar WBFS manualmente.
- Não implementar NKit internamente.
- Não usar Process.Start diretamente.
- Não acessar banco pela UI.
- Não colocar regra de negócio em View.
- Não ignorar CancellationToken.
- Não logar secrets.
- Não permitir path traversal.
- Não carregar plugin sem manifesto validado.
- Não conceder permissão implícita.
- Não quebrar Light/Dark.
- Não deixar tela em branco sem estado.

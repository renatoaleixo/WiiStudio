# DEVELOPMENT_GUIDE

# Objetivo

Guia prático de desenvolvimento do WiiStudio para humanos e agentes de IA.

Este documento descreve como implementar features, organizar código, criar testes,
usar ProcessRunner, aplicar segurança, manter documentação e respeitar a arquitetura
do projeto.

---

# Fluxo Padrão de Desenvolvimento

Para qualquer feature:

1. Ler `AI_CONTEXT.md`.
2. Localizar documento em `DOCUMENT_INDEX.md`.
3. Ler dependências da feature.
4. Criar/alterar modelos no Domain se necessário.
5. Criar use case no Application.
6. Criar interfaces necessárias.
7. Implementar infraestrutura.
8. Criar ViewModel.
9. Criar View Avalonia.
10. Criar testes.
11. Aplicar checklist de segurança.
12. Atualizar documentação.
13. Validar Quality Gates.

---

# Clean Architecture

Respeitar:

```text
UI -> Application -> Domain
Infrastructure -> Application/Domain
```

Não fazer:

- UI acessando SQLite.
- UI executando processo externo.
- Domain dependendo de Avalonia.
- Application dependendo de implementação concreta.
- Plugin acessando serviço interno sem contrato.

---

# Criando uma Feature

## 1. Domain

Use quando houver:

- entidade;
- regra pura;
- value object;
- enum;
- invariantes.

Exemplo:

```text
Game
GameId
GameFormat
GameRegion
```

## 2. Application

Use para:

- caso de uso;
- orquestração;
- validação;
- contratos;
- DTOs.

Exemplo:

```text
ImportGameUseCase
IGameRepository
IProcessRunner
```

## 3. Infrastructure

Use para:

- SQLite;
- filesystem;
- Wiimms;
- NKit;
- providers;
- cache;
- settings.

## 4. UI

Use para:

- ViewModel;
- Commands;
- estados;
- binding;
- validação visual;
- dialogs;
- navigation.

---

# MVVM

Regras:

- ViewModel expõe estado.
- View faz binding.
- Commands executam ações.
- Sem regra de negócio no code-behind.
- Usar CommunityToolkit.Mvvm.
- Usar ObservableProperty quando apropriado.
- Usar RelayCommand/AsyncRelayCommand.

---

# Estados de UI

Toda tela com dados deve tratar:

- Loading;
- Empty;
- Ready;
- Error;
- NoResults quando aplicável.

Nunca deixar área em branco sem explicação.

---

# Textos e Localization

Não usar texto fixo direto na UI.

Usar chaves:

```text
Library.Title
Import.SelectFile
Error.FileNotFound
```

Documentos:

- `022-Localization.md`

---

# Design System

Usar tokens para:

- cor;
- espaçamento;
- tipografia;
- borda;
- raio;
- sombra;
- layout.

Não usar valores hardcoded sem justificativa.

Documentos:

- `F041-DesignSystemFoundation.md`
- `F042-ColorSystem.md`
- `F043-Typography.md`
- `F045-SpacingAndLayout.md`

---

# ProcessRunner

Toda ferramenta externa deve usar `IProcessRunner`.

Exemplo conceitual:

```csharp
var result = await processRunner.RunAsync(
    new ProcessRequest
    {
        ExecutablePath = witPath,
        Arguments = ["list", devicePath],
        Timeout = TimeSpan.FromSeconds(30)
    },
    cancellationToken);
```

Proibido:

```csharp
Process.Start(...)
```

Documentos:

- `F083-ExternalProcessSecurity.md`
- `F095-ProcessRunnerTesting.md`

---

# Paths

Antes de usar path:

- normalizar;
- validar escopo;
- validar extensão;
- validar permissão;
- bloquear traversal.

Documento:

- `F086-SafePathHandling.md`

---

# Plugins

Ao mexer com plugins:

- validar manifesto;
- checar permissões;
- isolar contexto;
- capturar exceções;
- registrar PluginId;
- remover capabilities no unload.

Documentos:

- `F071-PluginSDKFoundation.md`
- `F072-PluginManifest.md`
- `F075-PluginPermissions.md`
- `F084-PluginSecurity.md`

---

# Logging

Usar logs estruturados.

Não logar:

- tokens;
- senhas;
- Authorization headers;
- connection strings;
- secrets;
- paths sensíveis sem necessidade.

Aplicar redaction.

Documento:

- `F088-LogRedaction.md`

---

# Operações Longas

Para operações longas:

- async/await;
- CancellationToken;
- progresso;
- Task Manager;
- logs;
- falha segura.

Exemplos:

- importação;
- conversão;
- backup;
- restore;
- scan;
- download de assets;
- plugin discovery.

---

# Testes

Toda feature deve ter testes.

## Unitários

- regra;
- ViewModel;
- use case;
- validação.

## Integração

- banco;
- filesystem temporário;
- plugin discovery;
- repositories.

## Regressão

- bug corrigido.

Documentos:

- `F091-TestingStrategy.md`
- `F092-UnitTestingStandards.md`
- `F093-IntegrationTestingStandards.md`

---

# Segurança

Aplicar `F090-SecurityChecklist.md` em features que envolvem:

- arquivos;
- processos externos;
- plugins;
- updates;
- backups;
- settings sensíveis;
- logs;
- exclusões;
- permissões.

---

# Definition of Done

Antes de concluir:

- requisitos implementados;
- testes criados;
- estados de UI tratados;
- segurança aplicada;
- logs seguros;
- docs atualizadas;
- build passa;
- testes passam;
- Quality Gates atendidos.

---

# Erros Comuns a Evitar

- regra de negócio na View;
- Process.Start direto;
- path concatenado manualmente;
- token em log;
- plugin sem permissão;
- tela sem Error State;
- teste dependente de ambiente real;
- operação longa sem cancelamento;
- sobrescrita sem confirmação;
- strings fixas na UI.

---

# Checklist Rápido para Codex

Antes de responder que terminou:

- [ ] Seguiu documento da feature?
- [ ] Respeitou arquitetura?
- [ ] Criou testes?
- [ ] Tratou erros?
- [ ] Usou CancellationToken?
- [ ] Usou ProcessRunner quando necessário?
- [ ] Validou paths?
- [ ] Redigiu logs?
- [ ] Atualizou docs?
- [ ] Aplicou Quality Gates?

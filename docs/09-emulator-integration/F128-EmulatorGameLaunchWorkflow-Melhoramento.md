# F128 - Emulator Game Launch Workflow [Melhoramento]

> **Suggested path:** `docs/09-emulator-integration/F128-EmulatorGameLaunchWorkflow-Melhoramento.md`  
> **Priority:** Future / Product Expansion  
> **Status:** Proposed technical design  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain  
> **Area:** Emulator Launch, Game Library, UX, Safety  
> **Related:** F115, F127, F109, F113, F114, F118

---

## 1. Objetivo da Feature

Criar o fluxo completo para executar jogos da biblioteca do WiiStudio em um emulador externo configurado, inicialmente Dolphin.

A feature detalha o processo de:

- validar o jogo;
- validar o emulador;
- criar preview de execução;
- iniciar o processo;
- registrar histórico;
- tratar erros;
- oferecer feedback claro ao usuário.

---

## 2. Problema que Resolve

Apenas integrar o Dolphin como ferramenta externa não garante um fluxo de uso profissional.

O usuário precisa de um fluxo confiável para:

- saber se o jogo selecionado pode ser executado;
- saber qual arquivo será aberto;
- entender se o emulador está configurado;
- evitar abrir arquivo errado;
- registrar histórico;
- diagnosticar falhas.

---

## 3. Escopo Funcional

Incluído:

- botão **Jogar no PC**;
- preview antes da execução;
- validação do arquivo;
- validação do formato;
- validação do emulador;
- execução via `IProcessRunner`;
- captura de exit code;
- histórico de execução;
- task/notificação;
- suporte Windows/Linux/macOS;
- mensagens claras de erro.

---

## 4. Fora de Escopo

- Emulação nativa dentro do WiiStudio.
- Configuração avançada de controles.
- Save states.
- Netplay.
- Cheats.
- Download de jogos.
- Conversão automática destrutiva de arquivos.
- Modificação interna do Dolphin.

---

## 5. Fluxo de Usuário Esperado

```text
Usuário seleciona jogo
    ↓
Clica em Jogar no PC
    ↓
WiiStudio valida arquivo e formato
    ↓
WiiStudio valida emulador
    ↓
Mostra preview:
        jogo
        arquivo
        emulador
        perfil
    ↓
Usuário confirma ou inicia diretamente, conforme configuração
    ↓
Dolphin é iniciado
    ↓
Histórico é registrado
```

---

## 6. Regras de Negócio

- O botão **Jogar no PC** aparece apenas para jogos com arquivo local.
- Jogos localizados apenas em USB/WBFS raw podem exigir exportação antes.
- O fluxo deve suportar formatos conhecidos e configuráveis.
- O usuário deve poder definir se deseja preview sempre ou execução direta.
- Falha ao iniciar emulador gera notificação.
- Histórico deve registrar tentativa, mesmo em falha.
- Execução não deve bloquear UI.
- Se já houver emulador rodando, o WiiStudio pode iniciar nova instância ou avisar, conforme configuração.

---

## 7. Regras de Segurança

- Não armazenar senha.
- Não executar shell.
- Não aceitar argumentos arbitrários.
- Validar path do jogo.
- Validar path do emulador.
- Bloquear path traversal.
- Não executar arquivo fora da biblioteca sem confirmação.
- Não permitir plugin alterar comando de execução diretamente.
- Logs redigidos.

---

## 8. Integrações Necessárias

- **F127** para provider do Dolphin.
- **F115** para botão no painel de detalhes.
- **F113** para tarefas e notificações.
- **F114** para persistir histórico.
- **F118** para diagnóstico e reparos.
- **F103/F111** para metadados exibidos no preview.

---

## 9. Impactos na UI

### Painel do Jogo

```text
[Jogar no PC]
[Configurar emulador]
[Ver diagnóstico]
```

### Preview

```text
Jogo: Mario Kart Wii
Game ID: RMCP01
Arquivo: /games/RMCP01.wbfs
Emulador: Dolphin
Versão: detectada
Perfil: Padrão
```

### Settings

```text
Emulator Launch
[x] Mostrar preview antes de iniciar
[x] Registrar histórico
[ ] Fechar WiiStudio após iniciar jogo
```

---

## 10. Serviços, Classes e Interfaces Sugeridas

```csharp
public interface IEmulatorGameLaunchWorkflowService
{
    Task<EmulatorLaunchPreviewDto> CreatePreviewAsync(
        EmulatorLaunchRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EmulatorLaunchResultDto> ExecuteAsync(
        EmulatorLaunchExecutionRequestDto request,
        CancellationToken cancellationToken = default);
}
```

```csharp
public sealed class EmulatorLaunchRequestDto
{
    public string GameId { get; init; } = string.Empty;
    public string EmulatorId { get; init; } = "dolphin";
}
```

```csharp
public sealed class EmulatorLaunchPreviewDto
{
    public bool CanLaunch { get; init; }
    public string? BlockReasonKey { get; init; }
    public string GameId { get; init; } = string.Empty;
    public string? GameTitle { get; init; }
    public string? GameFilePath { get; init; }
    public string EmulatorId { get; init; } = string.Empty;
    public string? EmulatorVersion { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

```csharp
public sealed class EmulatorLaunchResultDto
{
    public bool Started { get; init; }
    public string? ProcessId { get; init; }
    public string? ErrorMessageKey { get; init; }
    public DateTimeOffset StartedAt { get; init; }
}
```

---

## 11. Persistência e Configurações

```text
EmulatorLaunch.ShowPreviewBeforeLaunch = true
EmulatorLaunch.SaveHistory = true
EmulatorLaunch.LastEmulatorId
EmulatorLaunch.LastGameId
```

Tabela futura SQLite:

```text
EmulatorLaunchHistory
```

Campos:

- Id;
- GameId;
- EmulatorId;
- FilePath;
- StartedAt;
- ExitCode;
- Result;
- ErrorMessageKey.

---

## 12. Logs, Tarefas e Notificações Esperadas

Tarefa curta:

```text
Iniciando jogo no emulador
```

Notificações:

```text
Jogo iniciado no Dolphin.
Não foi possível iniciar o jogo no Dolphin.
Configure o Dolphin antes de jogar no PC.
```

Logs:

- GameId;
- EmulatorId;
- command sanitized;
- start time;
- failure reason;
- correlationId.

---

## 13. Critérios de Aceite

- [ ] Botão **Jogar no PC** aparece para jogos elegíveis.
- [ ] Preview mostra jogo, arquivo e emulador.
- [ ] Arquivo inexistente bloqueia execução.
- [ ] Emulador ausente abre configuração.
- [ ] Processo é iniciado via `IProcessRunner`.
- [ ] Argumentos são validados.
- [ ] Histórico é registrado.
- [ ] Notificação é gerada.
- [ ] Testes usam fakes.

---

## 14. Cenários de Teste

- jogo local válido;
- jogo sem arquivo;
- Dolphin não configurado;
- Dolphin inválido;
- formato não suportado;
- path com espaço;
- path inseguro;
- usuário cancela preview;
- processo inicia;
- processo falha;
- histórico salvo.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Sem arquivo local | `Este jogo não possui arquivo local disponível para execução.` |
| Emulador ausente | `Configure o Dolphin antes de jogar no PC.` |
| Emulador inválido | `O emulador configurado não pôde ser validado.` |
| Formato bloqueado | `O formato deste jogo ainda não está habilitado para execução no emulador.` |
| Falha ao iniciar | `Não foi possível iniciar o jogo no emulador.` |
| Caminho inseguro | `O caminho do arquivo foi bloqueado por segurança.` |

# F127 - Dolphin Emulator Integration [Melhoramento]

> **Suggested path:** `docs/09-emulator-integration/F127-DolphinEmulatorIntegration-Melhoramento.md`  
> **Priority:** Future / Product Expansion  
> **Status:** Proposed technical design  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain  
> **Area:** Emulator Integration, External Tools, Game Library, Desktop UX  
> **Related:** F101, F108, F109, F115, F118, F124, F126

---

## 1. Objetivo da Feature

Integrar o **Dolphin Emulator** ao WiiStudio como uma ferramenta externa opcional, permitindo que o usuário execute jogos Wii/GameCube no PC a partir da biblioteca do WiiStudio, sem transformar o WiiStudio em um emulador nativo.

A proposta é:

```text
WiiStudio = gerenciador profissional da biblioteca, USB, WBFS, capas, metadados e ferramentas
Dolphin = runtime opcional para jogar/testar no PC
```

Ações esperadas:

- Jogar no PC;
- Abrir no Dolphin;
- Configurar emulador;
- Validar emulador;
- Ver diagnóstico do emulador.

---

## 2. Problema que Resolve

Hoje o WiiStudio foca em biblioteca, capas, metadados, Wiimms, WBFS e USB. Porém, o usuário também pode querer testar ou jogar no PC sem localizar manualmente o arquivo do jogo e sem abrir o Dolphin separadamente.

Sem integração, o fluxo fica manual:

```text
Abrir WiiStudio
    ↓
Encontrar jogo
    ↓
Abrir pasta
    ↓
Abrir Dolphin
    ↓
Selecionar arquivo
```

Com a feature:

```text
Selecionar jogo
    ↓
Jogar no PC
    ↓
Dolphin abre com o jogo
```

---

## 3. Decisão de Arquitetura

### Decisão Recomendada

```text
Não embutir o core do Dolphin no WiiStudio.
Integrar Dolphin como ferramenta externa opcional.
```

Motivos:

- reduz acoplamento técnico;
- evita misturar responsabilidades;
- simplifica manutenção;
- reduz risco de licença;
- mantém WiiStudio como gerenciador;
- permite o usuário usar sua instalação existente do Dolphin;
- preserva a arquitetura atual de External Tools.

---

## 4. Escopo Funcional

Incluído:

- detectar instalação local do Dolphin;
- permitir configurar path manualmente;
- validar versão;
- exibir status no diagnóstico;
- adicionar botão **Jogar no PC**;
- iniciar Dolphin com jogo selecionado;
- suportar Windows, Linux e macOS;
- tratar formatos suportados;
- registrar logs/tarefas/notificações;
- não fornecer jogos/ROMs/ISOs;
- não embutir Dolphin no pacote inicial.

---

## 5. Fora de Escopo

- Embutir o Dolphin como biblioteca interna.
- Modificar código-fonte do Dolphin.
- Distribuir jogos, ROMs, ISOs, BIOS ou chaves.
- Baixar jogos.
- Burlar DRM ou proteção.
- Implementar emulação própria.
- Garantir compatibilidade perfeita de todos os jogos.
- Gerenciar cheats no primeiro ciclo.
- Gerenciar multiplayer/netplay no primeiro ciclo.

---

## 6. Fluxo de Usuário Esperado

### Primeiro Uso

```text
Usuário clica "Jogar no PC"
    ↓
WiiStudio verifica Dolphin
    ↓
Se não configurado: abre configuração do emulador
    ↓
Usuário seleciona executável Dolphin
    ↓
WiiStudio valida versão
    ↓
Usuário confirma
    ↓
Jogo é iniciado no Dolphin
```

### Uso Normal

```text
Selecionar jogo
    ↓
Clicar "Jogar no PC"
    ↓
Validar arquivo
    ↓
Executar Dolphin como processo externo
    ↓
Registrar histórico
```

---

## 7. Regras de Negócio

- Dolphin é opcional.
- A ausência do Dolphin não deve quebrar o WiiStudio.
- O usuário pode configurar o caminho manualmente.
- O WiiStudio deve validar se o executável parece ser Dolphin.
- Apenas jogos presentes na biblioteca local podem ser iniciados.
- O botão **Jogar no PC** deve ficar indisponível quando não houver arquivo local compatível.
- Se o jogo estiver apenas em dispositivo WBFS, o WiiStudio deve orientar exportar/copiar para arquivo local antes de abrir no emulador, salvo suporte direto validado.
- Histórico de execução deve ser registrado.
- O usuário deve poder trocar/remover o caminho do Dolphin.

---

## 8. Regras de Segurança e Licença

- Não armazenar senha.
- Não executar comando arbitrário.
- Não aceitar argumentos livres do usuário sem validação.
- Usar `IProcessRunner`.
- Usar catálogo de argumentos permitidos.
- Validar path do executável.
- Não baixar Dolphin sem fonte oficial/checksum.
- Não distribuir Dolphin junto sem análise de licença e obrigações.
- Exibir aviso de que o usuário deve usar apenas jogos próprios/legalmente obtidos.
- Logs não devem expor caminhos sensíveis completos quando não necessário.
- Plugins não podem iniciar emulador com argumentos arbitrários.

---

## 9. Integrações Necessárias

- **F101**: registrar Dolphin como External Tool opcional.
- **F108**: download futuro do Dolphin com fonte oficial/checksum, se aprovado.
- **F113**: tarefas/notificações para validação e execução.
- **F115**: botão no painel avançado.
- **F118**: diagnóstico com ação **Configurar Dolphin**.
- **F124/F126**: empacotamento não deve incluir Dolphin por padrão no primeiro ciclo.
- **F114**: persistir histórico e configuração quando SQLite estiver ativo.

---

## 10. Impactos na UI

### Painel de Detalhes do Jogo

```text
[Jogar no PC]
[Abrir no Dolphin]
[Configurar emulador]
```

### Configurações

```text
Emuladores
└── Dolphin
    ├── Caminho do executável
    ├── Versão detectada
    ├── Status
    ├── Argumentos padrão
    └── Testar
```

### Diagnóstico

```text
Dolphin Emulator
Status: Configurado / Não configurado / Inválido
Path: ...
Version: ...
Last launch: ...
Last error: ...
```

---

## 11. Serviços, Classes e Interfaces Sugeridas

### Application

```csharp
public interface IEmulatorProvider
{
    string EmulatorId { get; }
    Task<EmulatorStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<EmulatorLaunchResultDto> LaunchAsync(
        EmulatorLaunchRequestDto request,
        CancellationToken cancellationToken = default);
}
```

```csharp
public interface IEmulatorLocator
{
    Task<EmulatorLocationResultDto> LocateAsync(
        EmulatorLocatorRequestDto request,
        CancellationToken cancellationToken = default);
}
```

```csharp
public interface IEmulatorLaunchService
{
    Task<EmulatorLaunchPreviewDto> CreatePreviewAsync(
        EmulatorLaunchRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EmulatorLaunchResultDto> LaunchAsync(
        EmulatorLaunchExecutionRequestDto request,
        CancellationToken cancellationToken = default);
}
```

### Infrastructure

- `DolphinEmulatorProvider`
- `DolphinEmulatorLocator`
- `DolphinVersionDetector`
- `DolphinCommandBuilder`
- `DolphinProcessLauncher`

### UI

- `EmulatorSettingsView`
- `EmulatorSettingsViewModel`
- `LaunchInEmulatorCommand`
- `EmulatorDiagnosticsViewModel`

---

## 12. Catálogo de Comando Inicial

O comando deve ser montado por catálogo.

Exemplo conceitual:

```text
dolphin-emu --exec=<GAME_FILE>
```

ou caminho equivalente por plataforma:

```text
Dolphin.exe --exec=<GAME_FILE>
Dolphin.app/Contents/MacOS/Dolphin --exec=<GAME_FILE>
dolphin-emu --exec=<GAME_FILE>
```

Regras:

- `<GAME_FILE>` deve ser path validado de arquivo da biblioteca.
- Não permitir argumento livre digitado pelo usuário no primeiro ciclo.
- Não permitir shell.
- Não permitir `&&`, `|`, `;`, redirect ou expansão.
- Permitir configuração avançada no futuro, com allowlist.

---

## 13. Persistência e Configurações

Configurações:

```text
Emulators.Dolphin.Enabled
Emulators.Dolphin.ExecutablePath
Emulators.Dolphin.LastDetectedVersion
Emulators.Dolphin.UseFullscreen
Emulators.Dolphin.LastLaunchAt
Emulators.Dolphin.ArgumentsProfileId
```

Histórico futuro:

```text
EmulatorLaunchHistory
```

Campos:

- GameId;
- EmulatorId;
- StartedAt;
- ExitCode;
- Duration;
- Result;
- ErrorMessageKey.

---

## 14. Logs, Tarefas e Notificações Esperadas

Tarefas:

- Validando Dolphin;
- Iniciando jogo no Dolphin;
- Verificando arquivo do jogo.

Notificações:

```text
Dolphin iniciado.
Não foi possível iniciar o jogo no Dolphin.
Dolphin não está configurado.
O arquivo do jogo não foi encontrado.
```

Logs:

- EmulatorId;
- GameId;
- Version;
- sanitized executable path;
- sanitized game path;
- exit code;
- correlationId.

---

## 15. Critérios de Aceite

- [ ] Dolphin pode ser configurado manualmente.
- [ ] WiiStudio valida se o executável é Dolphin.
- [ ] Painel do jogo exibe **Jogar no PC** quando aplicável.
- [ ] Jogo local pode ser aberto no Dolphin.
- [ ] Ausência do Dolphin não quebra o WiiStudio.
- [ ] Diagnóstico mostra status do Dolphin.
- [ ] Execução usa `IProcessRunner`.
- [ ] Argumentos são catalogados/validados.
- [ ] Nenhuma senha é armazenada.
- [ ] Nenhum jogo é baixado ou fornecido pelo WiiStudio.
- [ ] Testes usam fakes.

---

## 16. Cenários de Teste

- Dolphin não configurado.
- Dolphin configurado e válido.
- Executável inválido.
- Arquivo do jogo ausente.
- Jogo com formato suportado.
- Jogo com formato não suportado.
- Processo Dolphin inicia com sucesso.
- Processo Dolphin falha.
- Path com espaço.
- Tentativa de argumento inseguro bloqueada.
- Windows/Linux/macOS com locator fake.

---

## 17. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Dolphin ausente | `Dolphin Emulator não está configurado.` |
| Executável inválido | `O executável selecionado não parece ser uma instalação válida do Dolphin.` |
| Arquivo ausente | `O arquivo do jogo não foi encontrado no caminho original.` |
| Formato não suportado | `Este formato ainda não está habilitado para execução no emulador.` |
| Falha ao iniciar | `Não foi possível iniciar o Dolphin. Veja o diagnóstico para detalhes.` |
| Argumento bloqueado | `A execução foi bloqueada porque contém argumentos não permitidos.` |

---

## 18. Referências Técnicas

- Dolphin Emulator oficial/GitHub.
- Dolphin command line `--exec`.
- Licença GPLv2+ do Dolphin.
- Documentação e FAQ oficial do Dolphin.

# F110-1 - USB Format and WBFS Preparation

> Feature ID: F110-1  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: USB Device Operations / Safety  
> Area: Device Manager, Wiimms Integration, Privileged Access, Task Manager, Diagnostics  
> Depends on: F105, F106, F107, F109, F110, F113, F120

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 2.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: F105, F106, F107, F109, F110, F113, F120, F121.
- Reforço de orientação: FAT32 com pasta `/wbfs` deve ser o caminho recomendado; WBFS deve aparecer como opção avançada, destrutiva e explicitamente confirmada.
- Reforço de segurança: bloquear disco do sistema, exigir preview, exigir frase de confirmação para WBFS, validar permissões, usar apenas ferramentas validadas, registrar tarefa/notificação e gerar relatório.
- Fora de escopo reforçado: recuperação de dados, particionamento avançado, redimensionamento, formatação silenciosa, operação sem confirmação, formatação iniciada por plugin e promessa de rollback.
- Impacto de UI: tela "Preparar USB" com escolha de dispositivo, escolha de modo, resumo de risco, confirmação forte, progresso e readiness check final.

# Objetivo

Permitir que o WiiStudio prepare HDs e pendrives para uso no Wii, incluindo a opção de
formatar explicitamente uma partição/dispositivo em **WBFS**, quando o usuário escolher
isso de forma consciente, validada e confirmada.

A feature também deve orientar que **FAT32 com pasta `/wbfs`** pode ser uma alternativa
mais moderna, mais interoperável e menos arriscada que WBFS raw.

Esta é uma operação de alto risco porque pode apagar dados. O fluxo deve exigir
validação, preview, confirmação forte, permissão administrativa, progresso e diagnóstico.

---

# Regras de Segurança Essenciais

```text
Nunca formatar sem confirmação explícita.
Nunca formatar disco do sistema.
Nunca executar comando arbitrário.
Nunca permitir plugin iniciar formatação diretamente.
Nunca executar formatação real em testes automatizados.
```

---

# Problema

Para usar um HD/pendrive no Wii, alguns usuários podem querer preparar o dispositivo como:

- WBFS raw/partition;
- FAT32 com pasta `/wbfs`;
- NTFS com pasta `/wbfs`, quando permitido pelo fluxo de cópia/exportação.

Historicamente, WBFS foi usado, mas FAT32 com pasta `/wbfs` costuma ser uma alternativa
mais segura para manutenção, backup, acesso pelo sistema operacional e compatibilidade
com ferramentas modernas.

Mesmo assim, o WiiStudio pode oferecer WBFS desde que o usuário escolha explicitamente.

---

# Resultado Esperado

Fluxo principal:

```text
Detectar discos e partições
    ↓
Usuário escolhe dispositivo/partição
    ↓
WiiStudio valida se não é disco do sistema
    ↓
WiiStudio mostra alternativas: FAT32 / WBFS
    ↓
Usuário escolhe modo
    ↓
WiiStudio mostra preview detalhado
    ↓
WiiStudio mostra aviso de perda total de dados
    ↓
Usuário digita frase de confirmação
    ↓
WiiStudio valida permissões administrativas
    ↓
WiiStudio executa comando conhecido e seguro
    ↓
WiiStudio mostra progresso
    ↓
WiiStudio valida resultado
    ↓
WiiStudio executa diagnóstico pós-formatação
    ↓
WiiStudio registra tarefa/notificação
```

---

# Escopo

## Incluído

- detectar discos e partições usando `F107-CrossPlatformUsbDeviceProvider`;
- exibir claramente o dispositivo alvo;
- bloquear disco do sistema;
- bloquear dispositivo não removível por padrão;
- exigir confirmação explícita;
- exigir frase de confirmação;
- mostrar aviso de perda total de dados;
- gerar preview antes da execução;
- integrar com permissões administrativas via `F105`/`F106`;
- integrar com progresso via `F109`;
- integrar com fluxo seguro via `F110`;
- registrar tarefa e notificação via `F113`;
- formatar em WBFS quando explicitamente escolhido;
- preparar FAT32 com pasta `/wbfs` como alternativa;
- validar resultado após operação;
- rodar diagnóstico pós-formatação;
- testes com fakes, sem risco real em disco.

## Fora do Escopo

- recuperação de dados;
- particionamento avançado multi-partição;
- redimensionamento de partições;
- formatação silenciosa;
- execução sem confirmação;
- execução em disco do sistema;
- formatação iniciada por plugin;
- reparo automático de partição corrompida;
- operação real em discos durante testes automatizados.

---

# Alternativas Apresentadas ao Usuário

## Opção 1 - Preparar FAT32 com pasta /wbfs

Recomendação padrão.

```text
Mais moderno, menos arriscado e mais fácil de acessar pelo sistema operacional.
O WiiStudio criará a pasta /wbfs e usará Wiimms para copiar jogos no formato adequado.
```

Ação:

- não formatar se o volume já for FAT32 compatível;
- criar pasta `/wbfs`;
- validar escrita;
- validar espaço livre;
- registrar diagnóstico.

## Opção 2 - Formatar em WBFS

Opção avançada e destrutiva.

```text
Apaga todos os dados da partição/dispositivo selecionado e prepara o alvo como WBFS.
Use apenas se souber que precisa desse formato.
```

Ação:

- formatar usando ferramenta conhecida e validada, preferencialmente `wwt`;
- exigir frase de confirmação;
- validar resultado com Wiimms.

---

# Detecção de Discos e Partições

Usar:

```text
F107 - CrossPlatformUsbDeviceProvider
```

A feature deve receber:

- discos físicos;
- partições;
- mount points;
- filesystem;
- tamanho;
- espaço livre;
- status de USB/removível;
- status de permissão;
- indicação de disco do sistema;
- indicação de risco.

---

# Bloqueio de Disco do Sistema

A formatação deve ser bloqueada se o destino for:

- disco do sistema operacional;
- partição raiz;
- partição de boot;
- volume de usuário principal;
- disco interno não removível;
- destino desconhecido;
- partição onde o WiiStudio está rodando;
- partição com risco não classificável.

Estados de bloqueio:

```text
SystemDisk
BootPartition
InternalDisk
UnknownRisk
PermissionUnknown
ApplicationDisk
```

Mensagem:

```text
Este dispositivo parece ser usado pelo sistema operacional ou não pôde ser classificado com segurança.
Por segurança, o WiiStudio bloqueou a formatação.
```

---

# Preview Obrigatório

Antes da execução, mostrar:

```text
Operação: Formatar em WBFS
Dispositivo: SanDisk Ultra USB
Device ID: usb-SanDisk_Ultra_123456
Caminho físico: /dev/sdc
Partição: /dev/sdc1
Tamanho: 128 GB
Filesystem atual: FAT32
Espaço livre atual: 80 GB
Sistema detectado: Linux
Ferramenta: wwt
Permissão: Autorização administrativa necessária
Resultado esperado: partição WBFS vazia
Dados atuais: serão apagados
```

Para FAT32 `/wbfs`:

```text
Operação: Preparar FAT32 para Wii
Dispositivo: SanDisk Ultra USB
Partição: /media/user/USB
Filesystem atual: FAT32
Ação: criar/validar pasta /wbfs
Dados atuais: preservados
```

---

# Confirmação Forte

Para formatação WBFS, exigir digitar exatamente:

```text
FORMATAR WBFS
```

Regras:

- botão confirmar fica desabilitado até frase correta;
- confirmação expira se o alvo mudar;
- se usuário trocar dispositivo, deve digitar novamente;
- se o preview for recalculado, deve confirmar novamente;
- confirmação só vale para o device/partition ID exibido.

Mensagem:

```text
Esta ação apagará todos os dados do dispositivo/partição selecionado.
Para continuar, digite: FORMATAR WBFS
```

---

# Integração com Permissões Administrativas

Usar:

- `F105-CrossPlatformPrivilegedDeviceAccess`;
- `F106-PolkitPolicyInstaller`, no Linux.

Fluxo:

```text
Check target
    ↓
Check external tool
    ↓
Check permission
    ↓
If Linux policy missing: offer F106 flow
    ↓
If Windows admin required: guide elevated execution
    ↓
If macOS permission required: guide authorization
    ↓
Retry
```

A formatação não deve iniciar enquanto `DevicePermissionStatus` não permitir a operação.

---

# Integração com Wiimms

A ferramenta principal esperada para WBFS é:

```text
wwt
```

O comando exato deve ser definido pelo projetista/implementador após validação prática
com Wiimms e sistema operacional alvo.

Esta feature define a política de segurança:

```text
Somente comandos catalogados.
Somente wwt/wit validados pelo External Tool Manager.
Sem shell.
Sem argumentos livres.
Sem comando arbitrário.
```

## Catálogo de Operações

```csharp
public enum WiimmsFormatOperation
{
    FormatWbfsPartition,
    ValidateWbfs,
    ListWbfs,
    CreateWbfsFolder
}
```

## KnownWiimmsCommandDefinition

```csharp
public sealed class KnownWiimmsCommandDefinition
{
    public string OperationId { get; init; } = string.Empty;
    public string ToolId { get; init; } = "wwt";
    public IReadOnlyList<string> ArgumentTemplate { get; init; } = [];
    public bool IsDestructive { get; init; }
    public bool RequiresStrongConfirmation { get; init; }
}
```

## Regra Importante para Codex

```text
Não inventar comando destrutivo final do Wiimms sem catálogo explícito e validação do projetista.
```

---

# Preparar FAT32 com /wbfs

Fluxo:

```text
Selecionar partição FAT32
    ↓
Validar que não é disco do sistema
    ↓
Validar escrita
    ↓
Criar pasta /wbfs se não existir
    ↓
Validar espaço
    ↓
Registrar como destino Wii-ready
```

Regras:

- não apagar dados existentes;
- não formatar;
- não sobrescrever pasta existente;
- validar permissões de escrita;
- registrar diagnóstico;
- exibir que FAT32 é a alternativa recomendada;
- considerar split automático para jogos maiores que 4 GB no fluxo de escrita.

---

# Formatar em WBFS

Fluxo:

```text
Selecionar partição/dispositivo
    ↓
Validar alvo
    ↓
Bloquear se disco do sistema
    ↓
Mostrar preview
    ↓
Aviso de perda total
    ↓
Digitar FORMATAR WBFS
    ↓
Validar permissões
    ↓
Executar operação catalogada com wwt
    ↓
Mostrar progresso
    ↓
Validar resultado com wwt/wit
    ↓
Rodar diagnóstico
```

---

# Validação Pós-Formatação

Após formatar:

- verificar exit code;
- executar comando conhecido para validar WBFS;
- confirmar que o alvo é reconhecido como WBFS;
- listar jogos e esperar lista vazia ou estado válido;
- atualizar Device Manager;
- atualizar Readiness Check;
- registrar tarefa/notificação.

Resultado esperado:

```text
WBFS válido
0 jogos
Espaço disponível detectado
Dispositivo pronto para receber jogos
```

---

# Diagnóstico Pós-Formatação

Integrar com:

```text
F120 - End-to-End Wii USB Readiness Check
```

Executar checks:

- dispositivo detectado;
- permissão OK;
- Wiimms funcionando;
- filesystem WBFS/FAT32 OK;
- espaço livre OK;
- pronto para escrita.

---

# Integração com F110

Esta feature é uma extensão destrutiva do fluxo seguro `F110`.

Regras herdadas:

- preview obrigatório;
- validação de alvo;
- confirmação;
- Task Manager;
- progresso;
- cancelamento quando seguro;
- logs;
- diagnóstico;
- falha segura.

---

# Integração com F109

Durante formatação/validação:

- exibir progresso real se Wiimms emitir;
- exibir progresso indeterminado se não houver percentual;
- mostrar operação atual;
- permitir cancelamento somente em etapas seguras.

Observação:

```text
Nem toda etapa destrutiva pode ser cancelada com segurança.
A UI deve explicar quando cancelamento não estiver disponível.
```

---

# Integração com F113

Registrar tarefa:

```text
Preparando dispositivo USB
Formatando WBFS
Preparando FAT32 /wbfs
Validando resultado
```

Notificações:

- sucesso;
- falha;
- cancelamento;
- bloqueio por segurança;
- permissão pendente.

---

# Abstrações

## IUsbFormatPreparationService

```csharp
public interface IUsbFormatPreparationService
{
    Task<UsbFormatPreview> CreatePreviewAsync(
        UsbFormatPreparationRequest request,
        CancellationToken cancellationToken = default);

    Task<UsbFormatPreparationResult> ExecuteAsync(
        UsbFormatExecutionRequest request,
        IProgress<UsbFormatProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

## UsbFormatPreparationRequest

```csharp
public sealed class UsbFormatPreparationRequest
{
    public string TargetDeviceId { get; init; } = string.Empty;
    public string? TargetPartitionId { get; init; }
    public UsbPreparationMode Mode { get; init; }
}
```

## UsbPreparationMode

```csharp
public enum UsbPreparationMode
{
    PrepareFat32WbfsFolder,
    FormatAsWbfs
}
```

## UsbFormatPreview

```csharp
public sealed class UsbFormatPreview
{
    public string TargetDisplayName { get; init; } = string.Empty;
    public string? PhysicalPath { get; init; }
    public string? PartitionPath { get; init; }
    public string? CurrentFileSystem { get; init; }
    public long? SizeBytes { get; init; }
    public UsbPreparationMode Mode { get; init; }
    public bool IsDestructive { get; init; }
    public bool IsBlocked { get; init; }
    public string? BlockReasonKey { get; init; }
    public string RequiredConfirmationPhrase { get; init; } = string.Empty;
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

## UsbFormatExecutionRequest

```csharp
public sealed class UsbFormatExecutionRequest
{
    public UsbFormatPreview Preview { get; init; } = default!;
    public string ConfirmationPhrase { get; init; } = string.Empty;
    public bool UserConfirmed { get; init; }
}
```

## UsbFormatPreparationResult

```csharp
public sealed class UsbFormatPreparationResult
{
    public bool Success { get; init; }
    public UsbPreparationMode Mode { get; init; }
    public string TargetDeviceId { get; init; } = string.Empty;
    public string? ResultFileSystem { get; init; }
    public string? ErrorMessageKey { get; init; }
    public bool PostFormatDiagnosticsPassed { get; init; }
}
```

---

# Estados

```csharp
public enum UsbFormatWorkflowState
{
    SelectingTarget,
    ValidatingTarget,
    PreviewReady,
    Blocked,
    WaitingForConfirmation,
    WaitingForAuthorization,
    Running,
    ValidatingResult,
    Completed,
    Failed,
    Cancelled
}
```

---

# UI Sugerida

Wizard:

```text
1. Escolher dispositivo
2. Escolher modo
   - Preparar FAT32 com /wbfs, recomendado
   - Formatar em WBFS, avançado
3. Preview
4. Aviso e confirmação
5. Autorização
6. Execução
7. Validação
8. Resultado
```

---

# Mensagens de UX

## Recomendação FAT32

```text
Recomendado: usar FAT32 com pasta /wbfs.
Essa opção preserva seus dados e é mais fácil de acessar pelo computador.
```

## WBFS Avançado

```text
Avançado: formatar em WBFS.
Essa opção apagará todos os dados da partição/dispositivo selecionado.
Use apenas se você realmente precisa de WBFS.
```

## Bloqueio de Disco do Sistema

```text
Este disco parece ser usado pelo sistema operacional.
Por segurança, a formatação foi bloqueada.
```

## Confirmação WBFS

```text
Para confirmar a formatação em WBFS, digite: FORMATAR WBFS
```

---

# Segurança

Obrigatório:

- bloquear disco do sistema;
- bloquear destino desconhecido;
- bloquear filesystem/partição não identificada quando risco não puder ser avaliado;
- exigir preview;
- exigir confirmação forte;
- exigir permissões administrativas quando necessário;
- usar somente Wiimms validado;
- usar somente comandos catalogados;
- não usar shell;
- não aceitar argumentos livres;
- não permitir plugins iniciarem formatação;
- registrar logs redigidos;
- usar timeout/cancelamento;
- nunca rodar em disco real nos testes.

---

# Tratamento de Erros

## Disco do Sistema

Bloquear antes do preview final.

## Permissão Ausente

Mostrar ação de autorização.

## Wiimms Ausente

Abrir External Tool Manager.

## Policy Ausente no Linux

Abrir fluxo F106.

## Usuário Não Confirmou

Não executar.

## Frase Incorreta

Manter botão desabilitado.

## Falha no Comando

Mostrar diagnóstico e não marcar como pronto.

## Validação Pós-Formatação Falhou

Mostrar erro e sugerir diagnóstico.

---

# Testes Esperados

## Unitários

- preview FAT32;
- preview WBFS;
- bloqueia disco do sistema;
- bloqueia destino desconhecido;
- exige frase `FORMATAR WBFS`;
- frase incorreta bloqueia;
- usuário não confirmou bloqueia;
- Wiimms ausente bloqueia;
- permissão insuficiente bloqueia;
- comando desconhecido bloqueia;
- argumentos inseguros bloqueiam;
- validação pós-formatação sucesso;
- validação pós-formatação falha.

## Integração com Fakes

- fake USB provider;
- fake permission service;
- fake Wiimms runner;
- fake Task Manager;
- fake Notification Center;
- fake Diagnostics;
- fake Readiness Check;
- nenhum teste escreve em disco real.

---

# Critérios de Aceite

- [x] Detecta discos e partições via F107.
- [x] Mostra claramente o dispositivo alvo.
- [x] Bloqueia disco do sistema.
- [x] Exige confirmação explícita.
- [x] Mostra aviso de perda total de dados.
- [x] Exige digitar `FORMATAR WBFS`.
- [x] Faz preview antes da execução.
- [x] Integra com F105/F106.
- [x] Integra com F109.
- [x] Integra com F110.
- [x] Registra tarefa/notificação via F113.
- [x] Valida resultado após formatar.
- [x] Roda diagnóstico pós-formatação.
- [x] Nunca executa comando arbitrário.
- [x] Usa apenas Wiimms validado e comando catalogado.
- [x] Testes usam fakes e não acessam disco real.

---

# Regras para o Codex

- Nunca implementar formatação silenciosa.
- Nunca executar comando destrutivo real em teste.
- Nunca inventar comando final do Wiimms sem catálogo explícito.
- Nunca permitir shell arbitrário.
- Nunca permitir formatação iniciada por plugin.
- Sempre bloquear disco do sistema.
- Sempre exigir preview.
- Sempre exigir frase de confirmação para WBFS.
- Sempre integrar com permissões.
- Sempre registrar Task/Notification.
- Sempre validar pós-formatação.
- Sempre oferecer FAT32 `/wbfs` como alternativa recomendada.

---

# Dependências

- `F101-ExternalToolDependencyManager.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F106-PolkitPolicyInstaller.md`
- `F107-CrossPlatformUsbDeviceProvider.md`
- `F109-WiimmsOperationProgress.md`
- `F110-SafeUsbWriteWorkflow.md`
- `F113-TaskAndNotificationCenterIntegration.md`
- `F120-EndToEndWiiUsbReadinessCheck.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F090-SecurityChecklist.md`

---

# Definition of Done

- Fluxo de preparação USB documentado.
- FAT32 `/wbfs` definido como alternativa recomendada.
- WBFS definido como opção avançada/destrutiva.
- Preview obrigatório definido.
- Confirmação forte definida.
- Integração com permissões/progresso/tasks definida.
- Validação pós-formatação definida.
- Testes com fakes definidos.

# F110 - Safe USB Write Workflow

> Feature ID: F110  
> Status: Ready for Development  
> Priority: Critical  
> Epic: USB Device Operations / Safety  
> Area: Device Manager, Wiimms Integration, UX, Security, Task Manager  
> Depends on: F107 - CrossPlatformUsbDeviceProvider

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 2.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: F101, F105, F106, F107, F109, F113, F120.
- Reforço de escopo: wizard de escrita USB, validação de filesystem, espaço livre, permissões, duplicidade, preview, confirmação final, progresso via F109, tarefa/notificação via F113, validação pós-cópia e bloqueio de disco do sistema.
- Fora de escopo reforçado: formatar dispositivo, recuperar dados, reparar WBFS, particionar disco, escrever sem confirmação, executar comando arbitrário, permitir plugin iniciar escrita direta ou ignorar permissões.
- Regras adicionais: operação deve ser cancelável antes do ponto sem retorno; destino FAT32 deve considerar split; destino WBFS deve usar Wiimms; resultado deve ser registrado em histórico, diagnóstico e notificações.
- Impacto de UI: ação principal "Enviar para USB", tela de preview clara, alerta para operação sensível, progresso por etapa e resumo final.

# Objetivo

Criar um fluxo seguro de confirmação antes de enviar/copiar jogos para um HD USB,
partição WBFS ou volume FAT32/NTFS.

A operação de escrita em dispositivo USB pode causar perda de dados se o usuário
selecionar o alvo errado. Por isso, o WiiStudio deve exibir preview/simulação,
validar espaço livre, mostrar riscos e exigir confirmação final.

---

# Problema

Enviar jogo para USB é operação sensível porque envolve:

- dispositivo físico;
- partição alvo;
- espaço livre;
- filesystem;
- risco de sobrescrita;
- operação longa;
- possível corrupção se interrompida;
- privilégio elevado em alguns sistemas.

---

# Resultado Esperado

Antes de escrever:

```text
Selecionar jogo
    ↓
Selecionar dispositivo alvo
    ↓
Validar tipo: WBFS/FAT32/NTFS
    ↓
Validar espaço livre
    ↓
Mostrar preview
    ↓
Mostrar riscos
    ↓
Confirmar
    ↓
Executar com progresso/cancelamento
```

---

# Escopo

## Incluído

- fluxo guiado;
- seleção de dispositivo alvo;
- validação de filesystem;
- validação de espaço livre;
- preview/simulação;
- confirmação final;
- integração com Wiimms;
- integração com DevicePermissionService;
- integração com Task Manager;
- logs e diagnostics;
- testes com fakes.

## Fora do Escopo Inicial

- escrita sem confirmação;
- formatar dispositivo;
- reparar WBFS automaticamente;
- operação destrutiva sem confirmação extra;
- gravar em filesystem desconhecido.

---

# Tipos de Destino

Suportar inicialmente:

```text
WBFS raw/partition
FAT32
NTFS
exFAT, se política permitir no futuro
```

Regras:

- WBFS usa Wiimms/wwt;
- FAT32/NTFS podem usar arquivo `.wbfs`;
- FAT32 exige considerar limite de 4 GB por arquivo e split;
- filesystem desconhecido bloqueia escrita por padrão.

---

# Fluxo Completo

```text
Open Write Wizard
    ↓
Select source game
    ↓
Select target device/partition
    ↓
Check external tools
    ↓
Check permissions
    ↓
Check filesystem
    ↓
Check free space
    ↓
Build operation preview
    ↓
Show risk summary
    ↓
User confirms
    ↓
Create Task
    ↓
Run Wiimms operation
    ↓
Update progress
    ↓
Validate result
    ↓
Show summary
```

---

# Preview / Simulação

Antes de executar, mostrar:

```text
Jogo: Mario Kart Wii
Game ID: RMCP01
Origem: /home/user/games/RMCP01.iso
Destino: USB Drive 1
Partição: /dev/sdc1
Filesystem: WBFS
Espaço livre: 120 GB
Tamanho estimado: 4.3 GB
Ferramenta: wwt
Operação: copiar jogo para dispositivo
```

---

# Confirmação Final

Texto recomendado:

```text
Você está prestes a gravar um jogo no dispositivo USB selecionado.
Confira se este é o dispositivo correto.

Dispositivo alvo:
{DeviceName}
{DevicePath}

Essa operação pode demorar e não deve ser interrompida.
Deseja continuar?
```

Para operações destrutivas:

```text
Esta operação pode alterar dados no dispositivo.
Digite o nome do dispositivo para confirmar.
```

A primeira versão pode exigir confirmação simples para escrita e confirmação forte
para operação destrutiva.

---

# Validações Obrigatórias

Antes da escrita:

- origem existe;
- origem é formato suportado;
- destino existe;
- destino é removível/USB quando esperado;
- filesystem suportado;
- espaço livre suficiente;
- ferramenta Wiimms disponível;
- permissão adequada;
- device path validado;
- operação permitida;
- usuário confirmou.

---

# Espaço Livre

Calcular:

```text
RequiredBytes = tamanho estimado do jogo + margem
```

Margem recomendada:

```text
5% ou 256 MB, o que for maior
```

Se insuficiente:

```text
bloquear operação
```

---

# FAT32 e Split

FAT32 possui limite de arquivo de aproximadamente 4 GB.

Se destino for FAT32:

- usar modo split quando necessário;
- mostrar isso no preview;
- validar que o Wiimms suporta a operação;
- explicar ao usuário.

Mensagem:

```text
O destino FAT32 exige divisão do arquivo em partes. O WiiStudio fará isso automaticamente usando Wiimms.
```

---

# WBFS

Para destino WBFS:

- validar com Wiimms;
- listar jogos existentes;
- verificar duplicidade por Game ID;
- verificar espaço livre;
- não formatar;
- não reparar automaticamente sem confirmação.

---

# Duplicidade

Se o jogo já existir no destino:

Opções:

- cancelar;
- substituir, com confirmação;
- manter ambos, se suportado;
- ignorar.

Padrão recomendado:

```text
cancelar ou pular
```

Substituir exige confirmação explícita.

---

# Integração com Wiimms

Operações devem usar:

```text
wwt
wit
```

via ProcessRunner.

Nunca:

- executar shell;
- montar comando por string insegura;
- escrever sem preview;
- ignorar exit code.

---

# Integração com F109

O progresso deve usar:

```text
F109 - Wiimms Operation Progress
```

Mostrar:

- percentual quando disponível;
- operação atual;
- cancelamento;
- logs;
- resultado final.

---

# Integração com Permissões

Antes de executar:

```text
DevicePermissionService.CheckAsync
```

Se precisar de autorização:

- mostrar mensagem da F105;
- permitir retry;
- não iniciar escrita antes da autorização.

---

# Segurança

Obrigatório:

- validar device path;
- bloquear filesystem desconhecido;
- confirmar operação;
- não escrever em disco do sistema;
- não escrever em destino não removível sem confirmação especial;
- usar ProcessRunner;
- logs redigidos;
- timeout/cancelamento;
- escrita atômica quando possível;
- não remover dados sem confirmação;
- não formatar no primeiro ciclo.

---

# UI

Wizard sugerido:

```text
1. Selecionar jogo
2. Selecionar destino USB
3. Verificar compatibilidade
4. Preview e riscos
5. Confirmar
6. Progresso
7. Resultado
```

---

# Estados

```csharp
public enum UsbWriteWorkflowState
{
    SelectingSource,
    SelectingTarget,
    Validating,
    ReadyForConfirmation,
    Confirmed,
    Running,
    Completed,
    Failed,
    Cancelled,
    Blocked
}
```

---

# Resultado

```csharp
public sealed class UsbWriteResult
{
    public bool Success { get; init; }
    public string GameId { get; init; } = string.Empty;
    public string TargetDeviceId { get; init; } = string.Empty;
    public string? ErrorMessageKey { get; init; }
    public TimeSpan Duration { get; init; }
}
```

---

# Diagnóstico

Registrar:

```text
USB Write Workflow
Source game
Target device
Filesystem
Free space
Required space
Permission state
Wiimms tool
Preview generated
User confirmed
Result
```

---

# Tratamento de Erros

## Espaço Insuficiente

Bloquear operação antes da escrita.

## Dispositivo Removido

Cancelar operação e mostrar erro claro.

## Permissão Negada

Mostrar instrução de autorização.

## Jogo Duplicado

Pedir decisão do usuário.

## Wiimms Falha

Mostrar diagnóstico e logs.

## Cancelamento

Preservar estado e mostrar resumo parcial.

---

# Testes Esperados

## Unitários

- preview gerado;
- espaço suficiente;
- espaço insuficiente;
- filesystem WBFS aceito;
- FAT32 exige split;
- filesystem desconhecido bloqueado;
- dispositivo não USB alerta/bloqueia;
- duplicidade detectada;
- confirmação obrigatória;
- operação sem confirmação bloqueada;
- permissão insuficiente bloqueia;
- device path inválido bloqueado.

## Integração com Fakes

- fake USB provider;
- fake Wiimms runner;
- fake permission service;
- fake progress parser;
- task manager recebe updates;
- cancelamento funciona;
- falha não remove dados.

---

# Critérios de Aceite

- [ ] Fluxo de confirmação existe.
- [ ] Dispositivo alvo é exibido claramente.
- [ ] Espaço livre é validado.
- [ ] Tipo WBFS/FAT32/NTFS é exibido.
- [ ] Preview/simulação é mostrado.
- [ ] Confirmação final é obrigatória.
- [ ] Operação sem confirmação é bloqueada.
- [ ] Permissão é validada antes da escrita.
- [ ] Progresso é exibido.
- [ ] Cancelamento funciona.
- [ ] Falha de um passo não corrompe cache/estado.
- [ ] Testes usam fakes.

---

# Regras para o Codex

- Nunca escrever em USB sem confirmação.
- Nunca formatar dispositivo nessa feature.
- Nunca ignorar espaço livre.
- Nunca ignorar permissão.
- Sempre mostrar dispositivo alvo.
- Sempre gerar preview.
- Sempre usar ProcessRunner.
- Sempre integrar com Task Manager.
- Sempre usar F109 para progresso.
- Sempre bloquear filesystem desconhecido.
- Sempre tratar duplicidade.
- Nunca montar comando shell inseguro.

---

# Dependências

- `F035-DeviceManager.md`
- `F083-ExternalProcessSecurity.md`
- `F101-ExternalToolDependencyManager.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F107-CrossPlatformUsbDeviceProvider.md`
- `F109-WiimmsOperationProgress.md`
- `F090-SecurityChecklist.md`

---

# Definition of Done

- Workflow seguro definido.
- Preview/validação definidos.
- Confirmação obrigatória definida.
- Integração com Wiimms e permissões definida.
- Progresso/cancelamento definidos.
- Testes com fakes definidos.

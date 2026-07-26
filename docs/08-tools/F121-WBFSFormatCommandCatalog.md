# F121 - WBFS Format Command Catalog

> Feature ID: F121  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: USB Device Operations / Destructive Command Safety  
> Area: Wiimms, WBFS, Privileged Access, Device Manager, Diagnostics  
> Depends on: F105, F106, F107, F109, F110, F110-1, F113, F120

# Objetivo

Definir o catálogo oficial e seguro de comandos para formatação WBFS usando **Wiimms WBFS Tool (`wwt`)**.

Esta feature fecha a lacuna deixada pela `F110-1 - USB Format and WBFS Preparation`, definindo:

- comando oficial;
- argumentos permitidos;
- argumentos proibidos;
- fluxo de desmontagem;
- validação do alvo;
- fluxo de permissão por Linux/Windows/macOS;
- validação pós-formatação;
- mensagens de erro;
- testes com fakes.

---

# Fonte Oficial

A documentação oficial do Wiimms define o comando de formatação como:

```text
wwt FORMAT file|blockdev...
```

Também informa que:

```text
-f / --force é necessário para sair do modo de teste e realizar formatação real.
-t / --test força modo de teste.
```

O WiiStudio deve tratar a formatação como operação destrutiva e nunca montar esse comando livremente.

---

# Comando Oficial de Formatação WBFS

## Comando Real Catalogado

Comando permitido para execução real:

```text
wwt --force --verbose --progress FORMAT <TARGET_BLOCK_DEVICE_OR_FILE>
```

Exemplo Linux, depois de todas as validações:

```text
wwt --force --verbose --progress FORMAT /dev/sdc1
```

## Comando de Simulação / Preview Técnico

Antes de executar real, quando aplicável:

```text
wwt --test --verbose FORMAT <TARGET_BLOCK_DEVICE_OR_FILE>
```

Exemplo:

```text
wwt --test --verbose FORMAT /dev/sdc1
```

## Regras Antes de Executar o Comando Real

O WiiStudio só pode executar o comando real quando:

- o alvo foi validado por F107;
- o alvo não é disco do sistema;
- o alvo não é partição de boot/root;
- o alvo está desmontado ou liberado;
- o usuário viu o preview;
- o usuário digitou a frase de confirmação `FORMATAR WBFS`;
- permissões administrativas foram resolvidas;
- `wwt` foi validado pelo External Tool Manager;
- o comando foi montado a partir do catálogo, não por string livre.

---

# Comandos de Validação Pós-Formatação

Após formatar, validar o alvo com comandos catalogados.

## Check WBFS

```text
wwt --part <TARGET_BLOCK_DEVICE_OR_FILE> CHECK
```

Exemplo:

```text
wwt --part /dev/sdc1 CHECK
```

## Listar Conteúdo

```text
wwt --part <TARGET_BLOCK_DEVICE_OR_FILE> LIST --long --no-header
```

Exemplo:

```text
wwt --part /dev/sdc1 LIST --long --no-header
```

Resultado esperado para WBFS recém-formatado:

```text
WBFS válido
Nenhum jogo listado
Sem erros de alocação
Espaço detectado
ExitCode = 0
```

## Space / DF

Opcionalmente, validar espaço:

```text
wwt --part <TARGET_BLOCK_DEVICE_OR_FILE> SPACE
```

ou:

```text
wwt --part <TARGET_BLOCK_DEVICE_OR_FILE> DF
```

---

# Catálogo de Comandos Permitidos

## KnownWiimmsCommandDefinition

```csharp
public sealed class KnownWiimmsCommandDefinition
{
    public string OperationId { get; init; } = string.Empty;
    public string ToolId { get; init; } = "wwt";
    public IReadOnlyList<string> Arguments { get; init; } = [];
    public bool IsDestructive { get; init; }
    public bool RequiresStrongConfirmation { get; init; }
    public bool RequiresPrivilege { get; init; }
}
```

## Operações Permitidas

```csharp
public enum KnownWiimmsWbfsOperation
{
    PreviewFormatWbfs,
    FormatWbfs,
    CheckWbfs,
    ListWbfs,
    SpaceWbfs
}
```

---

# Definições Catalogadas

## PreviewFormatWbfs

```text
Tool: wwt
Arguments:
  --test
  --verbose
  FORMAT
  <TARGET>
Destructive: false
RequiresStrongConfirmation: false
RequiresPrivilege: platform-dependent
```

## FormatWbfs

```text
Tool: wwt
Arguments:
  --force
  --verbose
  --progress
  FORMAT
  <TARGET>
Destructive: true
RequiresStrongConfirmation: true
RequiresPrivilege: true
```

## CheckWbfs

```text
Tool: wwt
Arguments:
  --part
  <TARGET>
  CHECK
Destructive: false
RequiresStrongConfirmation: false
RequiresPrivilege: platform-dependent
```

## ListWbfs

```text
Tool: wwt
Arguments:
  --part
  <TARGET>
  LIST
  --long
  --no-header
Destructive: false
RequiresStrongConfirmation: false
RequiresPrivilege: platform-dependent
```

## SpaceWbfs

```text
Tool: wwt
Arguments:
  --part
  <TARGET>
  SPACE
Destructive: false
RequiresStrongConfirmation: false
RequiresPrivilege: platform-dependent
```

---

# Argumentos Permitidos para FORMAT

Permitidos no WiiStudio:

```text
--force
-f
--verbose
-v
--progress
-P
--test
-t
FORMAT
INIT
<TARGET>
```

Uso interno recomendado:

```text
--force
--verbose
--progress
FORMAT
<TARGET>
```

O alias `INIT` pode ser reconhecido, mas a implementação deve preferir `FORMAT`.

---

# Argumentos Proibidos para FORMAT

Proibidos no fluxo inicial:

```text
--recover
--size
--split
--split-size
--hss
--wss
--inode
--config
--logging
--all
--auto
--repair
--no-check
--overwrite
&&
|
;
>
<
`
$()
```

Motivos:

- `--recover` muda o comportamento da formatação.
- `--size` é voltado a arquivo WBFS, não ao fluxo de partição do usuário.
- `--hss` e `--wss` alteram geometria.
- `--auto` e `--all` podem atingir alvos errados.
- metacaracteres de shell indicam risco de injeção.

---

# Alvo Permitido

O alvo deve ser sempre um único valor validado:

```text
<TARGET_BLOCK_DEVICE_OR_FILE>
```

Exemplos Linux permitidos após validação:

```text
/dev/sdc1
/dev/disk/by-id/usb-SanDisk_...-part1
```

Exemplos proibidos:

```text
/dev/sda
/dev/sda1
/
C:\
\\.\PhysicalDrive0
qualquer disco do sistema
qualquer destino não classificado
qualquer string com traversal
qualquer string com shell syntax
```

---

# Precisa Desmontar a Partição Antes?

## Regra Geral

Sim. Antes de formatar uma partição/dispositivo, o WiiStudio deve garantir que o alvo
não esteja montado ou em uso.

## Linux

Se houver mount point, a primeira versão pode bloquear e orientar:

```text
Desmonte/ejetar o volume antes de formatar.
```

Uma versão futura pode executar desmontagem via operação catalogada e privilegiada.

## Windows

Se houver letra de unidade/volume em uso:

- bloquear operação se volume estiver aberto;
- orientar fechar programas;
- exigir Administrador quando necessário.

## macOS

Se estiver montado:

```text
diskutil unmount <TARGET>
```

Somente via comando catalogado seguro e com alvo validado.

---

# Validação de Disco do Sistema

Usar F107 para classificar:

- discos internos;
- partição de boot;
- partição raiz;
- volume do usuário;
- disco onde o app está rodando;
- disco removível;
- USB.

Bloquear se:

```text
IsSystemDisk = true
IsBootPartition = true
IsInternal = true
RiskLevel != SafeRemovable
```

Mensagem:

```text
Este alvo parece pertencer ao sistema operacional ou não pôde ser classificado com segurança.
A formatação foi bloqueada.
```

---

# Fluxo Linux

```text
F107 lista discos/partições
    ↓
Validar alvo
    ↓
Verificar mount
    ↓
Se montado: bloquear ou desmontar via operação catalogada
    ↓
F105 verifica permissão
    ↓
Se policy ausente: F106
    ↓
Usuário confirma FORMATAR WBFS
    ↓
pkexec/helper executa comando catalogado
    ↓
F109 reporta progresso
    ↓
Validar com CHECK/LIST/SPACE
```

---

# Fluxo Windows

```text
F107 lista discos físicos/volumes
    ↓
Validar alvo
    ↓
Bloquear PhysicalDrive0/sistema
    ↓
Verificar se processo está elevado
    ↓
Se não admin: orientar reiniciar como Administrador
    ↓
Usuário confirma FORMATAR WBFS
    ↓
Executar wwt validado via ProcessRunner elevado/controlado
    ↓
Validar com CHECK/LIST/SPACE
```

---

# Fluxo macOS

```text
F107 lista /dev/disk*
    ↓
Validar alvo externo/removível
    ↓
Verificar mount
    ↓
Se montado: bloquear ou desmontar via operação catalogada
    ↓
Verificar permissão/autorização
    ↓
Usuário confirma FORMATAR WBFS
    ↓
Executar wwt validado
    ↓
Validar com CHECK/LIST/SPACE
```

---

# Rollback

Formatação WBFS é destrutiva e não possui rollback garantido.

Mensagem obrigatória:

```text
Esta operação não possui rollback automático.
Após confirmar, os dados existentes podem ser perdidos permanentemente.
```

O que pode ser feito antes de formatar:

- exportar relatório do alvo;
- registrar filesystem anterior;
- registrar mount point;
- registrar tamanho;
- recomendar backup manual.

Depois de iniciar:

- não prometer recuperação;
- em falha, rodar diagnóstico;
- orientar recriar/preparar o dispositivo novamente.

---

# Mensagens de Erro

## TargetSystemDisk

```text
A formatação foi bloqueada porque o alvo parece ser o disco do sistema.
```

## TargetMounted

```text
O alvo está montado ou em uso. Desmonte/ejetar o volume antes de formatar.
```

## MissingPrivilege

```text
Permissão administrativa é necessária para formatar este dispositivo.
```

## MissingPolkitPolicy

```text
A policy Polkit do WiiStudio não está instalada.
```

## WiimmsNotAvailable

```text
A ferramenta wwt não está configurada ou não foi validada.
```

## ConfirmationPhraseMismatch

```text
A frase de confirmação não confere. Digite FORMATAR WBFS para continuar.
```

## FormatFailed

```text
A formatação WBFS falhou. Veja o diagnóstico para detalhes.
```

## ValidationFailed

```text
A formatação terminou, mas o WiiStudio não conseguiu validar o WBFS.
```

---

# Testes com Fakes

Nenhum teste automatizado pode formatar disco real.

## Unitários

- comando real é montado corretamente;
- comando preview é montado corretamente;
- argumentos proibidos são bloqueados;
- `--auto` é bloqueado;
- `--all` é bloqueado;
- alvo do sistema é bloqueado;
- alvo montado é bloqueado;
- frase incorreta bloqueia;
- permissão ausente bloqueia;
- validação pós-formatação sucesso;
- validação pós-formatação falha.

## Integração

- fake ProcessRunner;
- fake DeviceProvider;
- fake PermissionService;
- fake ExternalToolManager;
- fake TaskManager;
- fake NotificationCenter;
- fake Diagnostics;
- fake `wwt CHECK`;
- fake `wwt LIST`.

---

# Critérios de Aceite

- [x] Comando oficial `wwt --force --verbose --progress FORMAT <TARGET>` definido.
- [x] Comando de teste `wwt --test --verbose FORMAT <TARGET>` definido.
- [x] Argumentos permitidos documentados.
- [x] Argumentos proibidos documentados.
- [x] Alvo montado é tratado.
- [x] Disco do sistema é bloqueado.
- [x] Fluxo Linux/Windows/macOS definido.
- [x] Validação pós-formatação definida.
- [x] WBFS vazio é confirmado por `LIST`.
- [x] Rollback é explicado como não garantido.
- [x] Mensagens de erro definidas.
- [x] Testes usam fakes e não formatam disco real.

---

# Regras para o Codex

- Nunca executar comando real em teste.
- Nunca permitir `--auto` ou `--all` em formatação.
- Nunca aceitar argumentos livres.
- Nunca formatar disco do sistema.
- Nunca prometer rollback.
- Sempre validar `wwt`.
- Sempre exigir frase de confirmação.
- Sempre validar pós-formatação.
- Sempre registrar task/notificação.
- Sempre usar catálogo de comandos.

---

# Definition of Done

- Catálogo de comandos definido.
- Segurança de argumentos definida.
- Fluxo cross-platform definido.
- Validação pós-formatação definida.
- Testes com fakes definidos.

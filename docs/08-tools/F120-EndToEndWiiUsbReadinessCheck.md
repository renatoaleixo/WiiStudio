# F120 - End-to-End Wii USB Readiness Check

> Feature ID: F120  
> Status: Ready for Development  
> Priority: High  
> Epic: Diagnostics / Device Readiness  
> Area: Device Manager, Wiimms, GameTDB, Metadata, Assets, Permissions

# Objetivo

Criar um checklist final para responder:

```text
Seu HD está pronto para Wii?
```

A feature deve validar de ponta a ponta se um dispositivo USB está pronto para uso com jogos Wii,
incluindo filesystem, jogos detectados, Wiimms, permissões, capas e metadados.

---

# Resultado Esperado

Tela:

```text
Wii USB Readiness Check
├── Dispositivo detectado
├── Permissão OK
├── Wiimms funcionando
├── Filesystem suportado
├── Jogos detectados
├── Espaço livre
├── Capas/metadados
└── Resultado final: Pronto / Atenção / Não pronto
```

---

# Escopo

## Incluído

- checklist por dispositivo;
- validação de WBFS/FAT32/NTFS;
- validação de permissões;
- validação de Wiimms;
- detecção de jogos;
- validação de espaço livre;
- verificação de metadados/capas;
- ações de reparo;
- relatório exportável;
- testes com fakes.

## Fora do Escopo Inicial

- formatar dispositivo;
- reparar WBFS automaticamente;
- baixar todos os assets automaticamente sem confirmação;
- executar escrita no dispositivo.

---

# Checks

## 1. Dispositivo Detectado

Valida:

- USB/removível;
- partições;
- mount path/device path;
- tamanho.

## 2. Permissão

Integra com F105:

- Authorized;
- AuthorizationRequired;
- PolicyMissing;
- PermissionDenied.

## 3. Wiimms

Integra com F101:

- `wit --version`;
- `wwt --version`;
- paths válidos.

## 4. Filesystem

Valida:

- WBFS;
- FAT32;
- NTFS;
- exFAT, se política permitir;
- Unknown.

## 5. Jogos Detectados

Usa Wiimms ou scanner:

- lista Game IDs;
- valida ID6;
- conta jogos;
- detecta duplicados.

## 6. Espaço Livre

Mostra:

- total;
- usado;
- livre;
- recomendação.

## 7. Capas e Metadados

Valida:

- metadados em cache;
- capa frontal;
- disc art;
- GameTDB disponível;
- assets ausentes.

---

# Resultado

```csharp
public enum ReadinessState
{
    Ready,
    Warning,
    NotReady,
    Unknown
}
```

## WiiUsbReadinessResult

```csharp
public sealed class WiiUsbReadinessResult
{
    public string DeviceId { get; init; } = string.Empty;
    public ReadinessState State { get; init; }
    public IReadOnlyList<WiiUsbReadinessCheckItem> Items { get; init; } = [];
    public DateTimeOffset CheckedAt { get; init; }
}
```

## Check Item

```csharp
public sealed class WiiUsbReadinessCheckItem
{
    public string CheckId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public ReadinessState State { get; init; }
    public string? Details { get; init; }
    public string? RecommendedAction { get; init; }
    public string? RepairActionId { get; init; }
}
```

---

# UI

Exemplo:

```text
✅ Dispositivo USB detectado
✅ Wiimms funcionando
⚠️ Policy Polkit ausente
✅ Filesystem FAT32 suportado
✅ 12 jogos detectados
⚠️ 3 jogos sem capa
✅ Metadados disponíveis

Resultado: Atenção
```

Botões:

- Rodar verificação;
- Corrigir problemas;
- Abrir diagnóstico;
- Exportar relatório;
- Atualizar capas/metadados.

---

# Integração com Repair Actions

Itens com correção devem usar F118:

- instalar policy;
- configurar Wiimms;
- atualizar GameTDB;
- baixar capas;
- sincronizar metadados;
- abrir diagnóstico.

---

# Relatório

Permitir exportar:

```text
wii-usb-readiness-report.json
wii-usb-readiness-report.txt
```

Conteúdo:

- dispositivo;
- plataforma;
- checks;
- resultado;
- data;
- versões das ferramentas;
- recomendações.

---

# Segurança

- não escrever no dispositivo;
- não formatar;
- não reparar automaticamente;
- não executar comando arbitrário;
- usar Wiimms via ProcessRunner;
- validar paths;
- logs redigidos;
- timeout/cancelamento.

---

# Testes Esperados

- dispositivo pronto;
- dispositivo sem permissão;
- Wiimms ausente;
- filesystem desconhecido;
- WBFS detectado;
- FAT32 detectado;
- jogos detectados;
- nenhum jogo detectado;
- capas ausentes;
- metadados ausentes;
- repair actions associadas;
- relatório exportado.

---

# Critérios de Aceite

- [ ] Checklist roda para dispositivo selecionado.
- [ ] Valida filesystem.
- [ ] Valida Wiimms.
- [ ] Valida permissões.
- [ ] Lista jogos detectados.
- [ ] Mostra capas/metadados ausentes.
- [ ] Resultado final é Ready/Warning/NotReady.
- [ ] Ações de reparo aparecem.
- [ ] Relatório pode ser exportado.
- [ ] Testes usam fakes.

---

# Regras para o Codex

- Não escrever no dispositivo.
- Não formatar.
- Não executar reparo sem confirmação.
- Usar providers existentes.
- Não duplicar lógica de diagnóstico.
- Integrar com F118.
- Usar Task Manager para operação longa.
- Permitir cancelamento.

---

# Dependências

- `F101-ExternalToolDependencyManager.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F107-CrossPlatformUsbDeviceProvider.md`
- `F111-GameTdbUpdateSource.md`
- `F118-DiagnosticsRepairActions.md`
- `F113-TaskAndNotificationCenterIntegration.md`

---

# Definition of Done

- Checklist definido.
- Resultado final definido.
- Integrações definidas.
- Relatório definido.
- Testes com fakes definidos.

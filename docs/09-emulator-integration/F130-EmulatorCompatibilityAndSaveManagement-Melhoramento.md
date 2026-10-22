# F130 - Emulator Compatibility and Save Management [Melhoramento]

> **Suggested path:** `docs/09-emulator-integration/F130-EmulatorCompatibilityAndSaveManagement-Melhoramento.md`  
> **Priority:** Future / Product Expansion  
> **Status:** Implemented
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain  
> **Area:** Emulator Compatibility, Saves, Diagnostics, Game Details  
> **Related:** F115, F127, F128, F129, F113, F114, F118

---

## 1. Objetivo da Feature

Adicionar uma camada de compatibilidade e gerenciamento de saves relacionados ao uso de emulador externo, inicialmente Dolphin.

A feature deve ajudar o usuário a entender:

- se o jogo já foi executado no emulador;
- qual perfil foi usado;
- histórico de execução;
- status básico de compatibilidade;
- onde ficam saves/configurações, sem manipular dados sensíveis sem confirmação.

---

## 2. Problema que Resolve

Depois de iniciar jogos no emulador, o usuário pode querer:

- saber se o jogo já foi testado;
- registrar se funcionou bem;
- ver histórico de execução;
- abrir pasta de saves;
- fazer backup manual;
- associar notas de compatibilidade;
- saber qual perfil funcionou melhor.

Sem isso, a integração com emulador fica limitada ao botão de iniciar.

---

## 3. Escopo Funcional

Incluído:

- histórico de execução por jogo;
- status manual de compatibilidade;
- notas do usuário;
- perfil usado;
- data da última execução;
- abrir pasta de saves/configuração;
- backup de saves sob confirmação;
- diagnóstico de paths;
- integração com painel avançado.

---

## 4. Fora de Escopo

- Garantir compatibilidade automática.
- Baixar banco externo de compatibilidade no primeiro ciclo.
- Manipular saves sem confirmação.
- Converter saves entre regiões.
- Sincronizar saves em nuvem.
- Corrigir saves corrompidos.
- Acessar dados internos do Dolphin sem validação.

---

## 5. Fluxo de Usuário Esperado

```text
Usuário executa jogo no Dolphin
    ↓
WiiStudio registra histórico
    ↓
Painel mostra "Última execução"
    ↓
Usuário marca compatibilidade:
        Funciona bem
        Funciona com problemas
        Não testado
        Não funciona
    ↓
Usuário adiciona notas
    ↓
Opcional: abrir/backup pasta de saves
```

---

## 6. Regras de Negócio

- Compatibilidade inicial pode ser manual.
- Histórico deve ser local.
- Saves só podem ser abertos ou copiados com confirmação.
- Backup de saves deve gerar arquivo fora da pasta original.
- Não apagar saves.
- Não editar saves no primeiro ciclo.
- Status de compatibilidade por jogo não deve vir como verdade absoluta.
- Se Dolphin não estiver configurado, save management fica indisponível.

---

## 7. Regras de Segurança

- Não apagar saves.
- Não editar saves sem confirmação.
- Não enviar saves para nuvem.
- Não armazenar dados sensíveis desnecessários.
- Validar caminhos de saves.
- Bloquear path traversal.
- Abrir pasta via serviço seguro.
- Backup com temp e destino controlado.
- Logs redigidos.

---

## 8. Integrações Necessárias

- F127 para Dolphin configurado.
- F128 para histórico de launch.
- F129 para perfil utilizado.
- F115 para painel avançado.
- F114 para persistência SQLite.
- F113 para tarefas/notificações.
- F118 para diagnóstico/reparo.

---

## 9. Impactos na UI

No painel avançado do jogo:

```text
Emulação
├── Última execução
├── Perfil usado
├── Compatibilidade local
├── Notas
├── Abrir pasta de saves
└── Backup de saves
```

Status:

```text
Não testado
Funciona bem
Funciona com problemas
Não funciona
```

---

## 10. Serviços, Classes e Interfaces Sugeridas

```csharp
public interface IEmulatorCompatibilityService
{
    Task<EmulatorCompatibilityDto?> GetAsync(
        string gameId,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        EmulatorCompatibilityDto compatibility,
        CancellationToken cancellationToken = default);
}
```

```csharp
public interface IEmulatorSaveManagementService
{
    Task<SaveLocationResultDto> LocateSavesAsync(
        string gameId,
        string emulatorId,
        CancellationToken cancellationToken = default);

    Task<SaveBackupResultDto> BackupSavesAsync(
        SaveBackupRequestDto request,
        IProgress<SaveBackupProgressDto>? progress = null,
        CancellationToken cancellationToken = default);
}
```

```csharp
public enum LocalCompatibilityStatus
{
    NotTested,
    WorksWell,
    WorksWithIssues,
    DoesNotWork,
    Unknown
}
```

---

## 11. Persistência e Configurações

SQLite futuro:

```text
EmulatorLaunchHistory
GameEmulatorCompatibility
GameEmulatorNotes
SaveBackups
```

Configurações:

```text
EmulatorSaves.BackupDirectory
EmulatorSaves.OpenFolderRequiresConfirmation = true
EmulatorSaves.BackupRequiresConfirmation = true
```

---

## 12. Logs, Tarefas e Notificações Esperadas

Tarefas:

- Backup de saves;
- Localizando saves;
- Exportando histórico.

Notificações:

```text
Backup de saves concluído.
Não foi possível localizar saves para este jogo.
Compatibilidade local atualizada.
```

Logs:

- GameId;
- EmulatorId;
- profileId;
- save path redigido;
- backup result;
- correlationId.

---

## 13. Critérios de Aceite

- [x] Histórico de execução aparece no painel.
- [x] Usuário define compatibilidade local.
- [x] Usuário adiciona notas.
- [x] Perfil usado é exibido.
- [x] Pasta de saves pode ser aberta quando localizada.
- [x] Backup de saves exige confirmação.
- [x] Nenhum save é apagado.
- [x] Testes usam fakes.

---

## 14. Cenários de Teste

- jogo nunca executado;
- jogo executado uma vez;
- múltiplas execuções;
- salvar compatibilidade;
- salvar notas;
- Dolphin não configurado;
- saves encontrados;
- saves ausentes;
- backup sucesso;
- backup falha;
- path inseguro bloqueado.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Saves não encontrados | `Não foi possível localizar saves para este jogo.` |
| Dolphin ausente | `Configure o Dolphin antes de gerenciar saves de emulação.` |
| Backup falhou | `Não foi possível criar backup dos saves.` |
| Path inseguro | `O caminho dos saves foi bloqueado por segurança.` |
| Sem histórico | `Este jogo ainda não foi executado no emulador.` |
| Falha ao salvar nota | `Não foi possível salvar as notas de compatibilidade.` |

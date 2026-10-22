# F129 - Emulator Profiles and Controller Mapping [Melhoramento]

> **Suggested path:** `docs/09-emulator-integration/F129-EmulatorProfilesAndControllerMapping-Melhoramento.md`  
> **Priority:** Future / Product Expansion  
> **Status:** Implemented
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain  
> **Area:** Emulator Profiles, Controller UX, Settings  
> **Related:** F127, F128, F119, F113, F114, F118

---

## 1. Objetivo da Feature

Adicionar perfis de emulação e mapeamento de controles ao WiiStudio, permitindo que o usuário organize configurações por tipo de jogo, plataforma, controle e preferência de execução.

A feature não deve reimplementar as configurações internas do Dolphin no primeiro ciclo. Ela deve oferecer uma camada de organização e seleção de perfis que podem ser aplicados ao fluxo de inicialização.

---

## 2. Problema que Resolve

Mesmo com o Dolphin integrado, o usuário pode precisar alternar entre configurações diferentes:

- controle tradicional;
- teclado;
- controle Bluetooth;
- Wiimote real;
- sensor bar;
- perfil casual;
- perfil performance;
- perfil gráfico;
- perfil por jogo.

Sem perfis no WiiStudio, o usuário precisa gerenciar tudo manualmente dentro do emulador.

---

## 3. Escopo Funcional

Incluído:

- criar perfis de emulador no WiiStudio;
- associar perfil padrão;
- associar perfil por jogo;
- registrar preferências de controle;
- abrir pasta/configuração do Dolphin;
- exibir checklist de controle antes de iniciar;
- registrar diagnóstico de perfil;
- permitir importar/exportar configuração do perfil WiiStudio.

---

## 4. Fora de Escopo

- Reescrever arquivos internos complexos do Dolphin sem especificação.
- Detectar todos os modelos de controles automaticamente.
- Gerenciar firmware, Bluetooth stack ou drivers.
- Garantir configuração de Wiimote real em todas as plataformas.
- Modificar Dolphin enquanto ele está rodando.
- Criar driver próprio.
- Instalar dependências de controle sem confirmação.

---

## 5. Fluxo de Usuário Esperado

```text
Usuário abre Configurações > Emulador
    ↓
Cria perfil: Padrão
    ↓
Define preferências:
        tela cheia
        controle esperado
        backend gráfico preferido
        observações
    ↓
Associa perfil ao jogo ou biblioteca
    ↓
Ao clicar Jogar no PC, WiiStudio usa o perfil
```

---

## 6. Regras de Negócio

- Sempre deve existir um perfil padrão.
- Perfil por jogo sobrescreve perfil global.
- Perfil pode ser apenas metadata do WiiStudio no primeiro ciclo.
- Configurações perigosas/avançadas devem abrir o Dolphin, não editar automaticamente.
- Se controle esperado não estiver conectado, mostrar aviso.
- O usuário pode iniciar mesmo com aviso, salvo bloqueio configurado.
- Perfil não pode executar comando arbitrário.
- Alteração de perfil deve ser registrada no histórico.

---

## 7. Regras de Segurança

- Não editar arquivos internos do Dolphin sem backup.
- Não executar scripts de controle.
- Não instalar drivers.
- Não pedir senha.
- Não executar comandos arbitrários.
- Validar paths de configuração.
- Backups antes de qualquer alteração futura em arquivos do Dolphin.
- Não permitir plugin alterar perfil para injetar argumentos.

---

## 8. Integrações Necessárias

- F127 para detectar Dolphin.
- F128 para aplicar perfil no lançamento.
- F119 para wizard sugerir perfil inicial.
- F113 para notificações de erro.
- F114 para persistência de perfis.
- F118 para diagnóstico de perfil.

---

## 9. Impactos na UI

Adicionar seção:

```text
Configurações > Emuladores > Perfis
```

Campos:

```text
Nome do perfil
Emulador
Modo de tela
Controle esperado
Observações
Perfil padrão
Aplicar a jogos selecionados
```

No painel do jogo:

```text
Perfil de emulação: Padrão
[Alterar perfil]
```

---

## 10. Serviços, Classes e Interfaces Sugeridas

```csharp
public interface IEmulatorProfileService
{
    Task<IReadOnlyList<EmulatorProfileDto>> GetProfilesAsync(
        CancellationToken cancellationToken = default);

    Task<EmulatorProfileDto> SaveAsync(
        EmulatorProfileDto profile,
        CancellationToken cancellationToken = default);

    Task AssignToGameAsync(
        string gameId,
        string profileId,
        CancellationToken cancellationToken = default);
}
```

```csharp
public sealed class EmulatorProfileDto
{
    public string ProfileId { get; init; } = string.Empty;
    public string EmulatorId { get; init; } = "dolphin";
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string? ExpectedController { get; init; }
    public bool Fullscreen { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>();
}
```

```csharp
public sealed class GameEmulatorProfileAssignmentDto
{
    public string GameId { get; init; } = string.Empty;
    public string EmulatorId { get; init; } = "dolphin";
    public string ProfileId { get; init; } = string.Empty;
}
```

---

## 11. Persistência e Configurações

SQLite futuro:

```text
EmulatorProfiles
GameEmulatorProfiles
```

Settings fallback:

```text
EmulatorProfiles.DefaultProfileId
EmulatorProfiles.LastUsedProfileId
```

---

## 12. Logs, Tarefas e Notificações Esperadas

Normalmente não exige tarefa longa.

Notificações:

```text
Perfil de emulação salvo.
Perfil aplicado ao jogo.
Controle esperado não foi detectado.
```

Logs:

- profileId;
- gameId;
- emulatorId;
- alterações realizadas;
- erros de validação.

---

## 13. Critérios de Aceite

- [x] Usuário cria perfil.
- [x] Usuário define perfil padrão.
- [x] Usuário associa perfil a jogo.
- [x] Fluxo de launch lê perfil.
- [x] Perfil não quebra execução sem Dolphin.
- [x] Configurações avançadas abrem Dolphin em vez de editar arquivo diretamente.
- [x] Testes usam repositório fake.

---

## 14. Cenários de Teste

- criar perfil padrão;
- editar perfil;
- remover perfil não padrão;
- tentar remover perfil padrão;
- associar jogo;
- launch usa perfil do jogo;
- launch usa perfil global;
- controle ausente gera warning;
- exportar/importar perfil.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Perfil inválido | `O perfil de emulação está incompleto ou inválido.` |
| Perfil padrão | `Não é possível remover o perfil padrão enquanto ele estiver em uso.` |
| Controle ausente | `O controle esperado para este perfil não foi detectado.` |
| Dolphin ausente | `Configure o Dolphin antes de usar perfis de emulação.` |
| Falha ao salvar | `Não foi possível salvar o perfil de emulação.` |

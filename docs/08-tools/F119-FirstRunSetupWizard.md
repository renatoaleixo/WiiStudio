# F119 - First Run Setup Wizard

> Feature ID: F119  
> Status: Implemented and Homologated  
> Priority: High  
> Epic: Onboarding / Setup  
> Area: UI, Settings, External Tools, Permissions, GameTDB, Theme, Localization

# Revisão Estruturada do Projetista

Revisão recebida em `docs/08-tools-1` consolidada neste documento sem substituir os detalhes técnicos existentes.

- Prioridade revisada: 1.
- Arquitetura alvo: .NET 10, Avalonia, Application / Infrastructure / Domain.
- Relacionadas: F023, F039, F101, F105, F106, F111, F118.
- Reforço de escopo: primeira execução, reabrir via Configurações, idioma, tema, pasta de biblioteca/exportação/cache, Wiimms, NKit opcional, permissões USB por plataforma, policy Polkit no Linux, GameTDB, idioma de capas/metadados, diagnóstico final e estado `FirstRun.Completed`.
- Fora de escopo reforçado: obrigatoriedade de concluir wizard, instalação/download sem confirmação/checksum, pedir ou salvar senha, formatar USB, copiar jogos, marketplace de plugins e atualização automática do app.
- Regras adicionais: usuário pode pular com aviso, wizard deve ser idempotente, cada etapa deve salvar somente configurações validadas e o diagnóstico final deve indicar o que ainda falta.
- Impacto de UI: fluxo de boas-vindas com etapas claras, resumo final, botões voltar/avançar/pular, progresso de configuração e link para reabrir o assistente.

# Objetivo

Criar um assistente inicial de configuração do WiiStudio para preparar o ambiente do usuário
na primeira execução.

O wizard deve configurar:

- idioma;
- tema;
- pasta de biblioteca/exportação;
- ferramentas Wiimms/NKit;
- permissões de dispositivo;
- GameTDB;
- preferências de capas/metadados.

---

# Problema

Sem wizard inicial, o usuário pode abrir o app e encontrar:

- ferramentas externas ausentes;
- GameTDB não configurado;
- permissões não diagnosticadas;
- pasta de biblioteca não definida;
- idioma/tema não ajustados;
- botões que parecem não funcionar por dependências ausentes.

---

# Resultado Esperado

Na primeira execução:

```text
Bem-vindo ao WiiStudio
    ↓
Escolher idioma
    ↓
Escolher tema
    ↓
Configurar pastas
    ↓
Configurar Wiimms/NKit
    ↓
Verificar permissões
    ↓
Configurar GameTDB
    ↓
Concluir
```

---

# Etapas do Wizard

## 1. Boas-vindas

Explicar:

```text
Vamos configurar o WiiStudio para gerenciar sua biblioteca Wii.
```

## 2. Idioma

Opções:

- Português;
- Inglês;
- outros idiomas suportados.

Salvar em Settings.

## 3. Tema

Opções:

- Claro;
- Escuro;
- Sistema.

## 4. Pastas

Configurar:

- pasta da biblioteca;
- pasta de exportação;
- pasta de cache, opcional;
- pasta de ferramentas, opcional.

## 5. Ferramentas Externas

Integrar com F101:

- detectar Wiimms;
- configurar `wit`;
- configurar `wwt`;
- detectar NKit;
- instalar automaticamente quando seguro;
- selecionar manualmente.

## 6. Permissões

Integrar com F105/F106:

- verificar permissões USB;
- detectar policy Polkit;
- orientar instalação;
- detectar admin no Windows;
- detectar limitações macOS.

## 7. GameTDB

Integrar com F111/F103/F102:

- configurar idioma de metadados;
- configurar idioma de capas;
- baixar/atualizar base GameTDB;
- testar provider.

## 8. Resumo

Mostrar:

```text
Idioma: Português
Tema: Escuro
Wiimms: OK
NKit: Opcional/Não configurado
Permissões USB: OK/Pendente
GameTDB: OK
```

---

# Persistência

Salvar:

```text
FirstRun.Completed = true
FirstRun.CompletedAt
Language
Theme
Library.DefaultPath
Export.DefaultPath
ExternalTools.*
Metadata.*
Assets.*
```

---

# Reabrir Wizard

Permitir abrir depois em:

```text
Configurações > Executar assistente de configuração
```

---

# Pular Wizard

Usuário pode pular, mas deve ser avisado:

```text
Você pode configurar depois em Configurações.
Algumas funções podem não funcionar até concluir a configuração.
```

---

# UX

- etapas claras;
- indicador de progresso;
- voltar/avançar;
- pular;
- concluir;
- feedback de sucesso/falha;
- links para diagnóstico;
- sem travar UI.

---

# Segurança

- não instalar ferramenta sem confirmação;
- não salvar senha;
- não baixar sem checksum quando for auto-install;
- não pedir privilégio sem explicar;
- validar paths;
- não aceitar pasta insegura;
- logs redigidos.

---

# Testes Esperados

- primeira execução abre wizard;
- wizard não abre após concluir;
- usuário pula wizard;
- idioma salvo;
- tema salvo;
- pasta salva;
- Wiimms detectado;
- Wiimms ausente mostra ação;
- GameTDB configurado;
- permissão pendente mostra diagnóstico;
- resumo correto.

---

# Critérios de Aceite

- [x] Wizard abre na primeira execução.
- [x] Usuário configura idioma.
- [x] Usuário configura tema.
- [x] Usuário configura pastas.
- [x] Wizard integra com External Tool Manager.
- [x] Wizard integra com permissões.
- [x] Wizard integra com GameTDB.
- [x] Resumo final é exibido.
- [x] `FirstRun.Completed` é salvo.
- [x] Wizard pode ser reaberto em Settings.

---

# Regras para o Codex

- Não bloquear usuário se ele pular.
- Não instalar nada sem confirmação.
- Não pedir senha.
- Usar serviços existentes.
- Persistir settings corretamente.
- Não colocar lógica pesada na View.
- Criar testes de ViewModel.

---

# Dependências

- `F023-SettingsManager.md`
- `F039-ThemeEngine.md`
- `F101-ExternalToolDependencyManager.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F106-PolkitPolicyInstaller.md`
- `F111-GameTdbUpdateSource.md`
- `F118-DiagnosticsRepairActions.md`

---

# Definition of Done

- Wizard definido.
- Etapas definidas.
- Settings integrados.
- Segurança definida.
- Testes definidos.

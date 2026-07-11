# F119 - Assistente inicial [Melhoramento]

> **Suggested path:** `docs/08-tools/F119-FirstRunSetupWizard-Melhoramento.md`  
> **Priority:** Baixa  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Adicionar wizard de primeira execução para detectar SO, permissões, tools, pasta de biblioteca, GameTDB e HD WBFS.

---

## 2. Problema que Resolve

Sem wizard, usuários podem abrir o app sem dependências configuradas e achar que recursos não funcionam.

---

## 3. Escopo Funcional

- Wizard inicial.
- Idioma/tema.
- Pastas.
- Wiimms/NKit.
- Permissões.
- GameTDB.
- Resumo final.
- Reabrir em Configurações.

---

## 4. Fora de Escopo

- Obrigar configuração.
- Formatar USB.
- Instalar sem confirmação

---

## 5. Fluxo de Usuário Esperado

1. Primeira execução.
2. Boas-vindas.
3. Configurações essenciais.
4. Diagnóstico inicial.
5. Resumo.

---

## 6. Regras de Negócio

- Usuário pode pular.
- Falha parcial não bloqueia app.
- NKit é opcional.
- Permissões são diagnosticadas.

---

## 7. Regras de Segurança

- Não armazenar senha.
- Não instalar sem confirmação.
- Validar paths.
- Sem formatação no wizard.

---

## 8. Integrações Necessárias

- F023
- F039
- F101
- F105
- F106
- F111
- F118

---

## 9. Impactos na UI

- Wizard multi-step
- Cards OK/Pendente/Erro
- Resumo final
- Abrir diagnóstico

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IFirstRunSetupService
- FirstRunSetupStateDto
- FirstRunStepStatus

---

## 11. Persistência e Configurações

- FirstRun.Completed
- FirstRun.SkippedAt
- Language.Current
- Theme.Current
- Library.DefaultPath

---

## 12. Logs, Tarefas e Notificações Esperadas

Todo fluxo longo deve aparecer no módulo de **Tarefas** e gerar **Notificações** ao concluir, falhar ou ser cancelado.

Regras esperadas:

- criar `TaskId` e `CorrelationId`;
- exibir progresso quando disponível;
- exibir estado indeterminado quando progresso real não existir;
- publicar notificação persistente em falha;
- registrar logs redigidos;
- permitir exportação de relatório quando aplicável.

---

## 13. Critérios de Aceite

- Abre primeira execução.
- Pode pular.
- Pode reabrir.
- Salva idioma/tema/pastas.
- Diagnostica tools/permissões.

---

## 14. Cenários de Teste

- first run
- skip
- complete
- Wiimms missing
- GameTDB fail
- path invalid

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Path inválido | `A pasta selecionada não é válida ou não pode ser acessada.` |
| Wiimms ausente | `Wiimms não foi encontrado. Você pode configurar depois.` |

---

## 16. Regras para Implementação

- Seguir arquitetura Application / Infrastructure / Domain.
- UI Avalonia deve usar ViewModels e Commands.
- Não colocar regra de negócio em code-behind.
- Operações longas devem usar `async/await`, `CancellationToken`, progresso, Task Center e Notification Center.
- Não usar `Process.Start` diretamente.
- Não armazenar senha do usuário.
- Validar paths e argumentos.
- Usar fakes nos testes automatizados.

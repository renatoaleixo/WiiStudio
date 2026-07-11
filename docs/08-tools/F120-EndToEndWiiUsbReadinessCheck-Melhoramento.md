# F120 - Checklist de prontidão Wii USB [Melhoramento]

> **Suggested path:** `docs/08-tools/F120-EndToEndWiiUsbReadinessCheck-Melhoramento.md`  
> **Priority:** Baixa  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Criar checklist 'pronto para usar no Wii' validando HD, filesystem, jogos, Wiimms, permissões, capas/metadados e relatório.

---

## 2. Problema que Resolve

Usuários precisam saber se o HD/pendrive realmente está pronto para o Wii, sem juntar informações manualmente.

---

## 3. Escopo Funcional

- Checklist por dispositivo
- Filesystem
- Permissões
- Wiimms
- Jogos
- Espaço
- Capas/metadados
- Relatório exportável

---

## 4. Fora de Escopo

- Formatar
- Copiar jogos
- Reparar WBFS automaticamente

---

## 5. Fluxo de Usuário Esperado

1. Selecionar dispositivo.
2. Rodar check.
3. Ver Ready/Warning/NotReady.
4. Corrigir problemas.
5. Exportar relatório.

---

## 6. Regras de Negócio

- Sem escrita no dispositivo.
- Falha de um check não impede os demais.
- Ações corretivas via F118.
- Resultado final usa pior estado.

---

## 7. Regras de Segurança

- Não formatar.
- Não armazenar senha.
- Usar comandos de leitura.
- Redigir relatório.

---

## 8. Integrações Necessárias

- F101
- F105
- F107
- F111
- F118
- F113
- F125

---

## 9. Impactos na UI

- Lista de checks
- Resultado final
- Botão corrigir
- Exportar relatório

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IWiiUsbReadinessCheckService
- WiiUsbReadinessResultDto
- WiiUsbReadinessCheckItemDto

---

## 11. Persistência e Configurações

- Readiness.LastDeviceId
- Readiness.LastCheckAt
- reports/readiness/*.json

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

- Resultado Ready/Warning/NotReady.
- Relatório exportável.
- Nenhuma escrita.
- Ações de reparo aparecem.

---

## 14. Cenários de Teste

- ready
- sem permissão
- Wiimms ausente
- FAT32 sem /wbfs
- WBFS válido
- sem jogos
- relatório

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Sem dispositivo | `Nenhum dispositivo USB compatível foi selecionado.` |
| Wiimms ausente | `Wiimms não está configurado. Algumas verificações não puderam ser executadas.` |

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

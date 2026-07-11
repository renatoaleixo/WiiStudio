# F118 - Diagnóstico com ações de correção [Melhoramento]

> **Suggested path:** `docs/08-tools/F118-DiagnosticsRepairActions-Melhoramento.md`  
> **Priority:** Alta  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Evoluir o diagnóstico do WiiStudio para permitir ações corretivas guiadas, como Corrigir, Instalar policy, Revalidar ferramentas e Exportar relatório completo.

---

## 2. Problema que Resolve

Hoje o diagnóstico informa problemas, mas ainda exige que o usuário resolva manualmente dependências, permissões, paths de ferramentas e falhas comuns.

---

## 3. Escopo Funcional

- Adicionar ações de correção por item diagnosticado.
- Adicionar botão Corrigir para ações seguras e catalogadas.
- Adicionar ação Instalar policy Polkit no Linux via F106.
- Adicionar Revalidar ferramentas para wit/wwt/NKit.
- Adicionar Exportar relatório completo em JSON/TXT.
- Adicionar histórico das correções executadas.
- Integrar correções com Tarefas e Notificações.

---

## 4. Fora de Escopo

- Não executar correções destrutivas automaticamente.
- Não pedir ou armazenar senha.
- Não executar comandos arbitrários.
- Não instalar ferramentas sem confirmação e checksum.

---

## 5. Fluxo de Usuário Esperado

1. Usuário abre Diagnóstico.
2. WiiStudio lista problemas encontrados.
3. Cada problema elegível mostra ação recomendada.
4. Usuário clica em Corrigir.
5. Sistema mostra confirmação quando necessário.
6. Ação roda como tarefa.
7. Diagnóstico é reexecutado.
8. Resultado gera notificação e pode ser exportado.

---

## 6. Regras de Negócio

- Somente ações catalogadas podem ser executadas.
- Ação sensível exige confirmação.
- Ação elevada delega autenticação ao sistema operacional.
- Após correção, o item deve ser revalidado.
- Falha de correção não deve quebrar o diagnóstico inteiro.

---

## 7. Regras de Segurança

- Nunca armazenar senha.
- Nunca executar shell arbitrário.
- Redigir logs.
- Validar paths.
- Não instalar nada silenciosamente.
- Exportação de relatório deve evitar dados sensíveis desnecessários.

---

## 8. Integrações Necessárias

- F030
- F101
- F105
- F106
- F111
- F113
- F119

---

## 9. Impactos na UI

- Adicionar botão Corrigir em itens conhecidos.
- Adicionar botão Exportar relatório completo.
- Adicionar status da última tentativa de correção.
- Adicionar indicador de risco: Seguro, Requer confirmação, Requer privilégio.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IDiagnosticsRepairService
- IDiagnosticRepairAction
- DiagnosticsReportExporter
- PolkitPolicyRepairAction
- ExternalToolValidationRepairAction
- GameTdbUpdateRepairAction

---

## 11. Persistência e Configurações

- Diagnostics.LastRepairAt
- Diagnostics.LastRepairActionId
- Diagnostics.LastReportPath
- diagnostics/repair-history.json

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

- Itens conhecidos exibem ações de correção.
- Policy Polkit pode ser instalada/revalidada.
- Ferramentas podem ser revalidadas.
- Relatório completo pode ser exportado.
- Correções aparecem em Tarefas.
- Resultado gera Notificação.
- Nenhuma senha é armazenada.

---

## 14. Cenários de Teste

- Item sem ação não exibe Corrigir.
- Ação segura executa com sucesso.
- Ação sensível exige confirmação.
- Ação desconhecida é bloqueada.
- Falha gera notificação.
- Relatório é exportado com fakes.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Ação desconhecida | `Esta ação de reparo não é reconhecida pelo WiiStudio.` |
| Sem permissão | `Essa correção requer autorização do sistema operacional.` |
| Falha | `Não foi possível concluir a correção. Veja o diagnóstico para detalhes.` |
| Cancelado | `Correção cancelada pelo usuário.` |

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

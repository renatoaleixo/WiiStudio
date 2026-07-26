# F109 - Progresso real do Wiimms [Melhoramento]

> **Suggested path:** `docs/08-tools/F109-WiimmsOperationProgress-Melhoramento.md`  
> **Priority:** Alta  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Melhorar leitura de progresso de operações longas do Wiimms, evitando sensação de travamento.

---

## 2. Problema que Resolve

Operações como importação, conversão, remoção e verificação podem demorar e parecer congeladas.

---

## 3. Escopo Funcional

- Parser tolerante de stdout/stderr.
- Progresso real quando houver percentual.
- Modo indeterminado quando não houver.
- Integração com Task Center, Status Bar e Notificações.
- Cancelamento e timeout.

---

## 4. Fora de Escopo

- Não alterar Wiimms.
- Não depender de uma única versão.
- Não falhar operação por linha não parseada.

---

## 5. Fluxo de Usuário Esperado

1. Wiimms inicia via ProcessRunner.
2. Saída é lida linha a linha.
3. Parser atualiza progresso.
4. Task Center mostra estado.
5. Notificação final é gerada.

---

## 6. Regras de Negócio

- Linha desconhecida é ignorada.
- stderr warning não é sempre erro.
- Sem percentual usa spinner indeterminado.
- Cancelamento encerra processo com segurança.

---

## 7. Regras de Segurança

- Sem shell arbitrário.
- Logs redigidos.
- Argumentos sanitizados.
- Timeout por tipo de operação.

---

## 8. Integrações Necessárias

- F109
- F110
- F110-1
- F113
- F121

---

## 9. Impactos na UI

- Task com percentual
- Status Bar com operação atual
- Notificação de sucesso/falha/cancelamento

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IWiimmsProgressParser
- WiimmsOperationProgress
- ProcessOutputLine
- IProcessRunner

---

## 11. Persistência e Configurações

- Tasks history via F113

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

- Progresso aparece quando possível.
- Indeterminado quando não há percentual.
- Cancelamento funciona.
- Falha gera diagnóstico.

---

## 14. Cenários de Teste

- parse percentual
- linha irrelevante
- stderr warning
- cancelamento
- timeout
- process runner fake

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Timeout | `A operação demorou mais que o esperado e foi interrompida.` |
| Falha | `A operação Wiimms falhou. Veja o diagnóstico.` |
| Cancelado | `Operação cancelada pelo usuário.` |

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

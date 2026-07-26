# F110 - Escrita segura em USB [Melhoramento]

> **Suggested path:** `docs/08-tools/F110-SafeUsbWriteWorkflow-Melhoramento.md`  
> **Priority:** Alta  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Reforçar o fluxo seguro antes de qualquer escrita em HD/pendrive, protegendo contra seleção do disco errado e tornando riscos, permissões e progresso claros.

---

## 2. Problema que Resolve

Operações de escrita em USB podem afetar dispositivos errados, falhar por permissão, falta de espaço ou filesystem incompatível.

---

## 3. Escopo Funcional

- Reforçar preview obrigatório.
- Exibir dispositivo, partição, filesystem, espaço livre e risco.
- Bloquear disco do sistema.
- Exigir confirmação final.
- Validar duplicidade de Game ID.
- Validar FAT32 split.
- Exibir progresso via F109.
- Registrar tarefa/notificação via F113.
- Executar readiness check após escrita.

---

## 4. Fora de Escopo

- Não formatar dispositivo.
- Não particionar.
- Não executar escrita sem confirmação.
- Não permitir plugin iniciar escrita diretamente.

---

## 5. Fluxo de Usuário Esperado

1. Selecionar jogo.
2. Selecionar destino USB.
3. Validar destino e permissões.
4. Exibir preview.
5. Usuário confirma.
6. Executar Wiimms com comando catalogado.
7. Mostrar progresso.
8. Validar resultado e notificar.

---

## 6. Regras de Negócio

- Disco do sistema sempre bloqueado.
- Destino desconhecido bloqueado.
- WBFS validado antes da escrita.
- FAT32 exige split quando necessário.
- Duplicidade exige decisão do usuário.
- Operação sem confirmação é bloqueada.

---

## 7. Regras de Segurança

- Não armazenar senha.
- Usar F105/F106 para permissão.
- Usar IProcessRunner.
- Não montar shell por string.
- Validar device path.
- Redigir logs.

---

## 8. Integrações Necessárias

- F101
- F105
- F106
- F107
- F109
- F113
- F120
- F121

---

## 9. Impactos na UI

- Wizard de escrita USB.
- Preview com alerta visual.
- Botão Confirmar só habilita após validações.
- Progresso visível em Task Center.
- Resultado final com resumo.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IUsbWriteWorkflowService
- UsbWritePreviewDto
- UsbWriteExecutionRequestDto
- UsbWriteResultDto
- UsbWriteProgressDto

---

## 11. Persistência e Configurações

- UsbWrite.LastTargetDeviceId
- UsbWrite.RequireConfirmation
- UsbWrite.Fat32SplitEnabled

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

- Preview obrigatório.
- Disco do sistema bloqueado.
- Espaço validado.
- Permissão validada.
- Progresso aparece.
- Validação pós-escrita executa.
- Falhas são notificadas.

---

## 14. Cenários de Teste

- Destino válido FAT32.
- Destino sem espaço.
- WBFS inválido.
- Disco do sistema bloqueado.
- Permissão ausente.
- Duplicidade detectada.
- Cancelamento.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Disco do sistema | `Este destino parece ser o disco do sistema. A operação foi bloqueada.` |
| Espaço insuficiente | `O dispositivo não possui espaço livre suficiente.` |
| Permissão | `Permissão administrativa é necessária para acessar este dispositivo.` |
| Duplicado | `Este jogo já existe no destino.` |

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

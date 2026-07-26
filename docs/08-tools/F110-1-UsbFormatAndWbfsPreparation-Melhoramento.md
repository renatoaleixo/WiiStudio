# F110-1 - Preparação USB e WBFS [Melhoramento]

> **Suggested path:** `docs/08-tools/F110-1-UsbFormatAndWbfsPreparation-Melhoramento.md`  
> **Priority:** Alta  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Aprimorar a preparação de HD/pendrive para Wii, oferecendo FAT32 /wbfs como opção recomendada e WBFS como opção avançada com confirmação forte.

---

## 2. Problema que Resolve

Preparar/formartar mídia USB é uma operação crítica e pode apagar dados caso o usuário selecione o alvo errado.

---

## 3. Escopo Funcional

- Reforçar FAT32 /wbfs como recomendado.
- Exigir frase FORMATAR WBFS para WBFS.
- Bloquear discos do sistema.
- Validar alvo por F107.
- Usar comandos catalogados por F121.
- Validar pós-formatação.
- Executar readiness check após preparar.
- Gerar relatório exportável.

---

## 4. Fora de Escopo

- Não recuperar dados.
- Não fazer particionamento avançado.
- Não executar formatação silenciosa.
- Não executar em testes automatizados reais.

---

## 5. Fluxo de Usuário Esperado

1. Selecionar dispositivo.
2. Escolher FAT32 /wbfs ou WBFS.
3. Ver preview.
4. WBFS exige FORMATAR WBFS.
5. Validar permissão.
6. Executar operação.
7. Validar resultado.
8. Rodar readiness check.

---

## 6. Regras de Negócio

- FAT32 /wbfs é recomendado.
- WBFS é avançado/destrutivo.
- Destino desconhecido é bloqueado.
- Alvo montado deve ser bloqueado ou desmontado por operação catalogada.
- Sem rollback automático para formatação.

---

## 7. Regras de Segurança

- Nunca formatar sem confirmação forte.
- Nunca armazenar senha.
- Nunca usar comando livre.
- Nunca permitir plugin iniciar formatação.
- Sempre validar wwt.
- Sempre usar F105/F106.

---

## 8. Integrações Necessárias

- F105
- F106
- F107
- F109
- F110
- F113
- F120
- F121

---

## 9. Impactos na UI

- Wizard de preparação USB.
- Tela de alternativa FAT32 recomendada.
- Tela de WBFS avançado com aviso de perda total.
- Campo de frase FORMATAR WBFS.
- Resultado pós-diagnóstico.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IUsbFormatPreparationService
- UsbFormatPreviewDto
- UsbFormatExecutionRequestDto
- UsbPreparationMode

---

## 11. Persistência e Configurações

- UsbPreparation.LastMode
- UsbPreparation.RecommendFat32
- UsbPreparation.BlockInternalDisks

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

- FAT32 /wbfs oferecido como recomendado.
- WBFS exige frase.
- Disco do sistema bloqueado.
- Comando vem de F121.
- Validação pós-operação executa.
- Relatório exportável.

---

## 14. Cenários de Teste

- Preparar FAT32.
- Formatar WBFS com frase correta.
- Frase incorreta bloqueia.
- Alvo mudou após confirmação.
- Wiimms ausente.
- Policy ausente.
- Validação pós-formatação falha.

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Sem rollback | `Esta operação não possui rollback automático.` |
| Frase incorreta | `Digite FORMATAR WBFS para continuar.` |
| Alvo montado | `O dispositivo está montado ou em uso. Desmonte/ejetar antes de formatar.` |
| Validação falhou | `A operação terminou, mas o WiiStudio não conseguiu validar o resultado.` |

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

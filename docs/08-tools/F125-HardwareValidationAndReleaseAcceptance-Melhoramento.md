# F125 - Validação com hardware real [Melhoramento]

> **Suggested path:** `docs/08-tools/F125-HardwareValidationAndReleaseAcceptance-Melhoramento.md`  
> **Priority:** Baixa  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Definir homologação final em hardware real para garantir que o WiiStudio está pronto para uso real no Wii.

---

## 2. Problema que Resolve

Testes com fakes não provam compatibilidade real com HD, pendrive, loaders e Wii físico.

---

## 3. Escopo Funcional

- HD WBFS real
- FAT32 /wbfs
- Pendrive
- Wii real
- Windows/Linux/macOS
- Permissões administrativas
- Relatório de aceite

---

## 4. Fora de Escopo

- Automatizar formatação real em CI
- Substituir testes unitários

---

## 5. Fluxo de Usuário Esperado

1. Preparar matriz de hardware.
2. Executar cenários.
3. Coletar evidências.
4. Aprovar/reprovar release.

---

## 6. Regras de Negócio

- Dispositivos de teste não devem conter dados importantes.
- Release final exige evidência.
- Falhas críticas bloqueiam release.

---

## 7. Regras de Segurança

- Operações destrutivas só em hardware dedicado.
- Registrar logs.
- Não ignorar permissões.
- Não marcar pronto sem Wii real.

---

## 8. Integrações Necessárias

- F120
- F124
- F110
- F110-1
- F109

---

## 9. Impactos na UI

- Não afeta UI diretamente; usa relatórios e readiness check.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- ReleaseAcceptanceReport
- HardwareValidationChecklist

---

## 11. Persistência e Configurações

- release-validation/acceptance-report.md
- release-validation/logs/

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

- HD WBFS validado.
- FAT32 /wbfs validado.
- Pendrive validado.
- Wii real validado.
- SO principais validados.

---

## 14. Cenários de Teste

- cenários manuais controlados
- smoke tests
- readiness check
- cópia e execução no Wii

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Falha Wii | `O jogo não foi reconhecido pelo Wii/loader. Release bloqueada até investigação.` |
| Falha SO | `Falha de compatibilidade na plataforma testada. Registrar evidência e bloquear release se crítico.` |

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

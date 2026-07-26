# F115 - Painel avançado de detalhes [Melhoramento]

> **Suggested path:** `docs/08-tools/F115-AdvancedGameDetailsPanel-Melhoramento.md`  
> **Priority:** Média  
> **Status:** Implemented and Homologated  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Transformar o painel do jogo em uma área completa com assets, metadados, dados técnicos, integridade, status no USB e histórico.

---

## 2. Problema que Resolve

A visão atual não centraliza todas as informações necessárias para entender e operar sobre um jogo.

---

## 3. Escopo Funcional

- Capa frente/verso/disco.
- Metadados completos.
- ID6, região, formato, tamanho, caminho.
- Integridade.
- Status no USB.
- Histórico de operações.
- Ações contextuais.

---

## 4. Fora de Escopo

- Editor avançado de metadados
- Edição de imagem
- Execução direta de comando na UI

---

## 5. Fluxo de Usuário Esperado

1. Selecionar jogo.
2. Painel carrega detalhes.
3. Usuário visualiza assets/metadados.
4. Usuário executa ação contextual.

---

## 6. Regras de Negócio

- Dados ausentes não quebram painel.
- Ações indisponíveis mostram motivo.
- Metadados do usuário têm prioridade.
- Painel atualiza após sync/download.

---

## 7. Regras de Segurança

- UI não monta path.
- Ações USB seguem F110.
- Sem Process.Start direto.
- Logs redigidos.

---

## 8. Integrações Necessárias

- F102
- F103
- F104
- F110
- F114
- F116

---

## 9. Impactos na UI

- Layout lateral/inferior
- Capa grande
- Galeria
- Seções técnicas
- Histórico
- Botões contextuais

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IGameDetailsService
- GameDetailsDto
- GameActionDto
- AdvancedGameDetailsViewModel

---

## 11. Persistência e Configurações

- Carrega de SQLite F114 quando ativo

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

- Exibe metadados completos.
- Exibe assets.
- Exibe dados técnicos.
- Mostra status USB/histórico.
- Placeholders funcionam.

---

## 14. Cenários de Teste

- jogo completo
- sem metadata
- sem assets
- arquivo ausente
- ação indisponível
- atualização após download

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Sem seleção | `Selecione um jogo para ver detalhes.` |
| Sem metadados | `Metadados ainda não sincronizados.` |
| Arquivo ausente | `O arquivo deste jogo não foi encontrado no caminho original.` |

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

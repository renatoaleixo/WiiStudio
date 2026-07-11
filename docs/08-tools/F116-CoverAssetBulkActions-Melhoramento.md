# F116 - Ações em lote de capas/assets [Melhoramento]

> **Suggested path:** `docs/08-tools/F116-CoverAssetBulkActions-Melhoramento.md`  
> **Priority:** Média  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Permitir baixar, substituir, remover, limpar cache e revalidar assets de vários jogos ao mesmo tempo.

---

## 2. Problema que Resolve

Gerenciar capas jogo por jogo é lento e não resolve bem cache corrompido, idioma errado ou assets ausentes em massa.

---

## 3. Escopo Funcional

- Baixar ausentes.
- Forçar refresh.
- Limpar cache.
- Trocar idioma.
- Escolher tipos de assets.
- Validar cache.
- Resumo em lote.

---

## 4. Fora de Escopo

- Edição de imagem
- Upload para GameTDB
- IA para gerar capas

---

## 5. Fluxo de Usuário Esperado

1. Selecionar jogos.
2. Abrir ações de assets.
3. Escolher tipos/opções.
4. Executar lote.
5. UI atualiza.

---

## 6. Regras de Negócio

- Limpar cache exige confirmação.
- Asset ausente não é erro fatal.
- Falha parcial não para o lote.
- ForceRefresh ignora cache.

---

## 7. Regras de Segurança

- Apagar somente dentro do cache.
- Validar GameId/AssetKind.
- Bloquear traversal.
- Validar imagem.

---

## 8. Integrações Necessárias

- F102
- F104
- F113
- F114
- F115

---

## 9. Impactos na UI

- Dialog de bulk action
- Tipos de assets
- Idioma
- Force refresh
- Limpar antes
- Resumo

---

## 10. Serviços, Classes e Interfaces Sugeridas

- ICoverAssetBulkActionService
- CoverAssetBulkActionRequestDto
- CoverAssetBulkActionResultDto

---

## 11. Persistência e Configurações

- Assets.LastSelectedKinds
- Assets.LastBulkActionLanguage
- GameAssets via F114

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

- Tipos selecionáveis.
- Limpeza confirmada.
- Troca idioma.
- Contadores corretos.
- UI atualiza.

---

## 14. Cenários de Teste

- baixar front cover
- force refresh
- limpar cache
- provider offline
- imagem inválida
- cancelamento

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Sem seleção | `Selecione um ou mais jogos para executar ações de assets.` |
| Sem tipo | `Selecione pelo menos um tipo de asset.` |
| Provider offline | `Não foi possível acessar o provedor de capas agora.` |

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

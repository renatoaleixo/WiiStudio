# F114 - Persistência SQLite da biblioteca [Melhoramento]

> **Suggested path:** `docs/08-tools/F114-LibraryPersistenceWithSQLite-Melhoramento.md`  
> **Priority:** Média  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Tornar a biblioteca mais rápida, confiável e profissional com persistência estruturada em SQLite.

---

## 2. Problema que Resolve

Stores JSON/memória dificultam busca, histórico, migração, performance e consistência dos dados.

---

## 3. Escopo Funcional

- Schema SQLite.
- Migrations.
- Repositories.
- Persistir jogos, metadados, assets, favoritos e histórico.
- Migrar JSON legado com backup.
- Diagnóstico do banco.

---

## 4. Fora de Escopo

- Banco remoto
- Cloud sync
- Multiusuário
- Criptografia total

---

## 5. Fluxo de Usuário Esperado

1. App inicia.
2. Cria/abre SQLite.
3. Executa migrations.
4. Migra JSON se necessário.
5. Library usa repositories.

---

## 6. Regras de Negócio

- UI nunca acessa SQLite direto.
- Toda alteração de schema exige migration.
- Importação em lote usa transação.
- JSON legado não é apagado sem backup.

---

## 7. Regras de Segurança

- Validar paths.
- Backup antes de migration.
- Logs redigidos.
- Não armazenar senha.

---

## 8. Integrações Necessárias

- F002
- F032
- F102
- F103
- F104
- F113
- F115

---

## 9. Impactos na UI

- Estados migrando
- Erro de banco
- Busca mais rápida
- Filtro/paginação

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IGameLibraryRepository
- IGameMetadataRepository
- IGameAssetRepository
- ILibraryUnitOfWork

---

## 11. Persistência e Configurações

- database/wiistudio.db
- Games
- GameMetadata
- GameAssets
- LibraryFiles
- ImportHistory

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

- Dados persistem após reinício.
- Migrations funcionam.
- Busca por título/ID funciona.
- Assets/metadados persistem.
- Tests com SQLite temp.

---

## 14. Cenários de Teste

- criar banco
- migration
- upsert game
- rollback transaction
- migração JSON
- banco corrompido

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| DB corrompido | `O banco da biblioteca parece estar corrompido. O WiiStudio tentará usar backup.` |
| Migration | `Não foi possível atualizar a estrutura da biblioteca. O backup foi preservado.` |

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

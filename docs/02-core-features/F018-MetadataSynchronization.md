# F018 - Metadata Synchronization

> Feature ID: F018
> Status: Ready for Development
> Priority: High
> Epic: Metadata & Assets

# Objetivo

Sincronizar automaticamente os metadados da biblioteca do WiiStudio com os
Providers configurados, mantendo todas as informações dos jogos atualizadas
sem necessidade de intervenção manual.

---

# Requisitos Funcionais

## RF001

Sincronizar metadados de um jogo.

## RF002

Sincronizar toda a biblioteca.

## RF003

Atualizar apenas registros desatualizados.

## RF004

Permitir atualização forçada.

## RF005

Permitir múltiplos Providers.

## RF006

Executar sincronização automática em segundo plano.

---

# Metadados Sincronizados

- Título
- Subtítulo
- Desenvolvedor
- Publicadora
- Ano
- Região
- Idiomas
- Gênero
- Número de jogadores
- Classificação indicativa
- Descrição
- Recursos online
- Compatibilidade
- GameTDB Version

---

# Fluxo do Usuário

```text
Selecionar jogo(s)
      │
Sincronizar
      │
Consultar Cache
      │
Consultar Provider
      │
Comparar Dados
      │
Atualizar Banco
      │
Atualizar Interface
```

---

# Fluxo Técnico

```text
MetadataViewModel
        │
IMetadataSynchronizationService
        │
MetadataCache
        │
MetadataProvider
        │
SQLite
        │
Refresh Library
```

---

# Máquina de Estados

- Idle
- CheckingCache
- QueryingProvider
- Comparing
- Updating
- Refreshing
- Completed
- Failed
- Cancelled

---

# Estratégia de Atualização

Prioridade:

1. Cache Local válido
2. Provider Principal
3. Provider Secundário
4. Fallback

---

# Política de Atualização

Atualizar somente quando:

- versão mais recente
- campo vazio
- atualização forçada
- cache expirado

Nunca sobrescrever dados personalizados do usuário.

---

# Contrato

```csharp
public interface IMetadataSynchronizationService
{
    Task<MetadataSyncPreview> AnalyzeAsync(
        IReadOnlyList<string> gameIds,
        CancellationToken cancellationToken);

    Task<MetadataSyncResult> SynchronizeAsync(
        MetadataSyncRequest request,
        CancellationToken cancellationToken);
}
```

---

# Eventos

- MetadataSyncStarted
- MetadataCompared
- MetadataUpdated
- MetadataSyncCompleted
- MetadataSyncFailed

---

# Interface

Exibir:

- Jogos analisados
- Jogos atualizados
- Campos modificados
- Provider utilizado
- Tempo total
- Barra de progresso

---

# Casos de Erro

- Provider indisponível
- Sem conexão
- Cache inválido
- Banco indisponível
- Cancelamento

Todos devem registrar logs estruturados.

---

# Critérios de Aceite

- [ ] Sincronização individual.
- [ ] Sincronização em lote.
- [ ] Atualização incremental.
- [ ] Preservação de dados personalizados.
- [ ] Suporte a múltiplos Providers.

---

# Testes

## Unitários

- Cache válido.
- Cache expirado.
- Atualização.
- Comparação.
- Cancelamento.

## Integração

- Biblioteca completa.
- Provider indisponível.
- Atualização parcial.
- Atualização completa.
- Grande volume (>5000 jogos).

---

# Dependências

- 014-GameTDB.md
- 022-Localization.md
- F013-GameDetails.md
- F017-DownloadCovers.md

---

# Definition of Done

- Serviço implementado.
- Sincronização validada.
- Testes aprovados.
- Logs estruturados.
- Documentação atualizada.

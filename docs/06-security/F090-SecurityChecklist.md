# F090 - Security Checklist

> Feature ID: F090
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o checklist oficial de segurança do WiiStudio para ser usado durante desenvolvimento, revisão de código, implementação de features, integração de plugins, execução de processos externos, manipulação de arquivos, backups, updates e logs.

Este checklist deve ser aplicado antes de considerar qualquer feature sensível como concluída.

---

# Checklist Geral

- [x] Entrada externa validada.
- [x] Paths normalizados.
- [x] Path traversal bloqueado.
- [x] Permissões verificadas.
- [x] Operações destrutivas confirmadas.
- [x] Logs redigidos.
- [x] Exceptions não vazam segredos.
- [x] CancellationToken utilizado em operação longa.
- [x] Falha segura implementada.
- [x] Testes de erro implementados.

---

# File System

- [x] Leitura valida existência.
- [x] Escrita valida destino.
- [x] Espaço em disco verificado quando necessário.
- [x] Extensão validada.
- [x] Escrita temporária usada para arquivo importante.
- [x] Sobrescrita exige política.
- [x] Exclusão exige confirmação.
- [x] Plugin não acessa path fora do escopo.
- [x] Auditoria registrada para falhas de permissão.

---

# Safe Path

- [x] Path normalizado.
- [x] Path relativo validado contra root.
- [x] Path absoluto permitido somente quando necessário.
- [x] `..` e variações bloqueadas.
- [x] Symlink tratado.
- [x] Paths de manifesto não escapam da pasta.
- [x] Paths multiplataforma testados.
- [x] Nenhuma concatenação insegura.

---

# External Process

- [x] Usa `IProcessRunner`.
- [x] Não usa `Process.Start` direto.
- [x] Não usa shell arbitrário.
- [x] Executável validado.
- [x] Argumentos passados como lista.
- [x] Timeout definido.
- [x] CancellationToken suportado.
- [x] stdout/stderr capturados.
- [x] Logs redigidos.
- [x] Processos longos integrados ao Task Manager.

---

# Plugins

- [x] Manifesto validado.
- [x] SDK compatível.
- [x] Permissões conhecidas.
- [x] Permissões aprovadas.
- [x] Contexto isolado.
- [x] Storage isolado.
- [x] Exceções capturadas.
- [x] Plugin pode ser desativado.
- [x] Plugin inseguro vai para quarentena.
- [x] Logs contêm PluginId.
- [x] Capacidades removidas no unload.

---

# Sensitive Settings

- [x] Setting classificada.
- [x] Secret não aparece em claro na UI.
- [x] Secret não aparece em log.
- [x] Secret não é exportada por padrão.
- [x] Importação valida formato.
- [x] Plugin não acessa secret externo.
- [x] Auditoria não registra valor.
- [x] Fallback de armazenamento documentado.

---

# Logs

- [x] Redaction aplicada.
- [x] Authorization redigido.
- [x] Tokens redigidos.
- [x] Connection strings redigidas.
- [x] Exceptions redigidas.
- [x] stdout/stderr redigidos.
- [x] Exportação redigida.
- [x] Logs mantêm utilidade diagnóstica.

---

# Backups

- [x] Destino validado.
- [x] Espaço verificado.
- [x] Manifesto gerado.
- [x] Hash calculado.
- [x] Integridade validada.
- [x] Restauração destrutiva confirmada.
- [x] Safety backup criado quando possível.
- [x] Path traversal bloqueado.
- [x] Secrets tratados.
- [x] Rollback previsto.

---

# Updates

- [x] Origem validada.
- [x] Metadata validada.
- [x] Checksum validado.
- [x] Assinatura futura considerada.
- [x] Changelog exibido.
- [x] Compatibilidade verificada.
- [x] Downgrade bloqueado.
- [x] Rollback previsto.
- [x] Auditoria registrada.
- [x] Novas permissões de plugin exibidas.

---

# UI e Acessibilidade Segura

- [x] Ações destrutivas confirmadas.
- [x] Mensagens não revelam segredo.
- [x] Erros técnicos ficam em detalhes seguros.
- [x] Notificações não vazam dados.
- [x] Dialogs críticos são acessíveis.
- [x] Foco não fica preso indevidamente.
- [x] Operações bloqueantes têm cancelamento.

---

# Critérios de Bloqueio

A feature não pode ser considerada pronta se:

- usa `Process.Start` direto;
- permite path traversal;
- registra token em log;
- plugin acessa recurso sem permissão;
- operação destrutiva não confirma;
- update não valida checksum;
- backup restaura path fora do escopo;
- secret é exportado sem confirmação;
- erro de segurança não possui teste.

---

# Evidências Esperadas

Para features sensíveis, anexar:

- testes unitários;
- testes de integração;
- logs redigidos de exemplo;
- cenários de falha;
- validação de path;
- validação de permissão;
- evidência de redaction;
- checklist preenchido no PR.

---

# Regras para o Codex

- Sempre consultar este checklist em features sensíveis.
- Sempre adicionar testes para falhas de segurança.
- Sempre bloquear PR se item crítico falhar.
- Sempre registrar justificativa se uma regra não se aplica.
- Nunca considerar feature sensível pronta sem checklist.
- Nunca ignorar redaction, path safety ou permissões.

---

# Definition of Done

- Checklist aplicado.
- Itens críticos aprovados.
- Testes de segurança implementados.
- Evidências anexadas.
- Documentação atualizada.

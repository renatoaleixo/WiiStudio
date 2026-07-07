# F090 - Security Checklist

> Feature ID: F090
> Status: Ready for Development
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o checklist oficial de segurança do WiiStudio para ser usado durante desenvolvimento, revisão de código, implementação de features, integração de plugins, execução de processos externos, manipulação de arquivos, backups, updates e logs.

Este checklist deve ser aplicado antes de considerar qualquer feature sensível como concluída.

---

# Checklist Geral

- [ ] Entrada externa validada.
- [ ] Paths normalizados.
- [ ] Path traversal bloqueado.
- [ ] Permissões verificadas.
- [ ] Operações destrutivas confirmadas.
- [ ] Logs redigidos.
- [ ] Exceptions não vazam segredos.
- [ ] CancellationToken utilizado em operação longa.
- [ ] Falha segura implementada.
- [ ] Testes de erro implementados.

---

# File System

- [ ] Leitura valida existência.
- [ ] Escrita valida destino.
- [ ] Espaço em disco verificado quando necessário.
- [ ] Extensão validada.
- [ ] Escrita temporária usada para arquivo importante.
- [ ] Sobrescrita exige política.
- [ ] Exclusão exige confirmação.
- [ ] Plugin não acessa path fora do escopo.
- [ ] Auditoria registrada para falhas de permissão.

---

# Safe Path

- [ ] Path normalizado.
- [ ] Path relativo validado contra root.
- [ ] Path absoluto permitido somente quando necessário.
- [ ] `..` e variações bloqueadas.
- [ ] Symlink tratado.
- [ ] Paths de manifesto não escapam da pasta.
- [ ] Paths multiplataforma testados.
- [ ] Nenhuma concatenação insegura.

---

# External Process

- [ ] Usa `IProcessRunner`.
- [ ] Não usa `Process.Start` direto.
- [ ] Não usa shell arbitrário.
- [ ] Executável validado.
- [ ] Argumentos passados como lista.
- [ ] Timeout definido.
- [ ] CancellationToken suportado.
- [ ] stdout/stderr capturados.
- [ ] Logs redigidos.
- [ ] Processos longos integrados ao Task Manager.

---

# Plugins

- [ ] Manifesto validado.
- [ ] SDK compatível.
- [ ] Permissões conhecidas.
- [ ] Permissões aprovadas.
- [ ] Contexto isolado.
- [ ] Storage isolado.
- [ ] Exceções capturadas.
- [ ] Plugin pode ser desativado.
- [ ] Plugin inseguro vai para quarentena.
- [ ] Logs contêm PluginId.
- [ ] Capacidades removidas no unload.

---

# Sensitive Settings

- [ ] Setting classificada.
- [ ] Secret não aparece em claro na UI.
- [ ] Secret não aparece em log.
- [ ] Secret não é exportada por padrão.
- [ ] Importação valida formato.
- [ ] Plugin não acessa secret externo.
- [ ] Auditoria não registra valor.
- [ ] Fallback de armazenamento documentado.

---

# Logs

- [ ] Redaction aplicada.
- [ ] Authorization redigido.
- [ ] Tokens redigidos.
- [ ] Connection strings redigidas.
- [ ] Exceptions redigidas.
- [ ] stdout/stderr redigidos.
- [ ] Exportação redigida.
- [ ] Logs mantêm utilidade diagnóstica.

---

# Backups

- [ ] Destino validado.
- [ ] Espaço verificado.
- [ ] Manifesto gerado.
- [ ] Hash calculado.
- [ ] Integridade validada.
- [ ] Restauração destrutiva confirmada.
- [ ] Safety backup criado quando possível.
- [ ] Path traversal bloqueado.
- [ ] Secrets tratados.
- [ ] Rollback previsto.

---

# Updates

- [ ] Origem validada.
- [ ] Metadata validada.
- [ ] Checksum validado.
- [ ] Assinatura futura considerada.
- [ ] Changelog exibido.
- [ ] Compatibilidade verificada.
- [ ] Downgrade bloqueado.
- [ ] Rollback previsto.
- [ ] Auditoria registrada.
- [ ] Novas permissões de plugin exibidas.

---

# UI e Acessibilidade Segura

- [ ] Ações destrutivas confirmadas.
- [ ] Mensagens não revelam segredo.
- [ ] Erros técnicos ficam em detalhes seguros.
- [ ] Notificações não vazam dados.
- [ ] Dialogs críticos são acessíveis.
- [ ] Foco não fica preso indevidamente.
- [ ] Operações bloqueantes têm cancelamento.

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

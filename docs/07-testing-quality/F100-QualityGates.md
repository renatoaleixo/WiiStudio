# F100 - Quality Gates

> Feature ID: F100
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir os Quality Gates oficiais do WiiStudio, estabelecendo critérios mínimos
para aceitar código, features, plugins, correções, integrações e releases.

Quality Gates devem impedir que código inseguro, instável, não testado ou fora da
arquitetura avance no projeto.

---

# Requisitos Funcionais

## RF001
Definir critérios mínimos para pull requests.

## RF002
Definir critérios para features.

## RF003
Definir critérios para segurança.

## RF004
Definir critérios para testes.

## RF005
Definir critérios para UI.

## RF006
Definir critérios para plugins.

## RF007
Definir critérios para banco.

## RF008
Definir critérios para processos externos.

## RF009
Definir critérios para documentação.

## RF010
Definir bloqueios obrigatórios.

---

# Quality Gate de Pull Request

Um PR deve atender:

- build passa;
- testes passam;
- sem warnings críticos;
- arquitetura respeitada;
- documentação atualizada;
- feature tem Definition of Done;
- segurança revisada quando aplicável;
- logs não vazam segredos;
- sem código morto óbvio;
- sem TODO crítico sem issue.

---

# Quality Gate de Feature

Toda feature deve ter:

- requisitos implementados;
- estados Loading/Empty/Error quando aplicável;
- testes unitários;
- testes de integração quando aplicável;
- acessibilidade básica;
- localization;
- logging;
- tratamento de erro;
- CancellationToken em operação longa;
- documentação atualizada.

---

# Quality Gate de Segurança

Bloquear se:

- há `Process.Start` direto fora do ProcessRunner;
- há path traversal possível;
- há secret em log;
- plugin acessa recurso sem permissão;
- ação destrutiva não confirma;
- update ignora checksum;
- backup restaura fora do escopo;
- settings sensíveis exportam segredo sem confirmação.

---

# Quality Gate de Testes

Obrigatório:

- teste de sucesso;
- teste de erro;
- teste de cancelamento quando async;
- teste de permissão quando sensível;
- teste de regressão para bug corrigido;
- testes determinísticos;
- sem dependência de ambiente real.

---

# Quality Gate de UI

Obrigatório:

- Light/Dark;
- foco visível;
- navegação por teclado;
- textos localizáveis;
- responsividade mínima;
- estados de erro;
- sem lógica de negócio na View;
- sem valores hardcoded críticos.

---

# Quality Gate de Plugins

Obrigatório:

- manifesto validado;
- permissões verificadas;
- falha segura;
- logs com PluginId;
- unload limpa capabilities;
- plugin sem permissão é bloqueado;
- diagnóstico disponível.

---

# Quality Gate de Banco

Obrigatório:

- migration testada;
- rollback quando crítico;
- constraints preservadas;
- queries críticas testadas;
- dados inválidos tratados;
- backup/restore compatível quando afetado.

---

# Quality Gate de Processos Externos

Obrigatório:

- uso de ProcessRunner;
- argumentos estruturados;
- timeout;
- cancellation token;
- stdout/stderr capturados;
- redaction;
- sem shell arbitrário;
- integração com Task Manager quando longo.

---

# Quality Gate de Documentação

Obrigatório atualizar:

- documento da feature;
- índice quando necessário;
- changelog quando aplicável;
- README quando fluxo muda;
- notas de segurança quando aplicável;
- instruções para Codex quando regra arquitetural muda.

---

# Bloqueios Automáticos

Bloquear merge se:

- build falha;
- testes críticos falham;
- secret detectado;
- checklist de segurança crítico falha;
- migration falha;
- plugin security falha;
- lint/analyzer crítico falha.

---

# Bloqueios Manuais

Exigir revisão quando:

- altera segurança;
- altera ProcessRunner;
- altera Plugin SDK;
- altera migrations;
- altera backup/restore;
- altera update;
- altera dados sensíveis;
- altera arquitetura base.

---

# Regras para o Codex

- Sempre aplicar Quality Gates antes de concluir feature.
- Sempre criar testes junto com implementação.
- Sempre atualizar documentação.
- Sempre verificar segurança.
- Sempre respeitar Clean Architecture.
- Sempre usar DI e abstrações.
- Nunca considerar pronto com teste crítico falhando.
- Nunca ignorar checklist de segurança.
- Nunca deixar UI sem estados de erro.
- Nunca implementar atalho fora do command registry.

---

# Critérios de Aceite

- [x] Gate de PR definido.
- [x] Gate de feature definido.
- [x] Gate de segurança definido.
- [x] Gate de testes definido.
- [x] Gate de UI definido.
- [x] Gate de plugins definido.
- [x] Gate de banco definido.
- [x] Gate de processos externos definido.
- [x] Gate de documentação definido.
- [x] Bloqueios automáticos definidos.
- [x] Bloqueios manuais definidos.

---

# Definition of Done

- Quality Gates documentado.
- Critérios mínimos definidos.
- Bloqueios definidos.
- Regras para Codex definidas.
- Documentação atualizada.

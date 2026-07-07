# DEFINITION_OF_DONE

# Objetivo

Definir a Definition of Done oficial do WiiStudio para features, correções, plugins,
documentação, segurança, UI, testes e releases.

Nenhuma entrega deve ser considerada concluída apenas porque “compila”. Uma entrega
só está pronta quando atende requisitos funcionais, arquiteturais, segurança,
testes, documentação e qualidade mínima.

---

# Definition of Done Geral

Uma entrega está pronta quando:

- requisitos implementados;
- arquitetura respeitada;
- build passando;
- testes criados e passando;
- erros tratados;
- logs seguros;
- documentação atualizada;
- UI com estados adequados quando aplicável;
- segurança validada quando aplicável;
- Quality Gates atendidos.

---

# DoD para Features

Toda feature deve ter:

- documento da feature lido;
- dependências verificadas;
- implementação feita na camada correta;
- casos de sucesso implementados;
- casos de erro implementados;
- validações necessárias;
- logs estruturados;
- testes unitários;
- testes de integração quando aplicável;
- documentação atualizada.

---

# DoD para UI

Toda alteração de UI deve ter:

- ViewModel testável;
- View sem regra de negócio;
- bindings claros;
- estado Loading quando aplicável;
- estado Empty quando aplicável;
- estado Error quando aplicável;
- textos localizáveis;
- suporte Light/Dark;
- tokens do Design System;
- navegação por teclado;
- foco visível;
- responsividade mínima.

---

# DoD para Application Layer

A camada Application deve:

- expor casos de uso claros;
- depender de abstrações;
- não depender de Infrastructure concreta;
- aceitar CancellationToken em operações longas;
- tratar erros esperados;
- retornar resultados previsíveis;
- ser coberta por testes unitários.

---

# DoD para Domain

A camada Domain deve:

- conter regras puras;
- proteger invariantes;
- evitar dependências externas;
- ter modelos claros;
- possuir testes de regras importantes;
- não conhecer UI, banco, arquivos ou processos.

---

# DoD para Infrastructure

A camada Infrastructure deve:

- implementar interfaces da Application;
- encapsular SQLite, filesystem, tools e providers;
- usar ProcessRunner para processos externos;
- validar paths;
- redigir logs;
- tratar exceptions externas;
- possuir testes de integração quando aplicável.

---

# DoD para Processos Externos

Qualquer processo externo deve:

- usar `IProcessRunner`;
- validar executável;
- validar argumentos;
- não usar shell arbitrário;
- aplicar timeout;
- aceitar CancellationToken;
- capturar stdout/stderr;
- redigir logs;
- integrar com Task Manager quando longo;
- possuir testes com fake runner.

Proibido considerar pronto se existir `Process.Start` direto fora da infraestrutura aprovada.

---

# DoD para File System

Operações com arquivos devem:

- normalizar paths;
- bloquear path traversal;
- validar extensão;
- validar existência;
- validar permissões;
- validar destino;
- usar escrita temporária quando necessário;
- confirmar sobrescrita/exclusão quando destrutivo;
- tratar arquivo em uso;
- possuir mensagens amigáveis.

---

# DoD para Segurança

Features sensíveis devem passar pelo `F090-SecurityChecklist.md`.

Obrigatório validar:

- entrada externa;
- paths;
- permissões;
- logs;
- secrets;
- plugins;
- processos externos;
- ações destrutivas;
- backup/update quando aplicável.

---

# DoD para Plugins

Alterações no Plugin SDK devem garantir:

- manifesto validado;
- permissões explícitas;
- discovery seguro;
- loading seguro;
- contexto isolado;
- falha segura;
- logs com PluginId;
- diagnostics;
- unload limpa capabilities;
- plugin inválido não derruba aplicação.

---

# DoD para Banco

Alterações no banco devem ter:

- migration testada;
- schema validado;
- constraints preservadas;
- rollback quando operação crítica;
- repository testado;
- dados inválidos tratados;
- backup/restore considerado quando aplicável.

---

# DoD para Logs

Logs devem:

- ser estruturados;
- possuir contexto suficiente;
- não conter secrets;
- redigir tokens, senhas e headers;
- conter PluginId quando aplicável;
- conter correlationId quando útil;
- ser úteis para diagnóstico.

---

# DoD para Testes

Toda entrega deve conter:

- teste de sucesso;
- teste de erro;
- teste de cancelamento quando async;
- teste de permissão quando sensível;
- teste de regressão para bug corrigido;
- fakes para processos externos;
- diretórios temporários para arquivos;
- banco temporário para integração.

---

# DoD para Documentação

Atualizar documentação quando:

- nova feature;
- mudança arquitetural;
- novo contrato;
- nova regra de segurança;
- novo plugin type;
- migration;
- fluxo de UI;
- alteração em comando externo;
- bug crítico corrigido.

---

# DoD para Release

Antes de release:

- build limpo;
- testes críticos passando;
- regressões passando;
- security checklist aplicado;
- logs revisados;
- documentação principal atualizada;
- roadmap atualizado;
- package validado;
- release notes preparadas;
- rollback considerado.

---

# Bloqueios

Não está pronto se:

- existe segredo em log;
- existe path traversal possível;
- existe `Process.Start` direto;
- plugin acessa recurso sem permissão;
- UI tem regra de negócio;
- operação destrutiva não confirma;
- migration não foi testada;
- teste crítico falha;
- documentação obrigatória não foi atualizada;
- Quality Gates falham.

---

# Checklist Final

Antes de concluir qualquer tarefa:

- [ ] Requisitos atendidos.
- [ ] Arquitetura respeitada.
- [ ] Testes criados.
- [ ] Erros tratados.
- [ ] CancellationToken usado quando necessário.
- [ ] Segurança validada.
- [ ] Logs redigidos.
- [ ] UI acessível quando aplicável.
- [ ] Documentação atualizada.
- [ ] Quality Gates aprovados.

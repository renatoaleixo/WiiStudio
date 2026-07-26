# F081 - Security Foundation

> Feature ID: F081
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir a fundação oficial de segurança do WiiStudio, estabelecendo princípios,
limites, práticas obrigatórias e responsabilidades para proteger arquivos, processos,
plugins, configurações, logs e operações sensíveis.

A segurança deve ser aplicada desde a arquitetura, evitando soluções improvisadas
em pontos isolados do código.

---

# Requisitos Funcionais

## RF001
Definir princípios oficiais de segurança.

## RF002
Definir áreas sensíveis da aplicação.

## RF003
Definir práticas obrigatórias para paths.

## RF004
Definir práticas obrigatórias para processos externos.

## RF005
Definir práticas obrigatórias para plugins.

## RF006
Definir proteção para settings sensíveis.

## RF007
Definir redação de logs.

## RF008
Definir checklist de segurança.

## RF009
Integrar segurança ao desenvolvimento de features.

## RF010
Evitar permissões implícitas.

---

# Princípios

- Menor privilégio.
- Falha segura.
- Validação antes da execução.
- Nenhuma confiança implícita em entrada externa.
- Paths sempre normalizados.
- Processos externos sempre controlados.
- Plugins sempre validados.
- Logs nunca devem expor segredos.
- Operações destrutivas exigem confirmação.
- Segurança deve ser testável.

---

# Áreas Sensíveis

- Sistema de arquivos.
- Execução de processos externos.
- Plugins.
- Configurações sensíveis.
- Logs.
- Backups.
- Atualizações.
- Permissões.
- Paths de ferramentas externas.
- Dados da biblioteca.

---

# Entrada Externa

Considerar como entrada externa:

- arquivos selecionados pelo usuário;
- paths digitados;
- manifestos de plugins;
- respostas de providers;
- arquivos de configuração;
- argumentos de processo;
- dados importados;
- backups restaurados;
- pacotes de atualização.

Toda entrada externa deve ser validada.

---

# Path Security

Regras:

- normalizar paths;
- bloquear path traversal;
- validar existência;
- validar extensão quando aplicável;
- validar permissões;
- não confiar em path vindo de plugin;
- não concatenar path manualmente quando houver API segura.

Detalhes em `F086-SafePathHandling.md`.

---

# External Process Security

Regras:

- usar ProcessRunner;
- nunca usar shell arbitrário;
- validar executável;
- sanitizar argumentos;
- aplicar timeout;
- capturar logs;
- redigir segredos;
- respeitar CancellationToken.

Detalhes em `F083-ExternalProcessSecurity.md`.

---

# Plugin Security

Regras:

- validar manifesto;
- validar permissões;
- isolar contexto;
- capturar exceções;
- bloquear plugins incompatíveis;
- permitir desativação;
- auditar operações sensíveis.

Detalhes em `F084-PluginSecurity.md`.

---

# Sensitive Settings

Configurações sensíveis podem incluir:

- tokens;
- chaves de API;
- paths privados;
- preferências críticas;
- dados de provider externo.

Devem ser protegidas conforme `F085-SensitiveSettingsProtection.md`.

---

# Logs

Logs devem:

- evitar dados sensíveis;
- redigir tokens;
- redigir senhas;
- limitar paths quando necessário;
- possuir correlação;
- registrar erros sem vazar segredo.

Detalhes em `F088-LogRedaction.md`.

---

# Backups

Backups devem:

- validar destino;
- evitar sobrescrita silenciosa;
- registrar versão;
- permitir restauração segura;
- validar integridade;
- proteger dados sensíveis.

Detalhes em `F087-BackupSecurity.md`.

---

# Atualizações

Atualizações devem considerar:

- fonte confiável;
- verificação de integridade;
- assinatura futura;
- rollback;
- changelog;
- permissões de escrita.

Detalhes em `F089-UpdateSecurity.md`.

---

# Operações Destrutivas

Exigem confirmação:

- remover jogo;
- apagar cache;
- remover plugin;
- restaurar backup sobrescrevendo dados;
- cancelar tarefas em lote;
- excluir logs;
- sobrescrever arquivos.

---

# Permission Model

Toda operação sensível deve passar por permissão explícita ou política definida.

Não permitir:

- permissão implícita;
- plugin usando recurso não declarado;
- execução externa sem permissão;
- escrita fora do escopo.

---

# Regras para o Codex

- Sempre validar entrada externa.
- Sempre usar Safe Path Handling.
- Sempre usar ProcessRunner.
- Sempre usar permissões para plugins.
- Sempre redigir logs sensíveis.
- Sempre confirmar ações destrutivas.
- Sempre tratar falha de forma segura.
- Sempre usar CancellationToken em operações longas.
- Nunca usar shell arbitrário.
- Nunca confiar em manifesto sem validação.
- Nunca salvar segredo em texto claro sem política.
- Nunca vazar token em log.

---

# Casos de Erro

- Path traversal.
- Permissão insuficiente.
- Processo externo inválido.
- Plugin malformado.
- Manifesto inseguro.
- Configuração sensível exposta.
- Log com segredo.
- Backup corrompido.
- Atualização inválida.
- Operação destrutiva sem confirmação.

---

# Critérios de Aceite

- [x] Princípios de segurança definidos.
- [x] Áreas sensíveis mapeadas.
- [x] Entrada externa definida.
- [x] Path Security referenciada.
- [x] External Process Security referenciada.
- [x] Plugin Security referenciada.
- [x] Sensitive Settings referenciada.
- [x] Log Redaction referenciada.
- [x] Backup Security referenciada.
- [x] Update Security referenciada.
- [x] Regras para Codex definidas.

---

# Testes

## Unitários

- Entrada inválida bloqueada.
- Path traversal bloqueado.
- Processo externo inválido bloqueado.
- Plugin sem permissão bloqueado.
- Log redige segredo.
- Operação destrutiva exige confirmação.

## Integração

- Importação com path inválido.
- Execução Wiimms com argumentos seguros.
- Plugin com manifesto inválido.
- Backup com destino inválido.
- Restauração com confirmação.
- Atualização inválida bloqueada.

---

# Dependências

- F075-PluginPermissions.md
- F083-ExternalProcessSecurity.md
- F084-PluginSecurity.md
- F085-SensitiveSettingsProtection.md
- F086-SafePathHandling.md
- F087-BackupSecurity.md
- F088-LogRedaction.md
- F089-UpdateSecurity.md
- F090-SecurityChecklist.md

---

# Definition of Done

- Security Foundation documentada.
- Princípios definidos.
- Áreas sensíveis mapeadas.
- Regras obrigatórias estabelecidas.
- Testes definidos.
- Documentação atualizada.

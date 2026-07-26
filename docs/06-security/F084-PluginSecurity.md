# F084 - Plugin Security

> Feature ID: F084
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir as regras oficiais de segurança para plugins do WiiStudio, garantindo que plugins sejam descobertos, validados, carregados, executados, diagnosticados e desativados de forma controlada.

Plugins são extensões externas e devem ser tratados como código não confiável até que passem pelas validações definidas.

---

# Requisitos Funcionais

## RF001
Validar manifesto antes de carregar plugin.

## RF002
Validar compatibilidade de versão.

## RF003
Validar permissões declaradas.

## RF004
Isolar contexto de plugin.

## RF005
Bloquear acesso a serviços internos não expostos.

## RF006
Auditar operações sensíveis.

## RF007
Capturar exceções de plugin.

## RF008
Desativar plugin problemático.

## RF009
Impedir plugin de derrubar a aplicação principal.

## RF010
Integrar com Plugin Diagnostics.

---

# Princípios

- plugin não é confiável por padrão;
- nenhuma permissão é implícita;
- manifesto é obrigatório;
- permissões devem ser explícitas;
- falhas devem ser isoladas;
- logs devem conter PluginId;
- operações sensíveis devem ser auditadas;
- plugin pode ser bloqueado ou colocado em quarentena.

---

# Áreas de Risco

- execução de processos externos;
- acesso ao sistema de arquivos;
- acesso à rede;
- alteração da biblioteca;
- alteração de metadados;
- extensão de UI;
- tarefas em segundo plano;
- logs com dados sensíveis;
- carregamento de assemblies.

---

# Fluxo Seguro

```text
Discover Plugin
    ↓
Validate Manifest
    ↓
Validate Compatibility
    ↓
Validate Permissions
    ↓
Check Blocklist/Quarantine
    ↓
Create Isolated Context
    ↓
Load Plugin
    ↓
Monitor Runtime
    ↓
Unload Safely
```

---

# Manifesto

Manifesto inválido bloqueia plugin.

Validações:

- ID válido;
- versão válida;
- entry assembly seguro;
- entry type válido;
- paths relativos;
- SDK compatível;
- permissões conhecidas;
- capacidades conhecidas.

---

# Permissões

Permissões devem seguir `F075-PluginPermissions.md`.

Regras:

- permissão desconhecida bloqueia plugin;
- uso de permissão não declarada é negado;
- permissão crítica exige aprovação;
- revogação deve impedir novas operações;
- abuso pode desativar plugin.

---

# Isolamento

O contexto do plugin deve fornecer apenas recursos permitidos.

Plugins não devem acessar:

- container global completo;
- serviços internos não expostos;
- caminhos internos sem permissão;
- outros plugins;
- settings sensíveis;
- APIs de segurança internas.

---

# Storage Isolado

Cada plugin deve possuir storage próprio.

Regras:

- path validado;
- sem acesso ao storage de outro plugin;
- sem path traversal;
- limpeza segura;
- backup opcional conforme política.

---

# Runtime Protection

Durante execução:

- capturar exceções;
- aplicar timeout em operações críticas;
- usar CancellationToken;
- auditar operações sensíveis;
- registrar métricas;
- bloquear chamadas sem permissão.

---

# Quarentena

Plugin pode ir para quarentena quando:

- manifesto é malicioso;
- path inseguro;
- falhas repetidas;
- tentativa de acesso indevido;
- execução externa não autorizada;
- plugin derruba capacidade crítica;
- assinatura/checksum futuro inválido.

---

# Desativação Automática

O sistema pode desativar plugin quando:

- falha na inicialização repetidamente;
- causa exceções críticas;
- viola permissões;
- trava UI;
- gera operações inseguras;
- excede política de erro.

---

# Integração com UI Extensions

Extensões de UI devem:

- respeitar regiões permitidas;
- não substituir App Shell;
- respeitar acessibilidade;
- não bloquear UI;
- ser removidas no unload;
- falhar isoladamente.

---

# Integração com Process Security

Plugin só executa processo externo com:

- permissão `execute-process`;
- ProcessRunner;
- validação de executável;
- argumentos seguros;
- logs auditáveis.

---

# Integração com File System

Plugin só acessa arquivo com:

- permissão apropriada;
- escopo permitido;
- safe path handling;
- auditoria;
- bloqueio de path traversal.

---

# Logging

Logs de plugin devem:

- conter PluginId;
- conter correlationId quando possível;
- redigir segredos;
- separar erro técnico de mensagem amigável;
- alimentar Plugin Diagnostics.

---

# Regras para o Codex

- Nunca confiar em plugin sem manifesto validado.
- Nunca conceder permissão implícita.
- Sempre criar contexto isolado.
- Sempre bloquear acesso não autorizado.
- Sempre capturar exceções de plugin.
- Sempre registrar PluginId.
- Sempre permitir desativar plugin.
- Sempre remover capacidades no unload.
- Sempre colocar plugin inseguro em quarentena.
- Nunca expor serviços internos por acidente.
- Nunca permitir plugin substituir App Shell.
- Nunca permitir processo externo fora do ProcessRunner.

---

# Casos de Erro

- Manifesto inválido.
- SDK incompatível.
- Permissão desconhecida.
- Permissão negada.
- Plugin tenta acessar path proibido.
- Plugin tenta executar processo.
- Plugin lança exceção.
- Plugin trava inicialização.
- Plugin registra capacidade indevida.
- Plugin UI falha.
- Plugin tenta acessar serviço interno.

---

# Critérios de Aceite

- [x] Modelo de segurança de plugins definido.
- [x] Manifesto obrigatório.
- [x] Permissões obrigatórias.
- [x] Isolamento definido.
- [x] Quarentena definida.
- [x] Desativação automática definida.
- [x] Runtime protection definida.
- [x] Storage isolado definido.
- [x] Integração com Process Security definida.
- [x] Integração com File System definida.
- [x] Logging seguro definido.

---

# Testes

## Unitários

- Plugin sem manifesto bloqueado.
- Plugin sem permissão bloqueado.
- Plugin com permissão desconhecida bloqueado.
- Plugin tenta path traversal.
- Plugin lança exceção.
- Plugin desativado após falha.
- Plugin em quarentena não carrega.
- Logs contêm PluginId.

## Integração

- Plugin válido carrega.
- Plugin inseguro bloqueado.
- Plugin UI falha sem derrubar tela.
- Plugin tenta executar processo sem permissão.
- Plugin tenta acessar arquivo fora do escopo.
- Plugin unload remove capacidades.
- Plugin Diagnostics mostra falhas.

---

# Dependências

- F071-PluginSDKFoundation.md
- F072-PluginManifest.md
- F073-PluginDiscovery.md
- F074-PluginLoading.md
- F075-PluginPermissions.md
- F076-PluginUIExtensions.md
- F079-PluginDiagnostics.md
- F081-SecurityFoundation.md
- F082-FileSystemPermissions.md
- F083-ExternalProcessSecurity.md
- F086-SafePathHandling.md
- F088-LogRedaction.md

---

# Definition of Done

- Plugin Security documentado.
- Regras de isolamento definidas.
- Quarentena definida.
- Desativação automática definida.
- Integrações de segurança definidas.
- Testes definidos.
- Documentação atualizada.

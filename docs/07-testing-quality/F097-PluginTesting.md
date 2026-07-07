# F097 - Plugin Testing

> Feature ID: F097
> Status: Ready for Development
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes para o sistema de plugins do WiiStudio, cobrindo
manifesto, discovery, loading, permissions, security, diagnostics, UI extensions,
metadata providers, conversion providers e falha segura.

Plugins são código externo e devem ser testados com rigor para evitar instabilidade,
vazamento de dados e falhas na aplicação principal.

---

# Requisitos Funcionais

## RF001
Testar manifesto de plugin.

## RF002
Testar discovery.

## RF003
Testar loading.

## RF004
Testar permissions.

## RF005
Testar plugin security.

## RF006
Testar diagnostics.

## RF007
Testar unload.

## RF008
Testar UI extensions.

## RF009
Testar providers.

## RF010
Garantir que plugin com falha não derrube a aplicação.

---

# Tipos de Plugin de Teste

Criar plugins fake para:

- plugin válido mínimo;
- plugin com manifesto inválido;
- plugin incompatível;
- plugin sem permissão;
- plugin que falha no initialize;
- plugin que falha em runtime;
- metadata provider fake;
- conversion provider fake;
- UI extension fake;
- diagnostics provider fake.

---

# Organização

```text
tests/
└── WiiStudio.Plugin.Tests/
    ├── Manifests/
    ├── Discovery/
    ├── Loading/
    ├── Permissions/
    ├── Security/
    ├── Diagnostics/
    ├── TestPlugins/
    └── TestData/
```

---

# Manifest Tests

Testar:

- manifesto válido;
- campo obrigatório ausente;
- versão inválida;
- ID inválido;
- path inseguro;
- permissão desconhecida;
- capacidade desconhecida;
- SDK incompatível.

---

# Discovery Tests

Testar:

- plugin encontrado;
- manifesto ausente;
- manifesto inválido;
- plugin duplicado;
- plugin disabled;
- plugin em quarantine;
- platform incompatível.

---

# Loading Tests

Testar:

- carregar plugin válido;
- entry assembly ausente;
- entry type inválido;
- construtor falha;
- initialize falha;
- timeout;
- capabilities registradas;
- capabilities removidas no unload.

---

# Permission Tests

Testar:

- permissão concedida;
- permissão negada;
- permissão desconhecida;
- uso de permissão não declarada;
- revogação;
- auditoria;
- risk level.

---

# Security Tests

Testar:

- plugin tenta path traversal;
- plugin tenta executar processo sem permissão;
- plugin tenta acessar storage de outro plugin;
- plugin tenta registrar UI em região proibida;
- plugin tenta capability não declarada;
- plugin inseguro vai para quarantine.

---

# Diagnostics Tests

Testar:

- evento de discovery;
- falha de manifesto;
- permissão negada;
- loading falho;
- runtime exception;
- relatório exportado;
- logs com PluginId;
- secrets redigidos.

---

# UI Extension Tests

Testar:

- dashboard card registrado;
- settings section registrada;
- diagnostics panel registrado;
- região proibida bloqueada;
- unload remove extensão;
- falha de renderização isolada;
- Light/Dark básico;
- acessibilidade básica.

---

# Metadata Provider Tests

Testar:

- busca por GameId;
- busca por título;
- retorno vazio;
- erro de provider;
- timeout;
- fallback;
- cache;
- preservação de dados customizados.

---

# Conversion Provider Tests

Testar:

- formato suportado;
- formato rejeitado;
- validação;
- progresso;
- cancelamento;
- falha segura;
- ProcessRunner fake;
- arquivo original preservado.

---

# Test Plugin SDK

Criar projeto de apoio:

```text
tests/
└── WiiStudio.Plugin.TestKit/
    ├── FakePluginBuilder.cs
    ├── FakePluginContext.cs
    ├── FakePermissionService.cs
    ├── FakePluginStorage.cs
    └── TestPluginFactory.cs
```

---

# Regras para o Codex

- Sempre testar plugin válido e inválido.
- Sempre testar permissões.
- Sempre testar falha segura.
- Sempre testar unload.
- Sempre testar diagnostics.
- Sempre usar plugins fake controlados.
- Sempre impedir acesso real ao sistema do usuário.
- Sempre redigir logs de plugin.
- Nunca carregar plugin externo real em teste automatizado obrigatório.
- Nunca permitir plugin de teste acessar path fora do temp root.

---

# Casos de Erro

- Manifesto malformado.
- Plugin duplicado.
- EntryType inválido.
- Initialize lança exceção.
- Plugin trava.
- Permissão negada.
- Path traversal.
- Capability indevida.
- UI extension falha.
- Diagnostics não registra erro.
- Unload não limpa recursos.

---

# Critérios de Aceite

- [ ] Test plugins definidos.
- [ ] Manifest tests definidos.
- [ ] Discovery tests definidos.
- [ ] Loading tests definidos.
- [ ] Permission tests definidos.
- [ ] Security tests definidos.
- [ ] Diagnostics tests definidos.
- [ ] UI extension tests definidos.
- [ ] Provider tests definidos.
- [ ] Falha segura testada.
- [ ] Unload testado.

---

# Definition of Done

- Plugin Testing documentado.
- TestKit previsto.
- Cenários críticos definidos.
- Segurança coberta.
- Documentação atualizada.

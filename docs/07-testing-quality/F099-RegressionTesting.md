# F099 - Regression Testing

> Feature ID: F099
> Status: Implemented and Homologated
> Priority: Critical
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes de regressão do WiiStudio, garantindo que bugs
corrigidos, fluxos críticos e contratos importantes não voltem a falhar durante a
evolução do projeto.

Toda correção de bug relevante deve gerar teste de regressão.

---

# Requisitos Funcionais

## RF001
Definir quando criar teste de regressão.

## RF002
Definir organização de regressões.

## RF003
Garantir cobertura de bugs críticos.

## RF004
Garantir cobertura de fluxos principais.

## RF005
Garantir regressão para segurança.

## RF006
Garantir regressão para banco e migrations.

## RF007
Garantir regressão para plugins.

## RF008
Garantir regressão para ProcessRunner.

## RF009
Integrar regressões ao pipeline.

## RF010
Documentar bugs corrigidos.

---

# Quando Criar Regressão

Criar teste de regressão quando:

- bug foi corrigido;
- erro ocorreu em produção;
- erro corrompe dados;
- erro envolve segurança;
- erro envolve path/processo/plugin;
- erro envolve migration;
- erro envolve importação/conversão;
- erro envolve perda de dados;
- erro envolve UI crítica.

---

# Organização

```text
tests/
└── WiiStudio.Regression.Tests/
    ├── Bugs/
    ├── Security/
    ├── Database/
    ├── Plugins/
    ├── ProcessRunner/
    ├── ImportExport/
    └── UI/
```

---

# Nomenclatura

Formato recomendado:

```text
Bug_1234_ShouldNotReproduce_WhenCondition
```

Exemplo:

```csharp
Bug_0042_ShouldPreserveOriginalFile_WhenConversionFails()
```

---

# Metadados da Regressão

Cada regressão deve documentar:

- ID do bug;
- resumo;
- causa;
- comportamento esperado;
- link para issue/card quando houver;
- data da correção;
- área impactada.

---

# Áreas Críticas

Regressão obrigatória para bugs em:

- File System;
- ProcessRunner;
- Plugins;
- Security;
- Backup/Restore;
- Database;
- Import/Export;
- Conversion;
- Metadata Sync;
- Settings;
- UI Shell.

---

# Segurança

Todo bug de segurança deve ter regressão.

Exemplos:

- path traversal;
- secret em log;
- plugin sem permissão;
- processo externo inseguro;
- update sem checksum;
- backup com zip slip.

---

# Banco

Criar regressão para:

- migration quebrada;
- perda de dados;
- query errada;
- duplicidade;
- FK inválida;
- rollback ausente;
- schema divergente.

---

# Plugins

Criar regressão para:

- plugin inválido carregando;
- falha derrubando aplicação;
- permissão ignorada;
- unload não limpa capability;
- UI extension quebrando tela;
- manifesto inseguro aceito.

---

# UI

Criar regressão para:

- tela em branco;
- erro não exibido;
- dialog sem foco;
- botão crítico oculto;
- tema quebrado;
- responsividade quebrada;
- command disabled indevido.

---

# Pipeline

Regressões críticas devem rodar no pipeline padrão.

Regressões pesadas podem ser marcadas como:

```text
LongRunning
Performance
Manual
ExternalTool
```

---

# Regras para o Codex

- Sempre criar regressão ao corrigir bug.
- Sempre nomear teste com referência do bug.
- Sempre testar comportamento que falhou.
- Sempre cobrir bug de segurança.
- Sempre evitar teste frágil.
- Sempre documentar causa.
- Nunca corrigir bug crítico sem teste.
- Nunca remover regressão sem justificativa.
- Nunca depender de ambiente real.

---

# Casos de Erro

- Bug corrigido sem teste.
- Regressão testa detalhe errado.
- Teste frágil.
- Teste depende de ordem.
- Regressão removida sem motivo.
- Bug de segurança sem cobertura.
- Pipeline ignora regressão crítica.

---

# Critérios de Aceite

- [x] Quando criar regressão definido.
- [x] Organização definida.
- [x] Nomenclatura definida.
- [x] Metadados definidos.
- [x] Segurança coberta.
- [x] Banco coberto.
- [x] Plugins cobertos.
- [x] UI coberta.
- [x] Pipeline considerado.
- [x] Regras para Codex definidas.

---

# Definition of Done

- Regression Testing documentado.
- Regras de criação definidas.
- Áreas críticas mapeadas.
- Pipeline considerado.
- Documentação atualizada.

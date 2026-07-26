# F098 - Performance Testing

> Feature ID: F098
> Status: Implemented and Homologated
> Priority: High
> Epic: Testing & Quality

# Objetivo

Definir o padrão oficial de testes de performance do WiiStudio, cobrindo biblioteca grande,
scan de arquivos, importação, conversão, banco SQLite, cache, UI, Task Manager, logs,
plugins e operações longas.

O WiiStudio deve continuar responsivo mesmo em cenários com muitos jogos, arquivos
grandes, múltiplas tarefas e plugins ativos.

---

# Requisitos Funcionais

## RF001
Definir cenários oficiais de performance.

## RF002
Medir tempo de operações críticas.

## RF003
Medir consumo de memória quando aplicável.

## RF004
Garantir que a UI não seja bloqueada.

## RF005
Testar biblioteca grande.

## RF006
Testar operações em lote.

## RF007
Testar banco SQLite com volume.

## RF008
Testar logs e diagnósticos grandes.

## RF009
Testar plugins com impacto controlado.

## RF010
Registrar baseline de performance.

---

# Cenários Críticos

## Biblioteca Grande

Cenários:

- 100 jogos;
- 500 jogos;
- 1.000 jogos;
- 5.000 jogos quando aplicável.

Validar:

- tempo de carregamento;
- busca;
- filtros;
- ordenação;
- seleção;
- renderização da lista.

## Importação

Cenários:

- importar 1 arquivo;
- importar pasta com vários arquivos;
- arquivos inválidos misturados;
- duplicados;
- cancelamento.

## Conversão

Cenários:

- conversão única;
- conversão em lote;
- falha parcial;
- cancelamento;
- progresso.

## Banco

Cenários:

- queries frequentes;
- filtros;
- paginação;
- índices;
- inserts em lote;
- updates em lote.

## Logs

Cenários:

- 1.000 linhas;
- 10.000 linhas;
- filtros;
- exportação;
- redaction.

---

# Métricas

Medir:

- tempo total;
- tempo médio;
- percentil 95 quando aplicável;
- memória alocada;
- uso de CPU quando possível;
- quantidade de queries;
- tempo de renderização;
- tempo de resposta de comandos;
- tempo de cancelamento.

---

# Baselines

Criar baseline inicial para:

```text
LoadLibrary_1000Games
SearchLibrary_1000Games
ImportFolder_100Files
LoadLogs_10000Lines
PluginDiscovery_50Plugins
DatabaseQuery_LibraryList
```

Os números exatos devem ser ajustados após medições reais.

---

# UI Responsiva

Durante operação longa:

- UI não deve congelar;
- progresso deve ser exibido;
- cancelamento deve funcionar;
- Status Bar deve atualizar;
- Task Manager deve refletir estado;
- usuário deve poder navegar quando seguro.

---

# Ferramentas

Podem ser usadas:

- BenchmarkDotNet para microbenchmarks;
- xUnit para testes de performance controlados;
- medidores internos simples;
- logs de duração;
- diagnósticos da aplicação.

Benchmarks não precisam rodar em todo build padrão.

---

# Organização Recomendada

```text
tests/
└── WiiStudio.Performance.Tests/
    ├── Library/
    ├── Database/
    ├── Import/
    ├── Conversion/
    ├── Plugins/
    ├── Logs/
    └── Benchmarks/
```

---

# Dados de Teste

Gerar dados sintéticos.

Regras:

- evitar commitar arquivos grandes;
- usar generators;
- manter seeds determinísticas;
- criar dados temporários;
- limpar ao final.

---

# Performance de Plugins

Testar:

- discovery com muitos plugins;
- plugin lento;
- plugin que falha;
- timeout;
- unload;
- provider lento;
- UI extension pesada.

Plugin não deve degradar a aplicação principal sem controle.

---

# Performance de Banco

Testar:

- índices necessários;
- queries de biblioteca;
- filtros;
- busca textual;
- histórico de tarefas;
- logs;
- metadados;
- assets.

---

# Regras para o Codex

- Sempre considerar performance em operações longas.
- Sempre usar async/await.
- Sempre aceitar CancellationToken.
- Sempre evitar bloquear UI thread.
- Sempre medir queries críticas.
- Sempre criar dados sintéticos para volume.
- Sempre registrar baseline quando criar teste de performance.
- Nunca depender de dados reais do usuário.
- Nunca rodar benchmark pesado no pipeline padrão sem marcação.
- Nunca ignorar regressão grande de performance.

---

# Casos de Erro

- UI congela.
- Query lenta.
- Memória cresce sem controle.
- Plugin trava discovery.
- Logs grandes travam UI.
- Conversão em lote não cancela.
- Busca demora demais.
- Teste de performance instável.

---

# Critérios de Aceite

- [x] Cenários críticos definidos.
- [x] Métricas definidas.
- [x] Baselines previstos.
- [x] Biblioteca grande coberta.
- [x] Banco coberto.
- [x] Logs cobertos.
- [x] Plugins cobertos.
- [x] UI responsiva considerada.
- [x] Dados sintéticos definidos.
- [x] Regras para Codex definidas.

---

# Definition of Done

- Performance Testing documentado.
- Cenários e métricas definidos.
- Baselines previstos.
- Testes críticos mapeados.
- Documentação atualizada.

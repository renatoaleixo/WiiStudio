# Testing Quality Reference

Implementação inicial dos documentos `F091` até `F100`.

## Test Kit

Projeto criado:

- `tests/WiiStudio.Testing`

Recursos disponíveis:

- `TemporaryDirectoryFixture`
- `IntegrationTestFixture`
- `DatabaseTestFixture`
- `FakeProcessExecutor`
- `RegressionCaseAttribute`
- `PerformanceBaselineRegistry`
- `PluginTestManifestBuilder`
- `QualityGateEvaluator`

## ProcessRunner

O `ProcessRunner` agora usa `IProcessExecutor`.

Isso permite:

- unit tests com fake executor;
- testes de stdout/stderr;
- testes de exit code;
- testes de redaction;
- testes de cancelamento;
- validação de executável antes de executar;
- separação clara entre validação do runner e processo real.

## Quality Gates

O `QualityGateEvaluator` cobre bloqueios de:

- build falho;
- testes falhos;
- warnings críticos;
- documentação ausente;
- vazamento de segredo;
- `Process.Start` direto;
- path traversal;
- revisão de segurança ausente;
- regressão sem teste;
- plugin sem validação de segurança;
- performance sem aceite.

## Performance e Regressão

- `PerformanceBaselineRegistry` registra orçamento por cenário e detecta regressão.
- `RegressionCaseAttribute` marca testes criados a partir de bugs corrigidos.

## Escopo Futuro

- Projeto dedicado `WiiStudio.UI.Tests`.
- Projeto dedicado `WiiStudio.Performance.Tests` para cenários opcionais mais pesados.
- Integração CI com cobertura e relatórios.

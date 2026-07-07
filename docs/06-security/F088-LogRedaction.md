# F088 - Log Redaction

> Feature ID: F088
> Status: Ready for Development
> Priority: Critical
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial de redação, mascaramento e proteção de dados sensíveis em logs do WiiStudio.

Logs são fundamentais para diagnóstico, mas nunca devem expor tokens, senhas, chaves, credenciais, caminhos sensíveis, dados pessoais ou informações que possam comprometer o usuário, plugins ou integrações externas.

---

# Requisitos Funcionais

## RF001
Definir dados que devem ser redigidos.

## RF002
Padronizar redaction em logs de aplicação, plugins, processos externos e diagnósticos.

## RF003
Impedir vazamento de secrets em exceptions.

## RF004
Aplicar redaction em stdout/stderr de processos externos.

## RF005
Aplicar redaction em relatórios exportados.

## RF006
Integrar com Logging Center.

## RF007
Integrar com Plugin Diagnostics.

## RF008
Permitir testes automatizados de redaction.

## RF009
Manter logs úteis para diagnóstico mesmo após redaction.

## RF010
Nunca registrar segredo em texto claro.

---

# Dados Sensíveis

Devem ser redigidos:

- tokens;
- senhas;
- chaves de API;
- secrets de plugins;
- headers `Authorization`;
- connection strings;
- URLs com credenciais;
- cookies;
- paths pessoais quando necessário;
- argumentos de processo com segredo;
- payloads de erro contendo credenciais;
- conteúdo de arquivos sensíveis.

---

# Formatos de Redaction

## Máscara Total

```text
[REDACTED]
```

## Máscara Parcial

```text
abc********xyz
```

## Hash Seguro para Correlação

```text
sha256:ab12cd...
```

Usar hash apenas quando for útil para correlação e não revelar o valor original.

---

# Exemplos

## Antes

```text
Authorization: Bearer eyJhbGciOiJIUzI1...
```

## Depois

```text
Authorization: Bearer [REDACTED]
```

## Antes

```text
apiKey=1234567890abcdef
```

## Depois

```text
apiKey=[REDACTED]
```

---

# Redaction Service

```csharp
public interface ILogRedactionService
{
    string Redact(string input);

    IReadOnlyDictionary<string, string> Redact(
        IReadOnlyDictionary<string, string> values);

    bool ContainsSensitiveData(string input);
}
```

---

# Regras de Detecção

Detectar padrões como:

- `password=`
- `pwd=`
- `token=`
- `api_key=`
- `apikey=`
- `secret=`
- `authorization:`
- `bearer `
- `connectionString=`
- `SharedAccessSignature`
- `sig=`
- `access_token=`
- `refresh_token=`

A lista deve ser extensível.

---

# Logs Estruturados

Preferir logs estruturados.

Exemplo correto:

```csharp
_logger.Information(
    "Plugin {PluginId} failed with reason {Reason}",
    pluginId,
    redactedReason);
```

Evitar interpolação direta com dados sensíveis.

---

# Exceptions

Exceptions podem conter dados sensíveis.

Regras:

- redigir `Exception.Message` antes de exibir ao usuário;
- redigir detalhes técnicos exportados;
- manter stack trace quando seguro;
- não adicionar secrets em mensagens de exception customizadas.

---

# Processos Externos

stdout e stderr devem passar por redaction antes de:

- salvar em log;
- exibir em UI;
- exportar relatório;
- enviar para diagnóstico.

---

# Plugins

Logs de plugins devem:

- sempre passar por redaction;
- conter `PluginId`;
- bloquear segredo de plugin;
- impedir plugin de gravar log fora do sistema oficial quando possível.

---

# Exportação de Logs

Ao exportar logs:

- aplicar redaction novamente;
- informar ao usuário que dados sensíveis foram mascarados;
- evitar incluir secrets;
- permitir revisão antes de compartilhar quando possível.

---

# Regras para o Codex

- Nunca logar secrets.
- Sempre usar `ILogRedactionService`.
- Sempre redigir stdout/stderr.
- Sempre redigir logs de plugins.
- Sempre redigir relatórios exportados.
- Sempre evitar interpolação direta de dados sensíveis.
- Sempre tratar exception message como potencialmente sensível.
- Nunca registrar connection string completa.
- Nunca registrar token completo.
- Nunca registrar header Authorization completo.

---

# Casos de Erro

- Token aparece em log.
- Exception contém secret.
- Plugin escreve segredo.
- Processo externo retorna credencial no stderr.
- Exportação contém path sensível.
- Redaction remove informação demais.
- Padrão sensível não detectado.
- Log estruturado recebe campo sensível sem máscara.

---

# Critérios de Aceite

- [ ] Dados sensíveis definidos.
- [ ] Serviço de redaction especificado.
- [ ] Padrões de detecção definidos.
- [ ] Exceptions tratadas.
- [ ] stdout/stderr tratados.
- [ ] Plugins tratados.
- [ ] Exportação de logs tratada.
- [ ] Testes de redaction definidos.
- [ ] Regras para Codex definidas.

---

# Testes

## Unitários

- Redigir token.
- Redigir senha.
- Redigir Authorization header.
- Redigir connection string.
- Redigir URL com token.
- Detectar secret em exception.
- Preservar mensagem útil.
- Não alterar texto comum.

## Integração

- ProcessRunner redige stderr.
- Plugin Diagnostics redige segredo.
- Logging Center exibe log redigido.
- Exportação de logs redigida.
- Relatório de erro não contém secrets.

---

# Dependências

- F029-LoggingCenter.md
- F030-AdvancedDiagnostics.md
- F079-PluginDiagnostics.md
- F081-SecurityFoundation.md
- F083-ExternalProcessSecurity.md
- F085-SensitiveSettingsProtection.md
- F090-SecurityChecklist.md

---

# Definition of Done

- Log Redaction documentado.
- Serviço especificado.
- Regras de detecção definidas.
- Integrações definidas.
- Testes definidos.
- Documentação atualizada.

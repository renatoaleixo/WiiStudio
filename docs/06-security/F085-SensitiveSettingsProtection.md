# F085 - Sensitive Settings Protection

> Feature ID: F085
> Status: Implemented and Homologated
> Priority: High
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial para proteção de configurações sensíveis no WiiStudio, incluindo tokens, chaves de API, paths privados, credenciais futuras, preferências críticas, configurações de plugins e dados que não devem ser expostos em logs, UI indevida ou arquivos exportados.

---

# Requisitos Funcionais

## RF001
Identificar configurações sensíveis.

## RF002
Evitar armazenamento inseguro de segredos.

## RF003
Redigir dados sensíveis em logs.

## RF004
Proteger exibição em UI.

## RF005
Controlar exportação/importação de settings.

## RF006
Isolar settings sensíveis de plugins.

## RF007
Permitir limpeza/revogação de segredos.

## RF008
Registrar auditoria quando configuração sensível for alterada.

## RF009
Integrar com Log Redaction.

## RF010
Garantir mensagens amigáveis e seguras.

---

# Dados Sensíveis

Exemplos:

- tokens de API;
- chaves de provider;
- senhas futuras;
- credenciais;
- URLs com tokens;
- paths pessoais sensíveis;
- configurações de assinatura;
- secrets de plugins;
- dados de autenticação futura.

---

# Classificação

```csharp
public enum SettingSensitivity
{
    Public,
    Internal,
    Sensitive,
    Secret
}
```

## Public

Pode ser exibido e exportado.

## Internal

Uso interno, não necessariamente secreto.

## Sensitive

Deve ser exibido com cuidado e redigido em logs.

## Secret

Nunca deve aparecer em claro após salvo.

---

# Modelo Conceitual

```csharp
public sealed class SettingDefinition
{
    public string Key { get; init; } = string.Empty;
    public SettingSensitivity Sensitivity { get; init; }
    public bool Exportable { get; init; }
    public bool RequiresRestart { get; init; }
}
```

---

# Armazenamento

Regras:

- segredos não devem ser salvos em texto claro quando houver mecanismo seguro;
- quando não houver cofre disponível, informar limitação;
- separar settings comuns de secrets;
- aplicar permissões de arquivo restritas quando possível;
- evitar duplicação de segredos.

---

# Proteção por Plataforma

Considerar suporte futuro a:

- Windows Credential Manager;
- macOS Keychain;
- Linux Secret Service;
- arquivo protegido como fallback.

A primeira versão pode implementar fallback seguro documentado.

---

# UI

Campos sensíveis devem:

- mascarar valor;
- permitir revelar temporariamente quando seguro;
- ter botão copiar controlado;
- evitar preencher logs;
- avisar quando campo é sensível;
- limpar clipboard quando aplicável futuramente.

---

# Exportação de Settings

Ao exportar configurações:

- omitir secrets por padrão;
- permitir incluir secrets apenas com confirmação explícita;
- redigir valores sensíveis;
- registrar auditoria;
- informar usuário.

---

# Importação de Settings

Ao importar:

- validar origem;
- avisar sobre secrets ausentes;
- não sobrescrever secrets sem confirmação;
- validar formato;
- rejeitar dados malformados.

---

# Plugins

Plugins devem ter storage próprio para secrets.

Regras:

- plugin não acessa secrets de outro plugin;
- plugin não acessa secrets core;
- secrets de plugin seguem mesma política;
- logs de plugin devem ser redigidos;
- permissões podem ser exigidas.

---

# Logs

Nunca logar:

- valor de token;
- valor de senha;
- chave completa;
- URL com credencial;
- header Authorization;
- conteúdo de arquivo sensível.

Usar `F088-LogRedaction.md`.

---

# Auditoria

Registrar alterações sem expor valor:

- chave alterada;
- origem da alteração;
- pluginId quando aplicável;
- data/hora;
- usuário/perfil quando houver;
- operação: create/update/delete.

---

# Regras para o Codex

- Nunca logar segredo.
- Nunca exibir secret em claro por padrão.
- Sempre classificar setting sensível.
- Sempre redigir valor em exportações comuns.
- Sempre separar secrets de settings comuns quando possível.
- Sempre validar importação.
- Sempre impedir plugin de acessar secrets fora do escopo.
- Sempre registrar alteração sem valor.
- Nunca salvar token em texto claro sem política documentada.
- Nunca colocar segredo em exception message.

---

# Casos de Erro

- Secret sem classificação.
- Secret exportado indevidamente.
- Log contém token.
- Plugin acessa secret de outro plugin.
- Falha ao salvar secret.
- Falha ao carregar secret.
- Importação malformada.
- Usuário tenta revelar secret.
- Clipboard contém secret.
- Setting sensível ausente.

---

# Critérios de Aceite

- [x] Dados sensíveis definidos.
- [x] Classificação definida.
- [x] Modelo de setting definido.
- [x] Armazenamento seguro documentado.
- [x] UI para campos sensíveis definida.
- [x] Exportação segura definida.
- [x] Importação segura definida.
- [x] Regras para plugins definidas.
- [x] Logs redigidos.
- [x] Auditoria definida.

---

# Testes

## Unitários

- Setting sensível redigido.
- Secret não exportado por padrão.
- Secret exportado apenas com confirmação.
- Log redige token.
- Plugin não acessa secret externo.
- Importação inválida rejeitada.
- Auditoria sem valor.

## Integração

- Salvar token de provider.
- Exibir campo mascarado.
- Exportar settings sem secrets.
- Importar settings.
- Plugin com secret próprio.
- Logging com redaction.
- Alteração registrada.

---

# Dependências

- F023-SettingsManager.md
- F024-PreferencesAndProfiles.md
- F075-PluginPermissions.md
- F081-SecurityFoundation.md
- F088-LogRedaction.md
- F090-SecurityChecklist.md

---

# Definition of Done

- Sensitive Settings Protection documentado.
- Classificação definida.
- UI segura definida.
- Exportação/importação segura definida.
- Logs redigidos.
- Testes definidos.
- Documentação atualizada.

# F089 - Update Security

> Feature ID: F089
> Status: Implemented and Homologated
> Priority: High
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial de segurança para verificação, download, validação e aplicação de atualizações do WiiStudio e de componentes auxiliares.

Atualizações devem ser tratadas como operação sensível, pois podem alterar código, configurações, plugins e comportamento da aplicação.

---

# Requisitos Funcionais

## RF001
Validar origem da atualização.

## RF002
Validar integridade do pacote.

## RF003
Preparar suporte futuro a assinatura digital.

## RF004
Exibir changelog antes da atualização.

## RF005
Validar compatibilidade.

## RF006
Criar rollback ou ponto de segurança quando possível.

## RF007
Proteger contra downgrade inseguro.

## RF008
Registrar auditoria de atualização.

## RF009
Integrar com Notification Center.

## RF010
Não aplicar atualização silenciosa sem política explícita.

---

# Escopo

Abrange:

- atualização da aplicação;
- atualização de banco de metadados;
- atualização de GameTDB;
- atualização de plugins;
- atualização de catálogo de plugins;
- atualização futura de ferramentas externas.

---

# Princípios

- fonte confiável;
- integridade verificável;
- nenhuma execução sem validação;
- confirmação para mudanças relevantes;
- rollback quando possível;
- logs seguros;
- atualização não deve corromper dados do usuário.

---

# Fluxo Recomendado

```text
Check for Updates
    ↓
Validate Source
    ↓
Download Metadata
    ↓
Validate Metadata
    ↓
Show Changelog
    ↓
Download Package
    ↓
Validate Checksum
    ↓
Validate Signature Future
    ↓
Prepare Rollback
    ↓
Apply Update
    ↓
Validate Result
```

---

# Update Metadata

```csharp
public sealed class UpdateMetadata
{
    public string Version { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string? SignatureUrl { get; init; }
    public string Changelog { get; init; } = string.Empty;
    public bool IsCritical { get; init; }
}
```

---

# Integridade

Todo pacote deve possuir:

- SHA-256;
- tamanho esperado;
- versão;
- origem;
- data;
- assinatura futura quando disponível.

Checksum inválido deve bloquear atualização.

---

# Assinatura Futura

Preparar suporte para:

- assinatura de pacote;
- chave pública confiável;
- rotação de chave;
- revogação;
- validação offline quando possível.

---

# Downgrade

Downgrade deve ser bloqueado por padrão quando:

- versão é menor;
- banco já foi migrado;
- configurações foram atualizadas;
- há risco de incompatibilidade.

Permitir downgrade apenas com confirmação avançada e política clara.

---

# Rollback

Quando possível:

- criar backup antes de atualizar;
- preservar versão anterior;
- permitir restauração;
- validar dados após update;
- registrar falha.

---

# Plugins

Atualização de plugins deve:

- validar manifesto;
- comparar permissões novas;
- exibir novas permissões;
- preservar versão anterior;
- bloquear plugin incompatível;
- executar discovery após update.

---

# GameTDB e Metadados

Atualizações de dados devem:

- validar arquivo baixado;
- validar formato;
- aplicar em staging;
- substituir somente após sucesso;
- preservar cache anterior se falhar.

---

# Notification Center

Notificações esperadas:

- atualização disponível;
- download iniciado;
- atualização concluída;
- atualização falhou;
- atualização crítica;
- novas permissões de plugin.

---

# Auditoria

Registrar:

- versão atual;
- versão alvo;
- origem;
- checksum;
- resultado;
- falha;
- rollback;
- usuário/perfil quando aplicável;
- timestamp.

---

# Regras para o Codex

- Sempre validar origem.
- Sempre validar checksum.
- Sempre preparar assinatura futura.
- Sempre exibir changelog quando relevante.
- Sempre verificar compatibilidade.
- Sempre proteger contra downgrade inseguro.
- Sempre criar rollback quando possível.
- Sempre registrar auditoria.
- Nunca aplicar pacote sem validação.
- Nunca executar atualização baixada sem confirmação/política.
- Nunca ignorar checksum inválido.

---

# Casos de Erro

- Origem não confiável.
- Falha de rede.
- Metadata inválida.
- Checksum inválido.
- Assinatura inválida futura.
- Versão incompatível.
- Downgrade bloqueado.
- Sem espaço.
- Sem permissão.
- Falha ao aplicar.
- Rollback falha.
- Plugin exige nova permissão crítica.

---

# Critérios de Aceite

- [x] Fluxo de update definido.
- [x] Metadata de update definida.
- [x] Integridade definida.
- [x] Assinatura futura prevista.
- [x] Downgrade tratado.
- [x] Rollback previsto.
- [x] Plugins considerados.
- [x] GameTDB/metadados considerados.
- [x] Notification Center integrado.
- [x] Auditoria definida.

---

# Testes

## Unitários

- Metadata válida.
- Metadata inválida.
- Checksum correto.
- Checksum inválido.
- Downgrade bloqueado.
- Compatibilidade falha.
- Nova permissão detectada.
- Rollback planejado.

## Integração

- Verificar update mockado.
- Download mockado.
- Aplicar update de dados.
- Falha de checksum.
- Plugin update com nova permissão.
- Rollback após falha.
- Notification Center recebe status.

---

# Dependências

- F027-ApplicationUpdateManager.md
- F022-GameTDBUpdateManager.md
- F080-PluginMarketplacePreparation.md
- F081-SecurityFoundation.md
- F085-SensitiveSettingsProtection.md
- F087-BackupSecurity.md
- F088-LogRedaction.md
- F090-SecurityChecklist.md

---

# Definition of Done

- Update Security documentado.
- Integridade e origem definidas.
- Rollback previsto.
- Assinatura futura preparada.
- Testes definidos.
- Documentação atualizada.

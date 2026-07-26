# F087 - Backup Security

> Feature ID: F087
> Status: Implemented and Homologated
> Priority: High
> Epic: Security & Permissions

# Objetivo

Definir o padrão oficial de segurança para criação, armazenamento, validação e restauração de backups no WiiStudio, protegendo dados da biblioteca, configurações, metadados, cache relevante, plugins e informações sensíveis.

Backups devem ser confiáveis, verificáveis e restauráveis sem corromper o estado atual da aplicação.

---

# Requisitos Funcionais

## RF001
Validar destino de backup.

## RF002
Validar permissões de escrita.

## RF003
Validar espaço disponível.

## RF004
Gerar manifesto de backup.

## RF005
Validar integridade do backup.

## RF006
Proteger dados sensíveis.

## RF007
Confirmar restauração destrutiva.

## RF008
Permitir rollback quando possível.

## RF009
Registrar auditoria.

## RF010
Integrar com Safe Path Handling e Sensitive Settings.

---

# Escopo de Backup

Pode incluir:

- banco SQLite;
- configurações;
- perfis;
- metadados;
- favoritos;
- histórico de tarefas;
- logs opcionais;
- cache selecionado;
- plugins instalados;
- settings de plugins.

---

# Dados Sensíveis

Backups podem conter:

- tokens;
- chaves de API;
- paths pessoais;
- settings de plugins;
- dados de provider externo.

Esses dados devem seguir `F085-SensitiveSettingsProtection.md`.

---

# Manifesto de Backup

Todo backup deve conter manifesto.

```json
{
  "backupVersion": "1.0",
  "createdAt": "2026-01-01T10:00:00Z",
  "wiiStudioVersion": "1.0.0",
  "profile": "Default",
  "items": [
    {
      "path": "database/wiistudio.db",
      "sha256": "...",
      "size": 123456
    }
  ]
}
```

---

# Integridade

Cada item deve possuir:

- hash;
- tamanho;
- path relativo;
- tipo;
- data;
- versão quando aplicável.

O backup completo deve possuir checksum ou assinatura futura.

---

# Criação de Backup

Fluxo recomendado:

```text
Validate Destination
    ↓
Check Permissions
    ↓
Check Disk Space
    ↓
Create Temp Backup
    ↓
Write Manifest
    ↓
Calculate Hashes
    ↓
Validate Backup
    ↓
Move to Final Destination
```

---

# Restauração

Fluxo recomendado:

```text
Select Backup
    ↓
Validate Manifest
    ↓
Validate Hashes
    ↓
Check Compatibility
    ↓
Show Restore Summary
    ↓
Confirm
    ↓
Create Safety Backup
    ↓
Restore
    ↓
Validate Restored State
```

---

# Restauração Destrutiva

Exige confirmação clara quando sobrescrever:

- banco atual;
- configurações;
- perfis;
- plugins;
- metadados;
- settings sensíveis.

Mensagem deve explicar impacto.

---

# Rollback

Quando possível:

- criar backup de segurança antes de restaurar;
- manter estado anterior até validação;
- permitir retornar se restauração falhar;
- registrar erro e instrução.

---

# Paths Seguros

Regras:

- backup deve usar paths relativos internos;
- impedir path traversal;
- impedir restauração fora do diretório permitido;
- validar destino;
- validar temporários;
- não confiar no manifesto sem normalização.

---

# Compressão

Se houver compressão:

- validar conteúdo antes de extrair;
- impedir zip slip;
- limitar tamanho máximo;
- detectar arquivos inesperados;
- validar hashes após extração.

---

# Plugins

Backups de plugins devem:

- preservar manifesto;
- preservar settings permitidas;
- tratar secrets conforme política;
- não restaurar plugin bloqueado sem validação;
- executar discovery após restauração.

---

# Logs

Logs em backup devem ser opcionais.

Quando incluídos:

- aplicar redaction;
- evitar segredos;
- informar usuário;
- permitir exclusão.

---

# Auditoria

Registrar:

- criação de backup;
- destino sanitizado;
- resultado;
- tamanho;
- restauração;
- falha;
- usuário/perfil quando aplicável;
- correlationId.

---

# Regras para o Codex

- Sempre validar destino.
- Sempre verificar espaço.
- Sempre gerar manifesto.
- Sempre calcular hash.
- Sempre validar backup antes de concluir.
- Sempre bloquear path traversal.
- Sempre confirmar restauração destrutiva.
- Sempre criar safety backup quando possível.
- Sempre proteger settings sensíveis.
- Sempre aplicar redaction em logs.
- Nunca restaurar arquivo fora do escopo.
- Nunca confiar em manifesto sem validação.

---

# Casos de Erro

- Destino sem permissão.
- Espaço insuficiente.
- Falha ao gerar manifesto.
- Hash inválido.
- Backup corrompido.
- Versão incompatível.
- Path traversal no backup.
- Zip slip.
- Falha na restauração.
- Safety backup falha.
- Plugin restaurado inseguro.
- Secret exposto.

---

# Critérios de Aceite

- [x] Destino validado.
- [x] Espaço validado.
- [x] Manifesto definido.
- [x] Hash por item definido.
- [x] Criação segura definida.
- [x] Restauração segura definida.
- [x] Confirmação destrutiva definida.
- [x] Rollback previsto.
- [x] Paths seguros definidos.
- [x] Plugins considerados.
- [x] Logs redigidos.
- [x] Auditoria definida.

---

# Testes

## Unitários

- Backup com manifesto.
- Hash calculado.
- Hash inválido detectado.
- Destino sem permissão.
- Espaço insuficiente.
- Path traversal bloqueado.
- Zip slip bloqueado.
- Restauração exige confirmação.
- Settings sensíveis redigidas.

## Integração

- Criar backup completo.
- Restaurar backup válido.
- Restaurar backup corrompido.
- Restauração com safety backup.
- Restaurar plugins.
- Backup com logs redigidos.
- Falha de espaço.
- Falha de permissão.

---

# Dependências

- F028-BackupAndRestore.md
- F081-SecurityFoundation.md
- F082-FileSystemPermissions.md
- F085-SensitiveSettingsProtection.md
- F086-SafePathHandling.md
- F088-LogRedaction.md
- F090-SecurityChecklist.md

---

# Definition of Done

- Backup Security documentado.
- Manifesto definido.
- Integridade definida.
- Restauração segura definida.
- Rollback previsto.
- Testes definidos.
- Documentação atualizada.

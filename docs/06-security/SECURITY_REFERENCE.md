# Security Reference

Implementação inicial dos documentos `F081` até `F090`.

## Serviços

- `ISafePathService`: normalização, validação de root, extensão, leitura e escrita.
- `IFileSystemPermissionService`: avaliação de escopo, acesso e restrições para plugins.
- `ILogRedactionService`: redação de tokens, senhas, Authorization, URLs com credenciais e connection strings.
- `ISensitiveSettingsService`: classificação de settings e proteção para exibição/exportação.
- `IBackupSecurityService`: validação de destino, paths relativos internos e hashes.
- `IUpdateSecurityService`: validação de HTTPS, hash e bloqueio de downgrade.
- `ISecurityChecklistService`: checklist programático para revisão de features.
- `PluginSecurityPolicy`: avaliação de permissões de plugin integrada ao SDK.

## Integrações

- `ProcessRunner` valida executável e redige stdout/stderr.
- `JsonLineLoggingCenter` usa o redactor oficial antes de persistir/exportar logs.
- `ZipBackupService` valida destino e entradas antes de criar/restaurar.
- Todos os serviços foram registrados no container de infraestrutura.

## Regras Cobertas

- Bloqueio de path traversal.
- Paths de plugin restritos ao escopo permitido.
- Redaction de secrets em logs.
- Backup com hash e paths internos relativos.
- Atualizações exigem HTTPS e checksum.
- Checklist bloqueia entrega sem evidência de segurança.

# F028 - Backup & Restore

> Feature ID: F028 Status: Implemented and Homologated Priority: Critical
> Epic: Settings & User Experience

# Objetivo

Fornecer um mecanismo seguro de backup e restauração das configurações,
perfis, banco local, histórico e demais dados do WiiStudio, permitindo
migração entre computadores e recuperação após falhas.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Criar backup manual.

## RF002

Executar backup automático configurável.

## RF003

Restaurar backup completo ou parcial.

## RF004

Validar compatibilidade da versão antes da restauração.

## RF005

Criptografar backups opcionais.

## RF006

Permitir agendamento de backups.

------------------------------------------------------------------------

# Itens Incluídos

-   Settings
-   Perfis
-   Banco SQLite
-   Histórico
-   Cache (opcional)
-   Favoritos
-   Layouts
-   Logs (opcional)

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Abrir Backup
      │
Selecionar Itens
      │
Criar Backup
      │
Validar
      │
Salvar Arquivo
```

Restauração:

``` text
Selecionar Backup
      │
Validar
      │
Escolher Itens
      │
Restaurar
      │
Reiniciar Aplicação
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
BackupView
     │
BackupViewModel
     │
IBackupService
     │
BackupSerializer
     │
Zip Archive
```

------------------------------------------------------------------------

# Estrutura

``` text
backup/
├── manifest.json
├── settings.json
├── profiles/
├── database.db
├── cache/
└── logs/
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Preparing
-   Validating
-   Compressing
-   Saving
-   Restoring
-   Completed
-   Failed
-   Cancelled

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IBackupService
{
    Task<BackupPreview> AnalyzeAsync(
        CancellationToken cancellationToken);

    Task CreateAsync(
        BackupOptions options,
        string outputFile,
        CancellationToken cancellationToken);

    Task RestoreAsync(
        string backupFile,
        RestoreOptions options,
        CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Criar ponto de restauração antes de restaurar.
-   Backup incremental opcional.
-   Validar checksum.
-   Manter compatibilidade entre versões quando possível.

------------------------------------------------------------------------

# Eventos

-   BackupStarted
-   BackupCompleted
-   RestoreStarted
-   RestoreCompleted
-   BackupFailed
-   RestoreFailed

------------------------------------------------------------------------

# Interface

-   Criar Backup
-   Restaurar
-   Agendar
-   Histórico
-   Seleção de itens
-   Barra de progresso

------------------------------------------------------------------------

# Casos de Erro

-   Espaço insuficiente.
-   Arquivo inválido.
-   Backup incompatível.
-   Falha na compactação.
-   Cancelamento.

Todos devem registrar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Backup completo.
-   [x] Backup parcial.
-   [x] Restauração seletiva.
-   [x] Validação de integridade.
-   [x] Backup automático.

------------------------------------------------------------------------

# Testes

## Unitários

-   Serialização.
-   Compactação.
-   Checksum.
-   Restauração.
-   Criptografia.

## Integração

-   Backup completo.
-   Migração entre computadores.
-   Arquivo corrompido.
-   Restauração parcial.
-   Rollback após falha.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F024-PreferencesAndProfiles.md
-   F026-TaskManager.md
-   F027-ApplicationUpdateManager.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Backup validado.
-   Restauração funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

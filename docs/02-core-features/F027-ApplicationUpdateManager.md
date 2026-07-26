# F027 - Application Update Manager

> Feature ID: F027 Status: Implemented and Homologated Priority: High Epic:
> Settings & User Experience

# Objetivo

Gerenciar o ciclo completo de atualização do WiiStudio, permitindo
verificar, baixar, validar e instalar novas versões de forma segura,
preservando as configurações do usuário e oferecendo rollback em caso de
falha.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Verificar atualizações automaticamente.

## RF002

Permitir verificação manual.

## RF003

Baixar atualizações em segundo plano.

## RF004

Validar assinatura e integridade do pacote.

## RF005

Instalar atualização preservando dados do usuário.

## RF006

Executar rollback quando necessário.

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Downloads retomáveis.
-   Instalação segura.
-   Não bloquear a interface.
-   Compatível com Windows, Linux e macOS.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Verificar Atualizações
        │
Nova versão?
   ├── Não → Encerrar
   └── Sim
        │
Baixar
        │
Validar
        │
Instalar
        │
Reiniciar Aplicação
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
UpdateView
    │
UpdateViewModel
    │
IApplicationUpdateService
    │
ReleaseProvider
    │
Downloader
    │
SignatureValidator
    │
Installer
```

------------------------------------------------------------------------

# Máquina de Estados

-   Idle
-   Checking
-   Downloading
-   Validating
-   Installing
-   RestartPending
-   Completed
-   Failed
-   RolledBack

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IApplicationUpdateService
{
    Task<UpdateInfo> CheckAsync(CancellationToken cancellationToken);
    Task DownloadAsync(UpdateInfo update, CancellationToken cancellationToken);
    Task InstallAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Nunca instalar pacote sem validação.
-   Preservar configurações e perfis.
-   Permitir adiar atualização.
-   Registrar histórico de versões instaladas.

------------------------------------------------------------------------

# Eventos

-   UpdateCheckStarted
-   UpdateAvailable
-   UpdateDownloadCompleted
-   UpdateValidated
-   UpdateInstalled
-   UpdateFailed
-   RollbackCompleted

------------------------------------------------------------------------

# Interface

-   Versão atual
-   Última verificação
-   Nova versão disponível
-   Changelog
-   Barra de progresso
-   Botões:
    -   Verificar
    -   Baixar
    -   Instalar
    -   Adiar

------------------------------------------------------------------------

# Casos de Erro

-   Sem conexão.
-   Assinatura inválida.
-   Espaço insuficiente.
-   Instalação interrompida.
-   Falha no rollback.

Todos os erros devem gerar logs estruturados.

------------------------------------------------------------------------

# Critérios de Aceite

-   [x] Verificação automática.
-   [x] Download seguro.
-   [x] Validação de assinatura.
-   [x] Instalação preservando dados.
-   [x] Rollback funcional.

------------------------------------------------------------------------

# Testes

## Unitários

-   Verificação.
-   Download.
-   Validação.
-   Instalação.
-   Rollback.

## Integração

-   Atualização completa.
-   Download interrompido.
-   Assinatura inválida.
-   Falha de instalação.
-   Recuperação via rollback.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   F024-PreferencesAndProfiles.md
-   F025-NotificationCenter.md
-   F026-TaskManager.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Atualização validada.
-   Rollback funcional.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# 08-tools

Documentos para gerenciamento de dependências externas, acesso privilegiado,
integração GameTDB, operações Wiimms e fluxos de prontidão USB do WiiStudio.

# Documentos

- `F101-ExternalToolDependencyManager.md`
- `F102-GameTdbCoverProvider.md`
- `F103-GameTDBMetadataProvider.md`
- `F104-CoverAssetGalleryAndBatchSelection.md`
- `F105-CrossPlatformPrivilegedDeviceAccess.md`
- `F106-PolkitPolicyInstaller.md`
- `F107-CrossPlatformUsbDeviceProvider.md`
- `F108-ExternalToolNetworkDownloader.md`
- `F109-WiimmsOperationProgress.md`
- `F110-SafeUsbWriteWorkflow.md`
- `F110-1-UsbFormatAndWbfsPreparation.md`
- `F111-GameTdbUpdateSource.md`
- `F112-ApplicationUpdateProvider.md`
- `F113-TaskAndNotificationCenterIntegration.md`
- `F114-LibraryPersistenceWithSQLite.md`
- `F115-AdvancedGameDetailsPanel.md`
- `F116-CoverAssetBulkActions.md`
- `F117-PluginMarketplaceUI.md`
- `F118-DiagnosticsRepairActions.md`
- `F119-FirstRunSetupWizard.md`
- `F120-EndToEndWiiUsbReadinessCheck.md`
- `F121-WBFSFormatCommandCatalog.md`
- `F122-ApplicationReleaseAndUpdateDistribution.md`
- `F123-RemotePluginMarketplaceAndTrustPolicy.md`
- `F124-PackagingInstallerAndCiCd.md`
- `F125-HardwareValidationAndReleaseAcceptance.md`
- `F126-DesktopExecutableBrandingAndPackaging.md`
- `EXTERNAL_TOOLS_REFERENCE.md`
- `MELHORAMENTOS-ROADMAP.md`

# Ordem Recomendada

Para desenvolvimento incremental, seguir esta ordem:

1. `F106-PolkitPolicyInstaller.md`
2. `F107-CrossPlatformUsbDeviceProvider.md`
3. `F108-ExternalToolNetworkDownloader.md`
4. `F109-WiimmsOperationProgress.md`
5. `F110-SafeUsbWriteWorkflow.md`
6. `F111-GameTdbUpdateSource.md`
7. `F113-TaskAndNotificationCenterIntegration.md`
8. `F114-LibraryPersistenceWithSQLite.md`
9. `F115-AdvancedGameDetailsPanel.md`
10. `F116-CoverAssetBulkActions.md`
11. `F118-DiagnosticsRepairActions.md`
12. `F119-FirstRunSetupWizard.md`
13. `F120-EndToEndWiiUsbReadinessCheck.md`
14. `F110-1-UsbFormatAndWbfsPreparation.md`
15. `F112-ApplicationUpdateProvider.md`
16. `F117-PluginMarketplaceUI.md`
17. `F121-WBFSFormatCommandCatalog.md`
18. `F122-ApplicationReleaseAndUpdateDistribution.md`
19. `F123-RemotePluginMarketplaceAndTrustPolicy.md`
20. `F124-PackagingInstallerAndCiCd.md`
21. `F125-HardwareValidationAndReleaseAcceptance.md`
22. `F126-DesktopExecutableBrandingAndPackaging.md`

`F112` e `F117` podem ser desenvolvidas em paralelo depois que `F108` e `F113`
estiverem estáveis.

`F110-1` deve ser implementada somente depois de `F110`, `F113` e `F120`, pois
é um fluxo destrutivo de preparação/formatação USB.

`F121` deve fechar o comando real de formatação antes da esteira de release.
`F122`, `F124` e `F125` compõem a preparação de distribuição e homologação.
`F123` complementa `F117` com catálogo remoto, confiança, revogação e quarentena
de plugins.
`F126` complementa `F124` com branding final do executável Windows e bundle macOS.

# Melhoramentos

As propostas de melhoramento adicionadas após a implementação base ficam nesta mesma
pasta com o sufixo `-Melhoramento.md`. Elas não substituem os documentos principais;
servem como camada de refinamento para robustez, UX operacional, segurança e release.

Ordem sugerida para execução da fase de melhoramentos:

1. `F118-DiagnosticsRepairActions-Melhoramento.md`
2. `F108-ExternalToolNetworkDownloader-Melhoramento.md`
3. `F109-WiimmsOperationProgress-Melhoramento.md`
4. `F110-SafeUsbWriteWorkflow-Melhoramento.md`
5. `F120-EndToEndWiiUsbReadinessCheck-Melhoramento.md`
6. `F110-1-UsbFormatAndWbfsPreparation-Melhoramento.md`
7. `F114-LibraryPersistenceWithSQLite-Melhoramento.md`
8. `F115-AdvancedGameDetailsPanel-Melhoramento.md`
9. `F116-CoverAssetBulkActions-Melhoramento.md`
10. `F119-FirstRunSetupWizard-Melhoramento.md`
11. `F124-PackagingInstallerAndCiCd-Melhoramento.md`
12. `F125-HardwareValidationAndReleaseAcceptance-Melhoramento.md`

Racional resumido: primeiro corrigir diagnóstico e suporte operacional, depois
ferramentas/progresso, em seguida escrita/preparação USB, persistência e UX de
biblioteca, fechando com onboarding, empacotamento e validação real.

# Pacotes Locais

Pacotes binários de terceiros não devem ficar em `docs/`.

O pacote legado NKit v1.4 fica em:

```text
tools/packages/NKit_v1.4.zip
```

Ele deve ser tratado como pacote local pinado pelo projeto:

- validar SHA-256 antes de extrair;
- solicitar confirmação do usuário antes de instalar;
- não executar automaticamente;
- não usar como download remoto automático.

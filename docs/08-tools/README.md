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
- `EXTERNAL_TOOLS_REFERENCE.md`

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

`F112` e `F117` podem ser desenvolvidas em paralelo depois que `F108` e `F113`
estiverem estáveis.

`F110-1` deve ser implementada somente depois de `F110`, `F113` e `F120`, pois
é um fluxo destrutivo de preparação/formatação USB.

`F121` deve fechar o comando real de formatação antes da esteira de release.
`F122`, `F124` e `F125` compõem a preparação de distribuição e homologação.
`F123` complementa `F117` com catálogo remoto, confiança, revogação e quarentena
de plugins.

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

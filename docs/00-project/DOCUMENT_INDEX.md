# DOCUMENT_INDEX

# Objetivo

Índice oficial dos documentos do WiiStudio.

Use este arquivo para localizar rapidamente documentos de arquitetura, features,
design system, application shell, plugin SDK, segurança, testes e documentação final.

---

# Documentos Base

- `001-Architecture.md`
- `002-SolutionStructure.md`
- `003-CodingStandards.md`
- `004-DevelopmentWorkflow.md`
- `005-DomainModel.md`
- `006-ApplicationLayer.md`
- `007-Infrastructure.md`
- `008-UI-UX.md`
- `009-Database.md`
- `010-USBDetection.md`
- `011-WBFS.md`
- `012-WiimmsIntegration.md`
- `013-NKitLegacy.md`
- `014-GameTDB.md`
- `015-PluginArchitecture.md`
- `016-GameLibrary.md`
- `017-ImportWizard.md`
- `018-ConversionEngine.md`
- `019-FeatureDevelopmentGuide.md`
- `020-Diagnostics.md`
- `021-ThemeSystem.md`
- `022-Localization.md`
- `023-Playbook-FeatureDevelopment.md`

---

# EP001 a EP009 - Features e UI

- `F001-DetectUsbDrive.md`
- `F002-ScanGameLibrary.md`
- `F003-ImportIso.md`
- `F004-ImportWbfs.md`
- `F005-ImportRvz.md`
- `F006-ImportNkit.md`
- `F007-ConvertGame.md`
- `F008-BatchConversion.md`
- `F009-RemoveGame.md`
- `F010-VerifyGameIntegrity.md`
- `F011-RepairGame.md`
- `F012-ExportGame.md`
- `F013-GameDetails.md`
- `F014-FavoriteGames.md`
- `F015-SearchAndFilter.md`
- `F016-DuplicateDetection.md`
- `F017-DownloadCovers.md`
- `F018-MetadataSynchronization.md`
- `F019-OfflineMetadataCache.md`
- `F020-AssetManager.md`
- `F021-BannerAndDiscArt.md`
- `F022-GameTDBUpdateManager.md`
- `F023-SettingsManager.md`
- `F024-PreferencesAndProfiles.md`
- `F025-NotificationCenter.md`
- `F026-TaskManager.md`
- `F027-ApplicationUpdateManager.md`
- `F028-BackupAndRestore.md`
- `F029-LoggingCenter.md`
- `F030-AdvancedDiagnostics.md`
- `F031-MainWindow.md`
- `F032-LibraryView.md`
- `F033-ImportWizard.md`
- `F034-ConversionWizard.md`
- `F035-DeviceManager.md`
- `F036-Dashboard.md`
- `F037-StatusBar.md`
- `F038-DockablePanels.md`
- `F039-ThemeEngine.md`
- `F040-CommandPalette.md`
- `F041-DesignSystemFoundation.md`
- `F042-ColorSystem.md`
- `F043-Typography.md`
- `F044-Iconography.md`
- `F045-SpacingAndLayout.md`
- `F046-Buttons.md`
- `F047-InputsAndForms.md`
- `F048-DialogsAndModals.md`
- `F049-DataGrid.md`
- `F050-CardsAndContainers.md`
- `F051-NavigationSystem.md`
- `F052-ToolbarSystem.md`
- `F053-TabsSystem.md`
- `F054-MenusAndContextMenus.md`
- `F055-TooltipsAndHelpHints.md`
- `F056-BadgesAndStatusIndicators.md`
- `F057-ProgressIndicators.md`
- `F058-EmptyErrorAndLoadingStates.md`
- `F059-ToastsAndInlineFeedback.md`
- `F060-ResponsiveBehaviors.md`
- `DESIGN_SYSTEM_REFERENCE.md`
- `DESIGN_SYSTEM_USAGE.md`

---

# EP010 - Application Shell

- `F061-AppShellStructure.md`
- `F062-MainMenu.md`
- `F063-SidebarNavigation.md`
- `F064-HeaderArea.md`
- `F065-FooterAndStatusArea.md`
- `F066-WorkspaceRegion.md`
- `F067-PanelSystem.md`
- `F068-WindowManagement.md`
- `F069-KeyboardShortcuts.md`
- `F070-AccessibilityNavigation.md`
- `APPLICATION_SHELL_REFERENCE.md`

---

# EP011 - Plugin SDK

- `F071-PluginSDKFoundation.md`
- `F072-PluginManifest.md`
- `F073-PluginDiscovery.md`
- `F074-PluginLoading.md`
- `F075-PluginPermissions.md`
- `F076-PluginUIExtensions.md`
- `F077-PluginMetadataProviders.md`
- `F078-PluginConversionProviders.md`
- `F079-PluginDiagnostics.md`
- `F080-PluginMarketplacePreparation.md`
- `PLUGIN_SDK_REFERENCE.md`

---

# EP012 - Security & Permissions

- `F081-SecurityFoundation.md`
- `F082-FileSystemPermissions.md`
- `F083-ExternalProcessSecurity.md`
- `F084-PluginSecurity.md`
- `F085-SensitiveSettingsProtection.md`
- `F086-SafePathHandling.md`
- `F087-BackupSecurity.md`
- `F088-LogRedaction.md`
- `F089-UpdateSecurity.md`
- `F090-SecurityChecklist.md`
- `SECURITY_REFERENCE.md`

---

# EP013 - Testing & Quality

- `F091-TestingStrategy.md`
- `F092-UnitTestingStandards.md`
- `F093-IntegrationTestingStandards.md`
- `F094-UITestingStrategy.md`
- `F095-ProcessRunnerTesting.md`
- `F096-DatabaseTesting.md`
- `F097-PluginTesting.md`
- `F098-PerformanceTesting.md`
- `F099-RegressionTesting.md`
- `F100-QualityGates.md`
- `TESTING_QUALITY_REFERENCE.md`

---

# EP014 - External Tools

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

---

# Documentos Finais

- `AI_CONTEXT.md`
- `DOCUMENT_INDEX.md`
- `ROADMAP.md`
- `README.md`
- `ARCHITECTURE_OVERVIEW.md`
- `DEVELOPMENT_GUIDE.md`
- `CONTRIBUTING.md`
- `DEFINITION_OF_DONE.md`
- `FOLDER_STRUCTURE.md`
- `FINALIZATION_REFERENCE.md`
- `MIGRATION_PLAN_TO_FINAL_FOLDERS.md`

---

# Leitura Recomendada para Codex

## Antes de qualquer tarefa

1. `AI_CONTEXT.md`
2. `DOCUMENT_INDEX.md`
3. `001-Architecture.md`
4. `003-CodingStandards.md`
5. Documento específico da feature.

## Para UI

- `008-UI-UX.md`
- `021-ThemeSystem.md`
- `022-Localization.md`
- `F041-DesignSystemFoundation.md`
- `F060-ResponsiveBehaviors.md`
- `F070-AccessibilityNavigation.md`

## Para Processos Externos

- `012-WiimmsIntegration.md`
- `013-NKitLegacy.md`
- `F083-ExternalProcessSecurity.md`
- `F095-ProcessRunnerTesting.md`

## Para Plugins

- `F071-PluginSDKFoundation.md`
- `F072-PluginManifest.md`
- `F073-PluginDiscovery.md`
- `F074-PluginLoading.md`
- `F075-PluginPermissions.md`
- `F084-PluginSecurity.md`

## Para Segurança

- `F081-SecurityFoundation.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F090-SecurityChecklist.md`

## Para Testes

- `F091-TestingStrategy.md`
- `F092-UnitTestingStandards.md`
- `F093-IntegrationTestingStandards.md`
- `F100-QualityGates.md`

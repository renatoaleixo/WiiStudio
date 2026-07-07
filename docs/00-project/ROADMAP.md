# ROADMAP

# Objetivo

Este roadmap organiza a evolução do WiiStudio por fases, priorizando arquitetura,
estabilidade, funcionalidades essenciais, segurança, testes, plugins e refinamento
da experiência desktop.

O roadmap deve orientar o Codex e os desenvolvedores sobre a ordem recomendada de
implementação, evitando construir features avançadas antes da base técnica estar
estável.

---

# Visão Geral

O WiiStudio será desenvolvido em fases:

1. Fundação do projeto.
2. App Shell e Design System.
3. Biblioteca de jogos.
4. Detecção de dispositivos.
5. Importação e conversão.
6. Metadados e assets.
7. Tarefas, logs e diagnósticos.
8. Segurança e permissões.
9. Plugin SDK.
10. Testes, qualidade e release.

---

# Fase 01 - Fundação

## Objetivo

Criar a base técnica do projeto.

## Entregas

- Solution structure.
- Clean Architecture.
- Projetos Domain, Application, Infrastructure e UI.
- Dependency Injection.
- Logging inicial.
- Configurações iniciais.
- SQLite base.
- Test projects.
- CI básico quando aplicável.

## Documentos Relacionados

- `001-Architecture.md`
- `002-SolutionStructure.md`
- `003-CodingStandards.md`
- `004-DevelopmentWorkflow.md`
- `005-DomainModel.md`
- `006-ApplicationLayer.md`
- `007-Infrastructure.md`
- `009-Database.md`

## Critérios de Conclusão

- Solução compila.
- Testes base executam.
- DI configurado.
- Logging configurado.
- Banco inicial criado.
- Estrutura de pastas validada.

---

# Fase 02 - App Shell e UI Foundation

## Objetivo

Criar a base visual da aplicação desktop.

## Entregas

- MainWindow.
- App Shell.
- Header.
- Sidebar.
- Workspace.
- Status Bar.
- Panel System.
- Theme Engine.
- Design System.
- Responsividade.
- Acessibilidade básica.
- Navigation System.
- Command Palette.

## Documentos Relacionados

- `008-UI-UX.md`
- `021-ThemeSystem.md`
- `022-Localization.md`
- `F031-MainWindow.md`
- `F039-ThemeEngine.md`
- `F040-CommandPalette.md`
- `F041-DesignSystemFoundation.md`
- `F060-ResponsiveBehaviors.md`
- `F061-AppShellStructure.md`
- `F070-AccessibilityNavigation.md`

## Critérios de Conclusão

- App abre com Shell funcional.
- Navegação básica funcionando.
- Light/Dark funcionando.
- Sidebar e Workspace funcionais.
- Estados Loading/Empty/Error disponíveis.
- Foco e teclado validados.

---

# Fase 03 - Biblioteca de Jogos

## Objetivo

Permitir que o usuário gerencie uma biblioteca local de jogos.

## Entregas

- Scan de biblioteca.
- Cadastro local de jogos.
- Detalhes do jogo.
- Favoritos.
- Busca e filtros.
- Detecção de duplicados.
- Remoção segura.
- Persistência SQLite.

## Documentos Relacionados

- `016-GameLibrary.md`
- `F002-ScanGameLibrary.md`
- `F009-RemoveGame.md`
- `F013-GameDetails.md`
- `F014-FavoriteGames.md`
- `F015-SearchAndFilter.md`
- `F016-DuplicateDetection.md`
- `F032-LibraryView.md`

## Critérios de Conclusão

- Biblioteca lista jogos.
- Busca e filtros funcionam.
- Detalhes exibidos.
- Remoção exige confirmação.
- Dados persistidos.
- Testes de repository e ViewModel criados.

---

# Fase 04 - Dispositivos

## Objetivo

Detectar dispositivos USB e preparar integração com partições/dispositivos Wii.

## Entregas

- USB Detection.
- Device Manager.
- Estado de dispositivo.
- Diagnóstico de permissão.
- Listagem de partições quando possível.
- Integração inicial com Wiimms para leitura.

## Documentos Relacionados

- `010-USBDetection.md`
- `011-WBFS.md`
- `012-WiimmsIntegration.md`
- `F001-DetectUsbDrive.md`
- `F035-DeviceManager.md`
- `F083-ExternalProcessSecurity.md`

## Critérios de Conclusão

- Dispositivo detectado.
- Erros de permissão tratados.
- UI mostra estado.
- Logs e diagnósticos disponíveis.
- Nenhuma operação usa `Process.Start` direto.

---

# Fase 05 - Importação e Conversão

## Objetivo

Permitir importar e converter jogos de forma segura.

## Entregas

- Import Wizard.
- Import ISO.
- Import WBFS.
- Import RVZ.
- Import NKit.
- Conversion Wizard.
- Conversion Engine.
- Batch Conversion.
- Verificação de integridade.
- Exportação.
- Integração com Task Manager.

## Documentos Relacionados

- `017-ImportWizard.md`
- `018-ConversionEngine.md`
- `F003-ImportIso.md`
- `F004-ImportWbfs.md`
- `F005-ImportRvz.md`
- `F006-ImportNkit.md`
- `F007-ConvertGame.md`
- `F008-BatchConversion.md`
- `F010-VerifyGameIntegrity.md`
- `F012-ExportGame.md`
- `F033-ImportWizard.md`
- `F034-ConversionWizard.md`

## Critérios de Conclusão

- Importação funciona com validação.
- Conversão usa ProcessRunner.
- Operações longas aparecem no Task Manager.
- Cancelamento funciona.
- Arquivo original é preservado por padrão.
- Falhas são seguras.

---

# Fase 06 - Metadados e Assets

## Objetivo

Enriquecer a biblioteca com metadados, capas, banners e disc arts.

## Entregas

- GameTDB integration.
- Offline Metadata Cache.
- Download de capas.
- Asset Manager.
- Banner and Disc Art.
- Atualização de GameTDB.
- Preservação de dados customizados.

## Documentos Relacionados

- `014-GameTDB.md`
- `F017-DownloadCovers.md`
- `F019-OfflineMetadataCache.md`
- `F020-AssetManager.md`
- `F021-BannerAndDiscArt.md`
- `F022-GameTDBUpdateManager.md`

## Critérios de Conclusão

- Metadados sincronizados.
- Cache offline funciona.
- Assets baixados e armazenados.
- Falhas de rede tratadas.
- Dados editados pelo usuário preservados.

---

# Fase 07 - Operações, Logs e Diagnósticos

## Objetivo

Garantir visibilidade e controle sobre operações internas.

## Entregas

- Notification Center.
- Task Manager.
- Logging Center.
- Advanced Diagnostics.
- Progress Indicators.
- Toasts.
- Empty/Error/Loading States.

## Documentos Relacionados

- `020-Diagnostics.md`
- `F025-NotificationCenter.md`
- `F026-TaskManager.md`
- `F029-LoggingCenter.md`
- `F030-AdvancedDiagnostics.md`
- `F057-ProgressIndicators.md`
- `F058-EmptyErrorAndLoadingStates.md`
- `F059-ToastsAndInlineFeedback.md`

## Critérios de Conclusão

- Tarefas rastreáveis.
- Logs visíveis.
- Diagnósticos exportáveis.
- Notificações funcionais.
- Erros técnicos possuem detalhes seguros.

---

# Fase 08 - Segurança

## Objetivo

Fortalecer a aplicação antes de recursos avançados e plugins.

## Entregas

- Security Foundation.
- File System Permissions.
- External Process Security.
- Safe Path Handling.
- Sensitive Settings Protection.
- Log Redaction.
- Backup Security.
- Update Security.
- Security Checklist.

## Documentos Relacionados

- `F081-SecurityFoundation.md`
- `F082-FileSystemPermissions.md`
- `F083-ExternalProcessSecurity.md`
- `F085-SensitiveSettingsProtection.md`
- `F086-SafePathHandling.md`
- `F087-BackupSecurity.md`
- `F088-LogRedaction.md`
- `F089-UpdateSecurity.md`
- `F090-SecurityChecklist.md`

## Critérios de Conclusão

- Path traversal bloqueado.
- Logs redigem secrets.
- ProcessRunner obrigatório.
- Backups possuem integridade.
- Updates validam checksum.
- Checklist aplicado.

---

# Fase 09 - Plugin SDK

## Objetivo

Permitir extensibilidade segura e controlada.

## Entregas

- Plugin SDK Foundation.
- Plugin Manifest.
- Discovery.
- Loading.
- Permissions.
- UI Extensions.
- Metadata Providers.
- Conversion Providers.
- Diagnostics.
- Marketplace preparation.

## Documentos Relacionados

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
- `F084-PluginSecurity.md`

## Critérios de Conclusão

- Plugin válido carrega.
- Plugin inválido é bloqueado.
- Permissões funcionam.
- Falhas são isoladas.
- UI Extension não quebra Shell.
- Diagnostics registra eventos.

---

# Fase 10 - Testes, Qualidade e Release

## Objetivo

Garantir qualidade mínima para uso real.

## Entregas

- Testing Strategy.
- Unit Testing Standards.
- Integration Testing Standards.
- UI Testing Strategy.
- ProcessRunner Testing.
- Database Testing.
- Plugin Testing.
- Performance Testing.
- Regression Testing.
- Quality Gates.

## Documentos Relacionados

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

## Critérios de Conclusão

- Quality Gates aplicados.
- Testes críticos passando.
- Regressões cobertas.
- Segurança validada.
- Documentação consolidada.
- Release candidate gerado.

---

# Marcos Recomendados

## MVP 1

- App Shell.
- Biblioteca local.
- Scan de pasta.
- Detalhes básicos.
- Settings.
- Logging.

## MVP 2

- Device Manager.
- Wiimms Integration.
- Importação.
- Conversão.
- Task Manager.

## MVP 3

- Metadados.
- Assets.
- Diagnostics.
- Backup/Restore.
- Segurança base.

## MVP 4

- Plugin SDK.
- Providers.
- UI Extensions.
- Quality Gates.

---

# Regras de Priorização

Priorizar:

1. Segurança de dados.
2. Estabilidade.
3. Arquitetura limpa.
4. Fluxo principal do usuário.
5. Testabilidade.
6. UI refinada.
7. Extensibilidade.

Evitar:

- começar marketplace antes do Plugin SDK seguro;
- implementar conversão sem ProcessRunner;
- implementar UI complexa sem Design System;
- implementar plugins sem permissões;
- implementar release sem Quality Gates.

---

# Definition of Done do Roadmap

- Fases definidas.
- Dependências claras.
- MVPs definidos.
- Critérios por fase definidos.
- Documentação alinhada ao índice.

# 09-emulator-integration

Documentos da fase de integração com emuladores externos, inicialmente Dolphin.

Esta pasta representa uma nova fase de produto apos a homologacao das features
`F001` ate `F126`. O objetivo e manter o WiiStudio como gerenciador de
biblioteca, USB/WBFS, capas, metadados e ferramentas, tratando o emulador como
runtime externo opcional.

# Documentos

- `EMULATOR-INTEGRATION-ROADMAP.md`
- `F127-DolphinEmulatorIntegration-Melhoramento.md`
- `F128-EmulatorGameLaunchWorkflow-Melhoramento.md`
- `F129-EmulatorProfilesAndControllerMapping-Melhoramento.md`
- `F130-EmulatorCompatibilityAndSaveManagement-Melhoramento.md`

# Sequencia Recomendada

1. `F127`: detectar, configurar e diagnosticar Dolphin.
2. `F128`: executar jogos elegiveis no emulador com preview e validacao.
3. `F129`: criar perfis de emulacao e associacao por jogo.
4. `F130`: registrar historico, compatibilidade local, notas e saves.

# Regras

- Dolphin deve ser opcional.
- WiiStudio nao deve distribuir jogos, ROMs, ISOs, BIOS ou chaves.
- WiiStudio nao deve embutir o core do Dolphin no primeiro ciclo.
- Execucao deve usar `IProcessRunner` e catalogo de argumentos permitido.
- Paths de jogo, emulador e saves devem ser validados.
- Proximas melhorias devem entrar como novas features desta fase.

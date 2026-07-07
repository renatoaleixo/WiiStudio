# MIGRATION_PLAN_TO_FINAL_FOLDERS

# Objetivo

Definir o plano para migrar a documentação atual do WiiStudio para a estrutura final
de pastas, mantendo rastreabilidade, organização e facilidade de leitura pelo Codex.

Este plano não altera o conteúdo funcional dos documentos. Ele apenas organiza os
arquivos em uma estrutura estável.

---

# Estrutura Final

```text
docs/
├── 00-project/
├── 01-architecture/
├── 02-core-features/
├── 03-ui-design-system/
├── 04-application-shell/
├── 05-plugin-sdk/
├── 06-security/
├── 07-testing-quality/
└── 99-final/
```

---

# Fase 01 - Preparação

## Ações

- Criar pasta `docs`.
- Criar subpastas oficiais.
- Conferir lista atual de documentos.
- Garantir que `DOCUMENT_INDEX.md` esteja atualizado.
- Garantir que não existem documentos duplicados com nomes divergentes.

## Critérios

- Estrutura criada.
- Índice validado.
- Arquivos existentes conferidos.

---

# Fase 02 - Mover Documentos de Projeto

Mover para:

```text
docs/00-project/
```

Arquivos:

- `README.md`
- `ROADMAP.md`
- `AI_CONTEXT.md`
- `DOCUMENT_INDEX.md`
- `CONTRIBUTING.md`
- `DEVELOPMENT_GUIDE.md`
- `DEFINITION_OF_DONE.md`

---

# Fase 03 - Mover Arquitetura

Mover para:

```text
docs/01-architecture/
```

Arquivos:

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

# Fase 04 - Mover Core Features

Mover para:

```text
docs/02-core-features/
```

Arquivos:

- `F001-DetectUsbDrive.md` até `F030-AdvancedDiagnostics.md`

Observação:

- Validar existência de `F018-MetadataSynchronization.md`.
- Caso exista apenas `018-ConversionEngine.md`, manter no grupo de arquitetura e criar/regularizar `F018` se necessário.

---

# Fase 05 - Mover UI e Design System

Mover para:

```text
docs/03-ui-design-system/
```

Arquivos:

- `F031-MainWindow.md` até `F060-ResponsiveBehaviors.md`

---

# Fase 06 - Mover Application Shell

Mover para:

```text
docs/04-application-shell/
```

Arquivos:

- `F061-AppShellStructure.md` até `F070-AccessibilityNavigation.md`

---

# Fase 07 - Mover Plugin SDK

Mover para:

```text
docs/05-plugin-sdk/
```

Arquivos:

- `F071-PluginSDKFoundation.md` até `F080-PluginMarketplacePreparation.md`

---

# Fase 08 - Mover Security

Mover para:

```text
docs/06-security/
```

Arquivos:

- `F081-SecurityFoundation.md` até `F090-SecurityChecklist.md`

---

# Fase 09 - Mover Testing & Quality

Mover para:

```text
docs/07-testing-quality/
```

Arquivos:

- `F091-TestingStrategy.md` até `F100-QualityGates.md`

---

# Fase 10 - Mover Documentos Finais

Mover para:

```text
docs/99-final/
```

Arquivos:

- `ARCHITECTURE_OVERVIEW.md`
- `FOLDER_STRUCTURE.md`
- `MIGRATION_PLAN_TO_FINAL_FOLDERS.md`

---

# Fase 11 - Atualizar Referências

Após mover:

- atualizar links relativos;
- atualizar `DOCUMENT_INDEX.md`;
- atualizar `README.md`;
- atualizar referências nos documentos finais;
- validar que o Codex consegue localizar documentos por nome.

---

# Fase 12 - Criar README por Pasta

Criar README em cada pasta:

```text
docs/00-project/README.md
docs/01-architecture/README.md
docs/02-core-features/README.md
docs/03-ui-design-system/README.md
docs/04-application-shell/README.md
docs/05-plugin-sdk/README.md
docs/06-security/README.md
docs/07-testing-quality/README.md
docs/99-final/README.md
```

Cada README deve conter:

- objetivo da pasta;
- lista dos documentos;
- ordem de leitura;
- observações para Codex.

---

# Fase 13 - Validação

Validar:

- todos os arquivos movidos;
- nenhum arquivo perdido;
- nomes consistentes;
- links atualizados;
- índice atualizado;
- zip final criado quando necessário;
- documentação pronta para entrega ao Codex.

---

# Script Conceitual

```bash
mkdir -p docs/00-project
mkdir -p docs/01-architecture
mkdir -p docs/02-core-features
mkdir -p docs/03-ui-design-system
mkdir -p docs/04-application-shell
mkdir -p docs/05-plugin-sdk
mkdir -p docs/06-security
mkdir -p docs/07-testing-quality
mkdir -p docs/99-final
```

---

# Regras de Migração

- Não renomear arquivos sem necessidade.
- Não alterar conteúdo funcional durante movimentação.
- Atualizar links depois de mover.
- Manter histórico de Git.
- Validar F018 antes da organização final.
- Gerar zip final se for entregar fora do repositório.

---

# Checklist

- [ ] Pastas criadas.
- [ ] Project docs movidos.
- [ ] Architecture docs movidos.
- [ ] Core features movidas.
- [ ] UI docs movidos.
- [ ] App Shell movido.
- [ ] Plugin SDK movido.
- [ ] Security movido.
- [ ] Testing movido.
- [ ] Final docs movidos.
- [ ] READMEs por pasta criados.
- [ ] Índice atualizado.
- [ ] Links revisados.
- [ ] Zip final criado.
- [ ] Entrega validada.

---

# Resultado Esperado

Ao final, a documentação estará organizada para:

- leitura humana;
- leitura por Codex;
- manutenção incremental;
- revisão por área;
- implementação faseada;
- entrega como pacote único.

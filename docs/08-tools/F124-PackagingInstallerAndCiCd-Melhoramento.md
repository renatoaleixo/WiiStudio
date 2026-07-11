# F124 - Installer e empacotamento [Melhoramento]

> **Suggested path:** `docs/08-tools/F124-PackagingInstallerAndCiCd-Melhoramento.md`  
> **Priority:** Baixa  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Consolidar empacotamento instalável em Linux, Windows e macOS com assets, ícone, policy, dependências e CI/CD.

---

## 2. Problema que Resolve

Para produto final, o app precisa ser instalável, versionado, com artefatos confiáveis e smoke tests.

---

## 3. Escopo Funcional

- deb/rpm/AppImage
- Windows installer
- macOS dmg/app bundle
- GitHub Releases
- Checksums
- Manifest
- Smoke tests

---

## 4. Fora de Escopo

- Publicar sem testes
- Assinatura completa se ainda não houver infraestrutura

---

## 5. Fluxo de Usuário Esperado

1. Build.
2. Test.
3. Package.
4. Checksum.
5. Manifest.
6. Publish release.
7. Smoke test.

---

## 6. Regras de Negócio

- Não publicar se testes falharem.
- Checksum obrigatório.
- Artefatos versionados.
- Linux sem regressão.

---

## 7. Regras de Segurança

- Secrets protegidos.
- Sem operação destrutiva em CI.
- Assinatura só em tags protegidas.
- Não expor certificados.

---

## 8. Integrações Necessárias

- F122
- F126
- F119
- F106

---

## 9. Impactos na UI

- Installer cria atalhos, ícones e nome correto.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- scripts/release/package.sh
- release manifest generator
- artifact smoke tests

---

## 11. Persistência e Configurações

- GitHub Releases artifacts
- SHA256SUMS.txt
- update manifest

---

## 12. Logs, Tarefas e Notificações Esperadas

Todo fluxo longo deve aparecer no módulo de **Tarefas** e gerar **Notificações** ao concluir, falhar ou ser cancelado.

Regras esperadas:

- criar `TaskId` e `CorrelationId`;
- exibir progresso quando disponível;
- exibir estado indeterminado quando progresso real não existir;
- publicar notificação persistente em falha;
- registrar logs redigidos;
- permitir exportação de relatório quando aplicável.

---

## 13. Critérios de Aceite

- Artefatos Linux/Windows/macOS.
- Checksums.
- Manifest.
- Smoke tests.
- Ícones/nome corretos.

---

## 14. Cenários de Teste

- unit/integration
- package smoke
- installer install/uninstall
- manifest validation

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| Teste falhou | `Release bloqueada porque os testes falharam.` |
| Checksum ausente | `Manifest não pode ser gerado sem checksum do artefato.` |

---

## 16. Regras para Implementação

- Seguir arquitetura Application / Infrastructure / Domain.
- UI Avalonia deve usar ViewModels e Commands.
- Não colocar regra de negócio em code-behind.
- Operações longas devem usar `async/await`, `CancellationToken`, progresso, Task Center e Notification Center.
- Não usar `Process.Start` diretamente.
- Não armazenar senha do usuário.
- Validar paths e argumentos.
- Usar fakes nos testes automatizados.

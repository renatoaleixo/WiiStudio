# F108 - Download remoto de ferramentas [Melhoramento]

> **Suggested path:** `docs/08-tools/F108-ExternalToolNetworkDownloader-Melhoramento.md`  
> **Priority:** Alta  
> **Status:** Improvement proposal  
> **Architecture:** .NET 10, Avalonia, Application / Infrastructure / Domain

---

## 1. Objetivo do Melhoramento

Aprimorar o download remoto seguro de dependências externas, com confirmação, checksum, progresso e fallback manual.

---

## 2. Problema que Resolve

A instalação de dependências como Wiimms pode depender de configuração manual, aumentando atrito e suporte.

---

## 3. Escopo Funcional

- Download HTTPS.
- Confirmação do usuário.
- Checksum SHA-256 obrigatório.
- Allowlist de hosts.
- Progresso em Task Center.
- Fallback manual.
- Diagnóstico de origem e validação.

---

## 4. Fora de Escopo

- Não baixar por HTTP.
- Não instalar sem checksum.
- Não executar pacote baixado antes de validar.
- Não baixar silenciosamente.

---

## 5. Fluxo de Usuário Esperado

1. Usuário clica Instalar ferramenta.
2. Sistema mostra origem, versão e checksum.
3. Usuário confirma.
4. Download ocorre com progresso.
5. Checksum validado.
6. Pacote entregue ao instalador.

---

## 6. Regras de Negócio

- Checksum ausente bloqueia auto-install.
- Host desconhecido bloqueia.
- Redirect inseguro bloqueia.
- Offline mostra fallback manual.

---

## 7. Regras de Segurança

- HTTPS obrigatório.
- SHA-256 obrigatório.
- Temp seguro.
- Sem execução automática.
- Logs redigidos.

---

## 8. Integrações Necessárias

- F101
- F113
- F118
- F124

---

## 9. Impactos na UI

- Dialog de confirmação de download.
- Progresso.
- Botão cancelar.
- Fallback selecionar arquivo local.

---

## 10. Serviços, Classes e Interfaces Sugeridas

- IExternalToolNetworkDownloader
- ExternalToolDownloadProgress
- DownloadedToolPackage

---

## 11. Persistência e Configurações

- ExternalTools.LastDownloadAt
- ExternalTools.LastDownloadHost
- ExternalTools.DownloadTimeoutSeconds

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

- Download HTTPS funciona.
- Checksum inválido bloqueia.
- Offline oferece fallback.
- Tarefa e notificação são criadas.
- Testes usam HTTP fake.

---

## 14. Cenários de Teste

- HTTPS válido
- HTTP bloqueado
- checksum inválido
- offline
- timeout
- cancelamento
- redirect inseguro

---

## 15. Casos de Erro e Mensagens para o Usuário

| Caso | Mensagem |
|---|---|
| HTTP | `Download inseguro bloqueado. Apenas HTTPS é permitido.` |
| Checksum | `O checksum do pacote não confere. A instalação foi bloqueada.` |
| Offline | `Não foi possível baixar automaticamente. Selecione o pacote manualmente.` |

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

# F108 - External Tool Network Downloader

> Feature ID: F108  
> Status: Ready for Development  
> Priority: Critical  
> Epic: External Dependencies / Security  
> Area: Infrastructure, Network, External Tool Manager, Settings, Diagnostics  
> Depends on: F101 - External Tool Dependency Manager

# Objetivo

Implementar download seguro pela internet para ferramentas externas do WiiStudio,
fechando o fluxo do `ExternalToolDownloader`.

Hoje o downloader aceita pacote local, mas ainda não baixa pacotes remotos de forma
segura. Esta feature define download HTTPS com confirmação do usuário, checksum
obrigatório, fallback manual, progresso e testes sem internet real.

---

# Problema Atual

O `ExternalToolDownloader` ainda não cobre o fluxo completo:

```text
Baixar pacote oficial
    ↓
Validar origem
    ↓
Validar checksum
    ↓
Entregar pacote ao installer
```

Sem isso, o External Tool Manager depende de instalação manual.

---

# Resultado Esperado

Quando o usuário clicar em **Instalar automaticamente**:

```text
Mostrar origem, versão e checksum
    ↓
Usuário confirma
    ↓
Baixar via HTTPS
    ↓
Validar tamanho
    ↓
Validar SHA-256
    ↓
Salvar pacote temporário
    ↓
Entregar ao installer
```

---

# Escopo

## Incluído

- download HTTPS;
- confirmação do usuário;
- validação de origem confiável;
- validação de SHA-256;
- limite de tamanho;
- timeout;
- cancelamento;
- progresso;
- retry controlado;
- fallback manual;
- integração com External Tool Manager;
- integração com Task Manager;
- integração com Diagnostics;
- testes com HTTP fake.

## Fora do Escopo

- baixar por HTTP;
- baixar de mirror desconhecido;
- executar pacote baixado;
- instalar silenciosamente;
- baixar em pipeline de testes;
- aceitar checksum ausente para auto-install.

---

# Contrato

## IExternalToolNetworkDownloader

```csharp
public interface IExternalToolNetworkDownloader
{
    Task<DownloadedToolPackage> DownloadAsync(
        ExternalToolPackageSource source,
        IProgress<ExternalToolDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

---

## ExternalToolDownloadProgress

```csharp
public sealed class ExternalToolDownloadProgress
{
    public long? TotalBytes { get; init; }
    public long DownloadedBytes { get; init; }
    public double? Percentage { get; init; }
    public string? CurrentStatusKey { get; init; }
}
```

---

## ExternalToolDownloadState

```csharp
public enum ExternalToolDownloadState
{
    Pending,
    Confirming,
    Downloading,
    Downloaded,
    ValidatingChecksum,
    ChecksumValid,
    ChecksumInvalid,
    Cancelled,
    Failed,
    Offline
}
```

---

# Fonte Confiável

O download só pode iniciar se:

- `DownloadUrl` for HTTPS;
- `Sha256` estiver preenchido;
- `IsTrusted = true`;
- `ToolId` existir no manifesto interno;
- plataforma atual for suportada;
- usuário confirmar.

Manifesto recomendado:

```text
trusted-tools.json
```

---

# Validações Antes do Download

Validar:

- URL HTTPS;
- host permitido;
- checksum presente;
- versão presente;
- toolId conhecido;
- plataforma compatível;
- destino temporário seguro;
- tamanho máximo configurado;
- usuário confirmou.

---

# Validações Durante o Download

Durante o download:

- respeitar timeout;
- respeitar CancellationToken;
- reportar progresso;
- limitar tamanho máximo;
- interromper se exceder o limite;
- registrar status seguro.

---

# Validações Após Download

Após o download:

- calcular SHA-256;
- comparar com checksum esperado;
- bloquear se inválido;
- não entregar pacote ao installer em falha;
- registrar diagnóstico;
- limpar temporários quando seguro.

---

# Limites

Configuração inicial:

```text
ExternalTools.MaxDownloadBytes = 200 MB
ExternalTools.DownloadTimeoutSeconds = 120
```

Se exceder:

```text
DownloadTooLarge
```

---

# Segurança

Obrigatório:

- somente HTTPS;
- checksum obrigatório;
- não executar pacote baixado;
- salvar em diretório temporário controlado;
- validar tamanho;
- redigir logs;
- não seguir redirect para host não confiável sem política;
- não instalar silenciosamente;
- não baixar em background sem usuário solicitar;
- não permitir auto-install sem checksum pinado.

---

# Fallback Manual

Se download falhar ou estiver offline:

```text
Não foi possível baixar automaticamente.
Você pode selecionar o pacote ou executável manualmente.
```

A UI deve oferecer:

- selecionar pacote local;
- selecionar executável;
- abrir instruções;
- tentar novamente.

---

# Integração com Task Manager

Downloads devem aparecer como tarefa:

```text
Baixando Wiimms ISO Tools
Progresso: 42%
Cancelar
```

O cancelamento deve interromper o download e limpar arquivo parcial quando seguro.

---

# Integração com Diagnostics

Exibir:

```text
External Tool Downloader
Last download: ...
Last URL host: ...
Last result: ...
Checksum: valid/invalid
Offline: true/false
Last error: ...
```

---

# Integração com External Tool Manager

Fluxo completo:

```text
ExternalToolManager.InstallAsync
    ↓
Confirm user action
    ↓
ExternalToolNetworkDownloader.DownloadAsync
    ↓
Checksum validation
    ↓
ArchiveExtractor
    ↓
Tool validation
    ↓
Save Settings
```

---

# Tratamento de Erros

## Offline

Resultado:

```text
Offline
```

Ação:

- mostrar fallback manual;
- não tratar como falha crítica;
- permitir tentar novamente.

## Checksum Inválido

Resultado:

```text
ChecksumInvalid
```

Ação:

- bloquear instalação;
- remover pacote;
- registrar erro;
- orientar tentar novamente ou instalação manual.

## Timeout

Resultado:

```text
Failed/Timeout
```

Ação:

- cancelar download;
- limpar parcial;
- permitir retry.

## Tamanho Excedido

Resultado:

```text
DownloadTooLarge
```

Ação:

- interromper;
- limpar parcial;
- registrar diagnóstico.

---

# Testes Esperados

## Unitários

- URL HTTP bloqueada;
- HTTPS permitido;
- checksum ausente bloqueia;
- origem não confiável bloqueia;
- tamanho excedido bloqueia;
- checksum correto;
- checksum inválido;
- timeout;
- cancelamento;
- offline;
- redirect inseguro bloqueado.

## Integração com Fakes

- HTTP fake retorna pacote;
- HTTP fake retorna erro;
- HTTP fake retorna stream grande;
- progress reportado;
- Task Manager atualizado;
- Diagnostics atualizado;
- arquivo parcial limpo.

---

# Critérios de Aceite

- [ ] Downloader baixa pacote via HTTPS.
- [ ] Usuário precisa confirmar antes.
- [ ] Checksum é obrigatório.
- [ ] Checksum inválido bloqueia instalação.
- [ ] Fallback manual disponível.
- [ ] Progresso aparece no Task Manager.
- [ ] Cancelamento funciona.
- [ ] Offline é tratado.
- [ ] Testes não usam internet real.

---

# Regras para o Codex

- Nunca baixar por HTTP.
- Nunca baixar sem confirmação.
- Nunca instalar sem checksum.
- Nunca executar pacote baixado.
- Sempre usar temp seguro.
- Sempre validar SHA-256.
- Sempre permitir cancelamento.
- Sempre usar HTTP fake em testes.
- Nunca depender de internet real no pipeline.

---

# Dependências

- `F101-ExternalToolDependencyManager.md`
- `F083-ExternalProcessSecurity.md`
- `F086-SafePathHandling.md`
- `F088-LogRedaction.md`
- `F089-UpdateSecurity.md`
- `F090-SecurityChecklist.md`

---

# Definition of Done

- Download HTTPS seguro definido.
- Checksum obrigatório definido.
- Fallback manual definido.
- Task Manager e Diagnostics integrados.
- Testes com fakes definidos.

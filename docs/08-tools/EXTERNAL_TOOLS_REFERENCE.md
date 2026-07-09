# External Tools Reference

Implementação inicial da feature `F101-ExternalToolDependencyManager.md`.

## Contratos

- `IExternalToolManager`
- `IExternalToolLocator`
- `IExternalToolInstaller`
- `IExternalToolDownloader`
- `IChecksumValidator`
- `IArchiveExtractor`

Os contratos ficam em `src/WiiStudio.Application/ExternalTools/`.

## Implementação

- `ExternalToolManager`: orquestra detecção, configuração manual e instalação.
- `ExternalToolLocator`: localiza paths em Settings, PATH e diretórios oficiais.
- `ExternalToolInstaller`: instala pacote local após confirmação e checksum.
- `ExternalToolDownloader`: aceita pacote local e bloqueia download remoto por padrão.
- `ChecksumValidator`: valida SHA-256.
- `ZipArchiveExtractor`: extrai zip com bloqueio de zip slip.
- `NKitService`: integra `.nkit.iso` com o gerenciador de dependências externas.

## NKit

O pacote local pinado é:

```text
tools/packages/NKit_v1.4.zip
```

SHA-256:

```text
e600b0a2dbacf784779ce33b01259f197c5f8cfc7135f1831160d3f98bb431c6
```

A instalação local exige confirmação do usuário e validação de checksum.

## Wiimms

`WiimmsToolLocator` agora procura em:

1. caminho configurado;
2. PATH;
3. `tools/wiimms/`;
4. `tools/`;
5. pasta da aplicação.

## Segurança

- Nenhuma instalação silenciosa.
- Nenhum download remoto implementado por padrão.
- Checksum obrigatório antes da extração.
- Extração bloqueia path traversal.
- Validação de versão passa por `IProcessRunner`.

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

### Linux: cache de autorização Polkit para WBFS raw

O WiiStudio acessa partições WBFS raw por meio do `wwt`. Quando o acesso direto
ao dispositivo retorna `Permission denied`, a aplicação usa `pkexec` para pedir
autorização do sistema.

Por segurança, o WiiStudio **não armazena senha**. Para evitar que o usuário
digite a senha a cada processo, existe um helper Linux que instala uma action
Polkit própria para o executável `wwt`, usando `auth_admin_keep`.

Instalação uma vez:

```bash
tools/linux/install-wiistudio-polkit-policy.sh
```

Se o `wwt` não estiver no `PATH`:

```bash
tools/linux/install-wiistudio-polkit-policy.sh /caminho/para/wwt
```

O script cria:

```text
/usr/share/polkit-1/actions/com.wiistudio.wwt.policy
```

Depois disso, as chamadas `pkexec /caminho/para/wwt ...` usadas pelo WiiStudio
podem reutilizar a autorização enquanto o Polkit mantiver o cache ativo. A
duração exata é controlada pela distribuição Linux e pelo agente Polkit da
sessão gráfica.

## Segurança

- Nenhuma instalação silenciosa.
- Nenhum download remoto implementado por padrão.
- Checksum obrigatório antes da extração.
- Extração bloqueia path traversal.
- Validação de versão passa por `IProcessRunner`.

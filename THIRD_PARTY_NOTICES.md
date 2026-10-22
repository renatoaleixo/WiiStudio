# Avisos de software de terceiros

O WiiStudio é licenciado sob GPL-3.0-only. Essa licença cobre o código original
do WiiStudio, mas não substitui as licenças dos componentes listados abaixo.

## NKit Legacy 1.4

- Projeto: NKit v1
- Origem: https://github.com/Nanook/NKitv1
- Autor: Nanook
- Licença: MIT
- Uso no WiiStudio: pacote legado opcional em `tools/packages/NKit_v1.4.zip`

O pacote distribuído pelo WiiStudio contém `LICENSE-NKIT.txt` com o texto da
licença e o aviso de copyright originais.

## SharpCompress 0.22.0

- Projeto: SharpCompress
- Origem: https://github.com/adamhathcock/sharpcompress
- Autor: Adam Hathcock
- Licença: MIT
- Uso no WiiStudio: biblioteca modificada incluída pelo NKit Legacy 1.4

O pacote distribuído pelo WiiStudio contém `LICENSE-SHARPCOMPRESS.txt` com o
texto da licença e o aviso de copyright originais. A inclusão deste aviso não
significa que os autores do SharpCompress endossem o WiiStudio.

## Pacotes NuGet

O aplicativo também utiliza pacotes NuGet, incluindo Avalonia, .NET Extensions,
CommunityToolkit.Mvvm, FluentValidation, Serilog, SQLitePCLRaw e Tmds.DBus.
Cada pacote permanece sujeito à licença publicada pelo respectivo autor no
pacote e em seu repositório de origem. A relação exata de versões resolvidas
pode ser obtida com:

```bash
dotnet list WiiStudio.sln package --include-transitive
```

## Ferramentas externas

Dolphin Emulator, Wiimms ISO Tools, GameTDB e demais ferramentas ou serviços
detectados pelo WiiStudio não passam a fazer parte do WiiStudio. Quando não
estiverem incorporados ao instalador, sua instalação e utilização continuam
sujeitas aos termos dos respectivos projetos.

# WiiStudio - Roadmap de Integração com Emulador [Melhoramento]

> Pasta sugerida: `docs/09-emulator-integration`  
> Estratégia recomendada: integrar Dolphin como ferramenta externa opcional, sem embutir o core do emulador no WiiStudio.

## Features

1. `F127 - Dolphin Emulator Integration [Melhoramento]`
   - Detectar/configurar Dolphin.
   - Validar versão.
   - Exibir diagnóstico.
   - Botão básico para abrir no emulador.

2. `F128 - Emulator Game Launch Workflow [Melhoramento]`
   - Fluxo completo de execução.
   - Preview.
   - Validação de arquivo/formato.
   - Histórico e notificações.

3. `F129 - Emulator Profiles and Controller Mapping [Melhoramento]`
   - Perfis de emulação.
   - Preferências de controle.
   - Associação por jogo.

4. `F130 - Emulator Compatibility and Save Management [Melhoramento]`
   - Histórico de execução.
   - Compatibilidade local.
   - Notas.
   - Saves e backup seguro.

## Regras Globais

- Não distribuir jogos, ROMs ou ISOs.
- Não embutir Dolphin inicialmente.
- Não executar comandos arbitrários.
- Não armazenar senha.
- Usar `IProcessRunner`.
- Usar catálogo de argumentos.
- Respeitar licença do Dolphin se o binário for distribuído no futuro.
- Manter WiiStudio como gerenciador e Dolphin como runtime opcional.

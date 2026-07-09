# 08-tools

Documentos para gerenciamento de dependências externas do WiiStudio.

# Documentos

- `F101-ExternalToolDependencyManager.md`

# Pacotes Locais

Pacotes binários de terceiros não devem ficar em `docs/`.

O pacote legado NKit v1.4 fica em:

```text
tools/packages/NKit_v1.4.zip
```

Ele deve ser tratado como pacote local pinado pelo projeto:

- validar SHA-256 antes de extrair;
- solicitar confirmação do usuário antes de instalar;
- não executar automaticamente;
- não usar como download remoto automático.

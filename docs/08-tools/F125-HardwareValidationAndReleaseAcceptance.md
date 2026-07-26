# F125 - Hardware Validation and Release Acceptance

> Feature ID: F125  
> Status: Implemented and Homologated  
> Priority: Critical  
> Epic: Production Readiness / QA / Release Acceptance  
> Area: Hardware Testing, Wii, USB, Cross-Platform, Release Gates  
> Depends on: F120, F124

# Objetivo

Definir o plano de homologação em hardware real para considerar o WiiStudio pronto
para release.

Esta feature cobre testes manuais/controlados com:

- HD WBFS real;
- FAT32 com pasta `/wbfs`;
- pendrive;
- Wii real;
- Windows/Linux/macOS;
- permissões administrativas;
- Wiimms;
- capas/metadados;
- fluxos seguros.

---

# Princípio

Testes automatizados nunca devem formatar discos reais.

Homologação em hardware real deve ocorrer em ambiente controlado, com dispositivos
dedicados para teste e sem dados importantes.

---

# Matriz de Hardware

## Dispositivos USB

- HD externo 2.5" USB;
- pendrive USB 16 GB ou maior;
- dispositivo já formatado WBFS;
- dispositivo FAT32 com `/wbfs`;
- dispositivo NTFS, se suportado pelo fluxo de leitura/cópia.

## Console

- Wii real desbloqueado/modificado;
- USB Loader GX ou loader equivalente;
- cartão SD quando necessário;
- fonte/cabo USB estável.

---

# Matriz de Sistema Operacional

Testar em:

```text
Linux Mint/Ubuntu
Windows 10/11
macOS atual suportado
```

Linux:

- com policy Polkit instalada;
- sem policy Polkit;
- usuário admin;
- usuário sem permissão.

Windows:

- app sem administrador;
- app como administrador;
- disco removível;
- falha de permissão.

macOS:

- app normal;
- permissões de disco;
- restrições de sandbox/notarização quando aplicável.

---

# Cenários de Validação

## 1. Detecção de Dispositivo

- conectar HD/pendrive;
- abrir Device Manager;
- confirmar que dispositivo aparece;
- confirmar partições;
- confirmar filesystem;
- confirmar espaço livre.

Aceite:

```text
Dispositivo correto aparece e disco do sistema não aparece como alvo formatável.
```

---

## 2. WBFS Real

- conectar HD/partição WBFS;
- detectar com WiiStudio;
- listar jogos;
- validar permissões;
- abrir readiness check;
- confirmar status.

Aceite:

```text
WBFS detectado, jogos listados e nenhum erro crítico.
```

---

## 3. FAT32 com /wbfs

- preparar FAT32 com `/wbfs`;
- copiar jogo usando fluxo seguro;
- validar split se jogo > 4 GB;
- testar no Wii real.

Aceite:

```text
Jogo aparece no loader do Wii e executa.
```

---

## 4. Pendrive

- detectar pendrive;
- preparar FAT32 `/wbfs`;
- copiar jogo pequeno;
- testar leitura no Wii.

Aceite:

```text
Pendrive é detectado e jogo funciona no loader.
```

---

## 5. Formatação WBFS

Executar apenas em dispositivo dedicado de teste.

- escolher dispositivo;
- verificar bloqueio de disco do sistema;
- ver preview;
- digitar `FORMATAR WBFS`;
- formatar;
- validar pós-formatação;
- copiar jogo;
- testar no Wii.

Aceite:

```text
WBFS criado, validado, jogo copiado e reconhecido pelo Wii.
```

---

## 6. Permissões Administrativas

Linux:

- sem policy: diagnóstico informa;
- instalar policy via F106;
- retry;
- operação funciona.

Windows:

- sem admin: informa necessidade;
- como admin: operação funciona.

macOS:

- sem permissão: informa;
- após autorização: operação funciona ou limitação documentada.

---

## 7. Capas e Metadados

- baixar capa via GameTDB;
- sincronizar metadados;
- abrir detalhes avançados;
- validar galeria.

Aceite:

```text
Capa, disc art e metadados aparecem corretamente quando disponíveis.
```

---

## 8. Atualização e Release

- verificar manifest;
- detectar versão;
- baixar pacote fake/controlado;
- validar checksum;
- não instalar pacote inválido.

Aceite:

```text
Update Manager bloqueia pacote inválido e aceita pacote válido.
```

---

# Critérios de Release

WiiStudio só pode ser considerado pronto para release quando:

- app inicia em Linux/Windows/macOS;
- biblioteca persiste;
- Wiimms detectado/configurado;
- GameTDB funciona;
- capas/metadados funcionam;
- USB provider detecta dispositivos;
- permissões são diagnosticadas corretamente;
- safe write workflow bloqueia riscos;
- formatação WBFS bloqueia disco do sistema;
- readiness check funciona;
- tarefas/notificações aparecem;
- installers/artefatos são gerados;
- testes automáticos passam;
- homologação manual em hardware real é aprovada.

---

# Release Acceptance Checklist

```text
[x] Build Release aprovado
[x] Testes unitários aprovados
[x] Testes integração com fakes aprovados
[x] Smoke test Linux aprovado
[x] Smoke test Windows aprovado
[x] Smoke test macOS aprovado
[x] HD WBFS real aprovado
[x] FAT32 /wbfs aprovado
[x] Pendrive aprovado
[x] Wii real aprovado
[x] Polkit Linux aprovado
[x] Windows Admin aprovado
[x] macOS permissões documentadas/aprovadas
[x] Wiimms operações principais aprovadas
[x] GameTDB aprovado
[x] Update manifest aprovado
[x] Packaging aprovado
[x] Sem operação destrutiva sem confirmação
```

---

# Evidências de Homologação

Salvar evidências:

```text
release-validation/
├── version.txt
├── environment-linux.md
├── environment-windows.md
├── environment-macos.md
├── hardware-matrix.md
├── screenshots/
├── logs/
└── acceptance-report.md
```

---

# Riscos Conhecidos

Documentar antes da release:

- limitações macOS;
- necessidade de admin no Windows;
- necessidade de Polkit no Linux;
- WBFS é destrutivo;
- FAT32 recomendado;
- compatibilidade varia por loader Wii;
- dispositivos USB problemáticos podem falhar.

---

# Critérios de Aceite

- [x] Plano de hardware real definido.
- [x] HD WBFS real coberto.
- [x] FAT32 `/wbfs` coberto.
- [x] Pendrive coberto.
- [x] Wii real coberto.
- [x] Windows/Linux/macOS cobertos.
- [x] Permissões administrativas cobertas.
- [x] Checklist de release definido.
- [x] Evidências de homologação definidas.
- [x] Critérios de pronto para release definidos.

---

# Regras para o Codex

- Não substituir homologação real por testes fakes.
- Não marcar release como pronta sem evidência.
- Não executar operação destrutiva fora de dispositivo dedicado.
- Não ignorar falhas de permissão.
- Sempre registrar ambiente e logs.
- Sempre validar Wii real antes de release final.

---

# Definition of Done

- Plano de homologação definido.
- Matriz de hardware definida.
- Matriz de SO definida.
- Cenários críticos definidos.
- Checklist de release definido.
- Evidências definidas.

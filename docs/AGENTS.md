# AGENTS.md

## Papel da IA

Você é um engenheiro de software responsável por implementar o WiiStudio respeitando integralmente esta especificação.

## Regras

1. Nunca quebrar a Clean Architecture.
2. Nunca acessar Infrastructure diretamente pela UI.
3. Toda ViewModel deve utilizar MVVM.
4. Todo serviço deve ser registrado por DI.
5. Usar async/await sempre que possível.
6. Receber CancellationToken em operações longas.
7. Não duplicar código.
8. Escrever testes unitários.
9. Manter código legível.
10. Não adicionar dependências sem justificativa.

## Convenções

- Classes: PascalCase
- Métodos: PascalCase
- Campos privados: _camelCase
- Interfaces: prefixo I
- Nullable habilitado
- Warnings tratados como erros

## Commits

Conventional Commits.

## Branches

feature/
bugfix/
hotfix/
release/

## Objetivo

Criar um software open source de qualidade profissional.

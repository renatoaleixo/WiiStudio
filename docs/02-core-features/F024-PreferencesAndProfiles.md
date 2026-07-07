# F024 - Preferences & Profiles

> Feature ID: F024 Status: Ready for Development Priority: High Epic:
> Settings & User Experience

# Objetivo

Permitir que diferentes usuários ou contextos utilizem perfis
independentes de configuração, facilitando alternância de preferências,
portabilidade e personalização do WiiStudio.

------------------------------------------------------------------------

# Requisitos Funcionais

## RF001

Criar perfis.

## RF002

Editar perfis.

## RF003

Excluir perfis.

## RF004

Selecionar perfil ativo.

## RF005

Exportar e importar perfis.

## RF006

Duplicar perfis existentes.

------------------------------------------------------------------------

# Escopo do Perfil

-   Configurações gerais
-   Tema
-   Idioma
-   Diretórios
-   Preferências de conversão
-   Preferências de download
-   Layout da biblioteca
-   Atalhos

------------------------------------------------------------------------

# Requisitos Não Funcionais

-   Troca de perfil em menos de 2 segundos.
-   Persistência automática.
-   Compatível com futuras contas online.

------------------------------------------------------------------------

# Fluxo do Usuário

``` text
Abrir Perfis
      │
Criar/Selecionar Perfil
      │
Aplicar Configurações
      │
Continuar utilizando a aplicação
```

------------------------------------------------------------------------

# Fluxo Técnico

``` text
ProfilesView
      │
ProfilesViewModel
      │
IProfileService
      │
ProfileRepository
      │
profiles/
```

------------------------------------------------------------------------

# Estrutura

``` text
profiles/
├── Default/
│   └── profile.json
├── Gaming/
│   └── profile.json
└── Development/
    └── profile.json
```

------------------------------------------------------------------------

# Máquina de Estados

-   Loading
-   Selecting
-   Editing
-   Saving
-   Applied
-   Failed

------------------------------------------------------------------------

# Contrato

``` csharp
public interface IProfileService
{
    Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken);
    Task<UserProfile> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken);
    Task SetActiveAsync(Guid profileId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid profileId, CancellationToken cancellationToken);
    Task ExportAsync(Guid profileId, string filePath, CancellationToken cancellationToken);
    Task ImportAsync(string filePath, CancellationToken cancellationToken);
}
```

------------------------------------------------------------------------

# Regras de Negócio

-   Sempre deve existir um perfil ativo.
-   O perfil padrão não pode ser excluído.
-   Alterações são persistidas automaticamente.
-   Perfis incompatíveis devem ser migrados quando possível.

------------------------------------------------------------------------

# Eventos

-   ProfileCreated
-   ProfileUpdated
-   ProfileDeleted
-   ProfileActivated
-   ProfileImported
-   ProfileExported

------------------------------------------------------------------------

# Interface

-   Lista de perfis
-   Perfil ativo destacado
-   Criar
-   Duplicar
-   Renomear
-   Excluir
-   Importar
-   Exportar

------------------------------------------------------------------------

# Casos de Erro

-   Nome duplicado.
-   Arquivo inválido.
-   Perfil incompatível.
-   Falha na persistência.

------------------------------------------------------------------------

# Critérios de Aceite

-   [ ] Criar perfis.
-   [ ] Alternar perfis.
-   [ ] Persistência automática.
-   [ ] Importação/exportação.
-   [ ] Migração entre versões.

------------------------------------------------------------------------

# Testes

## Unitários

-   Criação.
-   Exclusão.
-   Ativação.
-   Exportação.
-   Importação.

## Integração

-   Troca de perfil.
-   Reinício da aplicação.
-   Perfil legado.
-   Arquivo corrompido.

------------------------------------------------------------------------

# Dependências

-   F023-SettingsManager.md
-   021-ThemeSystem.md
-   022-Localization.md

------------------------------------------------------------------------

# Definition of Done

-   Serviço implementado.
-   Persistência validada.
-   Testes aprovados.
-   Logs estruturados.
-   Documentação atualizada.

# 022 - Localization Specification

> Status: Accepted\
> Version: 1.0.0

# Objetivo

Definir a arquitetura de internacionalização (i18n) e localização (l10n)
do WiiStudio para suportar múltiplos idiomas sem alterar código.

------------------------------------------------------------------------

# Objetivos

-   Interface multilíngue.
-   Troca de idioma em tempo real.
-   Recursos centralizados.
-   Suporte a expansão futura.

------------------------------------------------------------------------

# Idiomas Iniciais

  Idioma               Código
  -------------------- --------
  Português (Brasil)   pt-BR
  Inglês               en-US

Futuros:

-   es-ES
-   fr-FR
-   de-DE
-   ja-JP

------------------------------------------------------------------------

# Arquitetura

``` text
Presentation
    │
ILocalizationService
    │
LocalizationManager
    │
Resources/
 ├── pt-BR.axaml
 ├── en-US.axaml
 └── ...
```

------------------------------------------------------------------------

# Estrutura

``` text
assets/
└── localization/
    ├── pt-BR/
    ├── en-US/
    └── shared/
```

------------------------------------------------------------------------

# Regras

-   Nenhum texto literal em Views ou ViewModels.
-   Toda string deve possuir chave.
-   Chaves devem ser estáveis.

Exemplo:

``` text
Library.Import
Library.Remove
Settings.Language
Diagnostics.Run
```

------------------------------------------------------------------------

# Interface

``` csharp
public interface ILocalizationService
{
    string GetString(string key);

    Task SetCultureAsync(string culture);

    IReadOnlyList<CultureInfo> GetAvailableCultures();
}
```

------------------------------------------------------------------------

# Persistência

Salvar idioma em Settings.

Na inicialização:

1.  Ler configuração.
2.  Validar idioma.
3.  Aplicar cultura.
4.  Atualizar UI.

------------------------------------------------------------------------

# Datas e Números

Devem respeitar a cultura selecionada.

Nunca formatar manualmente.

------------------------------------------------------------------------

# Fallback

Se uma chave não existir:

1.  usar en-US;
2.  registrar warning no log.

------------------------------------------------------------------------

# Testes

Validar:

-   troca de idioma;
-   fallback;
-   chaves inexistentes;
-   atualização dinâmica.

------------------------------------------------------------------------

# Regras para o Codex

-   Nunca adicionar texto fixo.
-   Toda nova tela deve incluir recursos de localização.
-   Toda chave nova deve existir em todos os idiomas suportados.

------------------------------------------------------------------------

# Critérios de Aceite

-   Mudança de idioma sem reiniciar.
-   Recursos centralizados.
-   Fallback funcionando.
-   Compatível com expansão futura.

------------------------------------------------------------------------

# Checklist

-   [x] Serviço definido
-   [x] Estrutura criada
-   [x] Persistência documentada
-   [x] Fallback definido
-   [x] Testes especificados

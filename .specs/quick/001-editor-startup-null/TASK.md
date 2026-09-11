# Quick Task 001: Corrigir falha ao abrir o editor

**Date:** 2026-09-11
**Status:** Done

## Description

Impedir que o evento inicial da ferramenta Selecionar acesse controles ainda não criados durante o carregamento do XAML.

## Files Changed

- `src/Firaw.SnapCopyText/Views/EditorWindow.xaml.cs` — protege o evento durante a inicialização.

## Verification

- [x] Instanciar e exibir o editor com uma captura real sem exceção.
- [x] Executar todos os testes automatizados.
- [x] Republicar e iniciar o executável corrigido.

## Commit

`fix(editor): avoid startup event before controls load`

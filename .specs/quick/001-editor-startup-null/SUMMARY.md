# Quick Task 001 Summary

O evento `Checked` da ferramenta inicial era executado pelo carregador XAML antes da criação do controle de status. O manipulador agora retorna durante essa fase incompleta e mantém a ferramenta padrão definida pelo campo `_currentTool`.

**Verification:** o serviço capturou o desktop virtual, o editor foi criado e exibido com um recorte real sem exceção, os oito testes automatizados passaram, e o executável corrigido foi republicado e reaberto.

**Commit:** `fix(editor): avoid startup event before controls load`

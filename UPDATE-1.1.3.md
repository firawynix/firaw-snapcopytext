# Firaw - SnapCopyText 1.1.3

**Data:** 11 de setembro de 2026  
**Tipo:** correção da gravação de Print Screen nas preferências

## O que foi corrigido

- Os atalhos globais do Firaw são suspensos temporariamente ao abrir **Atalhos e preferências**.
- A janela de preferências registra o Print Screen diretamente enquanto o campo de atalho está em foco.
- O campo também observa a soltura da tecla como alternativa para teclados que não entregam o pressionamento normal.
- Ao salvar, cancelar ou fechar pelo **X**, os atalhos globais são restaurados automaticamente.
- O status continua avisando quando outro aplicativo ou o Windows ocupa o Print Screen.

## Resultado verificado

- A opção nativa **Usar Print Screen para abrir a captura de tela** está desativada neste computador.
- O Print Screen voltou a acionar o Firaw sem abrir a captura nativa do Windows.
- 30 de 30 testes automatizados passaram antes do empacotamento.
- Os aplicativos x64 e x86 foram abertos em modo de segundo plano e responderam corretamente.
- Aplicativo e launcher das duas arquiteturas estão na versão `1.1.3.0`.

## Caminhos

- Código: `C:\Users\Hugo\Firaw-SnapCopyText`
- Pacote local: `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.3\github-release`
- Atualização automática: `https://github.com/firawynix/firaw-snapcopytext/releases/latest/download/update.json`
- Release: `https://github.com/firawynix/firaw-snapcopytext/releases/tag/v1.1.3`

## Arquivos e hashes

- `Firaw-SnapCopyText-Setup-x64.exe`
  - Tamanho: `65.403.181 bytes`
  - SHA-256: `c686bdfe4bf7905bf2b357f01a1ee12b492e2ca2f528dfbeded5c5aa1449bb20`
- `Firaw-SnapCopyText-Setup-x86.exe`
  - Tamanho: `58.673.970 bytes`
  - SHA-256: `0a279f7f6ba02bd137d4c9d664ee76dea5caf5808ddb69fdc0dad4fab0457b9e`

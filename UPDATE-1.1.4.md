# Firaw - SnapCopyText 1.1.4

**Data:** 11 de setembro de 2026  
**Tipo:** novo perfil de atalhos Print Screen

## Atalhos do Firaw

- `Print Screen`: selecionar uma região.
- `Alt + Print Screen`: escolher e capturar um monitor.
- `Ctrl + Print Screen`: escolher e capturar uma janela.

Cada linha das preferências permite escolher entre o modo **Firaw** e o comportamento **Windows original**. O botão **Aplicar os 3 do Firaw** ativa o perfil completo; **Usar os 3 originais do Windows** devolve todas as combinações ao sistema.

## Conflitos com o Windows

- O Firaw passou a observar diretamente a tecla Print Screen, usando o registro global anterior apenas como alternativa.
- Somente as combinações atribuídas ao Firaw são bloqueadas; as demais continuam chegando ao Windows e a outros aplicativos.
- **Liberar Print Screen no Windows** desativa o recorte nativo para essa tecla no usuário atual e marca a combinação para o Firaw.
- **Restaurar recorte do Windows** reativa o comportamento nativo e devolve a combinação ao Windows após salvar.
- Alt e Ctrl não exigem alteração separada no Registro: a escolha Firaw/Windows de cada linha controla se o evento é tratado ou repassado.

## Compatibilidade

- As configurações anteriores continuam válidas.
- Ao abrir um arquivo de preferências antigo, Alt + Print Screen e Ctrl + Print Screen iniciam habilitados no perfil Firaw.
- O atalho personalizado continua disponível como combinação adicional e mantém o modo configurável.

## Verificações

- `Print Screen` abriu a sobreposição de seleção ajustável.
- `Alt + Print Screen` abriu a lista com os três monitores disponíveis neste computador.
- `Ctrl + Print Screen` abriu a lista de janelas disponíveis.
- 39 testes automatizados aprovados.

## Caminhos

- Código: `C:\Users\Hugo\Firaw-SnapCopyText`
- Pacote local: `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.4\github-release`
- Atualização automática: `https://github.com/firawynix/firaw-snapcopytext/releases/latest/download/update.json`
- Release: `https://github.com/firawynix/firaw-snapcopytext/releases/tag/v1.1.4`

## Arquivos e hashes

- `Firaw-SnapCopyText-Setup-x64.exe`
  - Tamanho: `65.412.111 bytes`
  - SHA-256: `bb38f2aed71766124c60115c1b2b19378985e70e84b2929f322f185d033dc1c6`
- `Firaw-SnapCopyText-Setup-x86.exe`
  - Tamanho: `58.687.136 bytes`
  - SHA-256: `7356bf926cb28b1f8e51bce48c2e117f23ff1ed1ee8fe6323d239d50534abda6`

# Firaw - SnapCopyText

Capturador de tela para Windows com edição rápida e extração local de texto em português e inglês.

## Abrir a versão pronta

Execute:

`artifacts\Firaw-SnapCopyText\Firaw.SnapCopyText.exe`

O pacote publicado é autocontido para Windows x64. Ele não precisa do SDK .NET instalado.

## Como usar

1. Abra o Firaw - SnapCopyText.
2. Clique em **Nova captura**, pressione **Print Screen** ou use **Ctrl + Shift + S**.
3. Arraste sobre a região desejada e solte o mouse.
4. Mova a região pela parte interna ou redimensione usando os oito pontos cianos; confirme em **Abrir editor** ou pressione **Enter**.
5. No editor, faça anotações e escolha **Copiar imagem**, **Salvar PNG**, **Copiar texto** ou **Selecionar texto**.
6. **Copiar texto** envia todo o OCR direto à área de transferência. Em **Selecionar texto**, arraste uma moldura ciana sobre o texto na própria imagem; ao soltar, somente aquele trecho é reconhecido e copiado, sem abrir outra janela.
7. Abra **Textos** para ver o histórico desta execução. Use Ctrl ou Shift para marcar vários trechos e **Copiar selecionados** para juntá-los.

Ao fechar ou minimizar a janela principal, o Firaw continua ativo no Olho de Hórus da bandeja do Windows. Clique duas vezes para abrir; clique com o botão direito para abrir, iniciar uma captura ou sair.

Se o Lightshot ou o Recorte do Windows estiver usando `Print Screen`, o Firaw mostra o conflito e mantém o atalho alternativo e o botão disponíveis.

## Ferramentas incluídas

- Caneta, seta, retângulo e marca-texto.
- Texto com diálogo no tema Firaw.
- Caixa de anotação escura com borda e texto na cor escolhida.
- Censura sólida para ocultar dados sensíveis.
- Cor e espessura configuráveis.
- Desfazer e refazer.
- Copiar imagem editada e salvar em PNG.
- Região ajustável em tempo real, com movimentação e oito pontos de redimensionamento.
- Ocultação sincronizada das janelas do Firaw para evitar miniaturas fantasma na captura.
- OCR local em português e inglês, com cópia integral ou seleção ciana diretamente sobre a imagem.
- Histórico de até 100 textos em memória, incluindo cópias feitas em outros programas enquanto o Firaw está ativo.
- Gaveta abre/fecha com importação do clipboard, seleção múltipla e cópia conjunta.
- Ícone próprio do Olho de Hórus e barra de título ciana em versões compatíveis do Windows 11.

## Privacidade

O aplicativo não possui login, galeria online nem upload automático. A captura e o OCR são processados localmente. Somente uma ação explícita do usuário copia ou salva o resultado.

## Desenvolvimento

Requisitos: SDK .NET 9 ou superior no Windows.

```powershell
dotnet restore Firaw.SnapCopyText.sln
dotnet build Firaw.SnapCopyText.sln
dotnet test Firaw.SnapCopyText.sln
dotnet run --project src\Firaw.SnapCopyText\Firaw.SnapCopyText.csproj
```

Publicação autocontida:

```powershell
dotnet publish src\Firaw.SnapCopyText\Firaw.SnapCopyText.csproj -c Release -r win-x64 --self-contained true -o artifacts\Firaw-SnapCopyText
```

## Identidade visual

A cor de destaque fica centralizada em `Themes/FirawTheme.xaml`. O token inicial `FirawCyanColor` é `#19D3E6` e pode ser ajustado uma vez para refletir toda a interface.

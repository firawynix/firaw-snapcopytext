# Firaw - SnapCopyText

Capturador de tela para Windows com edição rápida e extração local de texto em português e inglês.

## Abrir a versão pronta

Execute:

`artifacts\Firaw-SnapCopyText\Firaw.SnapCopyText.exe`

O pacote publicado é autocontido para Windows x64. Ele não precisa do SDK .NET instalado.

## Como usar

1. Abra o Firaw - SnapCopyText.
2. Clique em **Nova captura**, pressione **Print Screen** ou use **Ctrl + Shift + S**.
3. Arraste sobre a região desejada.
4. No editor, faça anotações e escolha **Copiar imagem**, **Salvar PNG** ou **Copiar texto**.
5. Ao copiar texto, revise o resultado reconhecido e confirme a cópia.

Se o Lightshot ou o Recorte do Windows estiver usando `Print Screen`, o Firaw mostra o conflito e mantém o atalho alternativo e o botão disponíveis.

## Ferramentas incluídas

- Caneta, seta, retângulo e marca-texto.
- Texto com diálogo no tema Firaw.
- Censura sólida para ocultar dados sensíveis.
- Cor e espessura configuráveis.
- Desfazer e refazer.
- Copiar imagem editada e salvar em PNG.
- OCR local em português e inglês com prévia editável.

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

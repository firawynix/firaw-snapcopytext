# Firaw - SnapCopyText

Capturador de tela para Windows com edição rápida e extração local de texto em português e inglês.

## Instalar a versão pronta

Baixe em <https://snapcopytext.firawynix.com.br> ou nas [releases do GitHub](https://github.com/firawynix/firaw-snapcopytext/releases/latest). Também aparece na aba Projetos do Firawynix Center.

Use o instalador x64 em computadores atuais ou o x86 para Windows de 32 bits. Os dois pacotes são autocontidos, não precisam do SDK .NET e instalam só para o usuário (sem administrador).

Depois da instalação, o atalho abre `Firaw.SnapCopyText.Launcher.exe`. Ele lê o `update.json` da release mais recente do GitHub (HTTPS), instala uma versão mais nova quando disponível — conferindo tamanho e SHA-256 — e abre normalmente o Firaw se o GitHub estiver fora do alcance.

## Como usar

1. Abra o Firaw - SnapCopyText.
2. Escolha **Selecionar região**, **Escolher janela** ou **Escolher monitor**. Janela e Monitor abrem uma lista no próprio Firaw com nomes e dimensões.
3. Na captura por região, arraste sobre a área desejada e solte o mouse.
4. Mova a região pela parte interna ou redimensione usando os oito pontos cianos; confirme em **Abrir editor** ou pressione **Enter**.
5. No editor, faça anotações e escolha **Copiar imagem**, **Salvar PNG**, **Copiar texto** ou **Selecionar texto**.
6. **Copiar texto** envia todo o OCR direto à área de transferência. Em **Selecionar texto**, arraste uma moldura ciana sobre o texto na própria imagem; ao soltar, somente aquele trecho é reconhecido e copiado, sem abrir outra janela.
7. Abra **Textos** para ver o histórico desta execução. Use Ctrl ou Shift para marcar vários trechos e **Copiar selecionados** para juntá-los.

Em **Atalhos e preferências**, escolha o modo usado pelo atalho, clique no campo e pressione sua própria combinação. `Print Screen` também pode ser gravada sozinha como atalho personalizado. A mesma tela permite ativá-la como atalho adicional e abrir diretamente as configurações de teclado do Windows caso a Ferramenta de Captura esteja ocupando essa tecla.

Nessa tela também é possível marcar **Iniciar o Firaw junto com o Windows**. A configuração vale somente para o usuário atual, não precisa de permissão de administrador e inicia discretamente na bandeja.

Ao fechar ou minimizar a janela principal, o Firaw continua ativo no Olho de Hórus da bandeja do Windows. No olho: clique esquerdo captura uma região; `Ctrl` + clique esquerdo abre as janelas; clique do botão do meio/scroll abre os monitores; botão direito abre o menu completo, inclusive **Abrir Firaw** e **Sair**.

Se o Lightshot ou o Recorte do Windows estiver usando `Print Screen`, o Firaw mostra o conflito e mantém o atalho alternativo e o botão disponíveis.

## Ferramentas incluídas

- Caneta, seta, retângulo e marca-texto.
- Texto com diálogo no tema Firaw.
- Caixa de anotação escura com borda e texto na cor escolhida.
- Censura sólida para ocultar dados sensíveis.
- Paleta visual com oito cores predefinidas e ciano Firaw selecionado inicialmente, além de espessura configurável.
- Desfazer e refazer.
- Copiar imagem editada e salvar em PNG.
- Região ajustável em tempo real, com movimentação e oito pontos de redimensionamento.
- Captura por janela com lista de programas e leitura da janela escolhida mesmo quando outra está à frente.
- Captura por monitor com lista de todas as telas, resolução e indicação do monitor principal.
- Atalho personalizado, modo padrão e uso opcional de Print Screen salvos por usuário.
- Ocultação sincronizada das janelas do Firaw para evitar miniaturas fantasma na captura.
- OCR local em português e inglês, com cópia integral ou seleção ciana diretamente sobre a imagem.
- Histórico de até 100 textos em memória, incluindo cópias feitas em outros programas enquanto o Firaw está ativo.
- Gaveta abre/fecha com importação do clipboard, seleção múltipla e cópia conjunta.
- Ícone próprio do Olho de Hórus e barra de título ciana em versões compatíveis do Windows 11.
- Lançador totalmente ciano, com botões e textos escuros de alto contraste.

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

Release completa com publicações autocontidas, launcher, instaladores Inno e manifesto de atualização:

```powershell
.\tools\build-release.ps1 -Version 1.1.2 -ReleaseNotes "o que mudou"
gh release create v1.1.2 -R firawynix/firaw-snapcopytext --title "Firaw - SnapCopyText 1.1.2" --notes "o que mudou" release\Firaw-SnapCopyText-1.1.2\github-release\*
```

Suba a versão também nos dois `.csproj` (o script grava `FileVersion` no executável, que é o que o launcher compara). A pasta `github-release` leva os dois instaladores, um `.sha256` de cada e o `update.json` — publique os cinco na mesma tag. O Firawynix Center pega a release nova sozinho (timer no servidor que confere o `.sha256`).

O site fica em `demo-site/dist` e é publicado em <https://snapcopytext.firawynix.com.br> (ver `demo-site/deploy/README.md`).

## Identidade visual

A cor de destaque fica centralizada em `Themes/FirawTheme.xaml`. O token inicial `FirawCyanColor` é `#19D3E6` e pode ser ajustado uma vez para refletir toda a interface.

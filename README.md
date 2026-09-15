# Firaw - SnapCopyText

Capturador de tela para Windows e Linux com edição rápida e extração local de texto em português e inglês.

A edição Linux fica em `linux/` e é distribuída como AppImage e `.deb`. Ela usa
a captura do desktop do Electron/PipeWire, atalhos globais e os mesmos modelos
Tesseract locais do projeto; nenhum print é enviado para serviço externo.

## Instalar a versão pronta

Baixe em <https://snapcopytext.firawynix.com.br> ou nas [releases do GitHub](https://github.com/firawynix/firaw-snapcopytext/releases/latest). Também aparece na aba Projetos do Firawynix Center.

Use o instalador x64 em computadores atuais ou o x86 para Windows de 32 bits. Os dois pacotes são autocontidos, não precisam do SDK .NET e instalam só para o usuário (sem administrador).

Depois da instalação, o atalho abre `Firaw.SnapCopyText.Launcher.exe`. Ele lê o `update.json` da release mais recente do GitHub (HTTPS), instala uma versão mais nova quando disponível — conferindo tamanho e SHA-256 — e abre normalmente o Firaw se o GitHub estiver fora do alcance.

## Como usar

1. Abra o Firaw - SnapCopyText.
2. Escolha **1 Região**, **2 Janela** ou **3 Monitor**. Janela e Monitor abrem uma lista no próprio Firaw com nomes e dimensões. Na tela principal, as teclas `1`, `2` e `3` abrem os mesmos modos.
3. Na captura por região, arraste sobre a área desejada e solte o mouse.
4. Mova a região pela parte interna ou redimensione usando os oito pontos cianos.
   Use **1 Selecionar novamente** para refazer o enquadramento, **2 Copiar imagem**
   para enviar a seleção à área de transferência ou **3 Abrir editor** para editar.
   `Ctrl+C` e `Enter` continuam disponíveis.
5. Nas listas de janela e monitor, escolha o alvo e use **Abrir no editor** ou
   **Copiar imagem**. `Ctrl+C` copia o alvo selecionado sem abrir o editor e
   `Esc` cancela a escolha.
6. No editor, use **+ Região**, **+ Janela** ou **+ Monitor** para acrescentar
   outras capturas à composição sem fechar o trabalho. Cada print pode ser
   selecionado, arrastado e redimensionado pelos cantos cianos. Use
   **1 Selecionar** para marcar e mover qualquer objeto, inclusive textos e
   caixas de anotação já criados. As ferramentas seguem de `1` a `0` e podem
   ser escolhidas pelas mesmas teclas. O editor abre na medida exata do primeiro
   recorte e reduz apenas a visualização quando a composição é maior que a área
   útil da tela. Use **Trazer à frente** ou **Enviar atrás** para controlar
   sobreposições. Ao concluir, use **Ajustar saída** para remover margens vazias.
   Pressione `Ctrl+C` para copiar a imagem editada inteira.
7. **Copiar texto** envia todo o OCR direto à área de transferência. Em **Selecionar texto**, arraste uma moldura ciana sobre o texto na própria imagem; ao soltar, somente aquele trecho é reconhecido e copiado, sem abrir outra janela.
8. Abra **Textos** para ver o histórico desta execução. Use Ctrl ou Shift para marcar vários trechos e **Copiar selecionados** para juntá-los.

Em **Atalhos e preferências**, escolha o modo usado pelo atalho, clique no campo e pressione sua própria combinação. `Print Screen` também pode ser gravada sozinha como atalho personalizado. A mesma tela permite ativá-la como atalho adicional e abrir diretamente as configurações de teclado do Windows caso a Ferramenta de Captura esteja ocupando essa tecla.

Nessa tela também é possível marcar **Iniciar o Firaw junto com o Windows**. A configuração vale somente para o usuário atual, não precisa de permissão de administrador e inicia discretamente na bandeja.

Ao fechar ou minimizar a janela principal, o Firaw continua ativo no Olho de Hórus da bandeja do Windows. No olho: clique esquerdo captura uma região; `Ctrl` + clique esquerdo abre as janelas; clique do botão do meio/scroll abre os monitores; botão direito abre o menu completo, inclusive **Abrir Firaw** e **Sair**.

Se o Lightshot ou o Recorte do Windows estiver usando `Print Screen`, o Firaw mostra o conflito e mantém o atalho alternativo e o botão disponíveis.

## Ferramentas incluídas

- Caneta, seta, retângulo e marca-texto.
- Texto com diálogo no tema Firaw.
- Caixa de anotação escura com borda e texto na cor escolhida.
- Censura sólida para ocultar dados sensíveis.
- Seleção por clique ou moldura, com movimentação individual ou em grupo depois da criação.
- Borracha por objeto inteiro, formato circular ou quadrado, com tamanho ajustável.
- Desfoque manual por área e proteção automática de e-mail, telefone, CPF/CNPJ, cartão e endereço IP usando OCR local.
- Paleta visual com oito cores predefinidas e ciano Firaw selecionado inicialmente, além de espessura configurável.
- Desfazer e refazer.
- Copiar imagem editada e salvar em PNG.
- Cópia direta por botão ou `Ctrl+C` na seleção de região, janela e monitor.
- Região ajustável em tempo real, com movimentação e oito pontos de redimensionamento.
- Composição com várias capturas de região, janela ou monitor, lado a lado, movíveis e redimensionáveis individualmente.
- Ordem de camadas entre prints e ajuste automático da saída ao espaço ocupado pela montagem.
- Captura por janela com lista de programas e leitura da janela escolhida mesmo quando outra está à frente.
- Captura por monitor com lista de todas as telas, resolução e indicação do monitor principal.
- Atalho personalizado, modo padrão e uso opcional de Print Screen salvos por usuário.
- Ocultação sincronizada das janelas do Firaw para evitar miniaturas fantasma na captura.
- OCR local em português e inglês, com duas leituras automáticas, ampliação controlada de textos pequenos e escolha do resultado mais confiável.
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
.\tools\build-release.ps1 -Version 1.1.10 -ReleaseNotes "o que mudou"
gh release create v1.1.10 -R firawynix/firaw-snapcopytext --title "Firaw - SnapCopyText 1.1.10" --notes "o que mudou" release\Firaw-SnapCopyText-1.1.10\github-release\*
```

Suba a versão também nos dois `.csproj` (o script grava `FileVersion` no executável, que é o que o launcher compara). A pasta `github-release` leva os dois instaladores, um `.sha256` de cada e o `update.json` — publique os cinco na mesma tag. O Firawynix Center pega a release nova sozinho (timer no servidor que confere o `.sha256`).

O site fica em `demo-site/dist` e é publicado em <https://snapcopytext.firawynix.com.br> (ver `demo-site/deploy/README.md`).

## Identidade visual

A cor de destaque fica centralizada em `Themes/FirawTheme.xaml`. O token inicial `FirawCyanColor` é `#19D3E6` e pode ser ajustado uma vez para refletir toda a interface.

# Camadas e saída ajustável — 1.1.10 (15 de setembro de 2026)

- O print selecionado pode ser trazido para frente ou enviado para trás.
- As anotações permanecem acima de todas as capturas.
- **Ajustar saída** remove margens vazias e usa o tamanho real da montagem.
- As novas operações participam do desfazer/refazer.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.10.md`.

---

# Composição com várias capturas — 1.1.9 (15 de setembro de 2026)

- O mesmo editor agora recebe novas capturas de região, janela e monitor.
- Cada print pode ser movido e redimensionado individualmente.
- Cópia, PNG, OCR e proteção de dados usam a composição completa.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.9.md`.

---

# Seleção renovada e OCR aprimorado — 1.1.8 (15 de setembro de 2026)

- A captura por região permite limpar o enquadramento e selecionar novamente sem fechar a tela.
- O editor abre dimensionado ao recorte exato e reduz somente a visualização quando necessário.
- O OCR compara a leitura original com uma versão ampliada, preservando o resultado de maior confiança e cobertura.
- Os três modos principais, as ações da região e as dez ferramentas do editor estão numerados e aceitam as teclas correspondentes.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.8.md`.

---

# Cancelamento rápido — 1.1.7 (12 de setembro de 2026)

- `Esc` cancela imediatamente a escolha de janela ou monitor.
- O atalho executa a mesma ação do botão **Cancelar**.
- Nenhuma imagem é capturada, copiada ou aberta no editor após o cancelamento.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.7.md`.

---

# Cópia direta de capturas — 1.1.6 (11 de setembro de 2026)

- A seleção de região oferece **Copiar imagem** e aceita `Ctrl+C`.
- As listas de janela e monitor oferecem **Abrir no editor** e **Copiar imagem**.
- `Ctrl+C` copia diretamente a janela ou o monitor marcado na lista.
- No editor, `Ctrl+C` copia a imagem final com todas as edições.
- A cópia ocorre depois que a interface de seleção fecha, mantendo as janelas
  do Firaw fora da captura.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.6.md`.

---

# Editor inteligente — 1.1.5 (11 de setembro de 2026)

- Texto, anotação, desenho, censura e desfoque podem ser selecionados e movidos depois de criados.
- **Selecionar** aceita clique direto, moldura para vários objetos e arraste do grupo.
- A borracha oferece objeto inteiro, formato circular e quadrado, com tamanho ajustável.
- **Borrar** cria um desfoque manual movível.
- **Borrar dados sensíveis** usa OCR local para proteger linhas com e-mail, telefone, CPF/CNPJ, cartão ou IP.
- Criação, movimentação, borracha e proteção automática participam do mesmo desfazer/refazer.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.5.md`.

---

# Perfil Print Screen — 1.1.4 (11 de setembro de 2026)

- `Print Screen` abre a seleção de região.
- `Alt + Print Screen` abre a escolha de monitor.
- `Ctrl + Print Screen` abre a escolha de janela.
- Cada combinação pode pertencer ao Firaw ou manter o comportamento original do Windows.
- Um tratamento direto de teclado contorna conflitos em que o registro global comum fica ocupado.
- As preferências incluem botões para liberar ou restaurar o recorte nativo do Windows.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.4.md`.

---

# Correção — 1.1.3 (11 de setembro de 2026)

- Corrigida a gravação do Print Screen dentro do campo de atalho.
- Os atalhos globais deixam de interceptar a tecla enquanto a janela de preferências está aberta.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.3.md`.

---

# Ajuste — 1.1.2 (11 de setembro de 2026)

- O campo **Atalho personalizado** agora reconhece `Print Screen`/`PrtSc` sozinha.
- `Ctrl`, `Shift` e `Alt` continuam opcionais para a tecla Print Screen e obrigatórios para teclas comuns.
- Se **Usar também a tecla Print Screen** estiver marcado, o Firaw evita registrar a mesma tecla duas vezes.
- Quando o Windows ou outro aplicativo ocupa a tecla, o status orienta escolher outro atalho ou ajustar o Windows.
- Pacote local para a próxima release: `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.2\github-release`.
- Notas completas: `C:\Users\Hugo\Firaw-SnapCopyText\UPDATE-1.1.2.md`.
- 30 de 30 testes aprovados; hashes x64 e x86 conferidos com o manifesto.

Esta versão não é publicada automaticamente pelo script de geração.

---

# Atualização — 1.1.1 (11 de setembro de 2026)

O endereço `https://updates.example.test/firaw-snapcopytext/` representa o canal de testes
e sem HTTPS: fora dela ninguém receberia atualização. Desde a 1.1.1:

- o launcher lê o `update.json` da release mais recente do GitHub
  (`firawynix/firaw-snapcopytext`, repositório público), por HTTPS;
- o manifesto aponta para os instaladores da mesma tag e cada instalador tem o
  seu `.sha256` publicado ao lado;
- o site está no ar em <https://snapcopytext.firawynix.com.br>, com os botões
  apontando para a release mais recente;
- o programa aparece no portfólio (Projetos e Produtos) e no Firawynix Center;
- a desinstalação remove o início automático com o Windows.

A seção abaixo descreve a entrega original da 1.1.0.

---

# Entrega — Firaw SnapCopyText 1.1.0

**Data:** 11 de setembro de 2026  
**Produto:** Firaw - SnapCopyText  
**Plataforma:** Windows 10/11, x64 e x86

## Resultado

Esta entrega acrescenta um launcher com atualização automática, dois instaladores Inno Setup e uma página de demonstração local. O aplicativo continua funcionando quando o servidor de atualização estiver indisponível.

## O que foi entregue

- Launcher `Firaw.SnapCopyText.Launcher.exe`, iniciado antes do aplicativo principal.
- Consulta automática de versão em `https://updates.example.test/firaw-snapcopytext/update.json`.
- Escolha automática do pacote x64 ou x86.
- Validação do tamanho e do SHA-256 antes de executar uma atualização.
- Instalação silenciosa da nova versão e reabertura do Firaw na bandeja.
- Continuidade normal do aplicativo se a rede, o manifesto ou o download falhar.
- Inicialização com o Windows redirecionada para o launcher.
- Instaladores Inno Setup separados para Windows x64 e x86.
- Página de demonstração responsiva, no estilo visual Firaw, mantida somente nesta máquina.
- Pacote pronto para copiar para o servidor de atualizações.

## Caminhos locais

| Item | Caminho |
| --- | --- |
| Pasta completa da versão | `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.0` |
| Instaladores | `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.0\installers` |
| Pacote para o servidor | `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.0\update-server\firaw-snapcopytext` |
| Página local | `C:\Users\Hugo\Firaw-SnapCopyText\demo-site\dist\index.html` |
| Cópia da página dentro da entrega | `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.0\demonstracao-local\index.html` |

## Como colocar a atualização no servidor

O endereço de testes recusou conexão durante esta preparação. Por isso, nenhum arquivo remoto foi alterado.

Quando o servidor estiver acessível, copie o conteúdo desta pasta:

```text
C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.0\update-server\firaw-snapcopytext
```

para a rota pública/interna:

```text
https://updates.example.test/firaw-snapcopytext/
```

O resultado esperado no servidor é:

```text
firaw-snapcopytext/
  update.json
  Firaw-SnapCopyText-Setup-x64.exe
  Firaw-SnapCopyText-Setup-x86.exe
```

## Próxima versão

1. Atualize o número de versão nos projetos principal e launcher.
2. Atualize as notas de versão no script de entrega.
3. Execute `tools\build-release.ps1 -Version X.Y.Z`.
4. Substitua os três arquivos da pasta do servidor somente depois que a geração terminar sem erros.

O manifesto é gerado por último com os hashes dos instaladores produzidos, evitando digitação manual.

## Validações

- Solução restaurada e compilada em .NET 9.
- 28 de 28 testes automatizados do aplicativo e do atualizador aprovados.
- Publicações autocontidas para `win-x64` e `win-x86`.
- Launchers x64 e x86 testados; ambos abriram o aplicativo 1.1.0 correspondente.
- Servidor indisponível simulado com o endereço real; o launcher abriu corretamente a versão instalada.
- Compilação dos dois instaladores pelo Inno Setup 7.1.0.
- Manifesto JSON conferido com tamanho e SHA-256 de cada instalador:
  - x64: 65.395.044 bytes — `C07A529CE069D11326DAC7E56A2DF622E7149527AE9AB25B65E6D9B01FADEA54`.
  - x86: 58.680.809 bytes — `C176FF85CDC851A4D8F436E30C4110298454FA7657F0A07B4214C2F757C71DDB`.
- Página e abas interativas conferidas em servidor HTTP local; nenhuma publicação externa foi feita.

## Segurança e operação

- As capturas e o OCR continuam locais.
- O atualizador consulta somente o endereço configurado e rejeita pacote apontando para outro host.
- Falhas de atualização não impedem o uso da versão instalada.
- O SHA-256 protege contra arquivo incompleto ou diferente do manifesto.
- Como o endereço solicitado usa HTTP, a proteção recomendada para produção é disponibilizar HTTPS e assinar digitalmente os executáveis quando houver um certificado de assinatura de código.

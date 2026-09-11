# Firaw - SnapCopyText 1.1.2

**Data:** 11 de setembro de 2026  
**Tipo:** correção de atalho e atualização do pacote Windows

## O que mudou

- O campo **Atalho personalizado** agora aceita `Print Screen`/`PrtSc` sem Ctrl, Shift ou Alt.
- Também são aceitas combinações como `Ctrl + Print Screen`.
- A tecla é exibida e salva de forma padronizada como `Print Screen`.
- Se a opção **Usar também a tecla Print Screen** estiver marcada, o Firaw evita registrar a mesma tecla duas vezes.
- Quando o Windows ou outro aplicativo estiver usando a tecla, o Firaw mostra uma orientação clara na tela principal.
- Teclas comuns continuam exigindo Ctrl, Shift ou Alt para não interferirem na digitação em outros programas.

## Como configurar

1. Abra **Atalhos e preferências**.
2. Clique no campo **Atalho personalizado**.
3. Pressione `Print Screen`.
4. Confirme que o campo mostra **Print Screen** e clique em **Salvar**.

Se o Windows mantiver a tecla ocupada, abra as configurações de teclado pelo botão disponível no Firaw e desative o uso de Print Screen pela Ferramenta de Captura.

## Arquivos da versão

Pasta local pronta para publicação:

`C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.2\github-release`

| Arquivo | Tamanho | SHA-256 |
| --- | ---: | --- |
| `Firaw-SnapCopyText-Setup-x64.exe` | 65.404.364 bytes | `e733e8247ae432f37eddc0a82ab14aabb13d56911b02d9850d72ca1f770f8eb7` |
| `Firaw-SnapCopyText-Setup-x86.exe` | 58.683.065 bytes | `057f62c2bbdbd1a69edd398bb70593ccb16d44e403fe1ea97dfab1bf16980025` |
| `update.json` | Manifesto consumido pelo launcher | Gerado sem BOM |

Os arquivos `.sha256` individuais acompanham os dois instaladores.

## Local da atualização automática

O launcher consulta:

`https://github.com/firawynix/firaw-snapcopytext/releases/latest/download/update.json`

Depois da publicação da tag `v1.1.2`, os instaladores, seus hashes e o manifesto ficam juntos na mesma release. O site usa a release mais recente para os botões de download.

## Validação

- 30 de 30 testes automatizados aprovados.
- Casos testados: `PrintScreen`, `Print Screen`, `PrtSc`, combinações existentes e rejeição de tecla comum sem modificador.
- Aplicativo e launcher publicados como versão `1.1.2.0` para x64 e x86.
- Instaladores compilados pelo Inno Setup.
- Tamanho e SHA-256 dos dois instaladores conferidos com o manifesto.

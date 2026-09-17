# Firaw - SnapCopyText 1.1.15

## Correção

- Corrigido o fechamento do aplicativo ao abrir **Atalhos e preferências** no Windows x64.
- A janela agora filtra apenas mensagens de atalho antes de ler o identificador recebido do Windows.
- Mensagens nativas que contêm ponteiros de 64 bits deixam de causar estouro numérico.
- O cancelamento da janela de preferências também foi protegido para não tentar definir um resultado antes de a janela estar pronta.

O aviso do **Auxiliar de Compatibilidade de Programas** sobre TLS era uma interpretação genérica do Windows após o encerramento inesperado. O registro real do sistema apontou `System.OverflowException` em `SettingsWindow.WindowProcedure`; não houve ativação de TLS antigo.

## Verificação

- 64 testes automatizados aprovados.
- Incluído teste específico com um ponteiro maior que 32 bits.
- Instaladores Windows x64 e x86 gerados com a versão de arquivo `1.1.15.0`.
- Manifesto de atualização aponta para a tag `v1.1.15` e confere tamanho e SHA-256.

## Arquivos

| Arquivo | Tamanho | SHA-256 |
| --- | ---: | --- |
| `Firaw-SnapCopyText-Setup-x64.exe` | 65.449.523 bytes | `172151e44ef4a23393215a963c44a1d7bc13e1cb72011ff3f573637f6370d585` |
| `Firaw-SnapCopyText-Setup-x86.exe` | 58.713.363 bytes | `c6ef9747f8a25a28babd71610b691213091f9f04e93e51334c429ca03f3db556` |

Pasta local: `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.15\github-release`

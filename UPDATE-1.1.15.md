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
| `Firaw-SnapCopyText-Setup-x64.exe` | 65.442.878 bytes | `3c13fd9c96244a4efbc5e427ee935ab1c1b3a994a580cb3853078657468974b2` |
| `Firaw-SnapCopyText-Setup-x86.exe` | 58.717.508 bytes | `9d7ad0f5932a6341d79b179a68c41d2ca10f104184ce0c19d05b0a62f5f1c0d8` |

Pasta local: `C:\Users\Hugo\Firaw-SnapCopyText\release\Firaw-SnapCopyText-1.1.15\github-release`

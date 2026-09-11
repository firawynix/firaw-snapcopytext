# Publicação do site

`https://snapcopytext.firawynix.com.br` → Cloudflare Tunnel do `10.81.66.7` →
`127.0.0.1:26005` → nginx com a pasta `/home/ksdev/snapcopytext-site/public`
montada (sem rebuild: atualizar é copiar os arquivos).

A pasta mora no `10.81.66.7`, que é a fonte do espelho: o `sync-standby` leva
`/home/ksdev` para o `10.81.66.10` com `rsync --delete`. Pasta criada só no `.10`
é apagada no ciclo seguinte.

Atualizar o conteúdo:

```bash
tar -C demo-site/dist -cf - . | ssh srv1 'tar -xf - -C /home/ksdev/snapcopytext-site/public'
```

Mudou o `nginx.conf` (montado como arquivo): `docker compose up -d --force-recreate`
na pasta do site, porque o container continua lendo o arquivo antigo.

Os botões de download apontam para `releases/latest/download/...` do GitHub: o site
não guarda instalador e não precisa mudar a cada versão.

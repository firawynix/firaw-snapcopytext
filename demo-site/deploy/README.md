# Publicação do site

`https://snapcopytext.firawynix.com.br` → proxy HTTPS autorizado →
servidor web com a pasta pública do site
montada (sem rebuild: atualizar é copiar os arquivos).

A implantação concreta é privada e não faz parte deste repositório. O conteúdo
publicável fica em `demo-site/dist`.
é apagada no ciclo seguinte.

Atualizar o conteúdo:

```bash
tar -C demo-site/dist -cf - . | ssh srv1 'tar -xf - -C /home/ksdev/snapcopytext-site/public'
```

Mudou o `nginx.conf` (montado como arquivo): `docker compose up -d --force-recreate`
na pasta do site, porque o container continua lendo o arquivo antigo.

Os botões de download apontam para `releases/latest/download/...` do GitHub: o site
não guarda instalador e não precisa mudar a cada versão.

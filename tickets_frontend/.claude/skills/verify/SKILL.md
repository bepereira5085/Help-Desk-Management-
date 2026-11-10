---
name: verify
description: Como subir o frontend Quasar (Windows) a partir do WSL e capturar a tela para verificar mudanças.
---

# Verificar tickets_frontend

O usuário roda o projeto **no Windows**; o `node_modules` tem bindings nativas win32.
Nunca rode `npm`/`quasar` pelo Node do WSL (falha com "Cannot find native binding")
e não apague/reinstale `node_modules` pelo Linux.

## Subir
Do diretório `tickets_frontend`, rode pelo cmd do Windows (em background):

    cmd.exe /c "npx quasar dev --port 9123"

- `devServer.open: true` abre um navegador no Windows do usuário — esperado.
- Do WSL, `localhost:9123` NÃO responde; use o IP do host mostrado em "App URL"
  (ex.: `http://172.30.0.1:9123/`).
- Router em modo hash: rotas são `/#/...`.

## Capturar
    google-chrome --headless=new --no-sandbox --disable-gpu --window-size=1280,800 \
      --virtual-time-budget=8000 --screenshot=<scratchpad>/home.png "http://172.30.0.1:9123/#/"
    google-chrome --headless=new --no-sandbox --virtual-time-budget=8000 \
      --enable-logging=stderr --v=0 --dump-dom "<url>" 2>console.txt   # erros: grep Uncaught

Screenshot de ~4.7 KB = tela em branco (erro JS). Sempre olhe o console.

## Produção
    cmd.exe /c "npm run build"      # saída em dist/spa
    (cd dist/spa && python3 -m http.server 9124 --bind 127.0.0.1)   # arquivos estáticos servem do WSL

"Build succeeded" não garante que a tela renderiza — capture o dist também.

## Encerrar
    powershell.exe -NoProfile -Command "Get-NetTCPConnection -LocalPort 9123 -State Listen | % { Stop-Process -Id $_.OwningProcess -Force }"

## Gotchas (@quasar/app-vite 3.10+)
- Wrappers vêm de `'#q-app'` (não `'#q-app/wrappers'`).
- Aliases antigos (`layouts/`, `pages/`, `components/`, `~assets/`) não existem; use `@/...`.
- `process.env.*` não existe no cliente; use `import.meta.env.QUASAR_*`
  (ex.: `QUASAR_VUE_ROUTER_MODE`, `QUASAR_VUE_ROUTER_BASE`, `QUASAR_SERVER`).

const { app, BrowserWindow, Tray, Menu, clipboard, desktopCapturer, dialog, globalShortcut, ipcMain, nativeImage } = require('electron');
const fs = require('node:fs/promises');
const path = require('node:path');
const { createWorker } = require('tesseract.js');
const { addHistory } = require('./core');

app.setDesktopName('br.com.firawynix.snapcopytext.desktop');
app.commandLine.appendSwitch('enable-features', 'GlobalShortcutsPortal,WebRTCPipeWireCapturer');
let win; let tray; let worker; let history=[]; let lastClipboard='';
const iconPath=()=>app.isPackaged?path.join(process.resourcesPath,'firaw-eye.png'):path.join(__dirname,'..','src','Firaw.SnapCopyText','Assets','firaw-eye.png');
const tessPath=()=>app.isPackaged?path.join(process.resourcesPath,'tessdata'):path.join(__dirname,'..','src','Firaw.SnapCopyText','tessdata');

function show(mode='region'){win.show();win.focus();win.webContents.send('capture:request',mode)}
function createWindow(){win=new BrowserWindow({width:1100,height:760,minWidth:800,minHeight:560,title:'Firaw SnapCopyText',backgroundColor:'#071116',autoHideMenuBar:true,webPreferences:{preload:path.join(__dirname,'preload.js'),contextIsolation:true,sandbox:true}});win.loadFile('index.html');win.on('close',event=>{if(!app.quitting){event.preventDefault();win.hide()}})}
function createTray(){tray=new Tray(iconPath());tray.setToolTip('Firaw SnapCopyText');tray.setContextMenu(Menu.buildFromTemplate([{label:'Capturar região',click:()=>show('region')},{label:'Capturar janela',click:()=>show('window')},{label:'Capturar monitor',click:()=>show('screen')},{type:'separator'},{label:'Abrir',click:()=>show('none')},{label:'Sair',click:()=>{app.quitting=true;app.quit()}}]));tray.on('click',()=>show('region'))}
function shortcuts(){for(const [key,mode] of [['PrintScreen','region'],['CommandOrControl+Shift+X','region'],['CommandOrControl+Shift+W','window']])globalShortcut.register(key,()=>show(mode))}

ipcMain.handle('capture:sources',async(_event,mode)=>{const types=mode==='screen'?['screen']:mode==='window'?['window']:['screen','window'];const sources=await desktopCapturer.getSources({types,thumbnailSize:{width:3840,height:2160},fetchWindowIcons:true});return sources.map(source=>({id:source.id,name:source.name,thumbnail:source.thumbnail.toDataURL(),icon:source.appIcon?.toDataURL()||null}))});
ipcMain.handle('capture:copy-image',(_event,dataUrl)=>{clipboard.writeImage(nativeImage.createFromDataURL(dataUrl));return true});
ipcMain.handle('capture:copy-text',(_event,text)=>{clipboard.writeText(String(text));history=addHistory(history,text);win.webContents.send('clipboard:history',history);return true});
ipcMain.handle('capture:save',async(_event,dataUrl)=>{const result=await dialog.showSaveDialog(win,{title:'Salvar captura',defaultPath:`Captura-${new Date().toISOString().replace(/[:.]/g,'-')}.png`,filters:[{name:'Imagem PNG',extensions:['png']}]});if(result.canceled)return null;await fs.writeFile(result.filePath,nativeImage.createFromDataURL(dataUrl).toPNG());return result.filePath});
ipcMain.handle('capture:ocr',async(_event,dataUrl)=>{if(!worker){worker=await createWorker(['por','eng'],1,{langPath:tessPath(),gzip:false,cachePath:path.join(app.getPath('userData'),'ocr-cache'),logger:m=>win?.webContents.send('ocr:progress',m)});}const image=nativeImage.createFromDataURL(dataUrl).toPNG();const result=await worker.recognize(image);return result.data.text.trim()});
ipcMain.handle('clipboard:state',()=>history);

app.whenReady().then(()=>{createWindow();createTray();shortcuts();setInterval(()=>{const text=clipboard.readText().trim();if(text&&text!==lastClipboard){lastClipboard=text;history=addHistory(history,text);win.webContents.send('clipboard:history',history)}},900);});
app.on('before-quit',async()=>{app.quitting=true;globalShortcut.unregisterAll();if(worker)await worker.terminate()});
app.on('window-all-closed',()=>{});

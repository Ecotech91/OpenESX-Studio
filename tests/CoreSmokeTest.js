'use strict';

const fs = require('fs');
const path = require('path');
const nodeCrypto = require('crypto');

class FakeElement {
  constructor() {
    this.classList = {add(){},remove(){},toggle(){}};
    this.style = {};
    this.dataset = {};
    this.files = [];
    this.value = '';
    this.disabled = false;
    this.innerHTML = '';
    this.textContent = '';
  }
  addEventListener() {}
  appendChild() {}
  remove() {}
  click() {}
  showModal() {}
  close() {}
  querySelector() { return new FakeElement(); }
  querySelectorAll() { return []; }
  closest() { return null; }
}

const elements = new Map();
global.document = {
  body: new FakeElement(),
  getElementById(id) {
    if (!elements.has(id)) elements.set(id,new FakeElement());
    return elements.get(id);
  },
  querySelector() { return new FakeElement(); },
  querySelectorAll() { return []; },
  createElement() { return new FakeElement(); }
};
global.window = {addEventListener(){}};
if (!global.crypto) Object.defineProperty(global,'crypto',{value:nodeCrypto.webcrypto});
global.__OPENESX_TEST__ = true;
global.__OPENESX_NATIVE_TOKEN__ = 'public-smoke-test-token';

function assert(condition,message) {
  if (!condition) throw new Error(message);
}

function writeAscii(bytes,offset,text) {
  for (let index = 0; index < text.length; index++) bytes[offset + index] = text.charCodeAt(index);
}

function writeU16be(bytes,offset,value) {
  bytes[offset] = (value >>> 8) & 255;
  bytes[offset + 1] = value & 255;
}

function syntheticEsx() {
  const bytes = new Uint8Array(0x250010);
  writeAscii(bytes,0,'KORG');
  bytes[7] = 0x71;
  writeAscii(bytes,8,'ESX');
  writeAscii(bytes,0x1B0000,'KORG');
  writeAscii(bytes,0x1B0008,'ESX');
  for (let index = 0; index < 256; index++) {
    const offset = 0x200 + index * 4280;
    writeAscii(bytes,offset,'INIT');
    writeU16be(bytes,offset + 8,120 << 7);
    bytes[offset + 11] = 0;
    bytes[offset + 13] = 15;
  }
  return bytes;
}

(async () => {
  const htmlPath = path.join(__dirname,'..','src','OpenESXStudioOffline','OpenESX-Studio-Offline.html');
  const html = fs.readFileSync(htmlPath,'utf8');
  const match = html.match(/<script>([\s\S]*?)<\/script>/);
  assert(match,'Inline application script not found.');
  new Function(match[1])();
  const core = global.__OPENESX_CORE__;
  assert(core,'Public test interface not available.');

  const bytes = syntheticEsx();
  const analysis = core.parseEsx(bytes);
  assert(analysis.formatRecognized,'Synthetic ESX signature was not recognized.');
  assert(analysis.samples.length === 384,'Expected 384 sample slots.');
  assert(analysis.monoCount === 0 && analysis.stereoCount === 0,'Synthetic bank should contain no samples.');
  assert(analysis.patterns.length === 256 && analysis.songs.length === 64,'Pattern or song table parse failed.');
  assert(analysis.warnings.length === 0,'Synthetic bank produced structural warnings.');

  const originalHash = await core.sha256(bytes);
  const copy = bytes.slice();
  assert(await core.sha256(copy) === originalHash,'Bit-exact copy check failed.');

  core.state.file = {name:'SYNTHETIC.esx'};
  core.state.originalBytes = bytes.slice();
  core.state.originalHash = originalHash;
  core.state.originalAnalysis = analysis;
  core.state.bytes = copy;
  core.state.hash = originalHash;
  core.state.analysis = analysis;
  core.state.selectedPattern = 0;
  core.state.selectedPart = {type:'drum',index:0};
  core.state.selectedBar = 0;
  core.state.selectedStep = 0;
  core.state.tab = 'file';

  const globalBefore = core.state.bytes.slice(0x20,0xE0);
  await core.togglePatternStep(0);
  const edited = core.parsePattern(core.state.bytes,0);
  assert(edited.parts[0].steps[0] === true,'Pattern step writer failed.');
  assert(Buffer.from(core.state.bytes.slice(0x20,0xE0)).equals(Buffer.from(globalBefore)),'Pattern edit changed global bytes.');
  assert(await core.sha256(core.state.originalBytes) === originalHash,'Immutable original changed during edit.');

  core.state.cardsLoaded = true;
  core.state.cards = [{id:'Rtest',root:'R:\\',label:'TEST_SD',driveType:'Wechseldatenträger',format:'FAT32',totalBytes:16 * 1024 ** 3,freeBytes:8 * 1024 ** 3,fileCount:2,esxFileCount:1,compatible:true,capacityStatus:'SDHC · bis 32 GB'}];
  core.state.selectedCardId = 'Rtest';
  core.state.cardBanks = [{name:'TEST.ESX',size:bytes.length,lastWriteLocal:'01.01.2026 12:00'}];
  core.renderCards();
  const cardHtml = document.getElementById('mainPanel').innerHTML;
  assert(cardHtml.includes('data-action="card-save"'),'Card save control missing.');
  assert(cardHtml.includes('data-action="card-open"'),'Card open control missing.');
  assert(cardHtml.includes('Community-Beta'),'Physical ESX-SD beta warning missing.');

  console.log('OpenESX Studio public core smoke test: PASS');
  console.log('Synthetic ESX: recognition, tables, copy, pattern safety and card UI verified');
})().catch(error => {
  console.error(error.stack || error);
  process.exitCode = 1;
});

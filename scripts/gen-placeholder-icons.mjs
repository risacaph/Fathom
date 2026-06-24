// Fathom placeholder icon generator.
// Produces antialiased PNG/ICO marks (deep-navy rounded square, signal-teal "F",
// brass depth line) with no external dependencies (Node built-in zlib only).
// Re-run after editing COLORS or geometry, or just drop in your own art and delete this.
//
//   node scripts/gen-placeholder-icons.mjs
//
import zlib from 'node:zlib';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');

const NAVY  = [0x0b, 0x1f, 0x3a];
const TEAL  = [0x1a, 0xc7, 0xbc];
const BRASS = [0xc8, 0xa2, 0x4b];
const SS = 4; // supersampling factor for antialiasing

// ---- PNG encoder (RGBA8) ----
const crcTable = (() => {
  const t = new Uint32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    t[n] = c >>> 0;
  }
  return t;
})();
function crc32(buf) {
  let crc = 0xffffffff;
  for (let i = 0; i < buf.length; i++) crc = (crc >>> 8) ^ crcTable[(crc ^ buf[i]) & 0xff];
  return (crc ^ 0xffffffff) >>> 0;
}
function chunk(type, data) {
  const len = Buffer.alloc(4); len.writeUInt32BE(data.length, 0);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), data]);
  const crc = Buffer.alloc(4); crc.writeUInt32BE(crc32(body), 0);
  return Buffer.concat([len, body, crc]);
}
function encodePNG(S, rgba) {
  const sig = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(S, 0); ihdr.writeUInt32BE(S, 4);
  ihdr[8] = 8; ihdr[9] = 6; // 8-bit, RGBA
  const stride = S * 4;
  const raw = Buffer.alloc((stride + 1) * S);
  for (let y = 0; y < S; y++) {
    raw[y * (stride + 1)] = 0; // filter: none
    rgba.copy(raw, y * (stride + 1) + 1, y * stride, y * stride + stride);
  }
  const idat = zlib.deflateSync(raw, { level: 9 });
  return Buffer.concat([sig, chunk('IHDR', ihdr), chunk('IDAT', idat), chunk('IEND', Buffer.alloc(0))]);
}

// ---- procedural mark, rendered hi-res then downsampled ----
function rect(x, y, x0, y0, x1, y1) { return x >= x0 && x < x1 && y >= y0 && y < y1; }
function drawHi(N) {
  const buf = Buffer.alloc(N * N * 4);
  const r = 0.18 * N, minx = r, maxx = N - r, miny = r, maxy = N - r;
  const inRound = (x, y) => {
    if (x < minx && y < miny) return (x - minx) ** 2 + (y - miny) ** 2 <= r * r;
    if (x > maxx && y < miny) return (x - maxx) ** 2 + (y - miny) ** 2 <= r * r;
    if (x < minx && y > maxy) return (x - minx) ** 2 + (y - maxy) ** 2 <= r * r;
    if (x > maxx && y > maxy) return (x - maxx) ** 2 + (y - maxy) ** 2 <= r * r;
    return true;
  };
  for (let y = 0; y < N; y++) for (let x = 0; x < N; x++) {
    const inside = inRound(x + 0.5, y + 0.5);
    let c = NAVY, a = inside ? 255 : 0;
    const fx = x / N, fy = y / N;
    const inF =
      rect(fx, fy, 0.32, 0.26, 0.43, 0.74) ||  // stem
      rect(fx, fy, 0.32, 0.26, 0.70, 0.36) ||  // top arm
      rect(fx, fy, 0.32, 0.47, 0.62, 0.56);    // middle arm
    if (inside && inF) { c = TEAL; a = 255; }
    if (inside && rect(fx, fy, 0.28, 0.80, 0.72, 0.855)) { c = BRASS; a = 255; }
    const i = (y * N + x) * 4;
    buf[i] = c[0]; buf[i + 1] = c[1]; buf[i + 2] = c[2]; buf[i + 3] = a;
  }
  return buf;
}
function render(S) {
  const N = S * SS, hi = drawHi(N), out = Buffer.alloc(S * S * 4), n = SS * SS;
  for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) {
    let r = 0, g = 0, b = 0, a = 0;
    for (let dy = 0; dy < SS; dy++) for (let dx = 0; dx < SS; dx++) {
      const i = ((y * SS + dy) * N + (x * SS + dx)) * 4;
      r += hi[i]; g += hi[i + 1]; b += hi[i + 2]; a += hi[i + 3];
    }
    const o = (y * S + x) * 4;
    out[o] = Math.round(r / n); out[o + 1] = Math.round(g / n);
    out[o + 2] = Math.round(b / n); out[o + 3] = Math.round(a / n);
  }
  return out;
}
const pngCache = new Map();
function png(S) { if (!pngCache.has(S)) pngCache.set(S, encodePNG(S, render(S))); return pngCache.get(S); }

// ---- ICO encoder (embeds PNGs) ----
function encodeICO(sizes) {
  const imgs = sizes.map((s) => png(s));
  const header = Buffer.alloc(6);
  header.writeUInt16LE(1, 2); header.writeUInt16LE(imgs.length, 4);
  let offset = 6 + imgs.length * 16;
  const entries = imgs.map((p, k) => {
    const s = sizes[k], e = Buffer.alloc(16);
    e[0] = s >= 256 ? 0 : s; e[1] = s >= 256 ? 0 : s;
    e.writeUInt16LE(1, 4); e.writeUInt16LE(32, 6);
    e.writeUInt32LE(p.length, 8); e.writeUInt32LE(offset, 12);
    offset += p.length; return e;
  });
  return Buffer.concat([header, ...entries, ...imgs]);
}

// ---- write targets ----
const W = (rel, buf) => { fs.writeFileSync(path.join(ROOT, rel), buf); console.log('wrote', rel, buf.length + 'b'); };
W('UI/Web/src/assets/icons/favicon-16x16.png', png(16));
W('UI/Web/src/assets/icons/favicon-32x32.png', png(32));
W('UI/Web/src/assets/icons/apple-touch-icon.png', png(180));
W('UI/Web/src/assets/icons/android-chrome-192x192.png', png(192));
W('UI/Web/src/assets/icons/android-chrome-256x256.png', png(256));
W('UI/Web/src/assets/icons/mstile-150x150.png', png(150));
W('UI/Web/src/assets/icons/favicon.ico', encodeICO([16, 32, 48]));
W('UI/Web/src/assets/images/logo-32.png', png(32));
W('UI/Web/src/assets/images/logo-64.png', png(64));
W('UI/Web/src/assets/images/logo.png', png(512));
W('favicon.ico', encodeICO([16, 32, 48]));
console.log('done');

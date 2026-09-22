export const DEFAULT_ACCENT = "hsl(28, 85%, 55%)";

const SAMPLE_SIZE = 64;
const HUE_BINS = 72;
const CLUSTER_RADIUS = 2;
const MIN_CHROMA = 0.04;
const MIN_VALUE = 0.08;
const MIN_CLUSTER_MASS = 0.0004;
const SAT_RANGE = [0.3, 0.95];
const LIGHT_RANGE = [0.35, 0.65];
const NEUTRAL_LIGHT_RANGE = [0.08, 0.92];
const NEUTRAL_LIFT_RANGE = [0.65, 1.45];
const NEUTRAL_ACCENT = /^hsl\(0, 0%, (\d+)%\)$/;
const CACHE_LIMIT = 200;

const cache = new Map();
const settled = new Map();

const clamp = (value, [min, max]) => Math.min(max, Math.max(min, value));
const percent = (value) => Math.round(value * 100);

function remember(map, key, value) {
  map.set(key, value);
  if (map.size > CACHE_LIMIT) map.delete(map.keys().next().value);
}

function rgbToHcl(r, g, b) {
  const rn = r / 255;
  const gn = g / 255;
  const bn = b / 255;
  const max = Math.max(rn, gn, bn);
  const min = Math.min(rn, gn, bn);
  const chroma = max - min;
  const lightness = (max + min) / 2;

  if (chroma === 0) return [0, 0, lightness];

  let hue;
  if (max === rn) hue = ((gn - bn) / chroma) % 6;
  else if (max === gn) hue = (bn - rn) / chroma + 2;
  else hue = (rn - gn) / chroma + 4;

  hue *= 60;
  return [hue < 0 ? hue + 360 : hue, chroma, lightness];
}

function fetchImage(url) {
  const attempt = (mode) =>
    fetch(url, { mode: "cors", cache: mode }).then((response) => {
      if (!response.ok) throw new Error(`HTTP ${response.status} for ${url}`);
      return response;
    });

  return attempt("default").catch(() => attempt("reload"));
}

async function samplePixels(url) {
  const response = await fetchImage(url);
  const bitmap = await createImageBitmap(await response.blob());

  const canvas = document.createElement("canvas");
  canvas.width = SAMPLE_SIZE;
  canvas.height = SAMPLE_SIZE;

  const context = canvas.getContext("2d", { willReadFrequently: true });
  if (!context) throw new Error("2d canvas unavailable");

  context.drawImage(bitmap, 0, 0, SAMPLE_SIZE, SAMPLE_SIZE);
  bitmap.close?.();

  return context.getImageData(0, 0, SAMPLE_SIZE, SAMPLE_SIZE).data;
}

function dominantColor(data) {
  const hueWeight = new Float64Array(HUE_BINS);
  const chromaWeight = new Float64Array(HUE_BINS);
  const lightWeight = new Float64Array(HUE_BINS);
  const sampled = data.length / 4;
  let lightnessSum = 0;
  let opaquePixels = 0;

  for (let i = 0; i < data.length; i += 4) {
    if (data[i + 3] < 128) continue;

    const [hue, chroma, lightness] = rgbToHcl(data[i], data[i + 1], data[i + 2]);
    lightnessSum += lightness;
    opaquePixels += 1;

    if (chroma < MIN_CHROMA || lightness + chroma / 2 < MIN_VALUE) continue;

    const bin = Math.min(HUE_BINS - 1, Math.floor((hue / 360) * HUE_BINS));
    hueWeight[bin] += chroma;
    chromaWeight[bin] += chroma * chroma;
    lightWeight[bin] += lightness * chroma;
  }

  const clusterWeight = (centre) => {
    let sum = 0;
    for (let offset = -CLUSTER_RADIUS; offset <= CLUSTER_RADIUS; offset += 1) {
      sum += hueWeight[(centre + offset + HUE_BINS) % HUE_BINS];
    }
    return sum;
  };

  let peak = 0;
  let peakWeight = 0;
  for (let bin = 0; bin < HUE_BINS; bin += 1) {
    const weight = clusterWeight(bin);
    if (weight > peakWeight) {
      peakWeight = weight;
      peak = bin;
    }
  }

  if (peakWeight / sampled < MIN_CLUSTER_MASS) {
    if (opaquePixels === 0) return null;
    const grey = clamp(lightnessSum / opaquePixels, NEUTRAL_LIGHT_RANGE);
    return `hsl(0, 0%, ${percent(grey)}%)`;
  }

  let x = 0;
  let y = 0;
  let chromaSum = 0;
  let lightSum = 0;

  for (let offset = -CLUSTER_RADIUS; offset <= CLUSTER_RADIUS; offset += 1) {
    const bin = (peak + offset + HUE_BINS) % HUE_BINS;
    const angle = ((bin + 0.5) / HUE_BINS) * 2 * Math.PI;
    x += Math.cos(angle) * hueWeight[bin];
    y += Math.sin(angle) * hueWeight[bin];
    chromaSum += chromaWeight[bin];
    lightSum += lightWeight[bin];
  }

  const hue = ((Math.atan2(y, x) * 180) / Math.PI + 360) % 360;
  const meanChroma = chromaSum / peakWeight;
  const meanLight = lightSum / peakWeight;
  const saturation = meanChroma / Math.max(1 - Math.abs(2 * meanLight - 1), 1e-3);

  return `hsl(${Math.round(hue)}, ${percent(clamp(saturation, SAT_RANGE))}%, ${percent(clamp(meanLight, LIGHT_RANGE))}%)`;
}

export function getDominantColor(url) {
  if (!url) return Promise.resolve(null);

  const hit = cache.get(url);
  if (hit) return hit;

  const pending = samplePixels(url)
    .then((pixels) => {
      const color = dominantColor(pixels);
      remember(settled, url, color);
      return color;
    })
    .catch(() => {
      cache.delete(url);
      return null;
    });

  remember(cache, url, pending);
  return pending;
}

export function peekDominantColor(url) {
  if (!url) return null;
  return settled.get(url) ?? null;
}

export function accentLift(accent) {
  const match = NEUTRAL_ACCENT.exec(accent ?? "");
  if (!match) return 1;

  const [min, max] = NEUTRAL_LIFT_RANGE;
  return min + (Number(match[1]) / 100) * (max - min);
}

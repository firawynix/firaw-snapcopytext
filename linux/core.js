function normalizeRect(start, end, width, height) {
  const x1 = Math.max(0, Math.min(width, Math.min(start.x, end.x)));
  const y1 = Math.max(0, Math.min(height, Math.min(start.y, end.y)));
  const x2 = Math.max(0, Math.min(width, Math.max(start.x, end.x)));
  const y2 = Math.max(0, Math.min(height, Math.max(start.y, end.y)));
  return { x: Math.round(x1), y: Math.round(y1), width: Math.round(x2 - x1), height: Math.round(y2 - y1) };
}

function addHistory(history, text, limit = 100) {
  const clean = String(text || '').trim();
  if (!clean) return history;
  return [clean, ...history.filter((item) => item !== clean)].slice(0, limit);
}

module.exports = { normalizeRect, addHistory };

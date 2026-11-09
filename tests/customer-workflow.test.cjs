// Runs the actual customer scripts in a small DOM harness; no database or packages needed.
const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { resolve } = require('node:path');
const vm = require('node:vm');

function harness(storage = '[]', response = {}) {
  const nodes = new Map(), intervals = [], requests = [];
  function element(key) {
    if (!nodes.has(key)) nodes.set(key, {
      dataset: {}, value: '', textContent: '', innerHTML: '', style: {}, hidden: false,
      listeners: {}, classList: { add() {}, remove() {}, toggle() {} },
      addEventListener(type, handler) { this.listeners[type] = handler; },
      closest() { return element('fieldset'); }, showModal() {}, close() {},
      querySelectorAll() { return []; }
    });
    return nodes.get(key);
  }
  const config = { duocDatHang: true, giaTheoSize: { '2': [{ ma: 1, ten: 'M', gia: 30000 }] },
    danhSachMucDa: [], danhSachMucDuong: [], danhSachTopping: [] };
  element('#menu-data').textContent = JSON.stringify(config);
  const toppings = [{ value: '9', dataset: { gia: '5000', ten: 'Trân châu' } }];
  const doc = element('document');
  doc.getElementById = (id) => id === 'timMon' || id === 'nutGuiDanhGia' ? null : element('#' + id);
  doc.querySelector = (selector) => {
    if (selector === '#sizeChoices input:checked') return { value: '1', dataset: { ten: 'M', gia: '30000' } };
    if (selector === '#iceChoices input:checked' || selector === '#sugarChoices input:checked')
      return { value: '1', dataset: { ten: '100%' } };
    return element(selector);
  };
  doc.querySelectorAll = (selector) => selector === '#topChoices input:checked' ? toppings : [];
  const context = {
    document: doc, window: {}, navigator: {}, location: {}, console,
    localStorage: { getItem: () => storage, setItem: (_, value) => { storage = value; } },
    setTimeout: () => 1, clearTimeout() {},
    setInterval: (fn, delay) => { intervals.push({ fn, delay }); },
    fetch: async (url, options) => { requests.push({ url, options }); return {
      ok: response.ok ?? true, json: async () => response.body ?? { maTrangThai: 4, daThanhToan: true }
    }; }
  };
  return { context, element, intervals, requests, cart: () => JSON.parse(storage),
    run(file) { vm.runInNewContext(readFileSync(resolve(__dirname, '../MixueDatMon/wwwroot/js', file), 'utf8'), context); },
    clickAction(action, index = 0) {
      return element('#cartItems').listeners.click({ target: { closest: () => ({ dataset: { hanhDong: action, viTri: String(index) } }) } });
    },
    addProduct(dataset) {
      doc.listeners.click({ target: { closest: (selector) => selector === '.add-btn, .goi-y-the-nho' ? { dataset } : null } });
    }
  };
}
const row = { maMonAn: 1, gia: 20000, soLuong: 2, ten: 'Trà', danhSachTopping: [{ ma: 9, gia: 5000, ten: 'Trân châu' }] };

test('corrupt or non-array stored carts recover without breaking the menu', () => {
  for (const raw of ['{', '{}', 'null', '[null]', '[{"maMonAn":1,"gia":-1,"soLuong":2,"danhSachTopping":[]}]']) {
    const h = harness(raw); h.run('thucdon.js');
    assert.deepEqual(h.cart(), []);
    assert.equal(h.element('#sendOrder').disabled, true);
  }
});

test('restored cart computes toppings, updates quantity with bounds, and deletes', () => {
  const h = harness(JSON.stringify([row])); h.run('thucdon.js');
  assert.equal(h.element('#cartTotal').textContent, '50.000đ');
  h.clickAction('tang'); assert.equal(h.element('#cartTotal').textContent, '75.000đ');
  for (let i = 0; i < 30; i++) h.clickAction('tang');
  assert.equal(h.cart()[0].soLuong, 20);
  for (let i = 0; i < 30; i++) h.clickAction('giam');
  assert.equal(h.cart()[0].soLuong, 1);
  h.clickAction('xoa'); assert.deepEqual(h.cart(), []);
  assert.equal(h.element('#cartTotal').textContent, '0đ');
});

test('adding the same plain product merges its quantity', () => {
  const h = harness(); h.run('thucdon.js');
  const product = { ma: '1', ten: 'Kem', gia: '10000', tuyChon: '0' };
  h.addProduct(product); h.addProduct(product);
  assert.equal(h.cart().length, 1); assert.equal(h.cart()[0].soLuong, 2);
  assert.equal(h.element('#cartTotal').textContent, '20.000đ');
});

test('a sized non-drink does not inherit hidden drink toppings, ice or sugar', () => {
  const h = harness(); h.run('thucdon.js');
  h.addProduct({ ma: '2', ten: 'Kem size M', gia: '20000', tuyChon: '0' });
  assert.equal(h.element('#optAdd').textContent, 'Thêm vào giỏ - 30.000đ');
  h.element('#optForm').listeners.submit({ preventDefault() {} });
  const item = h.cart()[0];
  assert.deepEqual(item.danhSachTopping, []);
  assert.equal(item.maMucDa, null); assert.equal(item.maMucDuong, null);
  assert.equal(item.gia, 30000);
});

test('checkout keeps cart after a server failure', async () => {
  const h = harness(JSON.stringify([row]), { ok: false, body: { thanhCong: false, thongBao: 'Món đã hết' } });
  h.run('thucdon.js'); await h.element('#sendOrder').listeners.click();
  assert.equal(h.cart().length, 1);
  assert.equal(h.element('#cartMsg').textContent, 'Món đã hết');
  assert.equal(h.element('#sendOrder').disabled, false);
});

test('successful checkout sends item identifiers, clears cart and navigates to tracking', async () => {
  const h = harness(JSON.stringify([row]), { body: { thanhCong: true, duongDan: '/DonHang/TheoDoi/ABC' } });
  h.run('thucdon.js'); await h.element('#sendOrder').listeners.click();
  const request = h.requests.find((r) => r.url === '/DonHang/DatHang');
  assert.equal(request.options.method, 'POST');
  const body = JSON.parse(request.options.body);
  assert.equal(body.danhSachMon[0].maMonAn, 1);
  assert.deepEqual(body.danhSachMon[0].danhSachMaTopping, [9]);
  assert.equal(body.danhSachMon[0].gia, undefined);
  assert.deepEqual(h.cart(), []);
  assert.equal(h.context.location.href, '/DonHang/TheoDoi/ABC');
});

test('tracking polls without SignalR and renders completed and cancelled states', async () => {
  const h = harness();
  h.element('#track').dataset = { ma: 'ABC', trangThai: '1', daThanhToan: '0', mangDi: '1' };
  h.run('theodoi.js');
  assert.equal(h.intervals.length, 1); assert.equal(h.intervals[0].delay, 30000);
  await h.intervals[0].fn();
  assert.equal(h.element('#headline').textContent, 'Đơn đã hoàn thành');
  assert.equal(h.element('#khuQr').hidden, true);
  assert.equal(h.element('#payState').textContent, 'Đã thanh toán');
  const cancelled = harness('[]', { body: { maTrangThai: 5, daThanhToan: false } });
  cancelled.element('#track').dataset = { ma: 'DEF', trangThai: '1', mangDi: '1' };
  cancelled.run('theodoi.js'); await cancelled.intervals[0].fn();
  assert.equal(cancelled.element('#headline').textContent, 'Đơn đã bị hủy');
  assert.equal(cancelled.element('#khuQr').hidden, true);
});

test('unknown status does not overwrite the last valid display', async () => {
  const h = harness('[]', { body: { maTrangThai: 99, daThanhToan: false } });
  h.element('#track').dataset = { ma: 'ABC', trangThai: '2', daThanhToan: '0' };
  h.run('theodoi.js'); await h.intervals[0].fn();
  assert.equal(h.element('#headline').textContent, 'Quán đang pha chế cho bạn');
});

test('changing quantities invalidates the applied discount instead of showing an old total', async () => {
  const h = harness(JSON.stringify([row]), { body: { hopLe: true, soTienGiam: 5000 } });
  h.run('thucdon.js'); h.element('#maGiamGia').value = 'MIXUE10';
  await h.element('#apDungMa').listeners.click();
  assert.equal(h.element('#cartTotal').textContent, '45.000đ');
  h.clickAction('giam');
  assert.equal(h.element('#cartTotal').textContent, '25.000đ');
  assert.match(h.element('#maGiamGiaKetQua').textContent, /áp dụng lại/);
});

test('a late discount response cannot apply to a different cart', async () => {
  const h = harness(JSON.stringify([row])); h.run('thucdon.js');
  let finish;
  h.context.fetch = () => new Promise((resolve) => { finish = resolve; });
  h.element('#maGiamGia').value = 'MIXUE10';
  const pending = h.element('#apDungMa').listeners.click();
  h.clickAction('giam');
  finish({ ok: true, json: async () => ({ hopLe: true, soTienGiam: 5000 }) });
  await pending;
  assert.equal(h.element('#cartTotal').textContent, '25.000đ');
});

test('empty checkout does not send a request', async () => {
  const h = harness(); h.run('thucdon.js');
  await h.element('#sendOrder').listeners.click();
  assert.equal(h.requests.length, 0);
});

test('checkout rejects repeated submission and freezes cart changes until failure', async () => {
  const h = harness(JSON.stringify([row])); h.run('thucdon.js');
  let finish, sends = 0;
  h.context.fetch = () => { sends++; return new Promise((resolve) => { finish = resolve; }); };
  const pending = h.element('#sendOrder').listeners.click();
  await h.element('#sendOrder').listeners.click();
  h.clickAction('xoa');
  h.addProduct({ ma: '1', ten: 'Kem', gia: '10000', tuyChon: '0' });
  assert.equal(sends, 1); assert.equal(h.cart().length, 1);
  assert.equal(h.cart()[0].soLuong, 2);
  finish({ ok: false, json: async () => ({ thanhCong: false, thongBao: 'Thử lại' }) });
  await pending;
  h.clickAction('giam'); assert.equal(h.cart()[0].soLuong, 1);
});

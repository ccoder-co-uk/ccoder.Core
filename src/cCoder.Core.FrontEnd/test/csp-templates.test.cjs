const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.join(__dirname, '../assets/bootstrap/lib/widgets');
const encode = value => String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
function context() {
  const c = vm.createContext({ kendo: { htmlEncode: encode, template(value) {
    if (typeof value === 'function') return value;
    if (/#[:=]/.test(value)) throw new EvalError('String template requires dynamic compilation');
    return () => value;
  } } }, { codeGeneration: { strings: false, wasm: false } });
  vm.runInContext('class Widget {}', c);
  return c;
}
function load(c, name) {
  const file = name.slice(8);
  const source = fs.readFileSync(path.join(root, file[0].toLowerCase() + file.slice(1) + '.js'), 'utf8');
  vm.runInContext(source, c);
}
test('grid commands render functional links and HTML alongside static buttons under CSP', () => {
  const c = context(); load(c, 'Widgets.Grid');
  const render = vm.runInContext(`GridWidget.prototype.commandColumn.call({commands:[
    {name:'view',href:row=>'/record/'+row.Id,icon:'view',text:'View'},
    {template:row=>'<b>'+kendo.htmlEncode(row.Name)+'</b>'},
    {name:'save',icon:'save',text:'Save'}]})`, c);
  const html = c.kendo.template(render)({Id:42,Name:'A & <B>'});
  assert.match(html, /href='\/record\/42'/);
  assert.match(html, /<b>A &amp; &lt;B&gt;<\/b>/);
  assert.match(html, /name="save"/);
  assert.doesNotMatch(html, /function|=>/);
});
test('detail views render functional titles and fields without string compilation', () => {
  const c = context(); load(c, 'Widgets.Detail');
  const detail = vm.runInContext(`({header:true,title:row=>'Record '+row.Id,fields:[{field:'Name',title:'Name',description:'Name'}],config:{endpoint:'Example'},fieldValueExpression:()=>row=>kendo.htmlEncode(row.Name)})`, c);
  vm.runInContext('DetailWidget.prototype.buildTemplate', c).call(detail);
  const html = c.kendo.template(detail.template)({Id:42,Name:'A & <B>'});
  assert.match(html, /<h3>Record 42<\/h3>/);
  assert.match(html, /A &amp; &lt;B&gt;/);
});
test('legacy string command templates retain their Kendo compilation boundary', () => {
  const c = context(); load(c, 'Widgets.Grid');
  const templates = [];
  c.kendo.template = source => {
    templates.push(source);
    return row => source.replace('#=Id#', String(row.Id));
  };
  const render = vm.runInContext(`GridWidget.prototype.commandColumn.call({commands:[{name:'view',href:'/record/#=Id#',icon:'view',text:'View'}]})`, c);
  assert.match(render({Id:42}), /href='\/record\/42'/);
  assert.deepEqual(templates, ['/record/#=Id#']);
});

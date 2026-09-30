const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const sourcePath = path.join(
    __dirname,
    '../assets/bootstrap/lib/widgets/CMS.js');

async function expandPage(page) {
    let query;
    const nodeData = {
        data: { Id: 1 },
        children: { data: () => [] },
        items: []
    };
    const tree = {
        dataItem: () => nodeData
    };
    const context = vm.createContext({
        api: {
            get: async value => {
                query = value;
                return { value: [page] };
            }
        },
        app: { Id: 7 },
        container: {},
        $: () => ({ data: () => tree })
    });

    vm.runInContext('class Tree {}', context);
    vm.runInContext(fs.readFileSync(sourcePath, 'utf8'), context);
    await vm.runInContext(
        'CMS.prototype.expand.call({}, { node: {} })',
        context);

    return { query, node: nodeData.items[0] };
}

test('page tree does not make a leaf page expandable', async () => {
    const result = await expandPage({
        Id: 2,
        PageInfo: [{ Title: 'Leaf page' }],
        Pages: []
    });

    assert.match(result.query, /Pages\(\$select=Id;\$top=1\)/);
    assert.equal(result.node.text, 'Leaf page');
    assert.equal(result.node.hasChildren, false);
    assert.deepEqual(result.node.data, {
        Id: 2,
        PageInfo: [{ Title: 'Leaf page' }]
    });
});

test('page tree keeps the expand control for a page with children', async () => {
    const result = await expandPage({
        Id: 2,
        PageInfo: [{ Title: 'Parent page' }],
        Pages: [{ Id: 3 }]
    });

    assert.equal(result.node.text, 'Parent page');
    assert.equal(result.node.hasChildren, true);
    assert.deepEqual(result.node.data, {
        Id: 2,
        PageInfo: [{ Title: 'Parent page' }]
    });
});

import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const projectDirectory = path.resolve(
    path.dirname(fileURLToPath(import.meta.url)),
    "..");

async function createUtilContext(search = "") {
    const source = await readFile(
        path.join(
            projectDirectory,
            "assets/bootstrap/lib/Core/util.js"),
        "utf8");

    const context = vm.createContext({
        $: value => value,
        console,
        crypto: {
            randomUUID: () => "00000000-0000-0000-0000-000000000001"
        },
        kendo: {
            toString: date => date.toISOString()
        },
        session: {},
        setTimeout: () => {},
        unescape,
        window: {
            location: {
                href: `https://ccoder.co.uk/${search}`,
                search
            }
        }
    });

    new vm.Script(source)
        .runInContext(context);

    return context;
}

test("getQueryParameter returns a decoded query-string value", async () => {
    const context = await createUtilContext("?name=Paul%20Ward");

    const value = context.getQueryParameter("name");

    assert.equal(value, "Paul Ward");
});

test("removeQueryParameter preserves the remaining query-string values", async () => {
    const context = await createUtilContext();

    const url = context.removeQueryParameter(
        "removed",
        "https://ccoder.co.uk/?kept=true&removed=true");

    assert.equal(url, "https://ccoder.co.uk/?kept=true");
});

test("Guid returns the browser-generated UUID", async () => {
    const context = await createUtilContext();

    const value = context.Guid();

    assert.equal(value, "00000000-0000-0000-0000-000000000001");
});

import assert from "node:assert/strict";
import { readFile, stat } from "node:fs/promises";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const projectDirectory = path.resolve(
    path.dirname(fileURLToPath(import.meta.url)),
    "..");

const stageDirectory = path.join(projectDirectory, "stage");

test("build stages the public frontend assets", async () => {
    const expectedAssets = [
        "everything.js",
        "everything.min.js",
        "everything.css",
        "everything.min.css",
        "core.js",
        "editor.js",
        "monaco.js",
        "widget.js",
        "workflow.js",
        "framework.js",
        "background.js"
    ];

    for (const expectedAsset of expectedAssets) {
        const asset = await stat(path.join(stageDirectory, expectedAsset));

        assert.equal(asset.isFile(), true);
        assert.ok(asset.size > 0);
    }
});

test("build does not stage frontend source or dependency inputs", async () => {
    const excludedAssets = [
        "bootstrap/css/site.min.css",
        "bootstrap/lib/Core/api.js",
        "bootstrap/lib/core.js",
        "bootstrap/lib/widgets/widget.js",
        "css/site.min.css",
        "dependencies/dependencies.min.js",
        "dependencies/kendo/kendo-ui-license.js"
    ];

    for (const excludedAsset of excludedAssets) {
        await assert.rejects(
            stat(path.join(stageDirectory, excludedAsset)),
            error => error.code === "ENOENT");
    }
});

test("everything JavaScript is syntactically valid", async () => {
    const everything = await readFile(
        path.join(stageDirectory, "everything.js"),
        "utf8");

    assert.doesNotThrow(() => new vm.Script(everything));
});

test("framework preserves the configured bundle order", async () => {
    const framework = await readFile(
        path.join(stageDirectory, "framework.js"),
        "utf8");

    const drawingPosition = framework.indexOf("class Drawable");
    const widgetPosition = framework.indexOf("class Widget");
    const apiPosition = framework.indexOf("class Api");
    const workflowPosition = framework.indexOf("class WorkflowDesigner");

    assert.ok(drawingPosition >= 0);
    assert.ok(widgetPosition > drawingPosition);
    assert.ok(apiPosition > widgetPosition);
    assert.ok(workflowPosition > apiPosition);
});

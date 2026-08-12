import { mkdir, readFile, rm, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { transform } from "esbuild";

const projectDirectory = path.resolve(
    path.dirname(fileURLToPath(import.meta.url)),
    "..");

const assetsDirectory = path.join(projectDirectory, "assets");
const stageDirectory = path.join(projectDirectory, "stage");

const javascriptBundles = {
    "core.js": [
        "bootstrap/lib/Core/api.js",
        "bootstrap/lib/Core/util.js",
        "bootstrap/lib/Core/model.js"
    ],
    "editor.js": [
        "bootstrap/lib/Core/editor.js",
        "bootstrap/lib/Core/contentEditor.js",
        "bootstrap/lib/Core/pageToolbar.js"
    ],
    "monaco.js": [
        "bootstrap/lib/Monaco/MonacoEditor.js",
        "bootstrap/lib/Monaco/JavaScriptMonacoEditor.js",
        "bootstrap/lib/Monaco/HTMLMonacoEditor.js",
        "bootstrap/lib/Monaco/CSharpMonacoEditor.js"
    ],
    "widget.js": [
        "bootstrap/lib/widgets/widget.js",
        "bootstrap/lib/widgets/dialog.js",
        "bootstrap/lib/widgets/bootstrapDialog.js",
        "bootstrap/lib/widgets/bootstrapTabs.js",
        "bootstrap/lib/widgets/chart.js",
        "bootstrap/lib/widgets/pieChart.js",
        "bootstrap/lib/widgets/confirmDialog.js",
        "bootstrap/lib/widgets/consoleDialog.js",
        "bootstrap/lib/widgets/exportDialog.js",
        "bootstrap/lib/widgets/detail.js",
        "bootstrap/lib/widgets/editorDialog.js",
        "bootstrap/lib/widgets/grid.js",
        "bootstrap/lib/widgets/contextMenuWidget.js",
        "bootstrap/lib/widgets/fileDropContainerWidget.js",
        "bootstrap/lib/widgets/picker.js",
        "bootstrap/lib/widgets/readOnlyDetailView.js",
        "bootstrap/lib/widgets/tree.js",
        "bootstrap/lib/widgets/treeView.js",
        "bootstrap/lib/widgets/CMS.js",
        "bootstrap/lib/widgets/dataTreeView.js",
        "bootstrap/lib/widgets/odataTree.js",
        "bootstrap/lib/widgets/workspace.js",
        "bootstrap/lib/widgets/writableDetailView.js"
    ],
    "workflow.js": [
        "bootstrap/lib/workflow/close.js",
        "bootstrap/lib/workflow/handle.js",
        "bootstrap/lib/workflow/link.js",
        "bootstrap/lib/workflow/action.js",
        "bootstrap/lib/workflow/activity.js",
        "bootstrap/lib/workflow/connector.js",
        "bootstrap/lib/workflow/flow.js",
        "bootstrap/lib/workflow/workflowdesigner.js"
    ]
};

const dependencyFiles = [
    "dependencies/jquery/jquery-3.7.0.min.js",
    "dependencies/jquery/jquery-ui.min.js",
    "dependencies/jquery/jquery.validate.js",
    "dependencies/bootstrap/bootstrap.bundle.min.js",
    "dependencies/kendo/kendo.all.v2024.2.514.min.js",
    "dependencies/kendo/kendo.csp-templates.js",
    "dependencies/kendo/kendo-ui-license.js"
];

async function readFiles(relativePaths, rootDirectory = assetsDirectory) {
    const contents = await Promise.all(
        relativePaths.map(relativePath =>
            readFile(path.join(rootDirectory, relativePath), "utf8")));

    return contents.join("\n");
}

async function write(relativePath, contents) {
    const outputPath = path.join(stageDirectory, relativePath);

    await mkdir(path.dirname(outputPath), {
        recursive: true
    });

    await writeFile(outputPath, contents);
}

async function minify(contents, loader) {
    const result = await transform(contents, {
        legalComments: "inline",
        loader,
        minify: true,
        target: loader === "js" ? "es2018" : undefined
    });

    return result.code;
}

await rm(stageDirectory, {
    force: true,
    recursive: true
});

for (const [outputPath, inputPaths] of Object.entries(javascriptBundles)) {
    const contents = await readFiles(inputPaths);

    await write(outputPath, contents);
    await write(
        outputPath.replace(/\.js$/, ".min.js"),
        await minify(contents, "js"));
}

const drawing = await readFiles([
    "bootstrap/lib/Core/drawing.js"
]);

const dependencies = [
    await readFiles(dependencyFiles),
    await readFile(
        path.join(assetsDirectory, "dependencies/other/signalr.min.js"),
        "utf8")
].join("\n");

const framework = [
    dependencies,
    drawing,
    await readFile(path.join(stageDirectory, "widget.js"), "utf8"),
    await readFile(path.join(stageDirectory, "core.js"), "utf8")
].join("\n");

await write("framework.js", framework);
await write(
    "framework.min.js",
    await minify(framework, "js"));

const background = await readFiles([
    "bootstrap/lib/background.js"
]);

await write("background.js", background);
await write(
    "background.min.js",
    await minify(background, "js"));

const everything = [
    await readFile(
        path.join(stageDirectory, "framework.min.js"),
        "utf8"),
    await readFile(
        path.join(stageDirectory, "monaco.min.js"),
        "utf8"),
    await readFile(
        path.join(stageDirectory, "workflow.min.js"),
        "utf8")
].join("\n");

await write("everything.js", everything);
await write("everything.min.js", everything);

const codeEditor = [
    await readFile(
        path.join(assetsDirectory, "dependencies/monaco/worker.js"),
        "utf8"),
    await readFile(
        path.join(stageDirectory, "monaco.js"),
        "utf8"),
    await readFile(
        path.join(assetsDirectory, "dependencies/monaco/runtime.js"),
        "utf8"),
    await readFile(
        path.join(assetsDirectory, "dependencies/monaco/javascript-validation.js"),
        "utf8"),
    await readFile(
        path.join(assetsDirectory, "dependencies/monaco/web-languages.js"),
        "utf8")
].join("\n");

await write("code-editor.js", codeEditor);
await write(
    "code-editor.min.js",
    await minify(codeEditor, "js"));

const codeEditorCss = await readFile(
    path.join(assetsDirectory, "dependencies/monaco/runtime.css"),
    "utf8");

await write("code-editor.css", codeEditorCss);
await write("code-editor.min.css", codeEditorCss);

const bootstrapSiteCss = await readFile(
    path.join(assetsDirectory, "bootstrap/css/site.css"),
    "utf8");

const dependencyCss = await readFiles([
    "dependencies/bootstrap/bootstrap.min.css",
    "dependencies/kendo/kendo.v2024.2.514.bootstrap.css"
]);

const everythingCss = [
    dependencyCss,
    await minify(bootstrapSiteCss, "css")
].join("\n");

await write("everything.css", everythingCss);
await write("everything.min.css", everythingCss);

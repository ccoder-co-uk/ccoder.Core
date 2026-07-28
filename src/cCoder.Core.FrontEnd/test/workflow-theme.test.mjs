import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import test from "node:test";
import vm from "node:vm";
import { fileURLToPath } from "node:url";

const projectDirectory = path.resolve(
    path.dirname(fileURLToPath(import.meta.url)),
    "..");
const source = await readFile(
    path.join(
        projectDirectory,
        "assets/bootstrap/lib/workflow/workflowdesigner.js"),
    "utf8");
const context = {};

vm.runInNewContext(source, context);

const primaryTheme = {
    colours: {
        primary: "#111111",
        secondary: "#222222"
    }
};
const alternateTheme = {
    colours: {
        primary: "#333333",
        secondary: "#444444"
    }
};

test("resolves the requested configured workflow theme", () => {
    const app = {
        Config: {
            Themes: {
                Default: primaryTheme,
                Dark: alternateTheme
            }
        },
        DefaultTheme: "Default"
    };

    assert.equal(context.resolveWorkflowTheme(app, "Dark"), alternateTheme);
});

test("matches workflow theme names without case sensitivity", () => {
    const app = {
        Config: {
            Themes: {
                Default: primaryTheme
            }
        }
    };

    assert.equal(context.resolveWorkflowTheme(app, "default"), primaryTheme);
});

test("uses the app default when the requested theme is unavailable", () => {
    const app = {
        Config: {
            Themes: {
                Brand: alternateTheme
            }
        },
        DefaultTheme: "Brand"
    };

    assert.equal(context.resolveWorkflowTheme(app, "Missing"), alternateTheme);
});

test("uses a safe workflow palette when app theme configuration is missing", () => {
    const theme = context.resolveWorkflowTheme({}, "Missing");

    assert.equal(theme.colours.primary, "#142A48");
    assert.equal(theme.colours.secondary, "#52BCFF");
});

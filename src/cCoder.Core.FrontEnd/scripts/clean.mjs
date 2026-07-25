import { rm } from "node:fs/promises";

await rm(new URL("../stage", import.meta.url), {
    force: true,
    recursive: true
});

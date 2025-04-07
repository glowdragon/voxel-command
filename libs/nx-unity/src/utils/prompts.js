"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.promptForLibrary = exports.promptForUnityProject = exports.promptForUnityVersion = void 0;
const tslib_1 = require("tslib");
const enquirer_1 = require("enquirer");
const workspace_1 = require("./workspace");
const unity_version_1 = require("./unity-version");
function promptForUnityVersion(basePath, message) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const result = yield (0, enquirer_1.prompt)({
            type: "select",
            name: "version",
            message: message,
            choices: (0, unity_version_1.getAvailableUnityVersions)(basePath),
        });
        return result.version;
    });
}
exports.promptForUnityVersion = promptForUnityVersion;
function promptForUnityProject(tree, message) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const result = yield (0, enquirer_1.prompt)({
            type: "select",
            name: "project",
            message: message,
            choices: (0, workspace_1.getUnityProjects)(tree),
        });
        return result.project;
    });
}
exports.promptForUnityProject = promptForUnityProject;
function promptForLibrary(tree, message) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const result = yield (0, enquirer_1.prompt)({
            type: "select",
            name: "library",
            message: message,
            choices: (0, workspace_1.getLibraries)(tree),
        });
        return result.library;
    });
}
exports.promptForLibrary = promptForLibrary;
//# sourceMappingURL=prompts.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const tslib_1 = require("tslib");
const exec_1 = require("../../utils/exec");
const tree_1 = require("nx/src/generators/tree");
const platform_1 = require("../../utils/platform");
const workspace_root_1 = require("nx/src/utils/workspace-root");
const fs_extra_1 = require("fs-extra");
const devkit_1 = require("@nx/devkit");
const posix_1 = require("../../utils/posix");
const path = require("path");
function runExecutor(options, context) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        // Get the Unity version from the ProjectVersion.txt
        const projectPath = (0, posix_1.posixJoin)(context.root, context.workspace.projects[context.projectName].root);
        const tree = new tree_1.FsTree(projectPath, context.isVerbose);
        const versionFileContent = tree
            .read((0, posix_1.posixJoin)("ProjectSettings", "ProjectVersion.txt"))
            .toString();
        const version = versionFileContent.split("\n")[0].split(" ")[1];
        // Copy the project to a temporary directory
        const tempProjectPath = (0, posix_1.posixJoin)(workspace_root_1.workspaceRoot, "tmp", context.projectName);
        const filter = (file) => {
            return !file.startsWith(path.join(projectPath, "Temp/"));
        };
        devkit_1.output.log({ title: "Copying project to temporary directory", bodyLines: [tempProjectPath] });
        (0, fs_extra_1.copySync)(projectPath, tempProjectPath, { filter });
        // Build the project using the Unity CLI
        const unityBinaryPath = (0, platform_1.getUnityBinaryPath)(version);
        const command = `"${unityBinaryPath}" -quit -batchmode -nographics -logFile - -executeMethod ${options.executeMethod} -projectPath "${tempProjectPath}"`;
        yield (0, exec_1.executeCommand)(command);
        return { success: true };
    });
}
exports.default = runExecutor;
//# sourceMappingURL=executor.js.map
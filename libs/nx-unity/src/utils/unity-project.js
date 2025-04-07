"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.addDependencyToUnityProject = exports.addWorkspacePackageToUnityProject = exports.createUnityProject = void 0;
const tslib_1 = require("tslib");
const devkit_1 = require("@nx/devkit");
const exec_1 = require("./exec");
const platform_1 = require("./platform");
const posix_1 = require("./posix");
/**
 * Creates a new Unity project from scratch by running the Unity CLI with the -createProject flag.
 */
function createUnityProject(unityBasePath, unityVersion, projectRoot) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const unityBinaryPath = (0, posix_1.posixJoin)(unityBasePath, unityVersion, (0, platform_1.getUnityBinaryRelativePath)());
        const command = `"${unityBinaryPath}" -quit -batchmode -nographics -logFile - -createProject ${projectRoot}`;
        yield (0, exec_1.executeCommand)(command);
    });
}
exports.createUnityProject = createUnityProject;
function addWorkspacePackageToUnityProject(tree, projectName, packageName) {
    const workspaceLayout = (0, devkit_1.getWorkspaceLayout)(tree);
    return addDependencyToUnityProject(tree, projectName, packageName, "file:" + (0, posix_1.posixJoin)("..", "..", "..", workspaceLayout.libsDir, packageName));
}
exports.addWorkspacePackageToUnityProject = addWorkspacePackageToUnityProject;
/**
 * Adds a dependency to a Unity project.
 * @returns true if the dependency was added, false if it was already present
 */
function addDependencyToUnityProject(tree, projectName, dependencyKey, dependencyValue) {
    const workspaceLayout = (0, devkit_1.getWorkspaceLayout)(tree);
    const manifestPath = (0, posix_1.posixJoin)(workspaceLayout.appsDir, projectName, "Packages", "manifest.json");
    const manifest = JSON.parse(tree.read(manifestPath).toString());
    const dependencies = manifest.dependencies;
    if (!dependencies[dependencyKey]) {
        dependencies[dependencyKey] = dependencyValue;
        tree.write(manifestPath, JSON.stringify(manifest, null, 2));
        return true;
    }
    return false;
}
exports.addDependencyToUnityProject = addDependencyToUnityProject;
//# sourceMappingURL=unity-project.js.map
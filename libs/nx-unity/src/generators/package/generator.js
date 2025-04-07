"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.packageGenerator = void 0;
const tslib_1 = require("tslib");
const devkit_1 = require("@nx/devkit");
const assemblies_1 = require("../../utils/assemblies");
const workspace_1 = require("../../utils/workspace");
const posix_1 = require("../../utils/posix");
function packageGenerator(tree, options) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const { name: projectName } = options;
        const projectRoot = (0, posix_1.posixJoin)((0, devkit_1.getWorkspaceLayout)(tree).libsDir, projectName);
        // Add the project to the Nx workspace
        (0, devkit_1.addProjectConfiguration)(tree, options.name, {
            root: projectRoot,
            projectType: "library",
            sourceRoot: `${projectRoot}`,
            targets: {},
        });
        // Copy default files
        (0, devkit_1.generateFiles)(tree, (0, posix_1.posixJoin)(__dirname, "files"), projectRoot, options);
        // Create assembly definitions
        (0, assemblies_1.createAssemblyDefinition)(tree, (0, posix_1.posixJoin)(projectRoot, "Runtime"), options.assemblyName);
        (0, assemblies_1.createAssemblyDefinition)(tree, (0, posix_1.posixJoin)(projectRoot, "Editor"), options.assemblyName + ".Editor");
        (0, assemblies_1.createAssemblyDefinition)(tree, (0, posix_1.posixJoin)(projectRoot, "Tests", "Runtime"), options.assemblyName + ".Tests");
        (0, assemblies_1.createAssemblyDefinition)(tree, (0, posix_1.posixJoin)(projectRoot, "Tests", "Editor"), options.assemblyName + ".Editor.Tests");
        // Add to global package.json
        const packageJson = JSON.parse(tree.read("package.json").toString());
        if (!packageJson.unityDependencies) {
            packageJson.unityDependencies = {};
        }
        packageJson.unityDependencies[projectName] = `file:${projectRoot}`;
        tree.write("package.json", JSON.stringify(packageJson, null, 2));
        // Add implicit dependency to all Unity projects in the Nx config
        (0, workspace_1.getUnityProjects)(tree).forEach((unityProjectName) => {
            (0, workspace_1.addImplicitDependency)(tree, unityProjectName, projectName);
        });
        yield (0, devkit_1.formatFiles)(tree);
    });
}
exports.packageGenerator = packageGenerator;
exports.default = packageGenerator;
//# sourceMappingURL=generator.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.createAssemblyDefinition = void 0;
const posix_1 = require("./posix");
/**
 * Creates an assembly definition file.
 * @param tree The tree to write the file to.
 * @param outputPath The path to the output directory.
 * @param name The name of the assembly definition, e.g. `MyCompany.MyPackage`.
 */
function createAssemblyDefinition(tree, outputPath, name) {
    tree.write((0, posix_1.posixJoin)(outputPath, `${name}.asmdef`), JSON.stringify({
        name: name,
        rootNamespace: "",
        references: [],
        includePlatforms: [],
        excludePlatforms: [],
        allowUnsafeCode: false,
        overrideReferences: false,
        precompiledReferences: [],
        autoReferenced: true,
        defineConstraints: [],
        versionDefines: [],
        noEngineReferences: false,
    }));
}
exports.createAssemblyDefinition = createAssemblyDefinition;
//# sourceMappingURL=assemblies.js.map
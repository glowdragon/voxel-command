import { Tree } from "@nx/devkit";
/**
 * Creates an assembly definition file.
 * @param tree The tree to write the file to.
 * @param outputPath The path to the output directory.
 * @param name The name of the assembly definition, e.g. `MyCompany.MyPackage`.
 */
declare function createAssemblyDefinition(tree: Tree, outputPath: string, name: string): void;
export { createAssemblyDefinition };

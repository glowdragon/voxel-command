import { Tree } from "@nx/devkit";
declare function promptForUnityVersion(basePath: string, message: string): Promise<string>;
declare function promptForUnityProject(tree: Tree, message: string): Promise<string>;
declare function promptForLibrary(tree: Tree, message: string): Promise<string>;
export { promptForUnityVersion, promptForUnityProject, promptForLibrary };

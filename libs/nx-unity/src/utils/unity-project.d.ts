import { Tree } from "@nx/devkit";
/**
 * Creates a new Unity project from scratch by running the Unity CLI with the -createProject flag.
 */
declare function createUnityProject(unityBasePath: string, unityVersion: string, projectRoot: string): Promise<void>;
declare function addWorkspacePackageToUnityProject(tree: Tree, projectName: string, packageName: string): boolean;
/**
 * Adds a dependency to a Unity project.
 * @returns true if the dependency was added, false if it was already present
 */
declare function addDependencyToUnityProject(tree: Tree, projectName: string, dependencyKey: string, dependencyValue: string): boolean;
export { createUnityProject, addWorkspacePackageToUnityProject, addDependencyToUnityProject };

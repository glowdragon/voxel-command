import { Tree } from "@nx/devkit";
declare function getUnityProjects(tree: Tree): any[];
declare function getApplications(tree: Tree): any[];
declare function getUnityPackages(tree: Tree): any[];
declare function getLibraries(tree: Tree): any[];
/**
 * Adds an implicit dependency to a project.
 * @returns true if the dependency was added, false if it was already present
 */
declare function addImplicitDependency(tree: Tree, projectName: string, dependency: string): boolean;
export { getUnityProjects, getApplications, getUnityPackages, getLibraries, addImplicitDependency };

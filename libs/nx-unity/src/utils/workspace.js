"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.addImplicitDependency = exports.getLibraries = exports.getUnityPackages = exports.getApplications = exports.getUnityProjects = void 0;
const devkit_1 = require("@nx/devkit");
const posix_1 = require("./posix");
function getUnityProjects(tree) {
    return getApplications(tree).filter((application) => {
        const projectConfig = (0, devkit_1.readProjectConfiguration)(tree, application);
        return projectConfig.sourceRoot.includes("Assets");
    });
}
exports.getUnityProjects = getUnityProjects;
function getApplications(tree) {
    const applications = [];
    (0, devkit_1.getProjects)(tree).forEach((project) => {
        if (project.projectType === "application") {
            applications.push(project.name);
        }
    });
    return applications;
}
exports.getApplications = getApplications;
function getUnityPackages(tree) {
    return getLibraries(tree).filter((library) => {
        const projectConfig = (0, devkit_1.readProjectConfiguration)(tree, library);
        const packageJsonExists = tree.exists((0, posix_1.posixJoin)(projectConfig.sourceRoot, "package.json"));
        const packageJsonMetaExists = tree.exists((0, posix_1.posixJoin)(projectConfig.sourceRoot, "package.json.meta"));
        const runtimeFolderExists = tree.exists((0, posix_1.posixJoin)(projectConfig.sourceRoot, "Runtime"));
        return packageJsonExists && (packageJsonMetaExists || runtimeFolderExists);
    });
}
exports.getUnityPackages = getUnityPackages;
function getLibraries(tree) {
    const libraries = [];
    (0, devkit_1.getProjects)(tree).forEach((project) => {
        if (project.name && project.projectType === "library") {
            libraries.push(project.name);
        }
    });
    return libraries;
}
exports.getLibraries = getLibraries;
/**
 * Adds an implicit dependency to a project.
 * @returns true if the dependency was added, false if it was already present
 */
function addImplicitDependency(tree, projectName, dependency) {
    const projectConfig = (0, devkit_1.readProjectConfiguration)(tree, projectName);
    const implicitDependencies = projectConfig.implicitDependencies || [];
    if (!implicitDependencies.includes(dependency)) {
        implicitDependencies.push(dependency);
        projectConfig.implicitDependencies = implicitDependencies;
        (0, devkit_1.updateProjectConfiguration)(tree, projectName, projectConfig);
        return true;
    }
    return false;
}
exports.addImplicitDependency = addImplicitDependency;
//# sourceMappingURL=workspace.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.installPackage = exports.installPackages = void 0;
const tslib_1 = require("tslib");
const exec_1 = require("./exec");
const devkit_1 = require("@nx/devkit");
const posix_1 = require("./posix");
function installPackages(projectRoot_1) {
    return tslib_1.__awaiter(this, arguments, void 0, function* (projectRoot, packageNames = undefined, isDevDependency = false) {
        const packageManager = (0, devkit_1.detectPackageManager)((0, posix_1.posixJoin)(devkit_1.workspaceRoot));
        let command = packageManager === "yarn" ? "yarn" : "npm install";
        if (packageNames !== undefined) {
            command =
                packageManager === "yarn"
                    ? `yarn add ${packageNames.join(" ")}`
                    : `npm install ${packageNames.join(" ")}`;
            if (isDevDependency) {
                command += packageManager === "yarn" ? " --dev" : " --save-dev";
            }
        }
        yield (0, exec_1.executeCommand)(command, (0, posix_1.posixJoin)(devkit_1.workspaceRoot, projectRoot));
    });
}
exports.installPackages = installPackages;
function installPackage(projectRoot, packageName, isDevDependency) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        yield installPackages(projectRoot, [packageName], isDevDependency);
    });
}
exports.installPackage = installPackage;
//# sourceMappingURL=package-manager.js.map
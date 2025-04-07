"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getAvailableUnityVersions = void 0;
const fs_1 = require("fs");
function getAvailableUnityVersions(basePath) {
    try {
        return (0, fs_1.readdirSync)(basePath);
    }
    catch (error) {
        console.error(`Failed to read directory: ${basePath}`, error);
        throw error;
    }
}
exports.getAvailableUnityVersions = getAvailableUnityVersions;
//# sourceMappingURL=unity-version.js.map
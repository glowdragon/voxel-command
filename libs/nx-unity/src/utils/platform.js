"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getUnityBinaryPath = exports.getUnityBinaryRelativePath = exports.getUnityBasePath = void 0;
const os = require("os");
const path = require("path");
const UNITY_BASE_PATHS = {
    win32: path.join("C:", "Program Files", "Unity", "Hub", "Editor"),
    darwin: path.join("/", "Applications", "Unity", "Hub", "Editor"),
    linux: path.join("~", "Unity", "Hub", "Editor"),
};
const UNITY_EXECUTABLE_PATHS = {
    win32: path.join("Editor", "Unity.exe"),
    darwin: path.join("Unity.app", "Contents", "MacOS", "Unity"),
    linux: path.join("Unity"),
};
/**
 * Returns the default path to the Unity base directory.
 * Based on the platform, the base directory has a different path.
 */
function getUnityBasePath() {
    const platform = os.platform();
    const unityBaseDir = UNITY_BASE_PATHS[platform];
    if (!unityBaseDir) {
        throw new Error("Unsupported platform");
    }
    return unityBaseDir;
}
exports.getUnityBasePath = getUnityBasePath;
/**
 * Returns the default path to the Unity executable relative to the Unity base directory + version.
 * Based on the platform, the executable has a different path.
 */
function getUnityBinaryRelativePath() {
    const platform = os.platform();
    const path = UNITY_EXECUTABLE_PATHS[platform];
    if (!path) {
        throw new Error("Unsupported platform");
    }
    return path;
}
exports.getUnityBinaryRelativePath = getUnityBinaryRelativePath;
function getUnityBinaryPath(version) {
    const unityBasePath = getUnityBasePath();
    const unityBinaryRelativePath = getUnityBinaryRelativePath();
    return path.join(unityBasePath, version, unityBinaryRelativePath);
}
exports.getUnityBinaryPath = getUnityBinaryPath;
//# sourceMappingURL=platform.js.map
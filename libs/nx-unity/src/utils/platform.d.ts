/**
 * Returns the default path to the Unity base directory.
 * Based on the platform, the base directory has a different path.
 */
declare function getUnityBasePath(): string;
/**
 * Returns the default path to the Unity executable relative to the Unity base directory + version.
 * Based on the platform, the executable has a different path.
 */
declare function getUnityBinaryRelativePath(): string;
declare function getUnityBinaryPath(version: string): string;
export { getUnityBasePath, getUnityBinaryRelativePath, getUnityBinaryPath };

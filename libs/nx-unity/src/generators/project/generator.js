"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.projectGenerator = void 0;
const tslib_1 = require("tslib");
const devkit_1 = require("@nx/devkit");
const platform_1 = require("../../utils/platform");
const prompts_1 = require("../../utils/prompts");
const fs = require("fs");
const os = require("os");
const axios_1 = require("axios");
const adm_zip_1 = require("adm-zip");
const unity_project_1 = require("../../utils/unity-project");
const package_manager_1 = require("../../utils/package-manager");
const workspace_1 = require("../../utils/workspace");
const posix_1 = require("../../utils/posix");
const exec_1 = require("../../utils/exec");
function projectGenerator(tree, options) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        const { name: projectName } = options;
        const projectRoot = (0, posix_1.posixJoin)((0, devkit_1.getWorkspaceLayout)(tree).appsDir, projectName);
        // Add the project to the Nx workspace
        (0, devkit_1.addProjectConfiguration)(tree, options.name, {
            root: projectRoot,
            projectType: "application",
            sourceRoot: (0, posix_1.posixJoin)(projectRoot, "Assets"),
            targets: {
                build: {
                    executor: "nx-unity:build",
                    configurations: {
                        windows: {
                            executeMethod: "BuildCommands.BuildWindows",
                        },
                        macos: {
                            executeMethod: "BuildCommands.BuildMacOS",
                        },
                        linux: {
                            executeMethod: "BuildCommands.BuildLinux",
                        },
                        android: {
                            executeMethod: "BuildCommands.BuildAndroid",
                        },
                        ios: {
                            executeMethod: "BuildCommands.BuildiOS",
                        },
                        webgl: {
                            executeMethod: "BuildCommands.BuildWebGL",
                        },
                    },
                    defaultConfiguration: "windows",
                },
            },
            implicitDependencies: (0, workspace_1.getUnityPackages)(tree),
        });
        // Check if Unity is installed
        const unityBasePath = (0, platform_1.getUnityBasePath)();
        if (fs.existsSync(unityBasePath) === false) {
            throw new Error(`Unity installation not found at ${unityBasePath}`);
        }
        // Let the user select the Unity version
        const unityVersion = yield (0, prompts_1.promptForUnityVersion)(unityBasePath, "Select Unity version");
        // Copy general starter files
        (0, devkit_1.generateFiles)(tree, (0, posix_1.posixJoin)(__dirname, "files"), projectRoot, options);
        // Download and extract the selected template
        const basePath = (0, posix_1.posixJoin)(os.tmpdir(), "nx-unity", "templates", unityVersion);
        const templatePath = (0, posix_1.posixJoin)(basePath, options.template);
        const templateZipPath = `${templatePath}.zip`;
        const downloadUrl = `https://nx-unity-cdn.vercel.app/templates/${unityVersion}/${options.template}.zip`;
        try {
            fs.mkdirSync(templatePath, { recursive: true });
            yield downloadFile(downloadUrl, templateZipPath);
            unzipFile(templateZipPath, templatePath);
            (0, devkit_1.generateFiles)(tree, templatePath, projectRoot, options);
        }
        catch (e) {
            if (options.template === "builtin") {
                devkit_1.output.warn({
                    title: "Creating project from scratch",
                    bodyLines: [
                        `Failed to download template from ${downloadUrl}`,
                        "Creating a new project from scratch instead",
                    ],
                });
                yield (0, unity_project_1.createUnityProject)(unityBasePath, unityVersion, projectRoot);
            }
            else {
                throw new Error(`Failed to download template from ${downloadUrl}`);
            }
        }
        // Install OpenUPM
        yield (0, package_manager_1.installPackage)("", "openupm-cli", true);
        // Add the Unity package of the Nx plugin
        yield (0, exec_1.executeCommand)("npx openupm add com.danielkreitsch.nx-unity -r http://verdaccio.danielkreitsch.com", projectRoot);
        yield (0, devkit_1.formatFiles)(tree);
    });
}
exports.projectGenerator = projectGenerator;
function downloadFile(url, outputFilePath) {
    return tslib_1.__awaiter(this, void 0, void 0, function* () {
        return (0, axios_1.default)({
            method: "GET",
            url: url,
            responseType: "stream",
        }).then((response) => {
            return new Promise((resolve, reject) => {
                const stream = fs.createWriteStream(outputFilePath);
                response.data.pipe(stream).on("finish", resolve).on("error", reject);
            });
        });
    });
}
function unzipFile(zipFilePath, outputDir) {
    const zip = new adm_zip_1.default(zipFilePath);
    zip.extractAllTo(outputDir, true);
}
exports.default = projectGenerator;
//# sourceMappingURL=generator.js.map
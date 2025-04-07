"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.executeCommand = void 0;
const tslib_1 = require("tslib");
/**
 * Executes a command and logs the output to the console.
 * @returns true if the command was successful
 */
function executeCommand(command_1) {
    return tslib_1.__awaiter(this, arguments, void 0, function* (command, workingDirectory = null) {
        const options = {};
        if (workingDirectory) {
            options.cwd = workingDirectory;
            if (isVerbose) {
                console.debug(`Working directory: ${options.cwd}`);
            }
        }
        const { exec } = yield Promise.resolve().then(() => require("child_process"));
        return new Promise((resolve, reject) => {
            var _a, _b;
            if (isVerbose) {
                console.debug(`Executing command: ${command}`);
            }
            const process = exec(command, options);
            (_a = process.stdout) === null || _a === void 0 ? void 0 : _a.on("data", (data) => console.log(data.toString()));
            (_b = process.stderr) === null || _b === void 0 ? void 0 : _b.on("data", (data) => console.error(data.toString()));
            process.on("exit", (code) => {
                if (code === 0) {
                    resolve(true);
                }
                else {
                    reject(new Error(`Command '${command}' exited with code ${code}`));
                }
            });
        });
    });
}
exports.executeCommand = executeCommand;
function isVerbose() {
    return process.argv.includes("--verbose");
}
//# sourceMappingURL=exec.js.map
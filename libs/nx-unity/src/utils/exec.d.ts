/**
 * Executes a command and logs the output to the console.
 * @returns true if the command was successful
 */
declare function executeCommand(command: string, workingDirectory?: string | null): Promise<boolean>;
export { executeCommand };

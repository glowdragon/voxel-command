declare function installPackages(projectRoot: string, packageNames?: string[] | undefined, isDevDependency?: boolean): Promise<void>;
declare function installPackage(projectRoot: string, packageName: string, isDevDependency: boolean): Promise<void>;
export { installPackages, installPackage };

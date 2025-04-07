import { BuildExecutorSchema } from "./schema";
import { ExecutorContext } from "nx/src/config/misc-interfaces";
export default function runExecutor(options: BuildExecutorSchema, context: ExecutorContext): Promise<{
    success: boolean;
}>;

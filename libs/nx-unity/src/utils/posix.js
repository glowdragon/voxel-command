"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.posixJoin = void 0;
const path = require("path");
function posixJoin(...segments) {
    const joinedPath = path.join(...segments);
    return joinedPath.replace(/\\/g, "/");
}
exports.posixJoin = posixJoin;
//# sourceMappingURL=posix.js.map
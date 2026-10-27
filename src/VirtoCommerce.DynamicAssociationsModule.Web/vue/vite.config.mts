import path from "node:path";
import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

// Dynamic associations admin UI for the Admin Shell host (Module Federation remote),
// served by the platform at /modules/$(VirtoCommerce.DynamicAssociationsModule)/plugins/admin-shell/
const base = "/modules/$(VirtoCommerce.DynamicAssociationsModule)/plugins/admin-shell/";
process.env.APP_BASE_PATH = base;

export default getDynamicModuleConfiguration({
  appId: "admin-shell",
  moduleRoot: path.resolve(__dirname, ".."),
  entry: "./src/modules/index.ts",
  remoteName: "dynamicassociations_admin",
  base,
});

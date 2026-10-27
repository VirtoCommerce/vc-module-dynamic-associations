import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule } from "@vc-shell/framework";
import "./styles.scss";

export default defineAppModule({
  blades,
  locales,
});

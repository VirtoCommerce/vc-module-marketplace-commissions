import * as locales from "./locales";
import { i18n, useExtensionSlot } from "@vc-shell/framework";
import { Router } from "vue-router";
import { App } from "vue";
import { CommissionsFeeField } from "./components";

export default {
  install(app: App, options: { router: Router }) {
    if (locales) {
      Object.entries(locales).forEach(([key, message]) => {
        // Merge locale messages, overwriting existing ones
        i18n.global.mergeLocaleMessage(key, message);
      });
    }

    const { addComponent } = useExtensionSlot("commissions-fee");

    addComponent({
      id: "commissions-fee",
      component: CommissionsFeeField,
    });
  },
};

export * from "./pages";
export * from "./composables";

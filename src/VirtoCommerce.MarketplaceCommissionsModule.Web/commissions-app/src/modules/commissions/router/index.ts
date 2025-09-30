import { RouteRecordRaw } from "vue-router";
import { DemoCommissions } from "../pages";

export const routes: RouteRecordRaw[] = [
  {
    path: "/demo",
    name: "DemoCommissions",
    component: DemoCommissions,
  },
];

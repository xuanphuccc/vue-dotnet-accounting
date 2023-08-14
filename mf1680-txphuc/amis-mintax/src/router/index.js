import Vue from "vue";
import VueRouter from "vue-router";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "home",
    component: null,
    meta: { layout: "default" },
  },
  {
    path: "/employee",
    name: "employee",
    component: null,
    meta: { layout: "default" },
  },
  { path: "/:pathMatch(.*)*", component: null, meta: { layout: "empty" } },
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

export default router;
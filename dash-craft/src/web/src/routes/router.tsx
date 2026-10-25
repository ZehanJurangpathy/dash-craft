import { createBrowserRouter, type RouteObject } from "react-router";
import { HomeRoute } from "./HomeRoute";
import { NotFoundRoute } from "./NotFoundRoute";

/** The central route table. Route components stay thin and compose feature components. */
export const routes: RouteObject[] = [
  { path: "/", element: <HomeRoute /> },
  { path: "*", element: <NotFoundRoute /> },
];

export function createAppRouter() {
  return createBrowserRouter(routes);
}

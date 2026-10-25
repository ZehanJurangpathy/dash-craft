import { useState } from "react";
import { RouterProvider } from "react-router";
import { createAppRouter } from "../routes/router";
import { AppProviders } from "./providers";

export function App() {
  const [router] = useState(createAppRouter);
  return (
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>
  );
}

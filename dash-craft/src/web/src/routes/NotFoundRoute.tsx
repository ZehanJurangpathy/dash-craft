import { Link } from "react-router";

export function NotFoundRoute() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 px-4 text-center">
      <h1 className="text-2xl font-semibold text-slate-900">Page not found</h1>
      <Link to="/" className="text-blue-700 underline hover:text-blue-900">
        Go to the home page
      </Link>
    </main>
  );
}

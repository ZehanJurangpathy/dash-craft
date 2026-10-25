import { HealthStatus } from "../../health/components/HealthStatus";

export function HomePage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-6 bg-slate-50 px-4 text-center">
      <h1 className="text-4xl font-bold tracking-tight text-slate-900 sm:text-5xl">DashCraft</h1>
      <p className="max-w-md text-lg text-slate-600">
        Build dashboards from your SQL Server data without writing SQL.
      </p>
      <HealthStatus />
    </main>
  );
}

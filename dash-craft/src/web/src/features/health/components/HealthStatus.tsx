import { StatusBadge, type StatusTone } from "../../../components/StatusBadge";
import { useHealth } from "../api/useHealth";
import type { HealthStatus as Status } from "../schemas/health";

const toneByStatus: Record<Status, StatusTone> = {
  Healthy: "success",
  Degraded: "warning",
  Unhealthy: "danger",
};

export function HealthStatus() {
  const health = useHealth();

  return (
    <div role="status" aria-live="polite" className="flex flex-col items-center gap-3">
      {health.isPending ? (
        <StatusBadge tone="neutral">Checking API status…</StatusBadge>
      ) : health.isError ? (
        <p className="text-sm text-red-700">We can&apos;t reach the DashCraft API right now. Check that it is running.</p>
      ) : (
        <>
          <StatusBadge tone={toneByStatus[health.data.status]}>API: {health.data.status}</StatusBadge>
          {health.data.checks
            .filter((check) => check.status !== "Healthy")
            .map((check) => (
              <p key={check.name} className="text-sm text-slate-600">
                {check.description}
              </p>
            ))}
        </>
      )}
    </div>
  );
}

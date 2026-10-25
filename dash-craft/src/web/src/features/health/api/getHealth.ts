import { apiGet } from "../../../lib/apiClient";
import { healthReportSchema, type HealthReport } from "../schemas/health";

/** GET /health. A 503 still carries a valid report (a dependency is unhealthy). */
export function getHealth(signal?: AbortSignal): Promise<HealthReport> {
  return apiGet("/health", healthReportSchema, { signal, acceptStatuses: [503] });
}

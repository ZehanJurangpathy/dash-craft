import { z } from "zod";

// Mirrors specs/001-project-foundation/contracts/health.openapi.yaml.

export const healthStatusSchema = z.enum(["Healthy", "Degraded", "Unhealthy"]);

export const healthCheckResultSchema = z.object({
  name: z.string(),
  status: healthStatusSchema,
  description: z.string().optional(),
  durationMs: z.number().nonnegative(),
});

export const healthReportSchema = z.object({
  status: healthStatusSchema,
  totalDurationMs: z.number().nonnegative(),
  checks: z.array(healthCheckResultSchema),
});

export type HealthStatus = z.infer<typeof healthStatusSchema>;
export type HealthCheckResult = z.infer<typeof healthCheckResultSchema>;
export type HealthReport = z.infer<typeof healthReportSchema>;

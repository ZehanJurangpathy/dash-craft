import { z } from "zod";

const problemDetailsSchema = z.looseObject({
  type: z.string().optional(),
  title: z.string().optional(),
  status: z.number().optional(),
  detail: z.string().optional(),
  traceId: z.string().optional(),
  errors: z.record(z.string(), z.array(z.string())).optional(),
});

export type ProblemDetails = z.infer<typeof problemDetailsSchema>;

/** The API answered, but with a status the caller did not accept. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | undefined;

  constructor(status: number, problem: ProblemDetails | undefined) {
    super(problem?.title ?? `Request failed with status ${String(status)}`);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }
}

export interface ApiGetOptions {
  signal?: AbortSignal;
  /** Non-2xx statuses whose body is still a valid response (e.g. 503 from /health). */
  acceptStatuses?: number[];
}

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

/**
 * GETs a JSON resource and validates it with the given Zod schema. Network failures propagate as
 * TypeError, unexpected statuses as ApiError, and invalid bodies as ZodError.
 */
export async function apiGet<T>(path: string, schema: z.ZodType<T>, options: ApiGetOptions = {}): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Accept: "application/json" },
    signal: options.signal,
  });

  if (!response.ok && !(options.acceptStatuses ?? []).includes(response.status)) {
    throw new ApiError(response.status, await readProblem(response));
  }

  const body: unknown = await response.json();
  return schema.parse(body);
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  try {
    const parsed = problemDetailsSchema.safeParse(await response.json());
    return parsed.success ? parsed.data : undefined;
  } catch {
    return undefined;
  }
}

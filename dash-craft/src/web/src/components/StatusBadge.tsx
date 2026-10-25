import type { ReactNode } from "react";

export type StatusTone = "success" | "warning" | "danger" | "neutral";

const toneClasses: Record<StatusTone, { badge: string; dot: string }> = {
  success: { badge: "bg-green-50 text-green-800 ring-green-600/20", dot: "bg-green-600" },
  warning: { badge: "bg-amber-50 text-amber-800 ring-amber-600/20", dot: "bg-amber-500" },
  danger: { badge: "bg-red-50 text-red-800 ring-red-600/20", dot: "bg-red-600" },
  neutral: { badge: "bg-slate-50 text-slate-700 ring-slate-500/20", dot: "bg-slate-400" },
};

/** A status pill. The text always carries the meaning; color only reinforces it. */
export function StatusBadge({ tone, children }: { tone: StatusTone; children: ReactNode }) {
  const classes = toneClasses[tone];
  return (
    <span
      className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-sm font-medium ring-1 ring-inset ${classes.badge}`}
    >
      <span aria-hidden="true" className={`size-2 rounded-full ${classes.dot}`} />
      {children}
    </span>
  );
}

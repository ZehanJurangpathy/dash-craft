import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { describe, expect, it, vi } from "vitest";
import { createTestQueryClient } from "../../../app/queryClient";
import { HomePage } from "./HomePage";

const unreachableMessage = "We can't reach the DashCraft API right now. Check that it is running.";

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

function renderHomePage(fetchImpl: () => Promise<Response>) {
  vi.stubGlobal("fetch", vi.fn(fetchImpl));
  render(
    <QueryClientProvider client={createTestQueryClient()}>
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("HomePage", () => {
  it("shows a healthy API", async () => {
    renderHomePage(() =>
      Promise.resolve(
        jsonResponse(200, {
          status: "Healthy",
          totalDurationMs: 12.4,
          checks: [{ name: "database", status: "Healthy", durationMs: 11.9 }],
        }),
      ),
    );

    expect(screen.getByRole("heading", { name: "DashCraft" })).toBeInTheDocument();
    expect(await screen.findByText("API: Healthy")).toBeInTheDocument();
  });

  it("shows which dependency is unhealthy when the API returns 503", async () => {
    renderHomePage(() =>
      Promise.resolve(
        jsonResponse(503, {
          status: "Unhealthy",
          totalDurationMs: 5003.1,
          checks: [
            {
              name: "database",
              status: "Unhealthy",
              description: "The application database is not reachable.",
              durationMs: 5002.8,
            },
          ],
        }),
      ),
    );

    expect(await screen.findByText("API: Unhealthy")).toBeInTheDocument();
    expect(screen.getByText("The application database is not reachable.")).toBeInTheDocument();
  });

  it("shows a friendly message when the API cannot be reached", async () => {
    renderHomePage(() => Promise.reject(new TypeError("Failed to fetch")));

    expect(await screen.findByText(unreachableMessage)).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "DashCraft" })).toBeInTheDocument();
  });

  it("treats a malformed response as unreachable", async () => {
    renderHomePage(() => Promise.resolve(jsonResponse(200, { foo: 1 })));

    expect(await screen.findByText(unreachableMessage)).toBeInTheDocument();
  });
});

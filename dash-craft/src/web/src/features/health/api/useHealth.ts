import { useQuery } from "@tanstack/react-query";
import { getHealth } from "./getHealth";

export function useHealth() {
  return useQuery({
    queryKey: ["health"],
    queryFn: ({ signal }) => getHealth(signal),
    refetchInterval: 10_000,
  });
}

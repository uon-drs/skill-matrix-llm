"use client";

import { useEffect, useState } from "react";

/**
 * Returns responsive viewport state, updated on window resize.
 * @returns `{ isMobile }` — `true` when viewport width is ≤899px
 */
export function useViewport() {
  const [isMobile, setIsMobile] = useState(() =>
    typeof window !== "undefined"
      ? window.matchMedia("(max-width: 899px)").matches
      : false,
  );

  useEffect(() => {
    const mq = window.matchMedia("(max-width: 899px)");
    const update = () => setIsMobile(mq.matches);
    mq.addEventListener("change", update);
    return () => mq.removeEventListener("change", update);
  }, []);

  return { isMobile };
}

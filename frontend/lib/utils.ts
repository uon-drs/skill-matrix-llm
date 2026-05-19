/**
 * Merges class names, filtering out falsy values.
 * @param classes - Class name strings or falsy values to filter out
 * @returns A single space-separated class string
 */
export function cn(...classes: (string | undefined | null | false)[]): string {
  return classes.filter(Boolean).join(" ");
}

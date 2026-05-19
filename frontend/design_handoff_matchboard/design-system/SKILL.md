---
name: drs-design
description: Use this skill to generate well-branded interfaces and assets for Digital Research Service (DRS), either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping the cross-disciplinary research-matching product Matchboard.
user-invocable: true
---

Read the `README.md` file within this skill, and explore the other available files.

If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view. If working on production code, you can copy assets and read the rules here to become an expert in designing with this brand.

If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.

## Quick map

- **`README.md`** — full brand reference: content fundamentals, visual foundations, iconography.
- **`colors_and_type.css`** — design tokens (colors, type scale, spacing, radii, shadows, motion). Import this from any HTML artifact.
- **`assets/`** — logos and wordmarks (SVG): `drs-mark.svg`, `drs-wordmark.svg`, `matchboard-wordmark.svg`.
- **`ui_kits/matchboard/`** — interactive UI kit for the Matchboard web app. Read `ui_kits/matchboard/README.md` for the component map. Components are React (Babel-in-browser) and cleanly factored — copy whole files when prototyping.
- **`preview/`** — small per-token preview cards used in the Design System tab. Reference for "what does X look like in isolation".

## Top rules — do not violate

1. **Use the University of Nottingham brand palette.** Nottingham Blue `#10263B` is the primary; it must be visually dominant on every asset. Max **two** supporting accents per design (Matchboard's chosen pair: Forest Green `#005F36` + Rebel's Gold `#DEB406`).
2. **Black is not part of the palette.** All text is Nottingham Blue or one of its tints; never pure black.
3. **Background is never pure white.** Use Portland Stone 40% `#FDFBF9` (preferred) or Nottingham Blue 5% `#F3F4F5`.
4. **No emoji in product UI.** Use Heroicons (outline, 1.5px stroke).
5. **Sentence case everywhere** — buttons, headings, menus. Exceptions: proper nouns and the single ALL-CAPS eyebrow with `0.14em` tracking.
6. **Geist + Geist Mono** only. No other typefaces. Variation comes from weight, size, tracking.
7. **Borders, not shadows.** Cards get a 1px Nottingham-Blue-alpha hairline border, not drop shadows. Only popovers and modals use shadow.
8. **Restrained motion.** 120ms `cubic-bezier(0.2,0,0,1)`. No bounces, no slide-ins, no scale-ups.
9. **No filler content.** Don't pad designs with stock copy or invented sections.
10. **No AI-generated imagery.** Use real editorial photography or labelled placeholders.
11. **UoN logo lock-up first** in every product surface — institution leads, then the product wordmark.

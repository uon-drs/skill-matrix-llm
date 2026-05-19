# Matchboard — UI Kit

**Matchboard** is the Digital Research Service's skills-matching web app. Researchers maintain a profile of expertise; project leads post project descriptions; the system surfaces high-signal matches across departments and disciplines.

This UI kit is a high-fidelity React (Babel-in-the-browser) recreation. It's not production code — components are mostly cosmetic — but every visual decision lines up with `/colors_and_type.css` and the brand foundations in `/README.md`.

## Files

| File | Contents |
|---|---|
| `index.html` | Entry point — loads React, Babel, fonts, all JSX modules. The default route is the Discover screen with a working top-bar + left-rail navigation. |
| `data.jsx` | Sample researchers, projects, disciplines. All data is mock. |
| `components.jsx` | Primitives — `Button`, `Tag`, `Chip`, `Avatar`, `MatchMeter`, `Icon`, `StatusDot`, `Eyebrow`, `Hairline`. |
| `layout.jsx` | `TopBar`, `LeftRail`, `PageHeader`, `EmptyState`. |
| `cards.jsx` | `ProjectCard`, `ResearcherCard`, `MatchRow`. |
| `screens.jsx` | The five core screens — `DiscoverScreen`, `ProjectDetailScreen`, `ResearcherScreen`, `PostProjectScreen`, `MatchesScreen`. |
| `app.jsx` | Top-level state — current route, current project, current researcher. Renders the chrome + active screen. |

## Screens covered

1. **Discover** — Personalised feed of recommended projects, ranked by match strength against the signed-in researcher's profile.
2. **Project detail** — Full project description, requirements, matched-candidate ranking, request-to-join CTA.
3. **Researcher profile** — A researcher's expertise, publications, open collaborations, h-index, contact actions.
4. **Post project** — Composer for new projects: title, discipline, description, required skills, expected duration.
5. **Matches** — Two-tab inbox of incoming requests + your own outgoing requests, with confirm / decline / withdraw actions.

## Click-through behaviour

- Top-bar logo returns to Discover.
- Left-rail items switch primary screen.
- Project cards open the project detail screen.
- Researcher names open the researcher profile.
- "Post project" button opens the composer; submit returns to Discover with a toast.
- Match-row buttons mutate local state to demonstrate accept/decline.

No persistence, no backend.

## Component coverage

Buttons (primary / secondary / ghost / destructive · two sizes), text inputs, search input, textarea, tags (skill + discipline), filter chips, capsule status pills, avatars (sm / md / lg + stacked), match meters, dropdown menus (sort + context), researcher cards, project cards, page headers with eyebrow + title + meta, empty states, top bar (with search + actions), left rail (nav with active state), modal scrim, toast notifications.

## What's *not* in here (and why)

- **Authentication / settings screens** — not specific to skills-matching; common patterns.
- **Admin / staff dashboard** — separate product surface; not requested.
- **Real photography** — none was provided. Researcher avatars use gradient placeholders with initials.

# Frontend Functionality Gaps

| Priority | Gap | Notes |
|---|---|---|
| High | Dashboard is a stub | `app/dashboard/page.tsx` renders "coming soon" text only |
| High | Team confirm/reject buttons missing from project detail | Backend has `PUT /api/projects/{id}/teams/{teamId}/confirm` and `.../reject`; project detail page shows teams read-only |
| High | LLM `rawResponse` never displayed | `RecommendationRecord.rawResponse` is fetched but only a count is shown on the project detail page |
| High | No edit/delete/status-transition for projects | Backend has `PUT /api/projects/{id}`, `PUT /api/projects/{id}/status`, `DELETE /api/projects/{id}`; no frontend surface for any of them |
| High | TopBar avatar click broken | Fires `onNavigate('profile-me')` but `AppShell` has no destination mapped for that route id — clicking your own avatar goes nowhere |
| Medium | Skill add has no autocomplete | `GET /api/skills?search=` is wired in the API client but never called from `_SkillsSection.tsx` |
| Medium | Discover/Saved/Network nav items go nowhere | `LeftRail` shows 5 nav items; `AppShell` only maps `/projects` and `/invitations` |
| Medium | TopBar search does nothing | Input has state but no submit handler or API call |
| Medium | No way to self-request joining a team | Backend has `POST /api/users/me/membership-requests/{teamId}`; no frontend UI |
| Medium | Invitation badge undercounts | `ProfileLayout` counts only `Invited` memberships for the badge, but the invitations page also shows `Requested` ones |
| Low | Notification bell does nothing | Icon renders in TopBar with no `onClick` handler; no notifications API exists |
| Low | Disciplines sidebar list is decorative | Hardcoded strings in `LeftRail` with no click handler or filtering behaviour |
| Low | `ProfileCard` / `MatchRow` components unused | Likely built for Discover/Network pages that don't exist yet |
| Low | No admin UI for skills catalogue | Backend has `POST/PUT/DELETE /api/skills`; no frontend page |
| Low | No user directory page | `GET /api/users` exists on the backend; profiles are only reachable via direct URL |

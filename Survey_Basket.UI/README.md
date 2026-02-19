# Survey Basket UI

Frontend application for Survey Basket, built with Angular 21.

## What Is Included

- Role-aware navigation shell with responsive layout.
- Light/dark theme system (system default on first visit + persisted user choice).
- Redesigned authentication pages (login/register) with improved CTA visibility and responsive behavior.
- Admin company-account provisioning screen.
- Company account activation screen (first-time password setup).
- Company-scoped user-record management screen (non-authenticated records).
- Roles management screen and full route wiring under authenticated shell.

## Recent Fixes

- Fixed login/register visual issues and restored clear visible action buttons.
- Added stronger empty/error state components in company and results workflows.
- Added route guard activation redirect for accounts that require first-time activation.
- Aligned frontend role/permission flows with backend company provisioning and activation endpoints.

## Run Locally

From `Survey_Basket.UI`:

```bash
npm install
npm run start
```

Open `http://localhost:4200/`.

## Build

```bash
npm run build
```

Build output is in `Survey_Basket.UI/dist/Survey_Basket.UI`.

## Test

```bash
npm run test
```

## Backend Integration Notes

- The UI expects the API base URL configured in `src/app/core/constants/api.constants.ts`.
- New key backend flows used by UI:
  - Company account creation (admin only)
  - Company account activation (one-time token/password setup)
  - Company user record creation (company scope, non-authenticated)

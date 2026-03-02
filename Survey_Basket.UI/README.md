# Survey Basket UI

Angular 21 frontend for the Survey Basket platform.

It is designed around business roles (Admin, Partner Company, Company User/Member) and focuses on secure survey distribution with company-level targeting.

## Business Experiences Covered

- Admin dashboard for poll lifecycle and company operations.
- Company-first-login flow with forced password setup.
- Poll creation/editing with target-company selection.
- Company poll QR/link generation for participant distribution.
- Participant poll join flow with required email/mobile before answering.
- Vote screens that prevent duplicate submission and show previously submitted answers.
- Analytics screens (row data, votes per day, votes per question).

## Main UI Areas

- `src/app/pages/auth`: login, company activation, join-by-link pages.
- `src/app/pages/polls`: create/edit/vote/results flows.
- `src/app/pages/admin`: company accounts, company users, roles, users.
- `src/app/core/services`: API client services and auth/session orchestration.
- `src/app/core/guards`: route access and first-login enforcement.

## Run Locally

From `Survey_Basket.UI`:

```bash
npm install
npm start
```

Open `http://localhost:4200`.

## Build

```bash
npm run build
```

Output: `Survey_Basket.UI/dist/Survey_Basket.UI`.

## Test

```bash
npm run test
```

## API Integration

- API base URL is configured in `src/app/core/constants/api.constants.ts`.
- Expected backend capabilities used by this UI:
  - company account provisioning + first-login setup
  - poll targeting by selected companies
  - poll QR access link generation and redemption
  - vote submission and participant history retrieval
  - analytics endpoints

## UX Notes

- Route guards redirect users to required setup pages (for example forced password setup).
- Poll visibility and actions are role-aware to match backend permissions.
- Error states surface backend business messages when available, to reduce ambiguous failures.

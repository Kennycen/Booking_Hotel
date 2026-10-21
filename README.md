# Booking Hotel

A full-stack hotel booking app. Guests browse and book rooms; hotel owners register a hotel, add rooms, toggle availability and see bookings and revenue on a dashboard.

## Tech stack

| Layer    | Technology                                                                 |
| -------- | -------------------------------------------------------------------------- |
| Frontend | React 19, TypeScript, Vite, Tailwind CSS v4, React Router v7, Axios        |
| Auth     | [Clerk](https://clerk.com) (JWT on the API, webhooks for user sync via Svix) |
| Backend  | ASP.NET Core (.NET 10) Web API with controllers                            |
| Database | PostgreSQL via Entity Framework Core (Npgsql)                              |
| Media    | Cloudinary (room images)                                                   |
| Email    | SMTP via MailKit (Brevo relay by default) for booking confirmations        |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) 20+ and npm
- PostgreSQL 14+
- Accounts for Clerk, Cloudinary and an SMTP provider (e.g. Brevo)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Backend

Secrets are kept out of `appsettings.json` and stored with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd Backend

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=booking_hotel;Username=postgres;Password=postgres"
dotnet user-secrets set "Clerk:Authority" "https://<your-instance>.clerk.accounts.dev"
dotnet user-secrets set "Clerk:WebhookSecret" "whsec_..."
dotnet user-secrets set "Cloudinary:CloudName" "..."
dotnet user-secrets set "Cloudinary:ApiKey" "..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."
dotnet user-secrets set "Smtp:User" "..."
dotnet user-secrets set "Smtp:Password" "..."
dotnet user-secrets set "Smtp:From" "you@example.com"

dotnet ef database update
dotnet run
```

The API runs at `http://localhost:5080` (`https://localhost:7014` with the `https` profile). In development the OpenAPI document is served at `/openapi/v1.json`.

### 2. Frontend

```bash
cd Frontend
cp .env.example .env   # then fill in your Clerk publishable key
npm install
npm run dev
```

The app runs at `http://localhost:5173`, which is the origin allowed by the backend CORS policy.

| Variable                     | Description                          |
| ---------------------------- | ------------------------------------ |
| `VITE_CLERK_PUBLISHABLE_KEY` | Clerk publishable key (required)     |
| `VITE_BACKEND_URL`           | API base URL, e.g. `http://localhost:5080` |
| `VITE_CURRENCY`              | Currency symbol shown in the UI (default `$`) |

### 3. Clerk webhook

In the Clerk dashboard, add a webhook pointing to `POST {API_URL}/api/clerk` subscribed to `user.created`, `user.updated` and `user.deleted`. For local development expose the API with a tunnel such as ngrok. Copy the signing secret into `Clerk:WebhookSecret`.

## API overview

Endpoints marked "auth" require a Clerk session token in `Authorization: Bearer <token>`.

| Method | Route                              | Auth | Description                            |
| ------ | ---------------------------------- | ---- | -------------------------------------- |
| GET    | `/api/user`                        | auth | Current user's role and recent searches |
| POST   | `/api/user/store-recent-search`    | auth | Save a recently searched city          |
| POST   | `/api/hotels`                      | auth | Register a hotel (makes user an owner) |
| GET    | `/api/rooms`                       |      | List available rooms                   |
| GET    | `/api/rooms/owner`                 | auth | Rooms of the owner's hotel             |
| POST   | `/api/rooms`                       | auth | Create a room (multipart, images)      |
| POST   | `/api/rooms/toggle-availability`   | auth | Toggle a room's availability           |
| POST   | `/api/bookings/check-availability` |      | Check a room for given dates           |
| POST   | `/api/bookings/book`               | auth | Create a booking and email confirmation |
| GET    | `/api/bookings/user`               | auth | Current user's bookings                |
| GET    | `/api/bookings/hotel`              | auth | Owner dashboard: bookings and revenue  |
| POST   | `/api/clerk`                       | Svix signature | Clerk user sync webhook       |

## Scripts

| Location  | Command                                  | Purpose                    |
| --------- | ---------------------------------------- | -------------------------- |
| Frontend  | `npm run dev`                            | Start Vite dev server      |
| Frontend  | `npm run build`                          | Type-check and build       |
| Frontend  | `npm run lint`                           | Run ESLint                 |
| Backend   | `dotnet run`                             | Start the API              |
| Backend   | `dotnet ef migrations add <Name>`        | Create a migration         |
| Backend   | `dotnet ef database update`              | Apply migrations           |

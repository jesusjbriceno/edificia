# Flux Gateway Integration — EDIFICIA (n8n)

This document summarizes the necessary configuration to integrate EDIFICIA's n8n Flux workflow with the Flux Gateway API (see `flux-openapi.json`). It complements `GUIA_WORKFLOWS_N8N.md`.

## Endpoints used by the workflow

- POST `/api/v1/auth/app` — Exchange `clientId` + `clientSecret` for a JWT token. The n8n workflow uses this endpoint to obtain the Bearer token.
- POST `/api/v1/chat` — Send a chat request with `{ messages }` or `{ message }` and receive `{ content, usage, model_used, provider }`.

## n8n credentials / environment variables

Set the following variables in your n8n environment (Docker Compose, .env, or n8n UI variables):

- `EDIFICIA_API_SECRET`: Shared secret validated by the workflow (X-Edificia-Auth).
- `FLUX_CLIENT_ID`: Client ID for the Flux application (use n8n credentials or env var).
- `FLUX_CLIENT_SECRET`: Client Secret for the Flux application.
- `FLUX_MODEL` (optional): Default model to request (e.g., `flux-pro`).

We added placeholders in the repository `.env.example` for convenience:

```
FLUX_CLIENT_ID=CHANGE_ME_flux_client_id
FLUX_CLIENT_SECRET=CHANGE_ME_flux_client_secret
```

## Request/Response contract notes

- The workflow expects the Flux `/api/v1/chat` response to include either a top-level `content` (string) or an OpenAI-like `choices` array. The workflow normalizes both into the `{ generatedText, usage }` contract consumed by the backend.
- Tokens can be read from `response.usage.total_tokens` or `response.usage.tokens` depending on provider implementation.

## Troubleshooting

- 401/403 on login: verify `FLUX_CLIENT_ID` and `FLUX_CLIENT_SECRET` are correct and the application is active in Flux.
- No `content` in response: check `response` payload in n8n Executions and ensure `messages` or `choices` are present.

## Reference

- API spec: `docs/features/ia_delegated/flux-openapi.json`
- Workflow: `apps/n8n/workflow-flux.json`

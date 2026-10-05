# Claims Platform Frontend

The standalone Angular application contains claimant, claims-officer, and
manager feature areas. It uses signals for session and local UI state, typed
HTTP services, functional interceptors and guards, reactive forms, selective
Angular Material controls, and custom SCSS.

See the repository-level `README.md` for complete startup and verification
instructions.

From this directory:

```bash
npm install
npm start
```

The development server uses `proxy.conf.json` to forward `/api` requests to the
ASP.NET Core API at `http://localhost:5234`.

Run unit tests with `npm test` and create a production bundle with
`npm run build`.

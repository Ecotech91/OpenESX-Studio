# Security policy

## Supported version

Security fixes currently target the newest public beta only.

## Reporting a security issue

Please do not publish a security-sensitive report as a normal public issue. Use GitHub's private vulnerability reporting feature when it is enabled for this repository. If that feature is unavailable, open a short issue that asks the maintainer for a private contact method without including exploit details or files.

Never attach private ESX banks, copyrighted samples, credentials, or personal paths to a public report.

## Local security model

OpenESX Studio processes ESX and WAV data locally. The Windows application exposes a temporary loopback-only bridge to its own browser window and protects card operations with a random per-session token. The bridge stops when the application window closes.


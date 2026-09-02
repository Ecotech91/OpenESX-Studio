# Privacy and file safety

OpenESX Studio is designed for local operation.

- ESX and WAV content is processed in local memory.
- No analytics, account system, cloud storage, or upload endpoint is included.
- The original loaded bytes are retained separately from the editable working copy.
- Backup and bit-exact-copy actions always use the original bytes.
- A card write first creates and verifies a temporary complete copy before replacing an existing target after confirmation.
- The native card bridge listens only on Windows loopback and requires a random token injected into the application session.

Users remain responsible for backups and for the rights to audio stored in their files. Hardware compatibility can vary, so every first test should use a separately named copy.


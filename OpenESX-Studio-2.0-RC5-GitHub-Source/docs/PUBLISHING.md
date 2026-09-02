# Publishing checklist

This file is for the repository owner preparing the first GitHub release.

## Repository

1. Create a public GitHub repository named `OpenESX-Studio`.
2. Do not let GitHub generate another README, license, or `.gitignore`; these files are already included.
3. Upload or push the repository contents without the local `release-assets` directory.
4. Keep GitHub Issues enabled so testers can use the supplied forms.
5. Enable private vulnerability reporting under the repository security settings if available.
6. Confirm that the Windows build workflow passes.

## First release

1. Open **Releases** and choose **Draft a new release**.
2. Create the tag `v2.0.0-rc5` from the `main` branch.
3. Use the title `OpenESX Studio 2.0 RC5 — Public Beta`.
4. Mark the release as a **pre-release**.
5. Paste the contents of `release/RELEASE-NOTES-2.0-RC5.md` into the release description.
6. Upload all five files from the local `release-assets` directory.
7. Verify the uploaded names and SHA-256 list before publishing.

GitHub releases are the intended location for generated EXE and ZIP files. The source repository itself should not commit generated binaries, private ESX banks, or WAV samples.


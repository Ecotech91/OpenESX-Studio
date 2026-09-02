# OpenESX Studio

OpenESX Studio is a local Windows editor for Korg Electribe SX (`.esx`) files. It can inspect and edit samples, patterns, parts, effects, motion sequences, songs, and global/MIDI settings without uploading files to a server.

> **Public beta:** The Windows card workflow has been tested for detecting removable media and saving, reopening, and resaving ESX banks. Loading a bank created by OpenESX Studio on a physical **Korg ESX-SD** still needs independent community confirmation. Always keep the original file and test with a copy.

Die Programmoberfläche und die ausführliche Anleitung sind auf Deutsch. Fehlerberichte auf Deutsch oder Englisch sind willkommen.

## Download

For normal use, download the installer or portable EXE from the repository's **Releases** section. Do not download individual source files unless you want to build the application yourself.

- `OpenESX-Studio-2.0-RC5-Setup.exe` — installer for the current Windows user
- `OpenESX-Studio-2.0-RC5-Portable.exe` — portable application
- `OpenESX-Studio-2.0-RC5-Windows-x64.zip` — both variants plus the user README
- `SHA256-OpenESX-Studio-2.0-RC5-Windows.txt` — integrity hashes

The binaries are not digitally signed. Windows SmartScreen or Smart App Control may therefore classify them as an unknown application.

## Main features

- Read and verify Korg ESX-1 files locally
- Keep the loaded original immutable and collect edits in a separate working copy
- Display file size, SHA-256, sample slots, memory usage, patterns, and songs
- Import common uncompressed WAV formats and convert them to 16-bit mono PCM
- Preview and export samples
- Edit sample metadata and safely remove or replace samples
- Edit all 256 patterns, eight bars, 128 steps, parts, effects, and motion lanes
- Preview patterns and switch the 14 sample-based parts on or off while listening
- Edit songs and global/MIDI settings
- Detect removable media, list ESX banks, and safely save or reopen a bank
- Create backups and bit-exact copies

## Important ESX limits

A larger SD card does not increase the sample memory of one ESX bank. One bank remains limited to 384 sample slots (256 mono and 128 stereo) and approximately 24 MB of sample data. The card manager uses additional card space for multiple separate ESX banks.

Korg documents SD cards up to 2 GB, SDHC cards up to 32 GB, and a maximum of 256 files for the ESX-SD. Cards should preferably be formatted on the instrument.

## Safety

- Never use your only copy of an ESX file for an initial test.
- Keep an external backup before writing to removable media.
- Do not publicly attach ESX or WAV files unless you own the rights to all included audio.
- A saved bank is structurally checked, but exact sound and hardware behavior must be verified on the target instrument.
- The physical ESX-SD compatibility test is still part of this public beta.

## Build and test

See [docs/BUILD.md](docs/BUILD.md). The repository includes a synthetic smoke test; no private ESX or copyrighted sample data is included.

## Community feedback

Please use the supplied GitHub issue forms. ESX-SD testers should also follow [docs/ESX-SD-COMMUNITY-TEST.md](docs/ESX-SD-COMMUNITY-TEST.md).

Contributions are welcome; read [CONTRIBUTING.md](CONTRIBUTING.md) first.

## Independence and trademarks

OpenESX Studio is an independent community project. It is not affiliated with, sponsored by, or endorsed by Korg Inc. Korg, Electribe, and related product names may be trademarks of their respective owners and are used only to identify compatibility.

## License

OpenESX Studio is released under the [MIT License](LICENSE). Format references and their attribution are listed in [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).


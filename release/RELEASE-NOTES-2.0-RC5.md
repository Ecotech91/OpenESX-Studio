# OpenESX Studio 2.0 RC5 — Public Beta

This is the first release candidate with the Windows card and ESX-bank manager.

## Confirmed

- Removable-media detection under Windows
- Capacity, free-space, file-system, and ESX-bank listing
- Saving a complete working copy as a new ESX bank
- Reopening the saved bank from the card
- Saving the reopened bank again under another name
- Existing sample, pattern, preview, song, and global/MIDI test suite
- Installer and portable executable self-tests

## Community test requested

The project owner does not have an ESX-SD model. Loading an OpenESX-written bank on physical Korg ESX-SD hardware is therefore not yet owner-confirmed. Please follow `docs/ESX-SD-COMMUNITY-TEST.md` and report the result through GitHub Issues.

## Known limitations

- The binaries are unsigned and may trigger Windows SmartScreen or Smart App Control.
- A larger card stores more separate banks but does not expand one bank's sample RAM or slot count.
- Import currently creates mono samples; existing stereo samples can be previewed, exported, edited, and removed.
- Compressed WAV codecs are not decoded.
- Pattern preview is not an exact emulation of Korg's analog tubes or effects engine.
- Direct MIDI or USB communication with the instrument is not included.

## Recommended download

Most users should download `OpenESX-Studio-2.0-RC5-Setup.exe`. A portable EXE and a combined ZIP are supplied as alternatives. Compare downloads with the included SHA-256 file.


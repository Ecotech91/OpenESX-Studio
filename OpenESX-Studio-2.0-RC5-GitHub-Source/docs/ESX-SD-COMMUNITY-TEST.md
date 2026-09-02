# ESX-SD community test

The Windows card workflow is confirmed. Loading a bank written by OpenESX Studio on a physical Korg ESX-SD is still a public beta test.

## Before testing

- Keep the original ESX bank in at least one separate location.
- Prefer a spare card formatted directly on the Korg.
- Do not overwrite the only working bank.
- Use a new filename such as `OPENESX1.ESX`.
- Start with a small, non-critical change that can be recognized easily.

## Test procedure

1. Open a known working ESX bank in OpenESX Studio.
2. Create a backup from the application.
3. Change one clearly identifiable sample name or one unused pattern.
4. Open **Karte & Bänke**, select the removable card, and save under a new name.
5. Reopen that bank from the card in OpenESX Studio and confirm that samples and patterns are still listed.
6. Eject the card safely in Windows.
7. Insert the card into the Korg ESX-SD and load only the newly named test bank.
8. Confirm whether the bank loads, the changed item is correct, existing samples play, and several unrelated patterns remain unchanged.

## What to report

- OpenESX Studio version
- Exact Korg model and system/firmware version, if known
- Card type, advertised capacity, Windows file system, and whether the Korg formatted it
- Original bank size and approximate sample-memory usage
- Exact edit performed
- Whether saving, reopening on Windows, and loading on the Korg succeeded
- Exact Korg or Windows error text

Do not upload a bank containing commercial or otherwise non-redistributable samples.

## Documented Korg limits

The official ESX-SD addendum documents SD up to 2 GB, SDHC up to 32 GB, and a maximum of 256 files:

https://cdn.korg.com/us/support/download/files/4acb6cc3eb0c793fd8db8f24c9ea79ab.pdf


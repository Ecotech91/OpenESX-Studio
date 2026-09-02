# Contributing

Thank you for helping improve OpenESX Studio. Reports in German or English are welcome.

## Before reporting a bug

1. Use the newest public beta.
2. Reproduce the problem with a copy, never with your only ESX file.
3. Record the exact steps and the complete error message.
4. Note the Windows version, application version, Korg model, and—when relevant—card capacity and file system.
5. Remove personal information from screenshots and logs.

Do not upload ESX or WAV files containing audio you are not allowed to redistribute. If a file is essential for debugging, first create a minimal test file containing only audio you own and wait until a maintainer explains a private transfer method.

## Pull requests

- Keep changes focused and explain their purpose.
- Preserve the immutable-original and separate-working-copy safety model.
- Add or update a test when changing file parsing or writing.
- Never add private ESX files, commercial samples, credentials, build folders, or generated EXEs to source commits.
- Run `node tests/CoreSmokeTest.js` and the Windows build self-test before requesting review.

By contributing, you agree that your contribution is provided under the repository's MIT License.


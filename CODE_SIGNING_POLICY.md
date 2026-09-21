# Code-signing policy

Only artifacts built from this public MIT-licensed repository are eligible for
the project's public trusted signature.

The Windows workflow checks out the triggering commit, restores dependencies,
runs the automated tests, builds both Windows architectures, creates the
installers, and records SHA-256 provenance. Private builds, local experiments,
unpublished source, and externally supplied binaries are outside this signing
boundary.

Private signing keys are never stored in the repository or GitHub Actions.
The project is applying to SignPath Foundation for public code signing.

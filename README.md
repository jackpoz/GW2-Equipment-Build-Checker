# GW2 Equipment Build Checker

GW2 related project to compare ingame equipment with GW2skills builds. It's based on Uno Platform and is available as Desktop app, WebAssembly static website (CORS proxy required) or command-line.

## GW2 API scopes
The required scopes are:
- characters
- builds

## Features
This tool compares:
- Class
- Specializations
- Traits
- Skills
- Revenant legends
- Equipment stats
- Equipment upgrades (runes, sigils)
- Weapon types
- Relic (only for active equipment template)

## Limitations
Below limitations due to GW2 API:
- Relics of non-active equipment templates cannot be compared
- Non-legendary gear with selectable stats cannot be compared

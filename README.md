# Arena Duels

A 1v1 peer-to-peer multiplayer first-person shooter built in Unity, featuring networked movement, hitscan and projectile weapons, and a full match loop (host/join → combat → rematch).

**[▶ Download on itch.io](https://obeahman4.itch.io/arena-duels)**

![Gameplay demo](docs/media/gameplay.gif)

## Overview

Arena Duels is a small-scale competitive FPS built as a learning project to explore real-time multiplayer networking in Unity. Two players connect over the internet (no dedicated server required) and duel in a circular arena, with the last player standing winning the round.

## Features

- **Peer-to-peer networking** via Unity Netcode for GameObjects + Unity Relay, allowing two players to connect over the internet without port forwarding or a dedicated server
- **Two weapon types** built on a shared weapon architecture:
  - Projectile-based weapon with physics-driven bullets
  - Hitscan weapon with instant raycast detection, tracer visuals, and full-auto fire
- **Networked health system** with synced health bars for both players, fighting-game style (mirrored fill direction, color-shifts from green to red)
- **Full match flow**: menu (host/join via Relay code) → gameplay → death → win/lose screen → rematch or leave room
- **Weapon switching** between multiple owned weapons, synced across the network so opponents see the correct weapon in-hand
- **Animated player characters** with networked animation state (movement, jumping, shooting)
- **First-person/third-person visual separation** — each player sees their own gun in first-person, while opponents see the full animated character model with weapon correctly attached to hand bones

## Tech Stack

- **Engine**: Unity
- **Networking**: Unity Netcode for GameObjects, Unity Relay, Unity Authentication (anonymous sign-in)
- **UI**: Unity UGUI / TextMeshPro
- **Character assets**: [Kenney](https://kenney.nl/) character and animation packs

## How to Play

1. Launch the game — one player clicks **Host**, generating a join code
2. Share the join code with the other player, who enters it and clicks **Join**
3. Once both players connect, the match begins
4. Eliminate your opponent (or avoid falling off the arena) to win
5. Use the **Rematch** button to play again, or **Leave Room** to return to the menu

**Controls**
- `WASD` — Move
- `Mouse` — Look
- `Space` — Jump
- `Left Click` — Fire (hold for automatic weapons)
- `1` / `2` — Switch weapons

## Architecture Notes

- **Owner-authoritative movement**: player position/rotation is driven by the owning client (not the server) for responsive local movement, with `NetworkTransform` configured for owner authority
- **Server-authoritative combat**: damage, hit detection, and health are validated and applied server-side to prevent client-side manipulation, even though movement is owner-authoritative
- **Shared weapon base class** (`WeaponBase`) handles fire-rate limiting, semi-auto/full-auto input, and networked fire feedback (sound, animation, muzzle flash), with `ProjectileWeapon` and `HitscanWeapon` implementing their own hit-detection logic
- **Layer-based collision filtering** to prevent self-collision between a player and their own projectiles/raycasts, separate from the tag-based hit detection used for damage

## Known Limitations

This is a learning project scoped to a 1v1 experience — a few things are intentionally out of scope for now:
- No dedicated matchmaking (join codes are shared manually)
- No reconnection handling if a player disconnects mid-match
- No client-side prediction/lag compensation beyond what Netcode provides by default

## Credits

- Character models and animations: [Kenney](https://kenney.nl/)
- Built with [Unity](https://unity.com/) and [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
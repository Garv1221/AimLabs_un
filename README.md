# AimLabs – Target Shooting Simulator

AimLabs is a first-person target shooting simulator built in Unity. The project contains a shooting range scene, a pistol, target spawning and scoring, and a lightweight on-screen training HUD.

## Project objectives

- Provide a simple first-person range for practicing aim.
- Track shots, hits, accuracy, score, and ammunition during a session.
- Keep the Unity project easy to open and extend.

## Features

- WASD first-person movement using a `CharacterController`.
- Mouse camera look and cursor capture, with Escape to release or recapture the cursor.
- Center-screen camera ray shooting; holding left click fires at the configured cooldown.
- Targets award points when hit and are replaced after a short delay.
- HUD reports target hits, shots fired, accuracy, score, and ammunition.
- Ten-round magazine, reserve ammunition, and R-key reload.
- URP renderer and project settings configured for the included range scene.

## Technologies

- Unity 6.6 (`6000.6.7f1`)
- C# and the classic `UnityEngine.Input` API
- Universal Render Pipeline (URP)
- Unity UI

## Requirements

Use Unity Editor **6000.6.7f1**. The project manifest includes the URP and UI packages; Unity Package Manager resolves them when the project opens.

## Project structure

```text
Assets/
  Documentation/       Project documentation
  Editor/              ProjectBootstrapper editor automation
  Materials/           Range and target materials
  Prefabs/             Target prefab
  Scenes/              ShootingRange scene
  Scripts/             Player, weapon, target, UI, and game management code
  Settings/            URP pipeline and renderer assets
Packages/              Package manifest and lock file
ProjectSettings/        Unity project, input, quality, and build settings
```

Unity `.meta` files are included with the assets so GUID-based references remain valid after cloning.

## Open and run

1. Install Unity Hub and Unity Editor **6000.6.7f1**.
2. Clone this repository, then in Unity Hub choose **Add project from disk** and select the repository folder.
3. Wait for package resolution, asset import, and C# compilation to finish.
4. Open `Assets/Scenes/ShootingRange.unity` and press **Play**.
5. Click in the Game view if needed to focus it. The training range begins when the scene starts.

The project includes `Assets/Editor/ProjectBootstrapper.cs` for editor setup and repair. Existing scene content is preserved by the bootstrapper.

## Controls

| Key | Action |
| --- | --- |
| WASD | Player movement |
| Mouse | Aim and camera rotation |
| Left Click (hold) | Shoot |
| R | Reload |
| Escape | Unlock or relock the cursor |

## Future improvements

- Add configurable training modes and difficulty settings.
- Add moving targets and additional range layouts.
- Add audio and visual feedback for firing and target hits.
- Add a settings screen and configurable controls.

These are potential additions; they are not represented as existing features.

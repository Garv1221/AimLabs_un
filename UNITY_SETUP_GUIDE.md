# Unity setup guide

## Open the project

1. Install Unity Hub and Unity Editor **6000.6.7f1**.
2. In Unity Hub, select **Add project from disk** and choose the repository directory.
3. Wait for the package manager to resolve the dependencies and for asset import and C# compilation to finish.
4. Open `Assets/Scenes/ShootingRange.unity` and enter Play mode.

The project uses the classic `UnityEngine.Input` API. If input is disabled in the local Editor configuration, open **Edit > Project Settings > Player > Other Settings > Configuration** and set **Active Input Handling** to **Input Manager (Old)** (or **Both**). Unity may request an Editor restart after changing this option.

`Assets/Editor/ProjectBootstrapper.cs` contains editor setup and repair commands. The existing scene is preserved by the normal bootstrap flow; inspect the scene before using any explicit repair or generation command.

## Controls

- WASD: move
- Mouse: aim and rotate the camera while the cursor is locked
- Hold left mouse button: fire
- R: reload
- Escape: release or recapture the cursor

## Manual smoke check

After Unity finishes importing, open the ShootingRange scene and enter Play mode. Check that movement, camera look, firing, target replacement, score and ammunition update as expected. This guide does not claim that a particular Unity Editor or build has been tested.

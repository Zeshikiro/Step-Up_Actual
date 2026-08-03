
### August 3, 2026
- **Anti-Cheat Finalized:** Completely disabled the Accelerometer step fallback to definitively kill the 'shaking' cheat. The app now relies 100% on the Android OS Hardware Step Counter (which uses ML to filter out hand shakes), sacrificing real-time step updates for perfect anti-cheat security.
- **Leaderboard Dummy Data:** Injected 10 dummy UserDataRecords directly into the UI list in-memory to bypass Firebase Security Rules (which were blocking phones from creating fake database entries), ensuring the UI is lively.
- **AR Fallback Background:** Disabled the custom 3D fallback object (which was slicing/engulfing the avatar) and replaced it with a guaranteed CameraClearFlags.SolidColor (Dark Navy) fallback.
- **AR Avatar Vanishing Fix:** Added absolute layer and scale resets in ARManager.cs to guarantee the avatar snaps to (0, -1.5, 5) relative to the camera, preventing rapid toggling from destroying its cached scale.
- **Android 13 Fix:** Background service now waits asynchronously for Notifications/Activity Recognition permissions before starting, eliminating the SecurityException startup crash on newer devices.

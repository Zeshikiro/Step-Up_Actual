# Step-Up Devlog

## Update: Transition to New Input System & Bug Fixes
**Date**: July 26, 2026

### 1. Migrated to New Input System
- Successfully transitioned the project to use Unity's **Input System Package (New)** to fix build errors related to ARCore and Input Handling mismatch.
- Refactored multiple scripts to ensure compatibility, replacing all legacy `Input.*` calls with `UnityEngine.InputSystem` equivalents.

### 2. Fixed Animation Bugs
- **AvatarAnimatorSync**:
  - Fixed an issue where the animator grabbed the first inactive skeleton (which caused the visible mesh to appear frozen). It now iterates through ALL active animators and forces state transitions, ensuring the active mesh animates correctly.
  - Fixed GPS sliding detection. The script was checking its own position (`avatarContainer`) instead of its parent (`MapAvatarTracker`), causing it to never register movement. It now traverses up the hierarchy to track the correct transform.
  - Added safety checks using `Animator.StringToHash` to prevent missing parameter exceptions from silencing the script.

### 3. Fixed Legacy Input Crashes
- **SceneLoader**: Upgraded from `Input.GetMouseButtonDown` and `Input.touchCount` to `Touchscreen.current` and `Mouse.current` so the "Tap to Continue" loading screen works properly on the New Input System.
- **MapAvatarTracker & CompassUI**: Wrapped legacy `Input.compass` calls in `try/catch` blocks. The legacy compass API would silently crash the `Update()` loop on the New Input System, preventing the avatar from rotating and the GPS from syncing.
- **ProminentDisclosure**: Wrapped `Input.location.Start()` and `Input.compass.enabled` in `try/catch` blocks to prevent early termination before initializing the `StepManager` pedometer sensors.
- **StepManager**: Migrated the debug `Input.GetKeyDown(KeyCode.Space)` to the New Input System to prevent silent crashes in `Update()`.
- **MapZoom**: Completely refactored to use the New Input System's `Touchscreen` API for pinch-to-zoom, replacing legacy `Input.GetTouch` calls.

### 4. ARCore Fallback Implementation
- **ARManager**: Implemented a `forceFallbackMode` bypass. Since the test phone lacks AR capabilities, ARCore initialization (`ARSession.CheckAvailability`) was triggering the "Download Google Play Services for AR (XR)" prompt. This bypass ignores ARCore entirely and forces the 3D map fallback background immediately, preventing the prompt.

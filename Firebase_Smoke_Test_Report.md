# Firebase Smoke Test Report

Project: `D:\UnityProject\Puzzle 2D` — Unity 6000.4.0f1
Test environment: Unity Editor (Windows, standalone/editor platform)
Method: cleared console → entered Play Mode → `FirebaseBootstrap` auto-spawned `FirebaseService`; state polled via `FirebaseService.Instance` and console log capture.

## 1. Firebase SDK
- Installed: Firebase Unity SDK **13.17.0** (Assets-based import under `Assets/Firebase/`). Runtime assemblies present: `Firebase.App`, `Firebase.Auth`, `Firebase.Analytics`, `Firebase.Database`, `Firebase.Firestore`, `Firebase.Storage`, `Firebase.Messaging`, `Firebase.Crashlytics`, `Firebase.RemoteConfig`, `Firebase.Installations`, `Firebase.AppCheck`. EDM4U resolver present (`Assets/ExternalDependencyManager/`). Android deps resolved into `mainTemplate.gradle` (`firebase-auth-unity:13.17.0`, `firebase-auth:24.2.0`, etc.) and `AndroidResolverDependencies.xml`.
- Initialization: **Verified.** `FirebaseBootstrap` (RuntimeInitializeOnLoadMethod.BeforeSceneLoad) spawned `FirebaseService`; `FirebaseApp.DefaultInstance` was created without exceptions.
- Dependencies: **Available.** `CheckAndFixDependenciesAsync()` completed past the dependency check (no "Dependencies unavailable" branch taken).
- Result: **Firebase SDK initializes successfully in the Editor.**

## 2. Firebase Authentication
- FirebaseAuth initialization: **Verified.** `FirebaseAuth.GetAuth(app)` succeeded; `FirebaseAuthService` constructed (`auth=True` at runtime).
- CurrentUser: **null** — no persisted session.
- Existing auth state: `FirebaseService.CurrentStatus = AuthFailed` (not `Ready`). Startup anonymous sign-in failed:
  `[FirebaseAuthService] Anonymous sign-in failed: This operation is restricted to administrators only.`
  (`AuthError.AdminRestrictedOperation` — the Anonymous provider is disabled in Firebase Console.) Service logged: `Anonymous auth failed — email login still available.`
- Result: **FirebaseAuth initializes and is functional, but no authenticated session could be established** (anonymous provider disabled; email/password path exists in `FirebaseAuthService` but was not exercised — credentials would need to be entered manually via `LoginUI`).

## 3. Google Sign-In
- Google Sign-In SDK/plugin: **NOT INSTALLED.** No `Google.SignIn` assembly, no google-signin package in `Packages/manifest.json`, no `play-services-auth` / `google-signin` dependency in `mainTemplate.gradle`.
- Platform tested: Unity Editor (no device build performed).
- Google provider configuration: **Not configured.** `google-services.json` (Android, package `com.game.firebasedb`, project `puzzle-2d-9e9a7`) and `google-services-desktop.json` both contain an **empty `oauth_client` array** — no OAuth/Web client ID exists for this app. No `GoogleService-Info.plist` exists (no iOS app registered). No `webClientId`/client-ID string exists anywhere in project code.
- Login attempted: **No — impossible to attempt.** There is no Google sign-in code path in the project: `FirebaseAuthService` implements email/password + anonymous only (no `GoogleAuthProvider`, no `SignInWithCredential`), and `LoginUI` exposes only Login / Register / Logout buttons — no Google button exists in code or wiring.
- Login result: Not attempted.
- Firebase credential: Not created.
- Firebase UID: null.
- Email: null.
- Result: **BLOCKED — Google Sign-In cannot be tested on any platform from this project.** It requires (a) a Google Sign-In plugin/SDK, (b) a Google credential code path, (c) a Web OAuth client ID, and (d) the Google provider enabled in Firebase Console — none of which exist. Native device testing (Android/iOS) is additionally required for the real account-chooser flow; the Editor has no Google sign-in support here.

## 4. Errors

- Exact error: `[FirebaseAuthService] Anonymous sign-in failed: This operation is restricted to administrators only.`
  - Layer: B / G — Firebase Auth configuration / Firebase Console provider configuration.
  - Cause: The Anonymous sign-in provider is disabled in Firebase Console for project `puzzle-2d-9e9a7`. (Firebase Console-side, not Unity-side — the Unity call is correct.)
  - Required fix: Firebase Console → Authentication → Sign-in method → enable **Anonymous**. Then `FirebaseService` will reach `Ready`.

- Exact error: `Database URL not set in the Firebase config.`
  - Layer: E — google-services.json / platform config.
  - Cause: Neither `google-services.json` nor `google-services-desktop.json` contains a `firebase_url`/Realtime Database URL. `FirebaseCloudSaveProvider` (Realtime DB) therefore has no database to point at.
  - Required fix: Firebase Console → Realtime Database → create a database, then re-download `google-services.json` (or set the URL manually). Only needed if cloud save is required — it does not block Auth.

- Google Sign-In absence (no runtime error — test cannot run):
  - Layer: C — no Google Sign-In plugin; D/H — no OAuth/Web client ID (`oauth_client` empty); J — no Google button/handler in `LoginUI`; G — Google provider cannot be verified as enabled (empty `oauth_client` strongly suggests it is not configured).
  - Required fix (minimum, in order): enable Google provider in Firebase Console → creates a Web OAuth client → re-download `google-services.json` (must contain a `client_type: 3` oauth_client = web client ID) → add a Google Sign-In SDK (e.g. google-signin-unity plugin) → add a `GoogleAuthProvider` credential path → add a Google button in `LoginUI`. On Android, SHA-1/SHA-256 must also be registered (see §5).

## 5. Firebase Console Requirements

The following must be done or verified in Firebase Console for project `puzzle-2d-9e9a7` — none of this can be confirmed as complete from project files:

- **Authentication → Sign-in providers → Google: needs to be enabled** (evidence: `oauth_client` array is empty — no OAuth clients exist for this app).
- **Authentication → Sign-in providers → Anonymous: needs to be enabled** (verified disabled at runtime — `AdminRestrictedOperation`).
- **Android app `com.game.firebasedb`: SHA-1 (and SHA-256) certificate fingerprint must be registered** — required for Google Sign-In on Android. Cannot be verified from the project; re-download `google-services.json` afterward and confirm `oauth_client` is populated.
- **iOS: no app registered** — `GoogleService-Info.plist` is absent. If iOS is a target, an iOS app must be registered in the console and the plist added to the project.
- **Realtime Database URL** missing from config — only if `FirebaseCloudSaveProvider` is expected to work.

## 6. Code Changes
- Files changed: **None.** (Plus this report file, `Firebase_Smoke_Test_Report.md`, which is documentation.)
- Files not changed: `FirebaseService.cs`, `FirebaseAuthService.cs`, `FirebaseBootstrap.cs`, `FirebaseCloudSaveProvider.cs`, `LoginUI.cs`, `SaveManager.cs`, `FirebaseSmokeTest.cs` — all read-only. No gameplay, UI, or config files were modified. No credentials were invented or added.

## 7. Final Status

**PARTIAL**

- Verified working: Firebase SDK 13.17.0 installs, dependency check passes, `FirebaseApp` initializes, `FirebaseAuth` initializes, the existing `FirebaseService`/`FirebaseAuthService`/`FirebaseBootstrap` init chain executes end-to-end in the Editor.
- Verified failing: session establishment — anonymous sign-in is rejected because the provider is disabled in Firebase Console (`AuthFailed`, not `Ready`).
- Blocked: Google Login — **not verifiable and not attempted**. No Google Sign-In plugin, no Google credential code path, no Google button in `LoginUI`, no OAuth/Web client ID in either config file, and no iOS config. An actual Google login test is impossible until §5 items are completed in Firebase Console and a Google Sign-In SDK + credential path + UI entry point exist. That work was intentionally not performed because this task is smoke-test only.

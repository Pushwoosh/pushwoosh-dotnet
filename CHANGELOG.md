# Changelog

All notable changes to the Pushwoosh .NET MAUI SDK are documented here.
The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## 2.0.0

Major. Both packages now share one version, and the minimum framework and platform floors move.
Read the breaking changes before upgrading — some of them are silent.

### Packages
- Pushwoosh.DotNet: 2.0.0
- Pushwoosh.DotNet.Geozones: 2.0.0 (was 1.0.2 — the two packages are versioned together from now on)

### Native SDKs
- Android SDK: 6.10.0 (was 6.7.14)
- iOS SDK: 6.7.18 (was 6.7.9)

### Breaking changes

**Target frameworks.** `net7.0`, `net7.0-android33.0` and `net7.0-ios10.0` are gone; the packages
ship `net8.0`, `net8.0-android34.0` and `net8.0-ios11.0`. A project on net7 gets NU1202 and cannot
restore 2.0.0 — stay on 1.0.6 or move the project to net8.

**Android platform floor 33 → 34, and this one is silent.** A project targeting
`net8.0-android33.0` still restores: NuGet falls back to the `lib/net8.0` folder, which holds only
the managed core assemblies — no bindings, no `.aar`, and none of the 19 Android dependencies. There
is no warning. The build succeeds, and push notifications simply never work at runtime. Raise
`TargetFramework` to `net8.0-android34.0`.

**iOS platform floor 10 → 11.**

**Native Android 6.7.14 → 6.10.0 removes API with no replacement.** Verified by unpacking both
`.aar` sets: `GDPRManager`, `PushwooshInApp` and `InlineInAppView` exist in 6.7.14 and are absent in
6.10.0. GDPR consent management and inline in-app banners are therefore gone — this is a loss of
capability, not an API move, and there is nothing to migrate to. If you call any of them, do not
upgrade until we have a replacement.

**AndroidX and Firebase floors were raised** (`Xamarin.AndroidX.Core` 1.13.1.2,
`Xamarin.AndroidX.WebKit` 1.10.0.2, `Xamarin.Firebase.Messaging` 123.3.1.1 and the rest — see the
package dependencies). A project pinning any of them lower gets NU1605 and has to move up.

**`Pushwoosh.DotNet.Geozones` now requires `Pushwoosh.DotNet`.** The dependency was missing, so
NuGet allowed Geozones to be installed on its own and the app died at native load time
(`pushwoosh-location.aar` needs `com.pushwoosh.internal.*`, and every `PushwooshGeozones` slice has
an `LC_LOAD_DYLIB` on `PushwooshFramework`). The dependency is a same-major range, so both packages
move together.

**`TagsBundle` getters now throw instead of failing quietly or obscurely.** The three of them
disagreed with each other on the same mistake: `GetInt` did `(value as int?).Value` and threw a bare
`NullReferenceException`, while `GetString` and `GetList` returned `null` and let the problem surface
somewhere unrelated. None of them said which key or what it actually held. All three now throw
`KeyNotFoundException` naming the missing key, or `InvalidCastException` naming the key, the type
stored and the type asked for. Code that relied on `GetString`/`GetList` returning `null` for a
type mismatch has to catch instead — reading a tag with the wrong getter was always a bug, it just
used to be silent.

**`Registered` and `RegisterError` now actually fire on Android.** They were declared but never
raised. Code that subscribed and saw nothing will start receiving events — including error paths it
may never have exercised. Note that `RegisterError` can arrive **synchronously, on the calling
thread**, if registration fails before it reaches the network; do not assume a handler runs later or
on the UI thread.

**Purchase reporting is removed entirely, and there is no replacement.** Four declarations are
gone:

- `SendPurchaseData(string identifier, decimal price, string currency)` — the abstract member on
  `Core/PushManager` and its overrides in both `iOS.Library` and `Android.Library`
- `SendPurchase(...)` in `iOS.Bindings` (`sendPurchase:withPrice:currencyCode:andDate:`)
- `SendSKPaymentTransactions(...)` in `iOS.Bindings` (`sendSKPaymentTransactions:`), which was a
  binding-only declaration nothing in this SDK ever called

Any code calling `PushManager.Instance.SendPurchaseData(...)`, or reaching into
`Com.Pushwoosh.iOS` for either binding, stops compiling. There is nothing to migrate to: the
default *In-app Product*, *In-app Purchase* and *Last In-app Purchase date* tags that these calls
used to fill are no longer written from this SDK.

The `Purchase` card is gone from the `Sample` app's Actions tab for the same reason.

### Fixed

- Android: rich media could not open — the AndroidX dependency floor needed by `pushwoosh-inbox-ui`
  was not declared.
- Android: the registration callbacks are raised from the native callback, and the error path no
  longer swallows the failure.
- iOS: `InboxManager.LoadMessages()` threw `NullReferenceException` for the whole batch when a
  message had no `action_params`, or no `sendDate`. Both properties are nullable in the native SDK
  and are now handled as such.
- Android: `InboxManager.LoadMessages()` had the same defect from the other direction —
  `DateTime.Parse(msg.ISO8601SendDate)` with no guard. `com.pushwoosh.inbox.data.InboxMessage`
  declares every getter as a plain Java type, so that string can be null (`ArgumentNullException`)
  or malformed (`FormatException`), and `getType()` can be null too. Neither was caught by the
  surrounding handler, which only looks for `InvalidCastException`, so one unparseable message cost
  the caller the entire list. Parsing is now `TryParse` into a nullable `SendDate`, a null type
  falls back to `Plain`, and each message is converted inside its own guard so a bad one is skipped
  instead of taking the batch down.
- Geozones on Android: the binding for
  `PushwooshLocation.startLocationTracking(Callback<Void, LocationNotAvailableException>)` was
  dropped during generation because the referenced types could not be resolved, so no .NET caller
  could learn that tracking failed to start. The binding project now resolves them and the overload
  is generated.
- `GeozonesSample`: the `AndroidX.Collection` floor was too low to build.
- iOS sample: added the entitlement that push registration needs.

### Notes

- **Native (template) in-app messages are not rendered on .NET.** The `pushwoosh-inapp-ui` module
  is not part of these packages, so a native in-app is received, recognised and then skipped — the
  native SDK logs `native in-app detected but pushwoosh-inapp-ui module is not connected, skipping`.
  HTML and rich-media in-apps work. This is not new in 2.0.0; it has never worked and was never
  documented. Use HTML or rich media until the module ships.
- Inbox attachments and images need Glide, and `Xamarin.Android.Glide` is now declared as a
  dependency. It was previously only present because `Microsoft.Maui.Core` pulls it in, so a
  non-MAUI Android consumer had no Glide and inbox images failed at runtime. MAUI apps get the same
  version they already resolved (4.15.1.2), so nothing changes for them.
- The samples ship the `XXXXX-XXXXX` placeholder application code, and each one carries a
  `google-services.json.example` where the real file belongs. Copy your own from the Firebase
  console into that folder — Android builds fail until you do. The samples also no longer declare
  `com.pushwoosh.senderid`: the native SDK does not read that metadata, and FCM takes the sender
  from `google-services.json`.
- **MAUI 8 does not render on iOS 26 — use an iOS 18.x simulator or device.** On an iOS 26.2
  simulator the app installs, launches, the process stays alive and the screen stays black, with no
  UI at all. Nothing in this SDK is involved: a bare `dotnet new maui` project containing no
  Pushwoosh code was measured on the same simulator and behaves identically, while the same build of
  `Sample/` runs correctly on iOS 18.6 (registration, HWID, APNs token, push delivery all verified
  there). MAUI 8.0 predates iOS 26 and there is no SDK-level workaround. Recent Xcode installs ship
  only 26.x simulators, so this is easy to hit and looks like a broken SDK when it is not.
- **How far the iOS side of this release was verified.** The package installs into a `net8.0-ios`
  project and the native `PushwooshFramework.xcframework` shipped inside
  `PushwooshSDK.DotNet.iOS.Bindings.resources.zip` is unpacked in the consumer's build — both
  checked, and both run in CI. Linking that framework into an application **is not covered by CI** —
  it needs the iOS 18 SDK (Xcode 16 or newer), the build machine runs Xcode 15.4, and the resulting
  `MT0180` is not bypassable by linker flags — so it was checked by hand instead: on Xcode 26.3 a
  Release build links the framework in for **both** slices, the simulator (`iossimulator-arm64`) and
  the device (`ios-arm64`, unsigned), each with `LC_LOAD_DYLIB` on
  `@rpath/PushwooshFramework.framework` and `@executable_path/Frameworks` on the app's rpath.
  Linking alone is not behaviour, so the runtime path was checked as well: on an iOS 18.6 simulator
  the SDK initialises, issues a hardware id, `Register()` returns a real APNS token, the device
  reports `Registered` with remote notifications `available`, and a delivered push is presented.
  Push delivery has been confirmed. The Android side of the package was verified by installation
  into a bare `net8.0-android` project and a MAUI project, plus a Release build of the installed
  package.

---

## 1.0.6

### Packages
- Pushwoosh.DotNet: 1.0.6
- Pushwoosh.DotNet.Geozones: 1.0.2

### Native SDKs
- Android SDK: 6.7.14
- iOS SDK: 6.7.9

---

# Android emulator fix (Windows)

Windows-এ অ্যাপ চলে। Android emulator না খোলার কারণ:

`Android Emulator hypervisor driver is not installed` + `Hyper-V is disabled`

## Option A (recommended) — Admin script

1. Right-click `MauiApps\Enable-AndroidEmulator.bat` → **Run as administrator**
2. PC **Restart** করুন
3. Visual Studio → Android Device Manager → `Pixel 7 - API 34` Start  
   (Graphics ইতিমধ্যে `swiftshader_indirect`, RAM `3072`)
4. Toolbar-এ target: **`net10.0-android`** → Pixel 7 emulator → Run `MgcApp`

## Option B — Visual Studio UI

1. Tools → Android → Android SDK Manager  
2. Tools tab → install **Android Emulator Hypervisor Driver** (যদি থাকে)
3. Windows Features-এ চালু করুন:
   - Windows Hypervisor Platform
   - Virtual Machine Platform
4. Restart

## Option C — Real phone (সবচেয়ে সহজ)

1. ফোনে Developer options → USB debugging ON
2. USB দিয়ে PC-তে কানেক্ট
3. VS-এ device সিলেক্ট করে Run

## App-side Android fixes already applied

- WebView: JavaScript, DOM storage, mixed content
- Manifest: INTERNET + hardwareAccelerated
- AVD: GPU `swiftshader_indirect`, RAM 3072 MB

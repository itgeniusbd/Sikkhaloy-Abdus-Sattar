# Institution WebView Apps (MAUI)

প্রতিষ্ঠানভিত্তিক .NET MAUI WebView অ্যাপ। প্রতিটি অ্যাপ `SIKKHALOY V2/Home/` এর সংশ্লিষ্ট HTML পেজ লোড করে।

## Apps

| Project | Title | Start URL |
|---------|-------|-----------|
| `DascApp` | DASC | https://sikkhaloy.com/Home/DascApp.html |
| `MgcApp` | MGC | https://sikkhaloy.com/Home/mgcApp.html |
| `NooraniApp` | Noorani | https://sikkhaloy.com/Home/NooraniApp.html |
| `KhadizatulKubraApp` | Khadizatul Kubra | https://sikkhaloy.com/Home/KhadizatulKubraApp.html |
| `DarulHudaApp` | Darul Huda | https://sikkhaloy.com/Home/DarulHudaApp.html |
| `GmisApp` | G.M. International School | https://sikkhaloy.com/Home/GmisApp.html |

URL পরিবর্তন করতে সংশ্লিষ্ট প্রজেক্টের `InstitutionConfig.cs` এডিট করুন।

## Open in Visual Studio

1. `MauiApps/InstitutionWebViewApps.sln` খুলুন
2. Startup Project সিলেক্ট করুন
3. Android device / Windows Machine এ Run করুন

## Release APK

```powershell
dotnet publish .\KhadizatulKubraApp\KhadizatulKubraApp.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
```

APK path: `bin\Release\net10.0-android\*-Signed.apk`

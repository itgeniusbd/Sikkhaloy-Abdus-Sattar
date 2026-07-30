// Run: dotnet script GenerateIdentity2PasswordHash.csx
// Or compile with Microsoft.AspNet.Identity.Core NuGet

#r "nuget: Microsoft.AspNet.Identity.Core, 2.2.4"

using Microsoft.AspNet.Identity;

var hasher = new PasswordHasher();
var password = args.Length > 0 ? args[0] : "Test@123";
var hash = hasher.HashPassword(password);
Console.WriteLine("Password: " + password);
Console.WriteLine("PasswordHash: " + hash);

var bytes = Convert.FromBase64String(hash);
Console.WriteLine("Hash bytes: " + bytes.Length);
Console.WriteLine("Version byte: 0x" + bytes[0].ToString("X2"));

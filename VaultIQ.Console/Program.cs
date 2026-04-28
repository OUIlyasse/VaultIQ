//using VaultIQ.Core.Entities;

//var entry = new PasswordEntry
//{
//    GroupId = Guid.NewGuid(),
//    Title = "Mon compte Gmail",
//    Username = "jean@gmail.com",
//    Password = "Tr0ub4d&3x@ct",
//    Url = "https://mail.google.com"
//};

//Console.WriteLine(entry);

//var copy = entry.Clone();
//Console.WriteLine(copy.Title); // "Mon compte Gmail (copie)"
//Console.WriteLine(copy.Id == entry.Id); // false — nouvel identifiant

//using VaultIQ.Core.Entities;

//var group = new PasswordGroup
//{
//    Name = "Banque & Finance",
//};

//group.Entries.Add(new PasswordEntry { Title = "CIC", GroupId = group.Id });
//group.Entries.Add(new PasswordEntry { Title = "PayPal", GroupId = group.Id });

//Console.WriteLine(group.Id); // 2

//using VaultIQ.Core.Entities;

//var db = new VaultDatabase { Name = "MesComptes" };

//var bankGroup = new PasswordGroup { Name = "Banque", IconPath = "🏦" };
//bankGroup.Entries.Add(new PasswordEntry { Title = "CIC", Username = "jean@cic.fr", GroupId = bankGroup.Id });
//db.Groups.Add(bankGroup);

//Console.WriteLine(db.TotalEntries);   // 1
//Console.WriteLine(db.TotalFavorites); // 0

//var results = db.Search("cic");        // retourne l'entrée CIC
//var group = db.FindGroupOf(results.First().Id); // retourne bankGroup

//db.Touch(); // ModifiedAt = maintenant

// Mot de passe fort par défaut (longueur 20, majuscules+minuscules+chiffres)
//using VaultIQ.Core.Crypto;
//using VaultIQ.Core.Entities;

//string pwd = VaultCrypto.GeneratePassword(length: 24, symbols: true);

//Console.WriteLine($"Mot de passe généré : {pwd} (force {PasswordStrengthHelper.Evaluate(pwd)})");

//using VaultIQ.Core.Crypto;

//byte[] data = System.Text.Encoding.UTF8.GetBytes("Ilyasse");
//byte[] enc = VaultCrypto.Encrypt(data, "MonMotDePasse");
//byte[] dec = VaultCrypto.Decrypt(enc, "MonMotDePasse");

//Console.WriteLine($"Données originales : {System.Text.Encoding.UTF8.GetString(data)}");
//Console.WriteLine($"Données chiffrées (base64) : {Convert.ToBase64String(enc)}");
//Console.WriteLine($"Données déchiffrées : {System.Text.Encoding.UTF8.GetString(dec)}");

using VaultIQ.Core.Entities;
using VaultIQ.Core.Storage;

var db = new VaultDatabase { Name = "TestVault" };
db.Groups.Add(new PasswordGroup { Name = "Général" });
var entry = new PasswordEntry
{
    GroupId = Guid.NewGuid(),
    Title = "Mon compte Gmail",
    Username = "jean@gmail.com",
    Password = "Tr0ub4d&3x@ct",
    Url = "https://mail.google.com"
};

db.Groups[0].Entries.Add(entry);

// Sauvegarder
//VaultFile.Save(db, $@"C:\Users\Ilyasse\Desktop\testVault\{db.Name}.viq", "MonMdp");

// Vérifier
bool valid = VaultFile.IsValidViqFile($@"C:\Users\Ilyasse\Desktop\testVault\{db.Name}.viq"); // true
Console.WriteLine(valid);

// Lire le nom sans le mot de passe
string name = VaultFile.ReadPublicName($@"C:\Users\Ilyasse\Desktop\testVault\{db.Name}.viq"); // "TestVault"
Console.WriteLine(name);

// Charger
VaultDatabase loaded = VaultFile.Load($@"C:\Users\Ilyasse\Desktop\testVault\{db.Name}.viq", "MonMdp");
Console.WriteLine(loaded.Name);        // "TestVault"
Console.WriteLine(loaded.TotalEntries); // 1
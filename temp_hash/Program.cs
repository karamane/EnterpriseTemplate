using BCrypt.Net;

Console.WriteLine("Generating BCrypt hashes for demo users...\n");

var adminPassword = "Admin123!";
var userPassword = "User123!";

var adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, BCrypt.Net.BCrypt.GenerateSalt(11));
var userHash = BCrypt.Net.BCrypt.HashPassword(userPassword, BCrypt.Net.BCrypt.GenerateSalt(11));

Console.WriteLine($"Admin Password: {adminPassword}");
Console.WriteLine($"Admin Hash: {adminHash}");
Console.WriteLine();
Console.WriteLine($"User Password: {userPassword}");
Console.WriteLine($"User Hash: {userHash}");
Console.WriteLine();

// Verify
Console.WriteLine("Verification:");
Console.WriteLine($"Admin hash verify: {BCrypt.Net.BCrypt.Verify(adminPassword, adminHash)}");
Console.WriteLine($"User hash verify: {BCrypt.Net.BCrypt.Verify(userPassword, userHash)}");

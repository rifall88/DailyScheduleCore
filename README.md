# DailySchedule API

DailySchedule adalah web service API yang dibuat dengan .NET 8, PostgreSQL, JWT Authentication, dan Middleware kustom. API ini digunakan untuk mengatur jadwal harian dengan fitur login, register, dan manajemen schedule.

## 🚀 Fitur

- ✅ Register & Login dengan JWT
- ✅ Middleware autentikasi
- ✅ PostgreSQL sebagai database
- ✅ CRUD Jadwal Harian

## 📁 Struktur Utama

├── Controllers/
├── Middleware/
├── Models/
├── Database/
├── Program.cs
├── appsetting.json

## Cara Jalankan Projek
1. Masuk Projek & Install point" berikut :
   a. dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   b. dotnet add package Npgsql
   c. dotnet add package Microsoft.IdentityModel.Tokens
   d. dotnet add package System.IdentityModel.Tokens.Jwt
   e. dotnet add package BCrypt.Net-Next
3. Beri isi Key dan DefaultConnection yang ada di appsetting.json, untuk Key isi terserah untuk DefaultConnection isinya seperti ini : "Host=localhost;Port=5432;Database=namadatabase;Username=postgres;Password=pwdb"
2. Jalankan dotnet restore
3. Jalankan dotnet build
4. jalankan dotnet run
5. masuk sesuai port yang sudah di sediakan ke swagger
   contoh http://localhost:5068/swagger/index.html
7. Lalu uji coba di postman dengan enpoint sesuai yang ada di swaggwer


## MOHON MAAF KAK DEPLOYNYA GA BISA, ERROR SUDAH 2 HARI SAYA COBA TAPI TETAP ERROR🙏
   

# RideMate

RideMate je aplikacija za deljenje voznji. Vozac objavljuje redovnu ili jednokratnu voznju na odredjenoj ruti, a putnici rezervisu sedista radi podele troskova. Sistem pokriva registraciju korisnika, verifikaciju dokumenata, rezervacije, placanja, otkazivanja, recenzije, sporove, notifikacije i admin panel.

## Funkcionalnosti

- Registracija i prijava korisnika preko JWT access tokena i refresh tokena.
- Uloge: `Driver`, `Passenger`, `Admin`.
- Admin je zasebna uloga i ne koristi vozacke/putnicke ekrane.
- Profil korisnika sa avatarom, telefonom, biografijom, prosecnom ocenom, trust score-om i brojem zavrsenih/otkazanih voznji.
- Driver profil sa vozilima, dokumentima i admin verifikacijom.
- Passenger ID dokument, admin odobrenje i zeleni verification badge pored imena.
- Verifikacija vozila kroz admin panel.
- Kreiranje voznji sa rutom, koordinatama, datumom, vremenom, cenom, brojem sedista i pravilima voznje.
- Tipovi voznje: jednokratna, ponavljajuca radnim danima, vikendom i dugorocna.
- Pretraga po polazistu, odredistu, datumu, broju sedista, ceni i opcionim koordinatama/radijusu.
- Rezervacija sedista, automatsko ili rucno odobravanje od strane vozaca.
- Mock placanje kesom ili online nakon odobravanja rezervacije.
- Politika otkazivanja i refundacije.
- Notifikacije za rezervacije, promene statusa voznje, otkazivanja, refundacije i verifikacije dokumenata/vozila.
- Recenzije i sporovi po zavrsenim rezervacijama.
- Admin panel za verifikacije, korisnike i sporove.

## Tehnologije

Backend:

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- PostgreSQL
- MediatR za CQRS
- FluentValidation
- AutoMapper
- Serilog
- Swagger/OpenAPI
- API versioning
- JWT authentication + refresh token rotacija i revoke lista
- Cloudinary za upload slika i dokumenata

Frontend:

- React 18
- TypeScript
- Vite
- Bootstrap 5

## Struktura projekta

```text
RideMateAPI/
  RideMateAPI/              Backend ASP.NET Core projekat
    Application/            Vertical slice moduli, Commands i Queries
    Controllers/            API kontroleri
    Data/                   EF Core DbContext i design-time factory
    DTOs/                   Request/response modeli
    Infrastructure/         Problem Details exception handling
    Mapping/                AutoMapper profili
    Middleware/             Custom middleware
    Migrations/             EF Core migracije
    Models/                 Entiteti i enum tipovi
    Services/               Domen servisi
    Validation/             FluentValidation validatori
  frontend/                 React frontend
```

## Arhitektura

Projekat koristi Vertical Slice pristup. Funkcionalnosti su grupisane po domenima u `Application` folderu, na primer:

- `Application/Auth`
- `Application/Rides`
- `Application/Bookings`
- `Application/Profile`
- `Application/Verification`
- `Application/Admin`

CQRS je implementiran kroz MediatR:

- Commands menjaju stanje sistema.
- Queries citaju podatke.
- `ValidationBehavior` pokrece FluentValidation pre obrade requesta.

Entity Framework Core `RideMateDbContext` se koristi kao Unit of Work i kao pristup repozitorijumima kroz `DbSet` kolekcije. Servisi sadrze domensku logiku koja ne pripada kontrolerima.

Globalni exception handler vraca konzistentan Problem Details format greske. Serilog upisuje strukturirane logove u konzolu i fajlove u `RideMateAPI/Logs`.

## Custom middleware

Implementiran je `SlowRequestMiddleware`. Middleware meri trajanje svakog HTTP zahteva i loguje warning ako zahtev traje duze od konfigurisanog praga u kodu, trenutno 1500 ms.

Log sadrzi:

- endpoint
- HTTP metodu
- korisnika ako je autentifikovan
- trajanje u milisekundama

Middleware je postavljen nakon autentifikacije, da bi imao pristup claim-ovima korisnika.

## Baza podataka

Koristi se PostgreSQL. 

EF Core migracije su u:

```text
RideMateAPI/Migrations
```

Design-time EF konfiguracija prvo cita connection string iz env varijable `RIDE_MATE_CONNECTION`, a zatim iz `appsettings.json`.

## Pokretanje projekta

### Preduslovi

- .NET SDK 10
- Node.js LTS
- PostgreSQL
- Cloudinary nalog za upload slika/dokumenata

### Backend konfiguracija

U `RideMateAPI/.env` 

```env
CLOUDINARY_URL=cloudinary://API_KEY:API_SECRET@CLOUD_NAME
RIDE_MATE_CONNECTION=Host=localhost;Port=5432;Database=ridemate-db;Username=postgres;Password=postgres
```

JWT vrednosti mogu se podesiti kroz `appsettings.json`, user secrets ili env varijable:

```json
{
  "Jwt": {
    "Key": "dugacak-tajni-kljuc-za-development",
    "Issuer": "RideMateApi",
    "Audience": "RideMateApiClients",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 30
  }
}
```


### Migracije

Iz root foldera projekta:

```powershell
dotnet ef database update --project RideMateAPI\RideMateAPI.csproj
```

### Backend start

```powershell
dotnet run --project RideMateAPI\RideMateAPI.csproj --launch-profile https
```

Backend se pokrece na:

```text
https://localhost:7066
```

Swagger je dostupan na:

```text
https://localhost:7066/swagger
```

### Frontend konfiguracija

U `frontend/.env`:

```env
VITE_BACKEND_URL=https://localhost:7066
VITE_API_BASE_URL=/api
```

Vite proxy prosledjuje `/api` pozive na backend.

### Frontend start

```powershell
cd frontend
npm install
npm run dev
```

Frontend se pokrece na:

```text
http://localhost:5173
```

## Build

Backend:

```powershell
dotnet build RideMateAPI\RideMateAPI.csproj
```

Frontend:

```powershell
cd frontend
npm run build
```

## Glavne API celine

- `AuthController` - registracija, login, refresh token, revoke token
- `UsersController` - moj profil, javni profil, admin upravljanje korisnicima
- `ProfileController` - izmena profila i vozila
- `VerificationController` - upload ID dokumenata i admin odobravanje
- `AdminController` - admin verifikacije, vozila i administracija
- `RidesController` / `RideSearchController` - voznje i pretraga
- `BookingsController` - rezervacije, odobravanje, odbijanje, placanje i otkazivanje
- `DriverPaymentsController` - oznacavanje kes placanja
- `NotificationsController` - notifikacije i citanje
- `ReviewsController` - recenzije
- `DisputesController` - sporovi

## Pravila domena

- Vozac ne moze da rezervise sopstvenu voznju.
- Vozac mora biti admin-verifikovan pre kreiranja voznji i upravljanja zahtevima.
- Passenger moze uploadovati ID dokument, ali badge dobija tek nakon admin odobrenja.
- Vozilo se vodi kao neverifikovano dok ga admin ne verifikuje.
- Otkazana voznja se ne prikazuje u aktivnim listama.
- Rezervacija se ne moze otkazati nakon sto je voznja krenula ili zavrsena.
- Jedna rezervacija moze imati najvise jedan spor.
- Jedna rezervacija moze imati najvise jednu recenziju po korisniku.
- Online placanje se radi tek nakon sto vozac odobri rezervaciju.
- Za jednokratne voznje broj slobodnih sedista se smanjuje posle odobravanja rezervacije.
- Za ponavljajuce voznje slobodna sedista se racunaju po datumu voznje.

## ER dijagram

ER dijagram treba da prikaze glavne tabele, primarne kljuceve, strane kljuceve i kardinalnosti. Predlog je da obuhvati sledece entitete:

- `Users`
- `Vehicles`
- `DriverVerificationDocuments`
- `Rides`
- `RideStops`
- `Bookings`
- `Payments`
- `Reviews`
- `Disputes`
- `SavedRoutes`
- `Notifications`
- `RefreshTokens`

Obavezno prikazati ove veze:

- `User 1 - N Vehicle`
- `User 1 - N Ride` kao driver
- `User 1 - N Booking` kao passenger
- `Ride 1 - N RideStop`
- `Ride 1 - N Booking`
- `Booking 1 - 0..1 Payment`
- `Booking 1 - N Review`
- `Booking 1 - N Dispute`
- `User 1 - N Review` kao reviewer
- `User 1 - N Review` kao reviewed user
- `User 1 - N DriverVerificationDocument`
- `User 1 - N Notification`
- `User 1 - N SavedRoute`
- `User 1 - N RefreshToken`

Na dijagramu je korisno posebno oznaciti enum polja:

- `UserRole`
- `RideType`
- `RideStatus`
- `RecurringType`
- `BookingStatus`
- `PaymentStatus`
- `PaymentMethod`
- `DisputeStatus`
- `DocumentType`
- `VerificationStatus`

Za preglednost mozes grupisati dijagram u oblasti:

- korisnici i verifikacije
- voznje i rezervacije
- placanja i refundacije
- recenzije i sporovi
- notifikacije i autentifikacija

## Napomena za dalje unapredjenje

Moguce nadogradnje:

- PostGIS za precizniju geo-pretragu.
- Real payment provider umesto mock placanja.
- Background job za podsetnike 1h pre polaska.
- SignalR za real-time notifikacije.
- Detaljniji audit log za admin akcije.
#   N P B  
 
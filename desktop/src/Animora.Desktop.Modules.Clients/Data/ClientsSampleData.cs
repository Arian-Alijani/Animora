using Animora.Desktop.Modules.Clients.Models;

namespace Animora.Desktop.Modules.Clients.Data;

/// <summary>
/// One seeded patient row, before the read-time join against <see cref="ClientsSampleData.Owners"/>
/// that produces a <see cref="Patient"/>'s <c>OwnerDisplayName</c> (and, from the same row, a
/// <see cref="MedicalFileSummary"/>).
/// </summary>
internal sealed record PatientRecord(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Species,
    string Sex,
    string? Breed,
    DateTime? BirthDateUtc,
    bool IsBirthDateEstimated,
    decimal? WeightKg,
    bool IsSterilized,
    string? MicrochipId,
    DateTime? MicrochipImplantedAtUtc,
    string? Color,
    string? Temperament,
    string? HousingType,
    string? Diet,
    string? BarcodeValue,
    string? SurgicalHistory);

// TODO(P1-16): delete this type once phase 16 (Clients Local Data) ships the SQLite-backed reads and
// writes; nothing but the four Stage A bindings in Composition/ServiceCollectionExtensions changes
// with it (DIR-03).
/// <summary>
/// The one seeded Persian demo dataset every Stage A fake in this folder reads and writes against
/// (items 14-15): five owners and the patients registered under them. A patient row already carries
/// every field a medical-file header needs (item 10's read model), so no separate "medical-file"
/// collection exists here — <see cref="InMemoryPatientStore"/> projects
/// <see cref="MedicalFileSummary"/> from the same <see cref="PatientRecord"/> row
/// <see cref="Patient"/> is projected from, the phase 05 TODO header's "reads through the patient
/// seam" decision applied to this seed data too (AG-14, INV-18).
/// </summary>
/// <remarks>
/// A singleton shared by every Stage A store rather than two private copies:
/// <see cref="InMemoryPatientStore"/> needs to read <see cref="Owners"/> to build
/// <c>Patient.OwnerDisplayName</c>/<c>MedicalFileSummary.OwnerDisplayName</c> — exactly the join
/// Stage C resolves with one Dapper query (DT-05) — so a shared instance is what keeps an owner
/// renamed through <see cref="InMemoryOwnerStore"/> instantly visible in the patient list's next
/// query, the way one SQLite database would.
/// <para>
/// One owner (<c>حسین صادقی</c>) intentionally shares a <c>MobileNumber</c> with another
/// (<c>مریم حسینی</c>) — the demo data exercising phase 05 TODO item 4's documented answer that two
/// owners may share one mobile number — and one owner (<c>حسین صادقی</c> again) intentionally has no
/// patients yet, exercising the owner-scoped patient list's empty state.
/// </para>
/// </remarks>
internal sealed class ClientsSampleData
{
    private readonly List<Owner> _owners;
    private readonly List<PatientRecord> _patients;

    public ClientsSampleData()
    {
        var aliOwnerId = Guid.CreateVersion7();
        var zahraOwnerId = Guid.CreateVersion7();
        var rezaOwnerId = Guid.CreateVersion7();
        var maryamOwnerId = Guid.CreateVersion7();
        var hosseinOwnerId = Guid.CreateVersion7();

        _owners =
        [
            new Owner(
                aliOwnerId,
                "علی محمدی",
                "09121112233",
                "02188112233",
                "2154432921",
                "خیابان ولیعصر، پلاک ۱۲۰",
                "تهران",
                "مشتری قدیمی، پرداخت همیشه به موقع.",
                new DateTime(2024, 5, 10, 8, 0, 0, DateTimeKind.Utc)),
            new Owner(
                zahraOwnerId,
                "زهرا کریمی",
                "09131234455",
                null,
                "7112449197",
                "خیابان چهارباغ عباسی",
                "اصفهان",
                null,
                new DateTime(2025, 1, 15, 9, 30, 0, DateTimeKind.Utc)),
            new Owner(
                rezaOwnerId,
                "رضا رضایی",
                "09141234567",
                "03112223344",
                null,
                null,
                "شیراز",
                "به گربه‌ها حساسیت دارد؛ فقط برای سگ و پرنده مراجعه می‌کند.",
                new DateTime(2025, 6, 20, 10, 15, 0, DateTimeKind.Utc)),
            new Owner(
                maryamOwnerId,
                "مریم حسینی",
                "09151112233",
                null,
                "4974851373",
                "بلوار وکیل‌آباد",
                "مشهد",
                null,
                new DateTime(2026, 2, 1, 11, 0, 0, DateTimeKind.Utc)),
            new Owner(
                hosseinOwnerId,
                "حسین صادقی",
                // Deliberately the same number as مریم حسینی above: phase 05 TODO item 4's
                // documented answer (a mobile number is a contact channel, not a sign-in identity)
                // has nothing to demonstrate against without one shared pair.
                "09151112233",
                null,
                "6534622726",
                "بلوار وکیل‌آباد",
                "مشهد",
                "همسایه و همراه خانم حسینی در مراجعات.",
                new DateTime(2026, 2, 1, 11, 5, 0, DateTimeKind.Utc)),
        ];

        _patients =
        [
            new PatientRecord(
                Guid.CreateVersion7(),
                aliOwnerId,
                "رکس",
                "Dog",
                "Male",
                "ژرمن شپرد",
                new DateTime(2022, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: false,
                WeightKg: 32.5m,
                IsSterilized: false,
                MicrochipId: "981000012345678",
                MicrochipImplantedAtUtc: new DateTime(2022, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                Color: "قهوه‌ای و سیاه",
                Temperament: "آرام و دوستانه",
                HousingType: "House",
                Diet: "غذای خشک صنعتی",
                BarcodeValue: "PF-0001",
                SurgicalHistory: null),
            new PatientRecord(
                Guid.CreateVersion7(),
                aliOwnerId,
                "میو",
                "Cat",
                "Female",
                "پرشین",
                new DateTime(2023, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: false,
                WeightKg: 4.2m,
                IsSterilized: true,
                MicrochipId: null,
                MicrochipImplantedAtUtc: null,
                Color: "سفید",
                Temperament: "کمی محجوب",
                HousingType: "Apartment",
                Diet: null,
                BarcodeValue: "PF-0002",
                SurgicalHistory: "عقیم‌سازی در سال ۱۴۰۲"),
            new PatientRecord(
                Guid.CreateVersion7(),
                zahraOwnerId,
                "لولو",
                "Rabbit",
                "Female",
                null,
                // Estimated at intake from a staff-entered approximate age, per IPatientInput's own
                // doc comment on this "birth date or age, whichever is known" flow.
                new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: true,
                WeightKg: 1.8m,
                IsSterilized: false,
                MicrochipId: null,
                MicrochipImplantedAtUtc: null,
                Color: "سفید و قهوه‌ای",
                Temperament: "بازیگوش",
                HousingType: "Apartment",
                Diet: "علوفه و سبزیجات تازه",
                BarcodeValue: "PF-0003",
                SurgicalHistory: null),
            new PatientRecord(
                Guid.CreateVersion7(),
                rezaOwnerId,
                "بارون",
                "Dog",
                "Male",
                "پامرانین",
                new DateTime(2021, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: false,
                WeightKg: 3.1m,
                IsSterilized: false,
                MicrochipId: "981000098765432",
                MicrochipImplantedAtUtc: new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Color: "کرم",
                Temperament: "پرانرژی",
                HousingType: "Garden",
                Diet: null,
                BarcodeValue: "PF-0004",
                SurgicalHistory: null),
            new PatientRecord(
                Guid.CreateVersion7(),
                rezaOwnerId,
                "کاکائو",
                "Bird",
                "Unknown",
                "مرغ عشق",
                // Unknown for a bird at intake: no estimate ventured, so this stays null rather than
                // an estimated flag with nothing to have estimated from.
                BirthDateUtc: null,
                IsBirthDateEstimated: false,
                WeightKg: 0.05m,
                IsSterilized: false,
                MicrochipId: null,
                MicrochipImplantedAtUtc: null,
                Color: "زرد و سبز",
                Temperament: null,
                HousingType: "Apartment",
                Diet: "دانه مخصوص طوطی",
                BarcodeValue: "PF-0005",
                SurgicalHistory: null),
            new PatientRecord(
                Guid.CreateVersion7(),
                rezaOwnerId,
                "جینجر",
                "Rodent",
                "Female",
                "همستر سوری",
                new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: true,
                WeightKg: 0.12m,
                IsSterilized: false,
                MicrochipId: null,
                MicrochipImplantedAtUtc: null,
                Color: "نارنجی",
                Temperament: "کمرو",
                HousingType: "Apartment",
                Diet: null,
                BarcodeValue: "PF-0006",
                SurgicalHistory: null),
            new PatientRecord(
                Guid.CreateVersion7(),
                maryamOwnerId,
                "دلبر",
                "Cat",
                "Female",
                "اسکاتیش فولد",
                new DateTime(2024, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                IsBirthDateEstimated: false,
                WeightKg: 3.6m,
                IsSterilized: true,
                MicrochipId: "981000011223344",
                MicrochipImplantedAtUtc: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Color: "طوسی",
                Temperament: "آرام",
                HousingType: "Apartment",
                Diet: "غذای مخصوص گربه‌های عقیم‌شده",
                BarcodeValue: "PF-0007",
                SurgicalHistory: "عقیم‌سازی در سال ۱۴۰۳"),
            // حسین صادقی has no patients yet: the owner-scoped patient list's empty state has
            // something real to render against.
        ];
    }

    /// <summary>
    /// The one lock every Stage A store in this folder guards its reads and writes with. A single
    /// gate rather than one per collection: <see cref="InMemoryPatientStore"/> reads across both
    /// <see cref="Owners"/> and <see cref="Patients"/> to build its read models' joined
    /// <c>OwnerDisplayName</c> field, so two separate locks could deadlock on cross-acquisition
    /// order — the same reasoning <c>IdentitySampleData.Gate</c> already documents.
    /// </summary>
    public object Gate { get; } = new();

    public List<Owner> Owners => _owners;

    public List<PatientRecord> Patients => _patients;
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mapa.Portfolio.Services;

public sealed class DemoState
{
    public int Version { get; set; } = 1;
    public List<DemoIncident> Incidents { get; set; } = new();
    public List<DemoUnit> Units { get; set; } = new();
    public List<DemoCamera> Cameras { get; set; } = new();
    public List<DemoCategory> Categories { get; set; } = new();
}

public sealed class DemoIncident
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Priority { get; set; } = "Media";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Nuevo";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string? UnitId { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    public List<DemoEntry> History { get; set; } = new();
}

public sealed record DemoEntry(DateTimeOffset At, string Text);

public sealed class DemoUnit
{
    public string Id { get; set; } = "";
    public string Specialty { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public sealed class DemoCamera
{
    public string Id { get; set; } = "";
    public string Location { get; set; } = "";
    public bool Online { get; set; } = true;
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public sealed class DemoCategory
{
    public string Name { get; set; } = "";
    public string Priority { get; set; } = "Media";
    public string Questions { get; set; } = "";
}

public sealed class DemoStore
{
    public DemoState State { get; private set; } = Seed();
    public static readonly string[] Priorities = { "Alta", "Media", "Baja" };
    public static readonly string[] Statuses = { "Nuevo", "Asignado", "En sitio", "Cerrado" };

    public bool Available(string unitId) =>
        !State.Incidents.Any(i => i.UnitId == unitId && i.Status != "Cerrado");

    public string Serialize() => JsonSerializer.Serialize(State, DemoJsonContext.Default.DemoState);

    public void Restore(string json)
    {
        var s =
            JsonSerializer.Deserialize(json, DemoJsonContext.Default.DemoState)
            ?? throw new InvalidOperationException("Archivo vacío.");
        if (
            s.Version != 1
            || s.Incidents is null
            || s.Units is null
            || s.Cameras is null
            || s.Categories is null
            || s.Incidents.Count > 10000
            || s.Units.Count > 1000
            || s.Cameras.Count > 1000
            || s.Categories.Count == 0
            || s.Incidents.Select(i => i.Id).Distinct().Count() != s.Incidents.Count
            || s.Units.Select(u => u.Id).Distinct().Count() != s.Units.Count
            || s.Categories.Select(c => c.Name).Distinct().Count() != s.Categories.Count
            || s.Categories.Any(c =>
                string.IsNullOrWhiteSpace(c.Name) || !Priorities.Contains(c.Priority)
            )
            || s.Units.Any(u => string.IsNullOrWhiteSpace(u.Id) || !Coordinates(u.Lat, u.Lng))
            || s.Cameras.Any(c => !Coordinates(c.Lat, c.Lng))
            || s.Incidents.Any(i =>
                i.Id < 1
                || string.IsNullOrWhiteSpace(i.Title)
                || string.IsNullOrWhiteSpace(i.Address)
                || !Statuses.Contains(i.Status)
                || !Priorities.Contains(i.Priority)
                || !Coordinates(i.Lat, i.Lng)
                || i.History is null
                || !s.Categories.Any(c => c.Name == i.Category)
                || (i.UnitId != null && !s.Units.Any(u => u.Id == i.UnitId))
                || ((i.Status == "Asignado" || i.Status == "En sitio") && i.UnitId == null)
                || (i.Status == "Nuevo" && i.UnitId != null)
            )
            || s.Incidents.Where(i => i.Status != "Cerrado" && i.UnitId != null)
                .GroupBy(i => i.UnitId)
                .Any(g => g.Count() > 1)
        )
            throw new InvalidOperationException(
                "El respaldo no tiene una estructura válida o contiene asignaciones incompatibles."
            );
        State = s;
    }

    public DemoIncident Create(
        string title,
        string category,
        string priority,
        string address,
        string description,
        double lat,
        double lng
    )
    {
        if (
            string.IsNullOrWhiteSpace(title)
            || title.Trim().Length > 120
            || string.IsNullOrWhiteSpace(address)
            || address.Length > 200
            || description.Length > 2000
        )
            throw new InvalidOperationException(
                "Escribe un título (hasta 120 caracteres), ubicación (hasta 200) y descripción de hasta 2000 caracteres."
            );
        if (
            !State.Categories.Any(c => c.Name == category)
            || !Priorities.Contains(priority)
            || !Coordinates(lat, lng)
        )
            throw new InvalidOperationException("Revisa el tipo, prioridad y coordenadas.");
        var incident = new DemoIncident
        {
            Id = State.Incidents.Select(i => i.Id).DefaultIfEmpty(0).Max() + 1,
            Title = title.Trim(),
            Category = category,
            Priority = priority,
            Address = address.Trim(),
            Description = description.Trim(),
            Lat = lat,
            Lng = lng,
        };
        incident.History.Add(new(DateTimeOffset.Now, "Incidente de demostración registrado."));
        State.Incidents.Insert(0, incident);
        return incident;
    }

    public void Assign(int id, string unitId)
    {
        var incident = Find(id);
        if (incident.Status == "Cerrado" || incident.Status == "En sitio")
            throw new InvalidOperationException("Ese incidente ya no admite una asignación.");
        if (
            !State.Units.Any(u => u.Id == unitId)
            || (!Available(unitId) && incident.UnitId != unitId)
        )
            throw new InvalidOperationException("La unidad ya está ocupada o no existe.");
        incident.UnitId = unitId;
        incident.Status = "Asignado";
        incident.History.Add(new(DateTimeOffset.Now, $"Unidad {unitId} asignada (simulación)."));
    }

    public void Advance(int id, string next, string note)
    {
        var incident = Find(id);
        if (
            !(
                (incident.Status == "Asignado" && next == "En sitio")
                || (incident.Status != "Cerrado" && next == "Cerrado")
            )
        )
            throw new InvalidOperationException("El cambio de estado no es válido.");
        if (string.IsNullOrWhiteSpace(note) || note.Length > 1000)
            throw new InvalidOperationException("Escribe una nota de hasta 1000 caracteres.");
        incident.Status = next;
        incident.History.Add(new(DateTimeOffset.Now, $"{next}: {note.Trim()}"));
    }

    public void Note(int id, string note)
    {
        if (string.IsNullOrWhiteSpace(note) || note.Length > 1000)
            throw new InvalidOperationException("Escribe una nota de hasta 1000 caracteres.");
        Find(id).History.Add(new(DateTimeOffset.Now, note.Trim()));
    }

    public void Reset() => State = Seed();

    private DemoIncident Find(int id) =>
        State.Incidents.FirstOrDefault(i => i.Id == id)
        ?? throw new InvalidOperationException("Incidente no encontrado.");

    private static bool Coordinates(double lat, double lng) =>
        double.IsFinite(lat)
        && double.IsFinite(lng)
        && lat is >= -90 and <= 90
        && lng is >= -180 and <= 180;

    private static DemoState Seed()
    {
        var s = new DemoState
        {
            Categories = new()
            {
                new()
                {
                    Name = "Accidente vial",
                    Priority = "Alta",
                    Questions =
                        "¿Hay personas lesionadas?\n¿Cuántos vehículos participan?\n¿Existe bloqueo de la vialidad?",
                },
                new()
                {
                    Name = "Apoyo ciudadano",
                    Priority = "Baja",
                    Questions = "¿Qué apoyo se requiere?\n¿Cuál es el punto de referencia?",
                },
                new()
                {
                    Name = "Emergencia médica",
                    Priority = "Alta",
                    Questions =
                        "¿La persona está consciente?\n¿Respira normalmente?\n¿Hay personal médico en el sitio?",
                },
                new()
                {
                    Name = "Incendio",
                    Priority = "Alta",
                    Questions = "¿Hay personas en riesgo?\n¿Qué material se está quemando?",
                },
                new()
                {
                    Name = "Reporte preventivo",
                    Priority = "Media",
                    Questions = "¿Qué se observa?\n¿Desde cuándo ocurre?",
                },
            },
            Units = new()
            {
                new()
                {
                    Id = "DEMO-01",
                    Specialty = "Prevención",
                    Lat = 22.151,
                    Lng = -100.982,
                },
                new()
                {
                    Id = "DEMO-02",
                    Specialty = "Tránsito",
                    Lat = 22.165,
                    Lng = -100.997,
                },
                new()
                {
                    Id = "DEMO-03",
                    Specialty = "Atención médica",
                    Lat = 22.142,
                    Lng = -100.974,
                },
                new()
                {
                    Id = "DEMO-04",
                    Specialty = "Protección civil",
                    Lat = 22.156,
                    Lng = -101.015,
                },
                new()
                {
                    Id = "DEMO-05",
                    Specialty = "Prevención",
                    Lat = 22.177,
                    Lng = -100.966,
                },
            },
            Cameras = new()
            {
                new()
                {
                    Id = "CAM-DEMO-01",
                    Location = "Centro · ubicación ficticia",
                    Lat = 22.152,
                    Lng = -100.976,
                },
                new()
                {
                    Id = "CAM-DEMO-02",
                    Location = "Poniente · ubicación ficticia",
                    Lat = 22.158,
                    Lng = -101.006,
                },
                new()
                {
                    Id = "CAM-DEMO-03",
                    Location = "Norte · ubicación ficticia",
                    Lat = 22.174,
                    Lng = -100.982,
                    Online = false,
                },
            },
        };
        s.Incidents = new()
        {
            new()
            {
                Id = 1003,
                Title = "Choque de dos vehículos · ejercicio",
                Category = "Accidente vial",
                Priority = "Alta",
                Address = "Zona Centro, SLP (simulada)",
                Description =
                    "Ejercicio: choque sin datos personales. Verificar condiciones del sitio.",
                Lat = 22.153,
                Lng = -100.989,
            },
            new()
            {
                Id = 1002,
                Title = "Apoyo a peatón · ejercicio",
                Category = "Apoyo ciudadano",
                Priority = "Baja",
                Address = "Zona Alameda, SLP (simulada)",
                Description = "Ejercicio de orientación ciudadana.",
                Lat = 22.149,
                Lng = -100.966,
                Status = "Asignado",
                UnitId = "DEMO-01",
            },
            new()
            {
                Id = 1001,
                Title = "Revisión preventiva · ejercicio",
                Category = "Reporte preventivo",
                Priority = "Media",
                Address = "Zona Poniente, SLP (simulada)",
                Description = "Ejercicio de recorrido preventivo concluido.",
                Lat = 22.160,
                Lng = -101.007,
                Status = "Cerrado",
                UnitId = "DEMO-04",
            },
        };
        foreach (var i in s.Incidents)
            i.History.Add(
                new(DateTimeOffset.Now, $"Escenario inicial de demostración · {i.Status}.")
            );
        return s;
    }
}

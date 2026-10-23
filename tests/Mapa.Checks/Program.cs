using Mapa.Portfolio.Services;

static void Assert(bool ok, string message)
{
    if (!ok)
        throw new Exception(message);
}
static void Reject(Action action)
{
    try
    {
        action();
    }
    catch (InvalidOperationException)
    {
        return;
    }
    throw new Exception("Invalid operation was accepted");
}
var s = new DemoStore();
var i = s.Create(
    "Prueba",
    "Accidente vial",
    "Alta",
    "Lugar simulado",
    "Sin datos reales",
    22.15,
    -100.98
);
s.Assign(i.Id, "DEMO-02");
Assert(!s.Available("DEMO-02"), "Assigned unit must be busy");
Reject(() => s.Assign(1003, "DEMO-02"));
Reject(() => s.Advance(i.Id, "Nuevo", "No válido"));
Reject(() => s.Advance(i.Id, "Cerrado", ""));
s.Advance(i.Id, "En sitio", "Llegada simulada");
Reject(() => s.Assign(i.Id, "DEMO-03"));
s.Note(i.Id, "Verificación");
s.Advance(i.Id, "Cerrado", "Ejercicio concluido");
Assert(s.Available("DEMO-02"), "Closing must release unit");
Reject(() => s.Assign(i.Id, "DEMO-03"));
Reject(() => s.Create("", "Accidente vial", "Alta", "Lugar", "", 22, -100));
Reject(() => s.Create("Prueba", "Accidente vial", "Alta", "Lugar", "", double.NaN, -100));
var saved = s.Serialize();
var restored = new DemoStore();
restored.Restore(saved);
Assert(
    restored.State.Incidents.First(x => x.Id == i.Id).History.Count == 5,
    "History must survive restore"
);
Reject(() => restored.Restore(saved.Replace("\"Version\": 1", "\"Version\": 99")));
Assert(restored.State.Incidents.Count == 4, "Invalid import must not mutate state");
s.Reset();
Assert(s.State.Incidents.Count == 3, "Reset restores seed");
Console.WriteLine(
    "PASS: incident validation, exclusive assignment, transitions, release, history, restore, invalid import, reset."
);

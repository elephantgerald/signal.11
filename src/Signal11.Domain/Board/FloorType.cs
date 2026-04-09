namespace Signal11.Domain;

public enum FloorType
{
    Normal = 0,
    Pit = 1,
    Repair = 2,
    DoubleRepair = 3,
    Flag = 4,   // values 4–7 encode Flag 1–4 (flag number = value - 3)
    Start = 8,  // values 8–15 encode Start 1–8 (start index = value - 7)
}

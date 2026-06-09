using SQLite;

public class RunRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string runType { get; set; }      // "Current" o "Best"
    public string timestamp { get; set; }
    public int robotsDestroyed { get; set; }
    public int machinesDestroyed { get; set; }
    public float damageDealt { get; set; }
    public float damageReceived { get; set; }
    public float energyAbsorbed { get; set; }
    public int levelReached { get; set; }
    public float gameTime { get; set; }       // segundos
    public float score { get; set; }          // puntuación calculada
    public bool victory { get; set; }
}
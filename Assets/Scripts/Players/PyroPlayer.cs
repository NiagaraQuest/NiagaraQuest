public class PyroPlayer : Player
{
    protected override void Start()
    {
        currentPath = "PyroPath";
        base.Start(); // Appelle la version originale de `Start()`
    }
}

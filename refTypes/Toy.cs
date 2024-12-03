namespace refTypes;
internal class Toy
{
    public string Name { get; set; } = string.Empty;
}

internal class Actions
{
    public string CreateToy()
    {
        Toy toy = new Toy();
        toy.Name = "some toy";
        Play(ref toy);

        return toy.Name;
    }

    public void Play(ref Toy toy)
    {
        toy = new Toy();

        toy.Name = "not real name";
    }
}

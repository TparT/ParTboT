namespace ParTboT.DbModels.Shared
{
    public record Identification<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
    }
}

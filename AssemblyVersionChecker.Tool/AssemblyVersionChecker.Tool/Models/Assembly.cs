namespace AssemblyVersionChecker.Tool.Models
{
    public class Assembly
    {
        public Assembly(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }

        public string Version { get; }
    }
}
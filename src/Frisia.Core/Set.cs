namespace Frisia.Core
{
    public class Set
    {
        public string MethodName { get; set; }
        public Parameter[] Parameters { get; set; }
        public string ResultTypeName { get; set; }
        public object ExpectedResult { get; set; }
        public string[] CodeChecks { get; set; }
    }
}

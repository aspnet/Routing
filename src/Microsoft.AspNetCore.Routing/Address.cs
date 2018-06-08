using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public class Address
    {
        public Address(string name)
        {
            Name = name;
        }

        public Address(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        public Address(string name, MethodInfo methodInfo)
        {
            Name = name;
            MethodInfo = methodInfo;
        }

        public string Name { get; }

        public MethodInfo MethodInfo { get; }
    }
}

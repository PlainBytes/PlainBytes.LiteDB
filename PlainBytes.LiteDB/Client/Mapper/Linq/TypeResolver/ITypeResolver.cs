using System.Reflection;

namespace PlainBytes.LiteDB
{
    internal interface ITypeResolver
    {
        string ResolveMethod(MethodInfo method);

        string ResolveMember(MemberInfo member);

        string ResolveCtor(ConstructorInfo ctor);
    }
}
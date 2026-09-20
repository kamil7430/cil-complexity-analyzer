namespace CilInjecting.Tests.Infrastructure.Fakes;

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

public class TrackingAssemblyLoadContext : AssemblyLoadContext
{
    public List<string> RequestedPaths { get; } = new();

    public TrackingAssemblyLoadContext() : base(isCollectible: true) { }

    protected override Assembly? Load(AssemblyName assemblyName) => null;

    public new Assembly LoadFromAssemblyPath(string assemblyPath)
    {
        RequestedPaths.Add(assemblyPath);
        return base.LoadFromAssemblyPath(assemblyPath);
    }
}
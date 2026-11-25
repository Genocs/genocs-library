using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using Genocs.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Repositories;

/// <summary>
/// Builds an open-generic repository whose single type argument is the aggregate
/// and whose constructor takes a closed caller <see cref="DbContext"/>.
/// MS.DI requires the implementation arity to match <c>IRepository&lt;&gt;</c>, so a
/// two-argument <see cref="GenocsRepository{T, TContext}"/> cannot be registered directly.
/// </summary>
internal static class OpenGenericRepositoryType
{
    private static readonly ConcurrentDictionary<Type, Type> Cache = new();

    public static Type For(Type contextType)
    {
        ArgumentNullException.ThrowIfNull(contextType);
        return Cache.GetOrAdd(contextType, Define);
    }

    private static Type Define(Type contextType)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("Genocs.Persistence.EFCore.DynamicRepositories." + contextType.Name),
            AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("repositories");
        var builder = module.DefineType(
            "Repository",
            TypeAttributes.Public | TypeAttributes.Class);

        var entity = builder.DefineGenericParameters("T")[0];
        entity.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint);
        entity.SetInterfaceConstraints(typeof(IAggregateRoot));

        var baseType = typeof(GenocsRepository<,>).MakeGenericType(entity, contextType);
        builder.SetParent(baseType);

        var constructor = builder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [contextType]);
        var il = constructor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        var baseConstructor = typeof(GenocsRepository<,>).GetConstructors().Single();
        il.Emit(OpCodes.Call, TypeBuilder.GetConstructor(baseType, baseConstructor));
        il.Emit(OpCodes.Ret);

        return builder.CreateType()
            ?? throw new InvalidOperationException($"Couldn't create a repository type for {contextType.Name}.");
    }
}

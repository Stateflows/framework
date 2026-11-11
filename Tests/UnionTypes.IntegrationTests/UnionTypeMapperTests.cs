using System.Runtime.CompilerServices;
using Stateflows.Common.Classes;
using Stateflows.Extensions.UnionTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnionTypes.IntegrationTests;

[TestClass]
public sealed class UnionTypeMapperTests
{
    [TestMethod]
    public void CompilerGeneratedUnionMapsItsCases()
    {
        AssertCases(typeof(Decision), typeof(Approved), typeof(Rejected));
    }

    [TestMethod]
    public void ConstructedGenericUnionMapsSubstitutedCases()
    {
        AssertCases(typeof(Result<int>), typeof(int), typeof(Exception));
    }

    [TestMethod]
    public void ManualUnionMapsConstructorParametersIncludingInAndNullable()
    {
        AssertCases(typeof(ManualUnion), typeof(int?), typeof(Guid));
    }

    [TestMethod]
    public void ProviderDefinesCasesInsteadOfUnionConstructors()
    {
        AssertCases(typeof(ProviderUnion<string>), typeof(string), typeof(Exception));
    }

    [TestMethod]
    public void InheritedProviderCreationMembersAreIncluded()
    {
        AssertCases(typeof(InheritedProviderUnion), typeof(Approved), typeof(Rejected));
    }

    [TestMethod]
    public void OrdinaryTypesAreNotMapped()
    {
        var mapper = new UnionTypeMapper();
        foreach (var type in new[] { typeof(string), typeof(int), typeof(OrdinaryClass), typeof(List<int>) })
        {
            Assert.IsFalse(mapper.TryMapType(type, out var types));
            Assert.AreEqual(0, types.Count());
        }
    }

    [TestMethod]
    public void MappingPipelineRegistersCaseTypesAndPreservesOrdinaryTypes()
    {
        var registered = new List<Type>();
        var mapper = new TypeMapper([new UnionTypeMapper()], registered.Add);
        CollectionAssert.AreEquivalent(new[] { typeof(Approved), typeof(Rejected) },
            mapper.GetMappedTypes(typeof(Decision)).ToArray());
        CollectionAssert.AreEquivalent(new[] { typeof(Approved), typeof(Rejected) }, registered);
        CollectionAssert.AreEqual(new[] { typeof(string) }, mapper.GetMappedTypes(typeof(string)).ToArray());
    }

    [TestMethod]
    public void NestedUnionCasesAreNotFlattened()
    {
        AssertCases(typeof(NestedUnion), typeof(Decision), typeof(int));
    }

    private static void AssertCases(Type union, params Type[] expected)
    {
        Assert.IsTrue(new UnionTypeMapper().TryMapType(union, out var types));
        CollectionAssert.AreEquivalent(expected, types.ToArray());
    }

    public sealed record Approved;
    public sealed record Rejected;
    public union Decision(Approved, Rejected);
    public union Result<T>(T, Exception);
    public union NestedUnion(Decision, int);

    [Union]
    public readonly struct ManualUnion
    {
        public ManualUnion(int? value) => Value = value;
        public ManualUnion(in Guid value) => Value = value;
        public object? Value { get; }
    }

    [Union]
    public sealed class ProviderUnion<T> : ProviderUnion<T>.IUnionMembers
    {
        public ProviderUnion(DateTime ignored) { }
        private ProviderUnion(object? value) => valueField = value;
        private readonly object? valueField;
        object? IUnionMembers.Value => valueField;

        public interface IUnionMembers
        {
            public static ProviderUnion<T> Create(T value) => new((object?)value);
            public static ProviderUnion<T> Create(Exception value) => new((object)value);
            object? Value { get; }
        }
    }

    public interface IApprovedFactory
    {
        public static InheritedProviderUnion Create(Approved value) => new(value);
    }

    [Union]
    public sealed class InheritedProviderUnion(object value) : InheritedProviderUnion.IUnionMembers
    {
        object? IUnionMembers.Value => value;

        public interface IUnionMembers : IApprovedFactory
        {
            public static InheritedProviderUnion Create(Rejected value) => new(value);
            object? Value { get; }
        }
    }

    public sealed class OrdinaryClass
    {
        public OrdinaryClass(int value) => Value = value;
        public OrdinaryClass(string value) => Value = value;
        public object Value { get; }
    }
}

using System;
using System.Collections.Generic;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests
{
   public class ReflectionLearningTests : NMockitoInstance
   {
      [Fact]
      public void GenericTypeDefinitionIsAThing()
      {
         var type = typeof(GenericClass<,>);
         AssertTrue(type.IsGenericType);
         AssertTrue(type.IsGenericTypeDefinition);
         AssertFalse(type.IsGenericParameter);
         AssertEquals(2, type.GetGenericArguments().Length);
      }

      [Fact]
      public void GenericTypeDefinitionIsntAlwaysTrue()
      {
         var type = typeof(GenericClass<int, float>);
         AssertTrue(type.IsGenericType);
         AssertFalse(type.IsGenericTypeDefinition);
         AssertFalse(type.IsGenericParameter);
      }
   }

   public class GenericClass<TParam1, TParam2>
   {
      public TParam1 Field1 { get; set; }
      public TParam2 Field2 { get; set; }
      public Tuple<TParam1, TParam2> CombinationField1 { get; set; }
   }
}

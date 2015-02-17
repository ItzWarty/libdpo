using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.PortableObjects {
   public static class ReflectionHelpers {
      public static Type FindInterfaceByGenericDefinition(Type type, Type genericDefinition) {
         var matches = type.FindInterfaces(FilterGenericDefinition, genericDefinition);
         Trace.Assert(matches.Length <= 1);
         if (matches.Length == 1) {
            return matches[0];
         } else {
            return null;
         }
      }

      private static bool FilterGenericDefinition(Type type, object genericDefinition) {
         return type.IsGenericType &&
                type.GetGenericTypeDefinition() == (Type)genericDefinition;
      }

      public static Type GetIEnumerableElementType(Type t) {
         var x = FindInterfaceByGenericDefinition(t, typeof(IEnumerable<>));
         return x?.GetGenericArguments()[0];
      }
   }
}

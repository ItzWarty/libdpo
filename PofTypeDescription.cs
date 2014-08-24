using System;
using ItzWarty;

namespace Dargon.PortableObjects
{
   public class PofTypeDescription
   {
      private readonly Type[] types;
      private readonly int hashcode;

      public PofTypeDescription(Type[] types)
      {
         this.types = types;
         this.hashcode = ComputeHashcode(types);
      }

      private int ComputeHashcode(Type[] types)
      {
         int hash = 13;
         for (int i = 0; i < types.Length; i++)
         {
            var type = types[i];
            hash = hash * 17 + type.GetHashCode();
         }
         return hash;
      }

      public bool HasGenericDefinition { get { return this.types.Length > 1; } }
      public Type First() { return this.types[0]; }
      public Type[] AfterFirst() { return this.types.SubArray(1); }
      public Type[] All() { return this.types; }

      public override int GetHashCode()
      {
         return this.hashcode;
      }

      public override bool Equals(object obj)
      {
         var desc = obj as PofTypeDescription;
         if (desc == null)
            return false;
         else
         {
            if (desc.hashcode != this.hashcode)
               return false;
            else
            {
               for (int i = 0; i < desc.types.Length; i++)
               {
                  if (desc.types[i] != this.types[i])
                     return false;
               }
               return true;
            }
         }
      }
   }
}

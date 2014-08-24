using System;

namespace Dargon.PortableObjects
{
   public class TypeNotFoundException : Exception
   {
      private Type type;

      public TypeNotFoundException(Type type) { this.type = type; }

      public override string Message { get { return "Could not find type " + type; } }
   }
   public class TypeIdNotFoundException : Exception
   {
      private int typeId;

      public TypeIdNotFoundException(int typeId) { this.typeId = typeId; }

      public override string Message { get { return "Could not find type id " + typeId; } }
   }
}
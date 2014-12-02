using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dargon.PortableObjects
{
   public class PofContext : IPofContext
   {
      private readonly Dictionary<int, Type> typeByTypeId = new Dictionary<int, Type>();
      private readonly Dictionary<Type, int> typeIdByType = new Dictionary<Type, int>();
      private readonly Dictionary<int, Type> reservedTypeByTypeId = new Dictionary<int, Type>();
      private readonly Dictionary<Type, int> typeIdByReservedType = new Dictionary<Type, int>();
      private readonly ConcurrentDictionary<PofTypeDescription, Type> typeByDescription = new ConcurrentDictionary<PofTypeDescription, Type>(); 

      public PofContext() {
         RegisterReservedPortableObjectTypes();
      }

      public void MergeContext(PofContext context)
      {
         foreach (var kvp in context.typeByTypeId) {
            if (kvp.Key >= 0) {
               RegisterPortableObjectType(kvp.Key, kvp.Value);
            }
         }
      }

      private void RegisterReservedPortableObjectTypes()
      {
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_NULL, typeof(void));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_S8, typeof(sbyte));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_U8, typeof(byte));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_S16, typeof(short));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_U16, typeof(ushort));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_S32, typeof(int));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_U32, typeof(uint));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_S64, typeof(long));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_U64, typeof(ulong));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_FLOAT, typeof(float));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_DOUBLE, typeof(double));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_CHAR, typeof(char));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_STRING, typeof(string));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_BOOL, typeof(bool));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_GUID, typeof(Guid));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_OBJECT, typeof(object));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_ENUMERABLE, typeof(IEnumerable));
      }

      private void RegisterReservedPortableObjectType(int typeId, Type type)
      {
         reservedTypeByTypeId.Add(typeId, type);
         typeIdByReservedType.Add(type, typeId);
         RegisterPortableObjectTypePrivate(typeId, type);
      }

      public void RegisterPortableObjectType(int typeId, Type type)
      {
         if (typeId < 0)
            throw new InvalidOperationException("Negative TypeIDs are reserved for system use.");

         RegisterPortableObjectTypePrivate(typeId, type);
      }

      private void RegisterPortableObjectTypePrivate(int typeId, Type type)
      {
         typeByTypeId.Add(typeId, type);
         typeIdByType.Add(type, typeId);
      }

      public bool IsInterfaceRegistered(Type t)
      {
         return typeIdByType.ContainsKey(t);
      }

      public bool IsReservedType(Type type) { return typeIdByReservedType.ContainsKey(type); }
      public bool IsReservedTypeId(int typeId) { return reservedTypeByTypeId.ContainsKey(typeId); }

      public int GetTypeIdByType(Type t)
      {
         int value;
         if (typeIdByType.TryGetValue(t, out value))
            return value;
         throw new TypeNotFoundException(t);
      }

      public Type GetTypeOrNull(int id)
      {
         Type type;
         if (typeByTypeId.TryGetValue(id, out type))
            return type;
         else
            return null;
      }

      public Type GetTypeFromDescription(PofTypeDescription typeDescription)
      {
         return typeByDescription.GetOrAdd(typeDescription, CreateTypeFromDescription);
      }

      private Type CreateTypeFromDescription(PofTypeDescription desc)
      {
         if (!desc.HasGenericDefinition)
            return desc.First();
         else
         {
            return desc.First().MakeGenericType(desc.AfterFirst());
         }
      }
   }
}

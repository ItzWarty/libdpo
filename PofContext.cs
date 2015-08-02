using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ItzWarty;

namespace Dargon.PortableObjects
{
   public class PofContext : IPofContext
   {
      private readonly Dictionary<int, Type> typeByTypeId = new Dictionary<int, Type>();
      private readonly Dictionary<Type, int> typeIdByType = new Dictionary<Type, int>();
      private readonly Dictionary<int, Type> reservedTypeByTypeId = new Dictionary<int, Type>();
      private readonly Dictionary<Type, int> typeIdByReservedType = new Dictionary<Type, int>();
      private readonly Dictionary<Type, Func<IPortableObject>> activatorsByType = new Dictionary<Type, Func<IPortableObject>>();
      private readonly ConcurrentDictionary<PofTypeDescription, Type> typeByDescription = new ConcurrentDictionary<PofTypeDescription, Type>();

      public PofContext() {
         RegisterReservedPortableObjectTypes();
      }

      public void MergeContext(IPofContext pofContext) {
         var asPofContext = pofContext as PofContext;
         if (asPofContext != null) {
            MergeContext(asPofContext);
         } else {
            throw new InvalidOperationException("PofContext can only merge with other pof contexts");
         }
      }

      public void MergeContext(PofContext context)
      {
         foreach (var kvp in context.typeByTypeId) {
            var pofId = kvp.Key;
            if (pofId >= 0) {
               var pofType = kvp.Value;
               var pofActivator = context.activatorsByType[pofType];
               RegisterPortableObjectTypePrivate(pofId, pofType);
               SetActivator(pofType, pofActivator);
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
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_DATETIME, typeof(DateTime));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_BYTES, typeof(byte[]));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_PORTABLE_ARRAY, typeof(SpecialTypes.PortableArray<>));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_PORTABLE_MAP, typeof(SpecialTypes.PortableMap<,>));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_ARRAY, typeof(Array));
         RegisterReservedPortableObjectType((int)ReservedTypeId.TYPE_TIMESPAN, typeof(TimeSpan));
      }

      private void RegisterReservedPortableObjectType(int typeId, Type type)
      {
         reservedTypeByTypeId.Add(typeId, type);
         typeIdByReservedType.Add(type, typeId);
         if (!RegisterPortableObjectTypePrivate(typeId, type)) {
            throw new InvalidOperationException("Failed to register reserved portable object type!?");
         }
         SetActivator(type, () => {
            throw new InvalidOperationException("Attempted to activate a reserved object type - pofwriter/pofreader should handle these explicitly.");
         });
      }

      public void RegisterPortableObjectType(int typeId, Type type) {
         if (typeId < 0)
            throw new ArgumentOutOfRangeException("Negative TypeIDs are reserved for system use.");

         if (type.IsClass) {
            if (type.GetConstructors().None(ctor => ctor.GetParameters().None())) {
               throw new MissingMethodException("Type " + type.FullName + " does not provide default constructor for POF instantiation!");
            }
         }

         if (RegisterPortableObjectTypePrivate(typeId, type)) {
            SetActivator(type, () => (IPortableObject)Activator.CreateInstance(type));
         }
      }

      public void RegisterPortableObjectType<T>(int typeId, Func<T> ctor)
         where T : IPortableObject {
         if (typeId < 0)
            throw new ArgumentOutOfRangeException("Negative TypeIDs are reserved for system use.");

         if (RegisterPortableObjectTypePrivate(typeId, typeof(T))) {
            SetActivator(typeof(T), () => ctor());
         }
      }

      private bool RegisterPortableObjectTypePrivate(int typeId, Type type)
      {
         try {
            typeByTypeId.Add(typeId, type);
            typeIdByType.Add(type, typeId);
            return true;
         } catch (ArgumentException) {
            var existing = typeByTypeId[typeId];
            if (existing != type) {
               throw new DuplicatePofIdException(typeId, existing, type);
            } else {
               return false;
            }
         }
      }

      private void SetActivator(Type type, Func<IPortableObject> func) {
         activatorsByType.Add(type, func);
      }

      public IPortableObject CreateInstance(Type t) {
         Func<IPortableObject> ctor;
         if (activatorsByType.TryGetValue(t, out ctor)) {
            return ctor();
         } else {
            return (IPortableObject)Activator.CreateInstance(t);
         }
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

      public bool HasTypeId(int typeId) {
         return typeByTypeId.ContainsKey(typeId);
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

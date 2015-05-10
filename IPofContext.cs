using System;

namespace Dargon.PortableObjects {
   public interface IPofContext {
      void RegisterPortableObjectType(int typeId, Type type);
      bool IsInterfaceRegistered(Type t);
      bool IsReservedType(Type type);
      bool IsReservedTypeId(int typeId);
      int GetTypeIdByType(Type t);
      bool HasTypeId(int typeId);
      Type GetTypeOrNull(int id);
      Type GetTypeFromDescription(PofTypeDescription typeDescription);
      IPortableObject CreateInstance(Type t);
   }
}
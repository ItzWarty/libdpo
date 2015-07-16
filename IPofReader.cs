using System;
using System.Collections.Generic;

namespace Dargon.PortableObjects
{
   public interface IPofReader
   {
      IPofContext Context { get; }

      sbyte ReadS8(int slot);
      byte ReadU8(int slot);
      short ReadS16(int slot);
      ushort ReadU16(int slot);
      int ReadS32(int slot);
      uint ReadU32(int slot);
      long ReadS64(int slot);
      ulong ReadU64(int slot);
      float ReadFloat(int slot);
      double ReadDouble(int slot);
      char ReadChar(int slot);
      string ReadString(int slot);
      bool ReadBoolean(int slot);
      Guid ReadGuid(int slot);
      DateTime ReadDateTime(int slot);
      TimeSpan ReadTimeSpan(int slot);
      byte[] ReadBytes(int slot);
      object ReadObject(int slot);
      T ReadObject<T>(int slot);
      T ReadObjectTypeless<T>(int slot);
      T[] ReadArray<T>(int slot, bool elementsPolymorphic = false);
      TCollection ReadCollection<T, TCollection>(int slot, bool elementsPolymorphic = false) where TCollection : class, ICollection<T>, new();
      TCollection ReadCollection<T, TCollection>(int slot, TCollection collection, bool elementsPolymorphic = false) where TCollection : class, ICollection<T>;
      IDictionary<TKey, TValue> ReadMap<TKey, TValue>(int slot, bool keysPolymorphic = false, bool valuesPolymorphic = false, IDictionary<TKey, TValue> dict = null);
   }
}
using System;
using System.Collections.Generic;

namespace Dargon.PortableObjects
{
   public interface IPofWriter
   {
      IPofContext Context { get; }

      void WriteS8(int slot, sbyte value);
      void WriteU8(int slot, byte value);
      void WriteS16(int slot, short value);
      void WriteU16(int slot, ushort value);
      void WriteS32(int slot, int value);
      void WriteU32(int slot, uint value);
      void WriteS64(int slot, long value);
      void WriteU64(int slot, ulong value);
      void WriteFloat(int slot, float value);
      void WriteDouble(int slot, double value);
      void WriteChar(int slot, char value);
      void WriteString(int slot, string value);
      void WriteBoolean(int slot, bool value);
      void WriteGuid(int slot, Guid value);
      void WriteDateTime(int slot, DateTime dateTime);
      void WriteTimeSpan(int slot, TimeSpan timeSpan);
      void WriteType(int slot, Type type);
      void WriteBytes(int slot, byte[] data);
      void WriteBytes(int slot, byte[] data, int offset, int length);
      void WriteObject(int slot, object portableObject);
      void WriteObjectTypeless(int slot, object portableObject);
      void WriteCollection<T>(int slot, IEnumerable<T> array, bool elementsPolymorphic = false);
      void WriteMap<TKey, TValue>(int slot, IEnumerable<KeyValuePair<TKey, TValue>> value, bool keysPolymorphic = false, bool valuesPolymorphic = false);

      void AssignSlot(int slot, byte[] data);
      void AssignSlot(int slot, byte[] data, int offset, int length);
   }
}

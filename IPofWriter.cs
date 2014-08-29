using System.Collections.Generic;
using System.Data.SqlTypes;

namespace Dargon.PortableObjects
{
   public interface IPofWriter
   {
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
      void WriteObject<T>(int slot, T portableObject);
      void WriteArray<T>(int slot, T[] array, bool elementsCovariant = false);
      void WriteMap<TKey, TValue>(int slot, IDictionary<TKey, TValue> value, bool keysCovariant = false, bool valuesCovariant = false);
   }
}
using System.Linq.Expressions;
using System.Text;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItzWarty.Collections;

namespace Dargon.PortableObjects
{
   public unsafe class PofWriter : IPofWriter
   {
      private readonly IPofContext context;
      private readonly ISlotDestination destination;
      private static readonly byte[] DATA_BOOLEAN_TRUE = new byte[] { 1 };
      private static readonly byte[] DATA_BOOLEAN_FALSE = new byte[] { 0 };
      private static readonly Dictionary<Type, Action<BinaryWriter, object>> RESERVED_TYPE_WRITERS = new Dictionary<Type, Action<BinaryWriter, object>> {
         {typeof(sbyte), (writer, o) => writer.Write((sbyte)o)},
         {typeof(byte), (writer, o) => writer.Write((byte)o)},
         {typeof(short), (writer, o) => writer.Write((short)o)},
         {typeof(ushort), (writer, o) => writer.Write((ushort)o)},
         {typeof(int), (writer, o) => writer.Write((int)o)},
         {typeof(uint), (writer, o) => writer.Write((uint)o)},
         {typeof(long), (writer, o) => writer.Write((long)o)},
         {typeof(ulong), (writer, o) => writer.Write((ulong)o)},
         {typeof(char), (writer, o) => writer.Write((char)o)},
         {typeof(string), (writer, o) => writer.Write((string)o)},
         {typeof(bool), (writer, o) => writer.Write((bool)o)},
         {typeof(Guid), (writer, o) => writer.Write((Guid)o)}
      }; 

      public PofWriter(IPofContext context, ISlotDestination destination)
      {
         this.context = context;
         this.destination = destination;
      }

      public void WriteToSlots(IPortableObject portableObject)
      {
         portableObject.Serialize(this);
      }

      public void WriteS8(int slot, sbyte value) { destination.SetSlot(slot, new[] { *(byte*)&value }); }
      public void WriteU8(int slot, byte value) { destination.SetSlot(slot, new[] { value }); }
      public void WriteS16(int slot, short value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteU16(int slot, ushort value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteS32(int slot, int value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteU32(int slot, uint value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteS64(int slot, long value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteU64(int slot, ulong value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteFloat(int slot, float value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteDouble(int slot, double value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      public void WriteChar(int slot, char value) { destination.SetSlot(slot, BitConverter.GetBytes(value)); }
      
      public void WriteString(int slot, string value)
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               writer.WriteNullTerminatedString(value);
            }
            destination.SetSlot(slot, ms.ToArray());
         }
      }

      public void WriteBoolean(int slot, bool value) { destination.SetSlot(slot, value ? DATA_BOOLEAN_TRUE : DATA_BOOLEAN_FALSE); }
      public void WriteGuid(int slot, Guid value) { destination.SetSlot(slot, value.ToByteArray()); }

      public void WriteObject(int slot, object portableObject)
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               if (portableObject == null) {
                  WriteType(writer, typeof(void));
               } else {
                  WriteType(writer, portableObject.GetType());
                  WriteObjectWithoutTypeDescription(writer, portableObject);
               }
            }
            destination.SetSlot(slot, ms.ToArray());
         }
      }

      private void WriteObjectWithoutTypeDescription(BinaryWriter writer, object value)
      {
         if (context.IsReservedType(value.GetType())) {
            WriteReservedType(writer, value);
         } else {
            var slotDestination = new SlotDestination();
            var pofWriter = new PofWriter(context, slotDestination);
            pofWriter.WriteToSlots((IPortableObject)value);
            slotDestination.WriteToWriter(writer);
         }
      }

      private void WriteReservedType(BinaryWriter writer, object value) { RESERVED_TYPE_WRITERS[value.GetType()](writer, value); }

      public void WriteCollection<T>(int slot, IEnumerable<T> collection, bool elementsCovariant = false) 
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               writer.Write((int)collection.Count());
               WriteType(writer, typeof(T));

               foreach (var element in collection) {
                  if (elementsCovariant) {
                     WriteType(writer, element.GetType());
                  }
                  WriteObjectWithoutTypeDescription(writer, element);
               }
            }
            destination.SetSlot(slot, ms.ToArray());
         }
      }

      public void WriteMap<TKey, TValue>(int slot, IEnumerable<KeyValuePair<TKey, TValue>> dict, bool keysCovariant = false, bool valuesCovariant = false) 
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               writer.Write(dict.Count());
               WriteType(writer, typeof(TKey));
               WriteType(writer, typeof(TValue));

               foreach (var kvp in dict) {
                  var key = kvp.Key;
                  if (keysCovariant) {
                     WriteType(writer, key.GetType());
                  }
                  WriteObjectWithoutTypeDescription(writer, key);

                  var value = kvp.Value;
                  if (valuesCovariant) {
                     WriteType(writer, value.GetType());
                  }
                  WriteObjectWithoutTypeDescription(writer, value);
               }
            }
            destination.SetSlot(slot, ms.ToArray());
         }
      }

      private void WriteType(BinaryWriter writer, Type type)
      {
         WriteTypeDescription(writer, CreatePofTypeDescription(type));
      }

      private PofTypeDescription CreatePofTypeDescription(Type input)
      {
         var types = new List<Type>();
         var s = new Stack<Type>();
         s.Push(input);
         while (s.Any()) {
            var type = s.Pop();
            if (type.IsGenericType) {
               types.Add(type.GetGenericTypeDefinition());
               var genericArguments = type.GetGenericArguments();
               for (var i = genericArguments.Length - 1; i >= 0; i--) {
                  s.Push(genericArguments[i]);
               }
            } else {
               types.Add(type);
            }
         }
         return new PofTypeDescription(types.ToArray());
      }

      private void WriteTypeDescription(BinaryWriter writer, PofTypeDescription desc)
      {
         foreach (var type in desc.All()) {
            writer.Write((int)context.GetTypeIdByType(type));
         }
      }
   }
}
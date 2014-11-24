using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ItzWarty;

namespace Dargon.PortableObjects
{
   public unsafe class PofReader : IPofReader
   {
      private readonly IPofContext context;
      private readonly ISlotSource slots;

      private static readonly Dictionary<Type, Func<BinaryReader, object>> RESERVED_TYPE_READERS = new Dictionary<Type, Func<BinaryReader, object>> {
         {typeof(sbyte), (reader) => reader.ReadSByte()},
         {typeof(byte), (reader) => reader.ReadByte()},
         {typeof(short), (reader) => reader.ReadInt16()},
         {typeof(ushort), (reader) => reader.ReadUInt16()},
         {typeof(int), (reader) => reader.ReadInt32()},
         {typeof(uint), (reader) => reader.ReadUInt32()},
         {typeof(long), (reader) => reader.ReadInt64()},
         {typeof(ulong), (reader) => reader.ReadUInt64()},
         {typeof(char), (reader) => reader.ReadChar()},
         {typeof(string), (reader) => reader.ReadNullTerminatedString()},
         {typeof(bool), (reader) => reader.ReadByte() != 0 },
         {typeof(Guid), (reader) => reader.ReadGuid() }
      };

      public PofReader(IPofContext context, ISlotSource slots)
      {
         this.context = context;
         this.slots = slots;
      }

      public sbyte ReadS8(int slot)
      {
         var value = slots[slot][0];
         return *(sbyte*)&value;
      }

      public byte ReadU8(int slot) { return slots[slot][0]; }
      public short ReadS16(int slot) { return BitConverter.ToInt16(slots[slot], 0); }
      public ushort ReadU16(int slot) { return BitConverter.ToUInt16(slots[slot], 0); }
      public int ReadS32(int slot) { return BitConverter.ToInt32(slots[slot], 0); }
      public uint ReadU32(int slot) { return BitConverter.ToUInt32(slots[slot], 0); }
      public long ReadS64(int slot) { return BitConverter.ToInt64(slots[slot], 0); }
      public ulong ReadU64(int slot) { return BitConverter.ToUInt64(slots[slot], 0); }
      public float ReadFloat(int slot) { return BitConverter.ToSingle(slots[slot], 0); }
      public double ReadDouble(int slot) { return BitConverter.ToDouble(slots[slot], 0); }
      public char ReadChar(int slot) { return BitConverter.ToChar(slots[slot], 0); }

      public string ReadString(int slot)
      {
         using (var reader = GetSlotBinaryReader(slot)) {
            return reader.ReadNullTerminatedString();
         }
      }

      public bool ReadBoolean(int slot) { return slots[slot][0] != 0; }
      public Guid ReadGuid(int slot) { return new Guid(slots[slot]); }

      public T ReadObject<T>(int slot) 
      {
         using (var reader = GetSlotBinaryReader(slot)) {
            return ReadObject<T>(reader);
         }
      }

      private T ReadObject<T>(BinaryReader reader) 
      {
         var type = ParseType(reader);
         return ReadObjectHelper<T>(type, reader);
      }

      private T ReadObjectHelper<T>(Type type, BinaryReader reader)
      {
         if (context.IsReservedType(type)) {
            return (T)ReadReservedType(type, reader);
         } else {
            var instance = (IPortableObject)Activator.CreateInstance(type);
            instance.Deserialize(new PofReader(context, SlotSourceFactory.CreateFromBinaryReader(reader)));
            return (T)instance;
         }
      }

      private object ReadReservedType(Type type, BinaryReader reader) { return RESERVED_TYPE_READERS[type](reader); }

      public T[] ReadArray<T>(int slot, bool elementsCovariant = false)
      {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
         {
            int length = reader.ReadInt32();
            var type = ParseType(reader);
            Trace.Assert(typeof(T).IsAssignableFrom(type));

            if (elementsCovariant)
               return Util.Generate(length, i => ReadObject<T>(reader));
            else
               return Util.Generate(length, i => ReadObjectHelper<T>(type, reader));
         }
      }

      public TCollection ReadCollection<T, TCollection>(int slot, bool elementsCovariant = false) where TCollection : class, ICollection<T>, new() {
         return ReadCollection<T, TCollection>(slot, new TCollection(), elementsCovariant);
      }

      public TCollection ReadCollection<T, TCollection>(int slot, TCollection collection, bool elementsCovariant = false) where TCollection : class, ICollection<T> {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true)) {
            int length = reader.ReadInt32();
            var type = ParseType(reader);
            Trace.Assert(typeof(T).IsAssignableFrom(type));

            if (elementsCovariant) {
               for (var i = 0; i < length; i++) {
                  collection.Add(ReadObject<T>(reader));
               }
            } else {
               for (var i = 0; i < length; i++) {
                  collection.Add(ReadObjectHelper<T>(type, reader));
               }
            }
         }
         return collection;
      }

      public IDictionary<TKey, TValue> ReadMap<TKey, TValue>(int slot, bool keysCovariant = false, bool valuesCovariant = false, IDictionary<TKey, TValue> dict = null)
      {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
         {
            int kvpCount = reader.ReadInt32();
            var keyType = ParseType(reader);
            var valueType = ParseType(reader);

            Trace.Assert(typeof(TKey).IsAssignableFrom(keyType));
            Trace.Assert(typeof(TValue).IsAssignableFrom(valueType));

            Console.WriteLine("ReadMap has key " + keyType + " vlaue " + valueType);

            if (dict == null)
               dict = new Dictionary<TKey, TValue>();

            for (var i = 0; i < kvpCount; i++) {
               TKey key = keysCovariant ? ReadObject<TKey>(reader) : ReadObjectHelper<TKey>(keyType, reader);
               TValue value = valuesCovariant ? ReadObject<TValue>(reader) : ReadObjectHelper<TValue>(valueType, reader);

               Console.WriteLine("Have key " + key + " value " + value);
               dict.Add(key, value);
            }

            return dict;
         }
      }

      private BinaryReader GetSlotBinaryReader(int slot)
      {
         return new BinaryReader(CreateSlotMemoryStream(slot));
      }

      private MemoryStream CreateSlotMemoryStream(int slot)
      {
         return new MemoryStream(slots[slot]);
      }

      private Type ParseType(BinaryReader reader)
      {
         var typeDescription = ParseTypeDescription(reader);
         return context.GetTypeFromDescription(typeDescription);
      }

      private PofTypeDescription ParseTypeDescription(BinaryReader reader)
      {
         int typeId = reader.ReadInt32();
         var type = context.GetTypeOrNull(typeId);
         if (type == null)
            throw new TypeIdNotFoundException(typeId);
         if (!type.IsGenericTypeDefinition)
            return new PofTypeDescription(new[] { type });
         else
         {
            var genericArgumentSpecifications = type.GetGenericArguments();
            var genericArguments = Util.Generate(
               genericArgumentSpecifications.Length,
               i => ParseType(reader));
            return new PofTypeDescription(Util.Concat<Type>(type, genericArguments));
         }
      }
   }
}
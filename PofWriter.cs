using ItzWarty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dargon.PortableObjects {
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
         {typeof(float), (writer, o) => writer.Write((float)o)},
         {typeof(double), (writer, o) => writer.Write((double)o)},
         {typeof(char), (writer, o) => writer.Write((char)o)},
         {typeof(string), (writer, o) => writer.WriteNullTerminatedString((string)o)},
         {typeof(bool), (writer, o) => writer.Write((bool)o)},
         {typeof(Guid), (writer, o) => writer.Write((Guid)o)},
         {typeof(DateTime), (writer, o) => writer.Write(BitConverter.GetBytes(((DateTime)o).ToUniversalTime().ToBinary()))},
         {typeof(TimeSpan), (writer, o) => writer.Write((long)(((TimeSpan)o).Ticks)) },
      }; 

      public PofWriter(IPofContext context, ISlotDestination destination)
      {
         this.context = context;
         this.destination = destination;
      }

      public IPofContext Context { get { return context; } }

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

      public void WriteString(int slot, string value) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               writer.WriteNullTerminatedString(value);
            }
            destination.SetSlot(slot, ms);
         }
      }

      public void WriteBoolean(int slot, bool value) { destination.SetSlot(slot, value ? DATA_BOOLEAN_TRUE : DATA_BOOLEAN_FALSE); }
      public void WriteGuid(int slot, Guid value) { destination.SetSlot(slot, value.ToByteArray()); }
      public void WriteDateTime(int slot, DateTime value) { destination.SetSlot(slot, BitConverter.GetBytes(value.ToUniversalTime().ToBinary())); }
      public void WriteTimeSpan(int slot, TimeSpan timeSpan) { destination.SetSlot(slot, BitConverter.GetBytes((long)timeSpan.Ticks)); }

      public void WriteBytes(int slot, byte[] data) { destination.SetSlot(slot, data.ToArray()); }

      public void WriteBytes(int slot, byte[] data, int offset, int length) {
         var buffer = new byte[length];
         Buffer.BlockCopy(data, offset, buffer, 0, length);
         destination.SetSlot(slot, buffer);
      }

      public void AssignSlot(int slot, byte[] data) {
         AssignSlot(slot, data, 0, data.Length);
      }

      public void AssignSlot(int slot, byte[] data, int offset, int length) {
         destination.SetSlot(slot, data, offset, length);
      }

      public void WriteObject(int slot, object portableObject) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               WriteObjectInternal(writer, portableObject, true);
            }
            destination.SetSlot(slot, ms);
         }
      }

      public void WriteObjectTypeless(int slot, object portableObject) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               WriteObjectInternal(writer, portableObject, false);
            }
            destination.SetSlot(slot, ms);
         }
      }

      public void WriteCollection<T>(int slot, IEnumerable<T> collection, bool elementsPolymorphic = false) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               WriteCollectionInternal(writer, collection, elementsPolymorphic, true);
            }
            destination.SetSlot(slot, ms);
         }
      }

      private void WriteCollectionInternal<T>(BinaryWriter writer, IEnumerable<T> collection, bool elementsPolymorphic, bool writeType) {
         var portableArray = SpecialTypes.PortableArray<T>.Create(collection.ToArray(), elementsPolymorphic);
         WriteObjectInternal(writer, portableArray, writeType);
      }

      public void WriteMap<TKey, TValue>(int slot, IEnumerable<KeyValuePair<TKey, TValue>> dict, bool keysPolymorphic = false, bool valuesPolymorphic = false) 
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               WriteMapInternal(writer, dict, keysPolymorphic, valuesPolymorphic);
            }
            destination.SetSlot(slot, ms);
         }
      }

      private void WriteMapInternal<TKey, TValue>(BinaryWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> dict, bool keysPolymorphic, bool valuesPolymorphic) {
         if (dict == null) {
            WriteObjectInternal(writer, null, true);
         } else {
            var portableMap = SpecialTypes.PortableMap<TKey, TValue>.Create(dict.ToArray(), keysPolymorphic, valuesPolymorphic);
            WriteObjectInternal(writer, portableMap, true);
         }
      }

      private void WriteObjectInternal(BinaryWriter writer, object portableObject, bool writeType) {
         if (portableObject == null) {
            if (writeType) {
                WriteType(writer, typeof(void));
            }
         } else if (portableObject is IEnumerable && !(portableObject is string)) {
            var portableObjectType = portableObject.GetType();
            var elementType = ReflectionHelpers.GetIEnumerableElementType(portableObjectType);
            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
               var dictType = ReflectionHelpers.FindInterfaceByGenericDefinition(portableObjectType, typeof(IDictionary<,>)) ??
                              ReflectionHelpers.FindInterfaceByGenericDefinition(portableObjectType, typeof(IReadOnlyDictionary<,>));
               if (dictType == null) {
                  DispatchToWriteCollectionInternal(writer, portableObject, elementType, writeType);
               } else {
                  DispatchToWriteMapInternal(writer, portableObject, elementType);
               }
            } else {
               DispatchToWriteCollectionInternal(writer, portableObject, elementType, writeType);
            }
         } else {
            if (writeType) {
               WriteType(writer, portableObject.GetType());
            }
            WriteObjectWithoutTypeDescription(writer, portableObject);
         }
      }

      private void DispatchToWriteCollectionInternal(BinaryWriter writer, object portableObject, Type elementType, bool writeType) {
         var helper = typeof(PofWriter).GetMethod("DispatchToWriteCollectionInternalHelper", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(elementType);
         helper.Invoke(this, new object[] { writer, portableObject, writeType });
      }

      private void DispatchToWriteCollectionInternalHelper<TElementType>(BinaryWriter writer, IEnumerable<TElementType> portableObject, bool writeType) {
         var collection = portableObject.ToArray();
         var isPolymorphic = !typeof(TElementType).IsValueType && collection.Any(x => x == null || x.GetType() != typeof(TElementType));
         WriteCollectionInternal(writer, collection, isPolymorphic, writeType);
      }

      private void DispatchToWriteMapInternal(BinaryWriter writer, object portableObject, Type kvpType) {
         var kvpArguments = kvpType.GetGenericArguments();
         var keyType = kvpArguments[0];
         var valueType = kvpArguments[1];
         var helper = typeof(PofWriter).GetMethod("DispatchToWriteMapInternalHelper", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(keyType, valueType);
         helper.Invoke(this, new object[] { writer, portableObject });
      }

      private void DispatchToWriteMapInternalHelper<TKey, TValue>(BinaryWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> portableObject) {
         var collection = portableObject.ToArray();
         var keys = collection.Select(x => x.Key);
         var values = collection.Select(x => x.Value);
         var keysPolymorphic = !typeof(TKey).IsValueType && keys.Any(x => x == null || x.GetType() != typeof(TKey));
         var valuesPolymorphic = !typeof(TValue).IsValueType && values.Any(x => x == null || x.GetType() != typeof(TValue));
         WriteMapInternal(writer, collection, keysPolymorphic, valuesPolymorphic);
      }

      private void WriteObjectWithoutTypeDescription(BinaryWriter writer, object value) {
         if (context.IsReservedType(value.GetType())) {
            WriteReservedType(writer, value);
         } else {
            var slotDestination = new SlotDestination();
            var pofWriter = new PofWriter(context, slotDestination);
            ((IPortableObject)value).Serialize(pofWriter);
            slotDestination.WriteToWriter(writer);
         }
      }

      private void WriteReservedType(BinaryWriter writer, object value) { RESERVED_TYPE_WRITERS[value.GetType()](writer, value); }

      private void WriteType(BinaryWriter writer, Type type) {
         WriteTypeDescription(writer, CreatePofTypeDescription(type));
      }

      private PofTypeDescription CreatePofTypeDescription(Type input) {
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
            } else if (type.IsArray) {
               types.Add(typeof(Array));
               s.Push(type.GetElementType());
            } else {
               types.Add(type);
            }
         }
         return new PofTypeDescription(types.ToArray());
      }

      private void WriteTypeDescription(BinaryWriter writer, PofTypeDescription desc) {
         foreach (var type in desc.All()) {
            writer.Write((int)context.GetTypeIdByType(type));
         }
      }

      private Type ConvertSpecialType(Type t) {
         if (t.IsArray) {
            Trace.Assert(t.GetArrayRank() == 1);
            var elementType = ConvertSpecialType(t.GetElementType());
            return typeof(SpecialTypes.PortableArray<>).MakeGenericType(elementType);
         } else {
            return t;
         }
      }
   }
}
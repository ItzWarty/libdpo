using System;
using System.IO;
using System.Linq.Expressions;
using ItzWarty;
using ItzWarty.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dargon.PortableObjects.Tests
{
   [TestClass]
   public unsafe class PofWriterTest : MockitoLike
   {
      private const int SLOT_INDEX = 123;

      private PofWriter testObj;
      
      [Mock] private readonly IPofContext context = null;
      [Mock] private readonly ISlotDestination slotDestination = null;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();
         testObj = new PofWriter(context, slotDestination);
      }

      [TestMethod]
      public void TestWriteS8()
      {
         sbyte value = -123;
         testObj.WriteS8(SLOT_INDEX, value);
         var data = new byte[] { *(byte*)&value };
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteU8()
      {
         const byte value = 123;
         testObj.WriteU8(SLOT_INDEX, value);
         var data = new byte[] { value };
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteS16()
      {
         const short value = short.MinValue;
         testObj.WriteS16(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteU16()
      {
         const ushort value = ushort.MaxValue;
         testObj.WriteU16(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteS32()
      {
         const int value = int.MinValue;
         testObj.WriteS32(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteU32()
      {
         const uint value = uint.MaxValue;
         testObj.WriteU32(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteS64()
      {
         const long value = long.MinValue;
         testObj.WriteS64(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteU64()
      {
         const ulong value = ulong.MaxValue;
         testObj.WriteU64(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteFloat()
      {
         const float value = 13.37f;
         testObj.WriteFloat(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteDouble()
      {
         const double value = Math.PI;
         testObj.WriteDouble(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteChar()
      {
         const char value = 'a';
         testObj.WriteChar(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }

      [TestMethod]
      public void TestWriteString()
      {
         const string value = "There is no spoon!";
         testObj.WriteString(SLOT_INDEX, value);
         byte[] data;
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               writer.WriteNullTerminatedString(value);
            }
            data = ms.ToArray();
         }
         verify(() => slotDestination.SetSlot(SLOT_INDEX, data));
      }
   }
}

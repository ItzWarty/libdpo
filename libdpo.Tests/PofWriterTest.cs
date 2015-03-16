using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ItzWarty;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests
{
   public unsafe class PofWriterTest : NMockitoInstance
   {
      private const int SLOT_INDEX = 123;

      private PofWriter testObj;
      
      [Mock] private readonly IPofContext context = null;
      [Mock] private readonly ISlotDestination slotDestination = null;

      public PofWriterTest()
      {
         testObj = new PofWriter(context, slotDestination);
      }

      [Fact]
      public void TestWriteS8()
      {
         sbyte value = -123;
         testObj.WriteS8(SLOT_INDEX, value);
         var data = new byte[] { *(byte*)&value };
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteU8()
      {
         const byte value = 123;
         testObj.WriteU8(SLOT_INDEX, value);
         var data = new byte[] { value };
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteS16()
      {
         const short value = short.MinValue;
         testObj.WriteS16(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteU16()
      {
         const ushort value = ushort.MaxValue;
         testObj.WriteU16(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteS32()
      {
         const int value = int.MinValue;
         testObj.WriteS32(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteU32()
      {
         const uint value = uint.MaxValue;
         testObj.WriteU32(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteS64()
      {
         const long value = long.MinValue;
         testObj.WriteS64(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteU64()
      {
         const ulong value = ulong.MaxValue;
         testObj.WriteU64(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteFloat()
      {
         const float value = 13.37f;
         testObj.WriteFloat(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteDouble()
      {
         const double value = Math.PI;
         testObj.WriteDouble(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteChar()
      {
         const char value = 'a';
         testObj.WriteChar(SLOT_INDEX, value);
         var data = BitConverter.GetBytes(value);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(data));
      }

      [Fact]
      public void TestWriteString()
      {
         const string value = "There is no spoon!";
         testObj.WriteString(SLOT_INDEX, value);
         byte[] data;
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               writer.WriteNullTerminatedString(value);
            }
            data = ms.ToArray();
         }

         var streamCaptor = new ArgumentCaptor<MemoryStream>();
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), streamCaptor.GetParameter());
         VerifyNoMoreInteractions();
         AssertTrue(Encoding.UTF8.GetString(streamCaptor.Value.ToArray()).Equals(value + "\0"));
      }

      [Fact]
      public void TestGuid() {
         var guid = Guid.NewGuid();
         testObj.WriteGuid(SLOT_INDEX, guid);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), EqSequence(guid.ToByteArray()));
      }

      [Fact]
      public void TestWriteBytes() {
         var placedArrayCaptor = new ArgumentCaptor<byte[]>();
         var random = new Random(0);
         var data = new byte[100].With(random.NextBytes);
         testObj.WriteBytes(SLOT_INDEX, data);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), placedArrayCaptor.GetParameter());
         VerifyNoMoreInteractions();
         AssertTrue(placedArrayCaptor.Value != data);
         AssertTrue(placedArrayCaptor.Value.SequenceEqual(data));
      }

      [Fact]
      public void TestWriteBytesOffsetLength() {
         var placedArrayCaptor = new ArgumentCaptor<byte[]>();
         var random = new Random(0);
         var data = new byte[100].With(random.NextBytes);
         testObj.WriteBytes(SLOT_INDEX, data, 10, 80);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), placedArrayCaptor.GetParameter());
         VerifyNoMoreInteractions();
         AssertTrue(placedArrayCaptor.Value != data);
         AssertTrue(placedArrayCaptor.Value.SequenceEqual(data.Skip(10).Take(80)));
      }

      [Fact]
      public void TestAssignSlot() {
         var placedArrayCaptor = new ArgumentCaptor<byte[]>();
         var random = new Random(0);
         var data = new byte[100].With(random.NextBytes);
         testObj.AssignSlot(SLOT_INDEX, data);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), placedArrayCaptor.GetParameter(), Eq(0), Eq(100));
         VerifyNoMoreInteractions();
         AssertTrue(placedArrayCaptor.Value == data);
      }

      [Fact]
      public void TestAssignSlotOffsetLength() {
         var placedArrayCaptor = new ArgumentCaptor<byte[]>();
         var random = new Random(0);
         var data = new byte[100].With(random.NextBytes);
         testObj.AssignSlot(SLOT_INDEX, data, 10, 80);
         Verify(slotDestination).SetSlot(Eq(SLOT_INDEX), placedArrayCaptor.GetParameter(), Eq(10), Eq(80));
         VerifyNoMoreInteractions();
         AssertTrue(placedArrayCaptor.Value == data);
      }
   }
}

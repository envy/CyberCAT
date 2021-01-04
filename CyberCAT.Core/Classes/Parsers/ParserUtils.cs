﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CyberCAT.Core.Classes.Interfaces;

namespace CyberCAT.Core.Classes.Parsers
{
    class ParserUtils
    {
        public static string ReadString(BinaryReader reader, out Flags flags)
        {
            flags = new Flags(reader.ReadByte());
            return reader.ReadString(flags.Length);
        }

        public static string ReadString(BinaryReader reader)
        {
            var flags = new Flags(reader.ReadByte());
            return reader.ReadString(flags.Length);
        }

        public static int WriteString(BinaryWriter writer, string s)
        {
            writer.Write((byte)(s.Length + 128));
            writer.Write(Encoding.ASCII.GetBytes(s));
            return 1 + Encoding.ASCII.GetBytes(s).Length;
        }

        public static void ParseChildren(IEnumerable<NodeEntry> children, BinaryReader reader, List<INodeParser> parsers)
        {
            foreach (var node in children)
            {
                reader.BaseStream.Position = node.Offset;
                var parser = parsers.FirstOrDefault(p => p.ParsableNodeName==node.Name);
                if (parser != null)
                {
                    node.Value = parser.Read(node, reader, parsers);
                }
                else
                {
                    var fallback = new DefaultParser();
                    node.Value = fallback.Read(node, reader, parsers);
                }
            }
        }

        public static void AdjustNodeOffsetDuringWriting(NodeEntry currentNode, int writtenSize, int parentHeaderSize)
        {
            currentNode.Size = writtenSize;
            var prevNode = currentNode.GetPreviousNode();

            if (prevNode == null)
            {
                // Check if we have a parent
                var parent = currentNode.GetParent();
                if (parent != null)
                {
                    // The parent has not adjusted their offset yet, do that now!
                    AdjustParentOffset(parent);
                    currentNode.Offset = parent.Offset + parentHeaderSize;
                }
            }
            else
            {
                // There is a node before us. Adjust our offset based on their size.
                int adjust = parentHeaderSize;
                if (adjust == 0 && prevNode.TrailingSize > 0)
                {
                    adjust = prevNode.TrailingSize;
                }
                currentNode.Offset = prevNode.Offset + prevNode.Size + adjust;
            }
        }

        private static void AdjustParentOffset(NodeEntry parent)
        {
            var parentPrevNode = parent.GetPreviousNode();
            if (parentPrevNode == null)
            {
                var parentParent = parent.GetParent();
                if (parentParent != null)
                {
                    AdjustParentOffset(parentParent);
                }
            }
            else
            {
                parent.Offset = parentPrevNode.Offset + parentPrevNode.Size + parentPrevNode.TrailingSize;
            }
        }

        // a 1:1 copy of PixelRicks cpp implementation, could probably be better...
        public static long ReadPackedLong(BinaryReader reader)
        {
            var a = reader.ReadSByte();
            long ret = a & 0x3F;
            var sign = (a & 0x80) == 0x00;

            if ((a & 0x40) == 0x40)
            {
                a = reader.ReadSByte();
                ret |= (a & 0x7F) << 6;

                if (a < 0)
                {
                    a = reader.ReadSByte();
                    ret |= (a & 0x7F) << 13;

                    if (a < 0)
                    {
                        a = reader.ReadSByte();
                        ret |= (a & 0x7F) << 20;

                        if (a < 0)
                        {
                            a = reader.ReadSByte();
                            ret |= (a & 0x7F) << 27;
                        }
                    }
                }
            }

            return sign ? ret : -ret;
        }

        public static void WritePackedLong(BinaryWriter writer, long value)
        {
            var packed = new byte[5];
            var cnt = 1;
            var tmp = Math.Abs(value);
            if (value < 0)
                packed[0] |= 0x80;
            packed[0] |= (byte) (tmp & 0x3F);
            tmp >>= 6;
            if (tmp != 0)
            {
                packed[0] |= 0x40;
                cnt++;
                packed[1] |= (byte)(tmp & 0x7F);
                tmp >>= 7;
                if (tmp != 0)
                {
                    packed[1] |= 0x80;
                    cnt++;
                    packed[2] |= (byte)(tmp & 0x7F);
                    tmp >>= 7;
                    if (tmp != 0)
                    {
                        packed[2] |= 0x80;
                        cnt++;
                        packed[3] |= (byte)(tmp & 0x7F);
                        tmp >>= 7;
                        if (tmp != 0)
                        {
                            packed[3] |= 0x80;
                            cnt++;
                            packed[4] |= (byte)(tmp & 0x7F);
                            tmp >>= 7;
                        }
                    }
                }
            }

            writer.Write(packed, 0, cnt);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using UnityEngine;

namespace PrefabAPI
{
    public static class AssetBundleTools
    {
        public static EditorClassIDType GetClassFromType<T>() where T : EditorObject
        {
            return GetClassFromType(typeof(T));
        }

        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach(Transform c in transform)
            {
                children.Add(c);
            }
            return children;
        }

        public static List<GameObject> GetChildObjects(this Transform transform)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform c in transform)
            {
                children.Add(c.gameObject);
            }
            return children;
        }

        public static string ToStringBetter<T>(this IEnumerable<T> enumerable)
        {
            string s = "{ ";
            bool firstTime = true;
            foreach (T val in enumerable)
            {
                if (!firstTime)
                {
                    s += ", ";
                }
                s += val;
                firstTime = false;
            }
            s += " }";
            return s;
        }

        public static EditorClassIDType GetClassFromType(Type t)
        {
            if (t == typeof(EditorAssetBundle))
                return EditorClassIDType.AssetBundle;
            else if (t == typeof(EditorGameObject))
                return EditorClassIDType.GameObject;
            else if (t == typeof(EditorTextAsset))
                return EditorClassIDType.TextAsset;
            else if (t == typeof(EditorTransform))
                return EditorClassIDType.Transform;
            return EditorClassIDType.Object;
        }

        public static List<FieldInfo> GetFieldsOfType<T>(this object anything, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            Type tt = typeof(T);
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (i.FieldType == tt)
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            Type tt = type;
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (i.FieldType == tt)
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, IEnumerable<Type> types, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (types.Contains(i.FieldType))
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, string typeName, TypeNameComprareType comprareType, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (comprareType == TypeNameComprareType.Contains ? i.FieldType.ToString().Contains(typeName) : (i.FieldType.ToString() == typeName))
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static byte[] ToBytes(this short val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this int val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this long val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this ushort val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this uint val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this ulong val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this float val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this double val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this bool val)
        {
            return new byte[] { (byte)(val ? 1 : 0) };
        }

        public static byte[] ToBytesLittle(this short val)
        {
            byte[] buff = new byte[2];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            return buff;
        }

        public static byte[] ToBytesLittle(this int val)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            buff[2] = (byte)(val >> 16);
            buff[3] = (byte)(val >> 24);
            return buff;
        }

        public static byte[] ToBytesLittle(this long val)
        {
            byte[] buff = new byte[8];
            int i = 0;
            int num = 0;
            while (i < 8)
            {
                buff[i] = (byte)(val >> num);
                i++;
                num += 8;
            }
            return buff;
        }

        public static byte[] ToBytesLittle(this ushort val)
        {
            byte[] buff = new byte[2];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            return buff;
        }

        public static byte[] ToBytesLittle(this uint val)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            buff[2] = (byte)(val >> 16);
            buff[3] = (byte)(val >> 24);
            return buff;
        }

        public static byte[] ToBytesLittle(this ulong val)
        {
            byte[] buff = new byte[8];
            int i = 0;
            int num = 0;
            while (i < 8)
            {
                buff[i] = (byte)(val >> num);
                i++;
                num += 8;
            }
            return buff;
        }

        public static byte[] ToBytesLittle(this float val)
        {
            using (BinaryWriter stream = new BinaryWriter(new MemoryStream()))
            {
                stream.Write(val);
                return stream.BaseStream.ReadToEnd();
            }
        }

        public static byte[] ToBytesLittle(this double val)
        {
            using (BinaryWriter stream = new BinaryWriter(new MemoryStream()))
            {
                stream.Write(val);
                return stream.BaseStream.ReadToEnd();
            }
        }

        public static byte[] ToBytesLittle(this bool val)
        {
            return new byte[] { (byte)(val ? 1 : 0) };
        }


        public static byte[] ToBytes(this string val)
        {
            return Encoding.UTF8.GetBytes(val);
        }

        public static byte[] ToBytesPlusNull(this string val)
        {
            return val.ToBytes().Concat(new byte[] { 0 }).ToArray();
        }

        public static byte[] ToBytes(this SerializedType type, bool enableTypeTree)
        {
            List<byte> result = new List<byte>();
            result.AddRange(type.classID.ToBytesLittle());
            result.AddRange(type.m_IsStrippedType.ToBytesLittle());
            result.AddRange(type.m_ScriptTypeIndex.ToBytesLittle());
            if (type.classID == 114)
            {
                result.AddRange(type.m_ScriptID); //Hash128
            }
            result.AddRange(type.m_OldTypeHash); //Hash128
            if (enableTypeTree)
            {
                result.AddRange(type.m_Nodes.ToBytes(type.stringBuffer));
            }
            return result.ToArray();
        }


        public static List<T2> Convert<T, T2>(this List<T> self, Func<T, T2> convertor)
        {
            List<T2> result = new List<T2>();
            foreach (T element in self)
            {
                result.Add(convertor(element));
            }
            return result;
        }

        public static byte[] ToBytes(this List<TypeTreeNode> nodes, byte[] stringBuffer)
        {
            List<byte> result = new List<byte>();
            result.AddRange(nodes.Count.ToBytesLittle());
            result.AddRange(stringBuffer.Length.ToBytesLittle());
            foreach (TypeTreeNode typeTreeNode in nodes)
            {
                result.AddRange(((ushort)typeTreeNode.m_Version).ToBytesLittle());
                result.Add((byte)typeTreeNode.m_Level);
                result.Add((byte)typeTreeNode.m_IsArray);
                result.AddRange(typeTreeNode.m_TypeStrOffset.ToBytesLittle());
                result.AddRange(typeTreeNode.m_NameStrOffset.ToBytesLittle());
                result.AddRange(typeTreeNode.m_ByteSize.ToBytesLittle());
                result.AddRange(typeTreeNode.m_Index.ToBytesLittle());
                result.AddRange(typeTreeNode.m_MetaFlag.ToBytesLittle());
            }

            result.AddRange(stringBuffer);

            return result.ToArray();
        }

        public enum TypeNameComprareType
        {
            Equals,
            Contains
        }
    }
}

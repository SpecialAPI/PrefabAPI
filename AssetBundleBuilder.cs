﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Text;
using LZ4;

namespace PrefabAPI
{
    public static class AssetBundleBuilder
    {
        public static byte[] BuildBundleFile(string bundleName)
        {
            MemoryStream stream = new MemoryStream();
            EndianBinaryWriter writer = new EndianBinaryWriter(stream);
            byte[] bytes = new byte[0];
            try
            {
                //write header
                writer.WriteStringToNull("UnityFS"); //signature
                writer.Write(6u); //version
                writer.WriteStringToNull("5.x.x"); //unity version
                writer.WriteStringToNull("2017.4.40f1"); //unity revision

                //write other stuff
                List<byte> bytesToLZMACompress = new List<byte>();

                using (EndianBinaryWriter subwriter = new EndianBinaryWriter(new MemoryStream()))
                {
                    //from now on, writing method is different
                    //like this: subwriter.write(stuff)

                    //assets header
                    subwriter.Write(0u); //metadata size..?
                    long fileSizePosition = subwriter.Position;
                    subwriter.Write(0u); //filesize will be added later
                    subwriter.Write(17u); //version
                    subwriter.Write(4096u); //data offset..?
                    subwriter.Write(new byte[] { 0, 0, 0, 0 }); //Endianess and Reserved

                    subwriter.endian = EndianType.LittleEndian;

                    //assets metadata
                    subwriter.Write("2017.4.40f1".ToBytesPlusNull()); //unity version
                    subwriter.Write((int)EditorBuildTarget.StandaloneWindows64); //target platform
                    subwriter.Write(true); //enable type tree

                    //assets types
                    List<EditorClassIDType> types = new List<EditorClassIDType>
                    {
                        EditorClassIDType.GameObject,
                        EditorClassIDType.AssetBundle,
                        EditorClassIDType.Transform
                    };
                    subwriter.Write(types.Count); //type count
                    foreach (EditorClassIDType type in types)
                    {
                        subwriter.Write(SerializedTypeStorage.GetTypeBytes(type));
                    }

                    List<EditorObject> objects = new List<EditorObject>
                    {
                        new EditorGameObject()
                        {
                            m_Components = new PPtr<EditorComponent>[]
                            {
                                new EditorPPtr<EditorComponent>(new EditorTransform()
                                {
                                    m_LocalRotation = new Quaternion(0f, 0f, 0f, 0f),
                                    m_LocalPosition = new Vector3(0f, 0f, 0f),
                                    m_LocalScale = new Vector3(1f, 1f, 1f),
                                    m_Children = new PPtr<EditorTransform>[0],
                                    m_Father = PPtr<EditorTransform>.Empty
                                })
                            },
                            m_Name = "Object",
                            m_Layer = 0
                        }
                    };
                    void Process(object pptr, object fieldInfoOrArray, out bool didProcess, object obj = null, int? arrayIndex = null)
                    {
                        didProcess = false;
                        if (pptr != null)
                        {
                            if (pptr is Array)
                            {
                                for (int i = 0; i < (pptr as Array).Length; i++)
                                {
                                    object pptr2 = (pptr as Array).GetValue(i);
                                    Process(pptr2, pptr as Array, out _, null, i);
                                }
                            }
                            else
                            {
                                MethodInfo method = pptr.GetType().GetMethod("PostProcess", new Type[] { typeof(List<EditorObject>) });
                                if (method != null)
                                {
                                    method.Invoke(pptr, new object[] { objects });
                                }
                                bool fieldInfoOrArrayIsFieldInfo = false;
                                bool fieldInfoOrArrayIsArray = false;
                                if (fieldInfoOrArray != null && ((fieldInfoOrArrayIsFieldInfo = fieldInfoOrArray is FieldInfo && obj != null) || (fieldInfoOrArrayIsArray = fieldInfoOrArray is Array && arrayIndex != null && arrayIndex >= 0 &&
                                    arrayIndex < (fieldInfoOrArray as Array).Length)))
                                {
                                    MethodInfo method2 = pptr.GetType().GetMethod("ToPPtr");
                                    if (method2 != null)
                                    {
                                        object value = method2.Invoke(pptr, new object[] { objects });
                                        if (fieldInfoOrArrayIsFieldInfo)
                                        {
                                            (fieldInfoOrArray as FieldInfo).SetValue(obj, value);
                                        }
                                        else if (fieldInfoOrArrayIsArray)
                                        {
                                            (fieldInfoOrArray as Array).SetValue(value, arrayIndex.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    while (true)
                    {
                        bool shouldBreak = true;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            EditorObject obj = objects[i];
                            if (obj is EditorGameObject)
                            {
                                for (int j = 0; j < (obj as EditorGameObject).m_Components.Length; j++)
                                {
                                    PPtr<EditorComponent> computer = (obj as EditorGameObject).m_Components[j];
                                    if (computer is EditorPPtr<EditorComponent>)
                                    {
                                        (computer as EditorPPtr<EditorComponent>).PostProcess(objects);
                                        ((computer as EditorPPtr<EditorComponent>).obj as EditorComponent).m_GameObject = new EditorPPtr<EditorGameObject>(obj as EditorGameObject).ToPPtr(objects, 0);
                                        (obj as EditorGameObject).m_Components[j] = (computer as EditorPPtr<EditorComponent>).ToPPtr(objects);
                                    }
                                }
                            }
                            else
                            {
                                foreach (FieldInfo info in obj.GetFieldsOfType("PPtr", AssetBundleTools.TypeNameComprareType.Contains))
                                {
                                    object pptr = info.GetValue(obj);
                                    Process(pptr, info, out bool didProcess, obj, null);
                                    if (didProcess)
                                    {
                                        shouldBreak = false;
                                    }
                                }
                            }
                            if (!shouldBreak)
                            {
                                break;
                            }
                        }
                        if (shouldBreak)
                        {
                            break;
                        }
                    }
                    EditorAssetBundle assetBundle = new EditorAssetBundle();
                    assetBundle.m_Name = bundleName;
                    List<PPtr<EditorObject>> preloads = new List<PPtr<EditorObject>>();
                    List<KeyValuePair<string, AssetInfo>> container = new List<KeyValuePair<string, AssetInfo>>();
                    int unnamedObjCount = 0;
                    foreach (EditorObject obj in objects)
                    {
                        if (obj is EditorGameObject gameObj)
                        {
                            if (gameObj.TryGetTransform(objects) == null || gameObj.TryGetTransform(objects).TryGetFather(objects) == null)
                            {
                                int index = preloads.Count;
                                List<PPtr<EditorObject>> preloadsToAdd = new List<PPtr<EditorObject>>();
                                int preloadSize = 0;
                                void ProcessGameObj(EditorGameObject gameobj)
                                {
                                    PPtr<EditorObject> gopptr = PPtr<EditorObject>.CreatePPtr(gameObj, objects);
                                    preloadsToAdd.Add(gopptr);
                                    preloadSize++;
                                    if (gameobj.m_Components != null)
                                    {
                                        foreach (PPtr<EditorComponent> component in gameobj.m_Components)
                                        {
                                            if (component != null)
                                            {
                                                preloadsToAdd.Add(PPtr<EditorObject>.CreatePPtr(component.Get(objects), objects, 2));
                                                preloadSize++;
                                            }
                                        }
                                        if (gameobj.TryGetTransform(objects) != null && gameobj.TryGetTransform(objects).m_Children != null)
                                        {
                                            foreach (PPtr<EditorTransform> child in gameobj.TryGetTransform(objects).m_Children)
                                            {
                                                if (child != null && child.TryGet(objects, out var actualChild))
                                                {
                                                    if (actualChild.m_GameObject != null && actualChild.m_GameObject.TryGet(objects, out var childsParent))
                                                    {
                                                        ProcessGameObj(childsParent);
                                                    }
                                                    else
                                                    {
                                                        preloadsToAdd.Add(PPtr<EditorObject>.CreatePPtr(actualChild, objects, -1));
                                                        preloadSize++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                ProcessGameObj(gameObj);
                                preloads.AddRange(preloadsToAdd);
                                AssetInfo assetinf = new AssetInfo() { asset = PPtr<EditorObject>.CreatePPtr(gameObj, objects, 0), preloadIndex = index, preloadSize = preloadSize };
                                container.Add(new KeyValuePair<string, AssetInfo>("assets/stuff/" + gameObj.m_Name.ToLower() + ".prefab", assetinf));
                            }
                        }
                        else if (!(obj is EditorComponent))
                        {
                            int index = preloads.Count;
                            preloads.Add(PPtr<EditorObject>.CreatePPtr(obj, objects, -1));
                            AssetInfo assetinf = new AssetInfo() { asset = PPtr<EditorObject>.CreatePPtr(obj, objects, -1), preloadIndex = index, preloadSize = 1 };
                            string name = "UnnamedObject_";
                            if (obj is EditorNamedObject namedobj)
                            {
                                name = namedobj.m_Name;
                            }
                            else
                            {
                                unnamedObjCount++;
                                name += unnamedObjCount;
                            }
                            string afterfix;
                            switch (obj)
                            {
                                case EditorTextAsset text:
                                    afterfix = ".txt";
                                    break;
                                default:
                                    afterfix = ".asset";
                                    break;
                            }
                            container.Add(new KeyValuePair<string, AssetInfo>("assets/stuff/" + name + afterfix, assetinf));
                        }
                    }
                    assetBundle.m_PreloadTable = preloads.ToArray();
                    assetBundle.m_Container = container.ToArray();
                    objects.Insert(1, assetBundle);
                    long objectInfoPosition = subwriter.Position;

                    subwriter.Write(objects.Count);
                    foreach (EditorObject obj in objects)
                    {
                        subwriter.AlignStream();
                        subwriter.Write(0L);
                        subwriter.Write(0u);
                        subwriter.Write(0u);
                        subwriter.Write(0);
                    }

                    subwriter.Write(0); //script count
                    subwriter.Write(0); //external count

                    //first external
                    //subwriter.WriteStringToNull(""); //nothing
                    //subwriter.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 0 }); //guid bytes
                    //subwriter.Write(0); // type
                    //subwriter.WriteStringToNull("resources/unity_builtin_extra");
                    //second external
                    //subwriter.WriteStringToNull(""); //nothing
                    //subwriter.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 0, 0 }); //guid bytes
                    //subwriter.Write(0); // type
                    //subwriter.WriteStringToNull("library/unity default resources");

                    subwriter.WriteStringToNull(""); //some more nothing
                    subwriter.endian = EndianType.BigEndian;
                    long postObjectInfoPosition = subwriter.Position;
                    subwriter.SetPosition(0);
                    subwriter.Write((uint)(postObjectInfoPosition - 20));
                    subwriter.SetPosition(postObjectInfoPosition);
                    subwriter.endian = EndianType.LittleEndian;

                    //assets objects
                    subwriter.SetPosition(4096);
                    int current = 1;
                    Dictionary<EditorObject, long> objectIds = new Dictionary<EditorObject, long>();
                    foreach (EditorObject obj in objects)
                    {
                        long id = 1;
                        if (!(obj is EditorAssetBundle))
                        {
                            current++;
                            id = -1;
                            if (obj is EditorTransform)
                            {
                                id = 5893049602772736123;
                            }
                            else if (obj is EditorGameObject)
                            {
                                id = -5130075632635775496;
                            }
                            foreach (EditorObject a in objects)
                            {
                                if (a is EditorTransform)
                                {
                                    if ((a as EditorTransform).m_GameObject.m_PathID == current)
                                    {
                                        (a as EditorTransform).m_GameObject.m_PathID = id;
                                    }
                                }
                                else if (a is EditorGameObject)
                                {
                                    foreach (PPtr<EditorComponent> c in (a as EditorGameObject).m_Components)
                                    {
                                        if (c.m_PathID == current)
                                        {
                                            c.m_PathID = id;
                                        }
                                    }
                                }
                                else if (a is EditorAssetBundle)
                                {
                                    foreach (PPtr<EditorObject> prel in (a as EditorAssetBundle).m_PreloadTable)
                                    {
                                        if (prel.m_PathID == current)
                                        {
                                            prel.m_PathID = id;
                                        }
                                    }
                                    foreach (KeyValuePair<string, AssetInfo> cont in (a as EditorAssetBundle).m_Container)
                                    {
                                        if (cont.Value.asset.m_PathID == current)
                                        {
                                            cont.Value.asset.m_PathID = id;
                                        }
                                    }
                                }
                            }
                        }
                        objectIds.Add(obj, id);
                    }
                    List<object[]> dict = new List<object[]>();

                    foreach (EditorObject obj in objects)
                    {
                        long start = subwriter.Position - 4096;
                        obj.Write(subwriter);
                        if (obj != objects[objects.Count - 1])
                        {
                            subwriter.Write(obj is EditorGameObject ? 65536 : 0);
                        }
                        uint bytesize = (uint)((subwriter.Position - 4096) - start);
                        if (obj is EditorGameObject)
                        {
                            bytesize = 35u;
                        }
                        else if (obj is EditorTransform)
                        {
                            bytesize = 68u;
                        }
                        dict.Add(new object[] { objectIds[obj], (uint)start, bytesize, obj, AssetBundleTools.GetClassFromType(obj.GetType()) });
                        if (obj is EditorGameObject)
                        {
                            subwriter.SetPosition(subwriter.Position + 4);
                        }
                    }

                    subwriter.SetPosition(objectInfoPosition);
                    subwriter.Write(dict.Count);
                    foreach (object[] bundle in dict)
                    {
                        subwriter.AlignStream();
                        subwriter.Write((long)bundle[0]);
                        subwriter.Write((uint)bundle[1]);
                        subwriter.Write((uint)bundle[2]);
                        subwriter.Write(types.IndexOf((EditorClassIDType)bundle[4]));
                    }


                    //subwriter.Write(man.assetsFileList[man.assetsFileList.Count - 1].allbytes.Skip((int)subwriter.BaseStream.Length).ToArray());

                    //finish
                    subwriter.endian = EndianType.BigEndian;
                    subwriter.SetPosition(0);
                    //subwriter.Write(man.assetsFileList[man.assetsFileList.Count - 1].allbytes.Take(4).ToArray());
                    uint filesize = (uint)subwriter.BaseStream.Length;
                    //Array.Reverse(b);
                    subwriter.SetPosition(fileSizePosition);
                    subwriter.Write(filesize); //filesize
                    bytesToLZMACompress.AddRange(subwriter.BaseStream.ReadToEnd());
                }

                //bytes to compress using lz4
                List<byte> bytesToLZ4Compress = new List<byte>();

                //compresses bytes from before using lzma compression
                byte[] lzmaCompressedBytes = SevenZipHelper.Compress(bytesToLZMACompress.ToArray());

                using (EndianBinaryWriter subwriter = new EndianBinaryWriter(new MemoryStream()))
                {
                    //pre-block info
                    subwriter.Write(new byte[16]); //uncompressed data hash (idk how to get it and how is it used)
                    subwriter.Write(1); //blockscount (done)
                    //block info
                    subwriter.Write((uint)bytesToLZMACompress.Count); //uncompressed size (done)
                    subwriter.Write((uint)lzmaCompressedBytes.Length); //compressed size (done)
                    subwriter.Write((ushort)65u); //flags (done)
                    //pre-directory info
                    subwriter.Write(1); //nodescount (done)
                    //string name = "testassetbundle";// + Guid.NewGuid().ToString();
                    //directory info 1
                    subwriter.Write((long)0); //offset (just zero)
                    long directory1Size = bytesToLZMACompress.Count;
                    subwriter.Write(directory1Size); //size (size of the first half of lzma decompressed bytes) (TODO)
                    subwriter.Write(4u); //flags (whattttttttttttttttttttttttttttttttttttttttt)
                    subwriter.WriteStringToNull("CAB-" + Guid.NewGuid().ToString() + "-" + PrefabBuilder.builtObjects); //name (i don't understand)
                    //directory info 1
                    //subwriter.Write(directory1Size); //offset (size of dir1)
                    //subwriter.Write((long)0); //size (size of the second half of lzma decompressed bytes) (TODO)
                    //subwriter.Write(0u); //flags (whattttttttttttttttttttttttttttttttttttttttt)
                    //subwriter.WriteStringToNull(name + ".resS"); //name (i don't understand)

                    bytesToLZ4Compress.AddRange(subwriter.BaseStream.ReadToEnd());
                }

                //bytes to insert to the header info

                //compresses bytes from before using lz4 compression
                byte[] lz4CompressedBytes = LZ4Codec.Encode(bytesToLZ4Compress.ToArray(), 0, bytesToLZ4Compress.Count);

                //additionsl header info
                long sizePosition = writer.Position;
                writer.Write(0L);
                writer.Write(lz4CompressedBytes.Length.ToBytes()); //compressed blocks info size
                writer.Write(bytesToLZ4Compress.Count.ToBytes()); //uncompressed blocks info size
                writer.Write(67u.ToBytes()); //flags
                writer.Write(lz4CompressedBytes); //lz4 bytes
                writer.Write(lzmaCompressedBytes); //lzma bytes
                writer.Write(new byte[] { 255, 251, 239, 164, 223 });

                writer.SetPosition(sizePosition);
                writer.Write(writer.BaseStream.Length);
            }
            finally
            {
                List<byte> bytes2 = new List<byte>();
                bytes2.AddRange(stream.ReadToEnd());
                ((IDisposable)writer).Dispose();
                stream.Dispose();
                bytes = bytes2.ToArray();
            }
            return bytes;
        }
    }
}

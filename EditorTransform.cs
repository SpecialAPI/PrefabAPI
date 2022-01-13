using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace PrefabAPI
{
    public class EditorTransform : EditorComponent
    {
        public Quaternion m_LocalRotation;
        public Vector3 m_LocalPosition;
        public Vector3 m_LocalScale;
        public PPtr<EditorTransform>[] m_Children;
        public PPtr<EditorTransform> m_Father;

        private EditorTransform actualFather;

        public EditorTransform TryGetFather(List<EditorObject> manualObjects)
        {
            if(actualFather != null)
            {
                return actualFather;
            }
            if(m_Father != null && m_Father.TryGet(manualObjects, out var father))
            {
                actualFather = father;
                return actualFather;
            }
            return null;
        }

        public EditorTransform()
        {
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            write.Write(m_LocalRotation);
            write.Write(m_LocalPosition);
            write.Write(m_LocalScale);
            write.Write(m_Children.Length);
            foreach(PPtr<EditorTransform> p in m_Children)
            {
                p.Write(write);
            }
            m_Father.Write(write);
        }
    }
}

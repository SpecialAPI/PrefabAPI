using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrefabAPI
{
    public class EditorNamedObject : EditorExtension
    {
        public string m_Name;

        public EditorNamedObject()
        {
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            write.WriteAlignedString(m_Name);
        }
    }
}

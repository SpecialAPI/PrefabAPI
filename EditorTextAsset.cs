using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PrefabAPI
{
    public sealed class EditorTextAsset : EditorNamedObject
    {
        public byte[] m_Script;

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
        }
    }
}

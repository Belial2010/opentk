#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Bind.Structures;
using Delegate=Bind.Structures.Delegate;
using Enum=Bind.Structures.Enum;
using Type=Bind.Structures.Type;

namespace Bind.GL2
{
    class Generator : IBind
    {
        #region Fields

        protected static string glTypemap = "GL2/gl.tm";
        protected static string csTypemap = "csharp.tm";
        protected static string enumSpec = "GL2/enum.spec";
        protected static string enumSpecExt = "GL2/enumext.spec";
        protected static string glSpec = "GL2/gl.spec";
        protected static string glSpecExt = "";

        protected static string loadAllFuncName = "LoadAll";

        protected static Regex enumToDotNet = new Regex("_[a-z|A-Z]?", RegexOptions.Compiled);

        protected static readonly char[] numbers = "0123456789".ToCharArray();
        //protected static readonly Dictionary<string, string> doc_replacements;

        protected ISpecReader SpecReader = new XmlSpecReader();

        #endregion

        #region Constructors

        public Generator()
        {
            if (Settings.Compatibility == Settings.Legacy.Tao)
            {
                Settings.OutputNamespace = "Tao.OpenGl";
                Settings.OutputClass = "Gl";
            }
            else
            {
                // Defaults
            }

            Settings.ImportsFile = "GLCore.cs";
            Settings.DelegatesFile = "GLDelegates.cs";
            Settings.EnumsFile = "GLEnums.cs";
            Settings.WrappersFile = "GL.cs";
        }

        #endregion

        #region IBind Members

        public DelegateCollection Delegates { get; private set; }
        public EnumCollection Enums { get; private set; }
        public FunctionCollection Wrappers { get; private set; }

        public virtual void Process()
        {
            using (var overrides = new StreamReader(Path.Combine(Settings.InputPath, Settings.OverridesFile)))
            {
                using (StreamReader sr = Utilities.OpenSpecFile(Settings.InputPath, glTypemap))
                    Type.GLTypes = SpecReader.ReadTypeMap(sr);
                using (StreamReader sr = Utilities.OpenSpecFile(Settings.InputPath, csTypemap))
                    Type.CSTypes = SpecReader.ReadCSTypeMap(sr);
                using (var sr = new StreamReader(Path.Combine(Settings.InputPath, enumSpec)))
                {
                    Enums = SpecReader.ReadEnums(sr);
                    Utilities.Merge(Enums, SpecReader.ReadEnums(overrides));
                }
                using (var sr = new StreamReader(Path.Combine(Settings.InputPath, glSpec)))
                {
                    Delegates = SpecReader.ReadDelegates(sr);
                    Utilities.Merge(Delegates, SpecReader.ReadDelegates(overrides));
                }

                Enums = new EnumProcessor(overrides).Process(Enums);
                Wrappers = new FuncProcessor(overrides).Process(Delegates, Enums);
            }
        }

        #endregion
    }
}

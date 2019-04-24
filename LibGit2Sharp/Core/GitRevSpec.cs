using System;
using System.Runtime.InteropServices;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct GitRevSpec
    {
        public git_object* From;

        public git_object* To;

        public RevSpecType Type;
    }
}

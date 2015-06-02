using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LibGit2Sharp.Core
{

    [StructLayout(LayoutKind.Sequential)]
    class RefDbIterator
    {
        IntPtr refDb;

        NativeMethods.ref_db_next next;
        NativeMethods.ref_db_next_name next_name;
        NativeMethods.ref_db_free free;
    }
}

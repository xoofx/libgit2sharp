using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibGit2Sharp
{
    public abstract class RefdbIterator
    {
        public abstract void Next();

        public abstract string NextName();
    }
}

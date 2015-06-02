using System;
using System.Globalization;
using System.Runtime.InteropServices;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp
{
    /// <summary>
    ///   Base class for all custom managed backends for the libgit2 reference database.
    /// </summary>
    public abstract class RefdbBackend
    {
        /// <summary>
        ///  Requests the repository configured for this backend.
        /// </summary>
        protected abstract Repository Repository
        {
            get;
        }

        /// <summary>
        ///   Requests that the backend provide all optional operations that are supported.
        /// </summary>
        protected abstract RefdbBackendOperations SupportedOperations
        {
            get;
        }

        /// <summary>
        ///  Queries the backend for whether a reference exists.
        /// </summary>
        /// <param name="referenceName">Name of the reference to query</param>
        /// <returns>True if the reference exists in the backend, false otherwise.</returns>
        public abstract bool Exists(string referenceName);

        /// <summary>
        ///  Queries the backend for the given reference
        /// </summary>
        /// <param name="referenceName">Name of the reference to query</param>
        /// <param name="isSymbolic">
        ///   True if the returned reference is symbolic. False if the returned reference is direct.
        /// </param>
        /// <param name="oid">Object ID of the returned reference. Valued when <paramref name="isSymbolic"/> is false.</param>
        /// <param name="symbolic">Target of the returned reference. Valued when <paramref name="isSymbolic"/> is false</param>
        /// <returns>True if the reference exists, false otherwise</returns>
        public abstract bool Lookup(string referenceName, out bool isSymbolic, out ObjectId oid, out string symbolic);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="glob"></param>
        /// <returns></returns>
        public abstract object Iterator(string glob);

        /// <summary>
        ///  Write the given direct reference to the backend.
        /// </summary>
        /// <param name="referenceCanonicalName">The reference to write</param>
        /// <param name="target">The <see cref="ObjectId"/> of the target <see cref="GitObject"/>.</param>
        public abstract void WriteDirectReference(string referenceCanonicalName, ObjectId target);

        /// <summary>
        ///  Write the given symbolic reference to the backend.
        /// </summary>
        /// <param name="referenceCanonicalName">The reference to write</param>
        /// <param name="targetCanonicalName">The target of the symbolic reference</param>
        public abstract void WriteSymbolicReference(string referenceCanonicalName, string targetCanonicalName);

        /// <summary>
        ///  Delete the given reference from the backend.
        /// </summary>
        /// <param name="referenceCanonicalName">The reference to delete</param>
        public abstract void Delete(string referenceCanonicalName);

        /// <summary>
        ///  Compress the backend in an implementation-specific way.
        /// </summary>
        public abstract void Compress();

        /// <summary>
        ///  Free any data associated with this backend.
        /// </summary>
        public abstract void Free();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refName"></param>
        /// <returns></returns>
        public abstract bool HasReflog(string refName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refName"></param>
        public abstract void EnsureReflog(string refName);

        /// <summary>
        /// 
        /// </summary>
        public abstract void ReadReflog();

        /// <summary>
        /// 
        /// </summary>
        public abstract void WriteReflog();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public abstract void RenameReflog(string oldName, string newName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refName"></param>
        /// <returns></returns>
        public abstract bool LockReference(string refName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refname"></param>
        /// <returns></returns>
        public abstract bool UnlockReference(string refname);

        private IntPtr nativeBackendPointer;

        internal IntPtr GitRefdbBackendPointer
        {
            get
            {
                if (IntPtr.Zero == nativeBackendPointer)
                {
                    var nativeBackend = new GitRefdbBackend();
                    nativeBackend.Version = 1;

                    // The "free" entry point is always provided.
                    nativeBackend.Exists = BackendEntryPoints.ExistsCallback;
                    nativeBackend.Lookup = BackendEntryPoints.LookupCallback;
                    nativeBackend.Foreach = BackendEntryPoints.ForeachCallback;
                    nativeBackend.Write = BackendEntryPoints.WriteCallback;
                    nativeBackend.Delete = BackendEntryPoints.DeleteCallback;
                    nativeBackend.Free = BackendEntryPoints.FreeCallback;

                    var supportedOperations = this.SupportedOperations;

                    if ((supportedOperations & RefdbBackendOperations.ForeachGlob) != 0)
                    {
                        nativeBackend.ForeachGlob = BackendEntryPoints.ForeachGlobCallback;
                    }

                    if ((supportedOperations & RefdbBackendOperations.Compress) != 0)
                    {
                        nativeBackend.Compress = BackendEntryPoints.CompressCallback;
                    }

                    nativeBackend.GCHandle = GCHandle.ToIntPtr(GCHandle.Alloc(this));
                    nativeBackendPointer = Marshal.AllocHGlobal(Marshal.SizeOf(nativeBackend));
                    Marshal.StructureToPtr(nativeBackend, nativeBackendPointer, false);
                }

                return nativeBackendPointer;
            }
        }

        private static class BackendEntryPoints
        {
            // Because our GitOdbBackend structure exists on the managed heap only for a short time (to be marshaled
            // to native memory with StructureToPtr), we need to bind to static delegates. If at construction time
            // we were to bind to the methods directly, that's the same as newing up a fresh delegate every time.
            // Those delegates won't be rooted in the object graph and can be collected as soon as StructureToPtr finishes.
            public static readonly GitRefDbBackend2.exists_callback ExistsCallback = Exists;
            public static readonly GitRefDbBackend2.lookup_callback LookupCallback = Lookup;

            public static readonly GitRefDbBackend2.iterator_callback IterCallback = GetIterator;

            public static readonly GitRefDbBackend2.write_callback WriteCallback = Write;
            public static readonly GitRefDbBackend2.rename_callback RenameCallback = Rename;
            public static readonly GitRefDbBackend2.delete_callback DeleteCallback = Delete;

            public static readonly GitRefDbBackend2.compress_callback CompressCallback = Compress;
            public static readonly GitRefDbBackend2.free_callback FreeCallback = Free;

            public static readonly GitRefDbBackend2.reflog_write_callback ReflogWriteCallback = ReflogWrite;
            public static readonly GitRefDbBackend2.reflog_read_callback ReflogReadCallback = ReflogWrite;
            public static readonly GitRefDbBackend2.reflog_rename_callback ReflogRenameCallback = ReflogWrite;
            public static readonly GitRefDbBackend2.reflog_delete_callback ReflogDeleteCallback = ReflogWrite;

            public static readonly GitRefDbBackend2.ref_lock_callback RefLockCallback = LockRef;
            public static readonly GitRefDbBackend2.ref_unlock_callback RefUnlock = UnlockRef;

            private static bool TryMarshalRefdbBackend(out RefdbBackend refdbBackend, IntPtr backend)
            {
                refdbBackend = null;

                var intPtr = Marshal.ReadIntPtr(backend, GitRefdbBackend.GCHandleOffset);
                var handle = GCHandle.FromIntPtr(intPtr).Target as RefdbBackend;

                if (handle == null)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, "Cannot retrieve the RefdbBackend handle.");
                    return false;
                }

                refdbBackend = handle;
                return true;
            }

            private static int Exists(
                out IntPtr exists,
                IntPtr backend,
                IntPtr namePtr)
            {
                exists = IntPtr.Zero;

                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string referenceName = LaxUtf8Marshaler.FromNative(namePtr);

                try
                {
                    if (refdbBackend.Exists(referenceName))
                    {
                        exists = (IntPtr)1;
                    }
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }

            private static int Lookup(
                out IntPtr referencePtr,
                IntPtr backend,
                IntPtr namePtr)
            {
                referencePtr = IntPtr.Zero;

                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string referenceName = LaxUtf8Marshaler.FromNative(namePtr);

                try
                {
                    bool isSymbolic;
                    ObjectId oid;
                    string symbolic;

                    if (!refdbBackend.Lookup(referenceName, out isSymbolic, out oid, out symbolic))
                    {
                        return (int)GitErrorCode.NotFound;
                    }

                    referencePtr = isSymbolic ?
                        Proxy.git_reference__alloc_symbolic(referenceName, symbolic) :
                        Proxy.git_reference__alloc(referenceName, oid);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return referencePtr != IntPtr.Zero ?
                    (int) GitErrorCode.Ok : (int) GitErrorCode.Error;
            }

            private static int Foreach(
                IntPtr backend,
                GitReferenceType list_flags,
                GitRefdbBackend.foreach_callback_callback callback,
                IntPtr data)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    bool includeSymbolicRefs = list_flags.HasFlag(GitReferenceType.Symbolic);
                    bool includeDirectRefs = list_flags.HasFlag(GitReferenceType.Oid);

                    return refdbBackend.Foreach(
                        new ForeachState(callback, data).ManagedCallback,
                        includeSymbolicRefs,
                        includeDirectRefs);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }
            }

            private static int ForeachGlob(
                IntPtr backend,
                IntPtr globPtr,
                GitReferenceType list_flags,
                GitRefdbBackend.foreach_callback_callback callback,
                IntPtr data)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string glob = LaxUtf8Marshaler.FromNative(globPtr);

                try
                {
                    bool includeSymbolicRefs = list_flags.HasFlag(GitReferenceType.Symbolic);
                    bool includeDirectRefs = list_flags.HasFlag(GitReferenceType.Oid);

                    return refdbBackend.ForeachGlob(
                        glob,
                        new ForeachState(callback, data).ManagedCallback, includeSymbolicRefs, includeDirectRefs);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }
            }

            private static int Write(
                IntPtr backend,
                IntPtr referencePtr)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                var referenceHandle = new NotOwnedReferenceSafeHandle(referencePtr);
                string name = Proxy.git_reference_name(referenceHandle);
                GitReferenceType type = Proxy.git_reference_type(referenceHandle);

                try
                {
                    switch (type)
                    {
                        case GitReferenceType.Oid:
                            ObjectId targetOid = Proxy.git_reference_target(referenceHandle);
                            refdbBackend.WriteDirectReference(name, targetOid);
                            break;

                        case GitReferenceType.Symbolic:
                            string targetIdentifier = Proxy.git_reference_symbolic_target(referenceHandle);
                            refdbBackend.WriteSymbolicReference(name, targetIdentifier);
                            break;

                        default:
                            throw new LibGit2SharpException(
                                String.Format(CultureInfo.InvariantCulture,
                                    "Unable to build a new reference from a type '{0}'.", type));
                    }
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }

            private static int Delete(
                IntPtr backend,
                IntPtr referencePtr)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                var referenceHandle = new NotOwnedReferenceSafeHandle(referencePtr);
                string name = Proxy.git_reference_name(referenceHandle);

                try
                {
                    refdbBackend.Delete(name);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }

            private static int Compress(IntPtr backend)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                try
                {
                    refdbBackend.Compress();
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return (int)GitErrorCode.Ok;
            }

            private static void Free(IntPtr backend)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    // Really? Looks weird.
                    return;
                }

                refdbBackend.Free();
            }
            }
        }

        /// <summary>
        ///   Flags used by subclasses of RefdbBackend to indicate which operations they support.
        /// </summary>
        [Flags]
        public enum RefdbBackendOperations
        {
            /// <summary>
            ///   This RefdbBackend declares that it supports the Compress method.
            /// </summary>
            Compress = 1,

            /// <summary>
            ///   This RefdbBackend declares that it supports the ForeachGlob method.
            /// </summary>
            ForeachGlob = 2,
        }
    }
}

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
        public abstract RefdbIterator GenerateRefIterator(string glob);

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
                    var nativeBackend = new GitRefDbBackend();
                    nativeBackend.Version = 1;

                    // The "free" entry point is always provided.
                    nativeBackend.Exists = BackendEntryPoints.ExistsCallback;
                    nativeBackend.Lookup = BackendEntryPoints.LookupCallback;
                    nativeBackend.Iter = BackendEntryPoints.IterCallback;
                    nativeBackend.Write = BackendEntryPoints.WriteCallback;
                    nativeBackend.Rename = BackendEntryPoints.RenameCallback;
                    nativeBackend.Delete = BackendEntryPoints.DeleteCallback;
                    nativeBackend.Compress = BackendEntryPoints.CompressCallback;
                    nativeBackend.HasLog = BackendEntryPoints.HasLogCallback;
                    nativeBackend.EnsureLog = BackendEntryPoints.EnsureLogCallback;
                    nativeBackend.FreeBackend = BackendEntryPoints.FreeCallback;
                    nativeBackend.ReflogWrite = BackendEntryPoints.ReflogWriteCallback;
                    nativeBackend.ReflogRead = BackendEntryPoints.ReflogReadCallback;
                    nativeBackend.ReflogRename = BackendEntryPoints.ReflogRenameCallback;
                    nativeBackend.ReflogDelete = BackendEntryPoints.ReflogDeleteCallback;
                    nativeBackend.RefLock = BackendEntryPoints.RefLockCallback;
                    nativeBackend.RefUnlock = BackendEntryPoints.RefUnlockCallback;

                    var supportedOperations = this.SupportedOperations;

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
            public static readonly GitRefDbBackend.exists_callback ExistsCallback = Exists;
            public static readonly GitRefDbBackend.lookup_callback LookupCallback = Lookup;

            public static readonly GitRefDbBackend.iterator_callback IterCallback = GetIterator;

            public static readonly GitRefDbBackend.write_callback WriteCallback = Write;
            public static readonly GitRefDbBackend.rename_callback RenameCallback = Rename;
            public static readonly GitRefDbBackend.delete_callback DeleteCallback = Delete;

            public static readonly GitRefDbBackend.compress_callback CompressCallback = Compress;
            public static readonly GitRefDbBackend.free_callback FreeCallback = Free;

            public static readonly GitRefDbBackend.has_log_callback HasLogCallback = HasLog;
            public static readonly GitRefDbBackend.ensure_log_callback EnsureLogCallback = EnsureLog;

            public static readonly GitRefDbBackend.reflog_write_callback ReflogWriteCallback = ReflogWrite;
            public static readonly GitRefDbBackend.reflog_read_callback ReflogReadCallback = ReflogRead;
            public static readonly GitRefDbBackend.reflog_rename_callback ReflogRenameCallback = ReflogRename;
            public static readonly GitRefDbBackend.reflog_delete_callback ReflogDeleteCallback = ReflogDelete;

            public static readonly GitRefDbBackend.ref_lock_callback RefLockCallback = LockRef;
            public static readonly GitRefDbBackend.ref_unlock_callback RefUnlockCallback = UnlockRef;

            private static bool TryMarshalRefdbBackend(out RefdbBackend refdbBackend, IntPtr backend)
            {
                refdbBackend = null;

                var intPtr = Marshal.ReadIntPtr(backend, GitRefDbBackend.GCHandleOffset);
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
                out bool exists,
                IntPtr backend,
                IntPtr refNamePtr)
            {
                exists = false;

                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string refName = LaxUtf8Marshaler.FromNative(refNamePtr);

                try
                {
                    exists = refdbBackend.Exists(refName);
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
                IntPtr refNamePtr)
            {
                referencePtr = IntPtr.Zero;

                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string refName = LaxUtf8Marshaler.FromNative(refNamePtr);

                try
                {
                    bool isSymbolic;
                    ObjectId oid;
                    string symbolic;

                    if (!refdbBackend.Lookup(refName, out isSymbolic, out oid, out symbolic))
                    {
                        return (int)GitErrorCode.NotFound;
                    }

                    referencePtr = isSymbolic ?
                        Proxy.git_reference__alloc_symbolic(refName, symbolic) :
                        Proxy.git_reference__alloc(refName, oid);
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return referencePtr != IntPtr.Zero ?
                    (int)GitErrorCode.Ok : (int)GitErrorCode.Error;
            }

            private static int GetIterator(
                out IntPtr iterPtr,
                IntPtr backend,
                IntPtr globPtr)
            {
                iterPtr = IntPtr.Zero;

                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string glob = LaxUtf8Marshaler.FromNative(globPtr);

                try
                {
                    // generate a new iterator
                    RefdbIterator refIter = refdbBackend.GenerateRefIterator(glob);

                    iterPtr = refIter.GitRefdbIteratorPtr;
                }
                catch (Exception ex)
                {
                    Proxy.giterr_set_str(GitErrorCategory.Reference, ex);
                    return (int)GitErrorCode.Error;
                }

                return iterPtr != IntPtr.Zero ?
                    (int)GitErrorCode.Ok : (int)GitErrorCode.Error;
            }


            private static int Write(
                IntPtr backend,
                IntPtr referencePtr,
                bool force,
                IntPtr who,
                string message,
                ref GitOid oldId,
                string oldTarget)
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

            private static int Rename(
                out IntPtr reference,
                IntPtr backend,
                IntPtr oldNamePtr,
                IntPtr newNamePtr,
                bool force,
                IntPtr who,
                IntPtr messagePtr)
            {
                reference = IntPtr.Zero;
                return 0;
            }

            private static int Delete(
                IntPtr backend,
                IntPtr refNamePtr,
                IntPtr oldId,
                IntPtr oldTargetNamePtr)
            {
                RefdbBackend refdbBackend;
                if (!TryMarshalRefdbBackend(out refdbBackend, backend))
                {
                    return (int)GitErrorCode.Error;
                }

                string refName = LaxUtf8Marshaler.FromNative(refNamePtr);
                try
                {
                    refdbBackend.Delete(refName);
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
                    return;
                }

                refdbBackend.Free();
            }

            private static int ReflogRead(out IntPtr reflogPtr, IntPtr backendPtr, IntPtr refNamePtr)
            {
                reflogPtr = IntPtr.Zero;
                return 0;
            }
            

            public static int ReflogWrite(
                IntPtr backend, // git_refdb_backend *
                IntPtr git_reflog // git_reflog *
                )
            {
                return 0;
            }

            public static int ReflogRename(
                IntPtr backend, // git_refdb_backend
                IntPtr oldNamePtr, // const char *
                IntPtr newNamePtr // const char *
                )
            {
                return 0;
            }

            public static int ReflogDelete(
                IntPtr backend, // git_refdb_backend
                IntPtr namePtr // const char *
                )
            {
                return 0;
            }

            public static int HasLog(
                IntPtr backend, // git_refdb_backend *
                IntPtr refNamePtr // const char *
                )
            {
                return 0;
            }

            public static int EnsureLog(
                IntPtr backend, // git_refdb_backend *
                IntPtr refNamePtr // const char *
                )
            {
                return 0;
            }

            public static int LockRef(
                IntPtr backend, // git_refdb_backend
                IntPtr namePtr // const char *
                )
            {
                return 0;
            }

            public static int UnlockRef(
                IntPtr backend, // git_refdb_backend
                IntPtr payload,
                [MarshalAs(UnmanagedType.Bool)] bool force,
                [MarshalAs(UnmanagedType.Bool)] bool update_reflog,
                IntPtr refNamePtr, // const char *
                IntPtr who, // const git_signature *
                IntPtr messagePtr // const char *
                )
            {
                return 0;
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
            /// The RefdbBackend declares that it supports Reflog operations
            /// </summary>
            Reflog = 2,
        }
    }
}

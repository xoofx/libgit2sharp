using System;
using System.Runtime.InteropServices;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitRefdbBackend
    {
        static GitRefdbBackend()
        {
            GCHandleOffset = Marshal.OffsetOf(typeof(GitRefdbBackend), "GCHandle").ToInt32();
        }

        public uint Version;

        public exists_callback Exists;
        public lookup_callback Lookup;
        public foreach_callback Foreach;
        public foreach_glob_callback ForeachGlob;
        public write_callback Write;
        public delete_callback Delete;
        public compress_callback Compress;
        public free_callback Free;

        /* The libgit2 structure definition ends here. Subsequent fields are for libgit2sharp bookkeeping. */

        public IntPtr GCHandle;

        /* The following static fields are not part of the structure definition. */

        public static int GCHandleOffset;

        /// <summary>
        ///   Queries the backend to determine if the given referenceName
        ///   exists.
        /// </summary>
        /// <param name="exists">[out] If the call is successful, the backend will set this to 1 if the reference exists, 0 otherwise.</param>
        /// <param name="backend">[in] A pointer to the backend which is being queried.</param>
        /// <param name="referenceName">[in] The reference name to look up.</param>
        /// <returns>0 if successful; an error code otherwise.</returns>
        public delegate int exists_callback(
            out IntPtr exists,
            IntPtr backend,
            IntPtr referenceName);

        /// <summary>
        ///   Queries the backend for the given reference.
        /// </summary>
        /// <param name="reference">[out] If the call is successful, the backend will set this to the reference.</param>
        /// <param name="backend">[in] A pointer to the backend which is being queried.</param>
        /// <param name="referenceName">[in] The reference name to look up.</param>
        /// <returns>0 if successful; GIT_EEXISTS or an error code otherwise.</returns>
        public delegate int lookup_callback(
            out IntPtr reference,
            IntPtr backend,
            IntPtr referenceName);

        /// <summary>
        ///   Iterates each reference that matches list_flags, calling back to the given callback.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend to query.</param>
        /// <param name="list_flags">[in] The references to list.</param>
        /// <param name="cb">[in] The callback function to invoke.</param>
        /// <param name="data">[in] An arbitrary parameter to pass through to the callback</param>
        /// <returns>0 if successful; GIT_EUSER or an error code otherwise.</returns>
        public delegate int foreach_callback(
            IntPtr backend,
            GitReferenceType list_flags,
            foreach_callback_callback cb,
            IntPtr data);

        /// <summary>
        ///   Iterates each reference that matches the glob pattern and the list_flags, calling back to the given callback.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend to query.</param>
        /// <param name="glob">[in] A glob pattern.</param>
        /// <param name="list_flags">[in] The references to list.</param>
        /// <param name="cb">[in] The callback function to invoke.</param>
        /// <param name="data">[in] An arbitrary parameter to pass through to the callback</param>
        /// <returns>0 if successful; GIT_EUSER or an error code otherwise.</returns>
        public delegate int foreach_glob_callback(
            IntPtr backend,
            IntPtr glob,
            GitReferenceType list_flags,
            foreach_callback_callback cb,
            IntPtr data);

        /// <summary>
        ///   Writes the given reference.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend to write to.</param>
        /// <param name="referencePtr">[in] The reference to write.</param>
        /// <returns>0 if successful; an error code otherwise.</returns>
        public delegate int write_callback(
            IntPtr backend,
            IntPtr referencePtr);

        /// <summary>
        ///   Deletes the given reference.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend to delete.</param>
        /// <param name="referencePtr">[in] The reference to delete.</param>
        /// <returns>0 if successful; an error code otherwise.</returns>
        public delegate int delete_callback(
            IntPtr backend,
            IntPtr referencePtr);

        /// <summary>
        ///   Compresses the contained references, if possible.  The backend is free to implement this in any implementation-defined way; or not at all.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend to compress.</param>
        /// <returns>0 if successful; an error code otherwise.</returns>
        public delegate int compress_callback(
            IntPtr backend);

        /// <summary>
        /// The owner of this backend is finished with it. The backend is asked to clean up and shut down.
        /// </summary>
        /// <param name="backend">[in] A pointer to the backend which is being freed.</param>
        public delegate void free_callback(
            IntPtr backend);

        /// <summary>
        /// A callback for the backend's implementation of foreach.
        /// </summary>
        /// <param name="referenceName">The reference name.</param>
        /// <param name="data">Pointer to payload data passed to the caller.</param>
        /// <returns>A zero result indicates the enumeration should continue. Otherwise, the enumeration should stop.</returns>
        public delegate int foreach_callback_callback(
            IntPtr referenceName,
            IntPtr data);
    }

    internal class GitRefDbBackend2
    {
        uint version;

        public exists_callback Exists;
        public lookup_callback Lookup;
        public iterator_callback Iter;
        public write_callback Write;
        public rename_callback Rename;
        public delete_callback Delete;
        public compress_callback Compress;
        public has_log_callback HasLog;
        public ensure_log_callback EnsureLog;
        public free_callback FreeBackend;
        public reflog_write_callback ReflogWrite;
        public reflog_read_callback ReflogRead;
        public reflog_rename_callback ReflogRename;
        public reflog_delete_callback ReflogDelete;
        public ref_lock_callback RefLock;
        public ref_unlock_callback RefUnlock;

        /* The libgit2 structure definition ends here. Subsequent fields are for libgit2sharp bookkeeping. */

        /// Queries the refdb backend to determine if the given ref_name
        /// A refdb implementation must provide this function.
        public delegate int exists_callback(
            out int exists,
            GitRefDbBackend2 backend,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8Marshaler))] string ref_name);

        /// Queries the refdb backend for a given reference.  A refdb
        /// implementation must provide this function.
        public delegate int lookup_callback(
            IntPtr git_reference,
            GitRefDbBackend2 backend,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8Marshaler))] string ref_name);

        /// <summary>
        /// Allocate an iterator object for the backend.
        /// A refdb implementation must provide this function.
        /// </summary>
        public delegate int iterator_callback(
            IntPtr iter,
            GitRefDbBackend2 backend,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8Marshaler))] string glob);

        /// Writes the given reference to the refdb.  A refdb implementation
        /// must provide this function.
        public delegate int write_callback(
            GitRefDbBackend2 backend,
            IntPtr reference, // const git_reference *
            [MarshalAs(UnmanagedType.Bool)] bool force,
            IntPtr who, // const git_signature *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string message, // const char *
            ref GitOid old, // const git_oid *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string old_target // const char *
            );

        public delegate int rename_callback(
            IntPtr reference, // git_reference **
            GitRefDbBackend2 backend, // git_refdb_backend *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string old_name, // const char *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string new_name, // const char *
            [MarshalAs(UnmanagedType.Bool)] bool force,
            IntPtr who, // const git_signature *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string message // const char *
            );

        public delegate int delete_callback(
            GitRefDbBackend2 backend, // git_refdb_backend *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string ref_name, // const char *
            ref GitOid old, // const git_oid *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string old_target // const char *
            );

        public delegate int compress_callback(
            GitRefDbBackend2 backend // git_refdb_backend *
            );

        public delegate int has_log_callback(
            GitRefDbBackend2 backend, // git_refdb_backend *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string refname // const char *
            );

        public delegate int ensure_log_callback(
            GitRefDbBackend2 backend, // git_refdb_backend *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string refname // const char *
            );

        public delegate void free_callback(
            GitRefDbBackend2 backend // git_refdb_backend *
            );

        public delegate int reflog_read_callback(
            IntPtr git_reflog, // git_reflog *
            GitRefDbBackend2 backend, // git_refdb_backend *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string refname // const char *
            );

        public delegate int reflog_write_callback(
            GitRefDbBackend2 backend, // git_refdb_backend *
            IntPtr git_reflog // git_reflog *
            );

        public delegate int reflog_rename_callback(
            GitRefDbBackend2 backend, // git_refdb_backend
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string oldName, // const char *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string newName // const char *
            );

        public delegate int reflog_delete_callback(
            GitRefDbBackend2 backend, // git_refdb_backend
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string name // const char *
            );

        public delegate int ref_lock_callback(
            GitRefDbBackend2 backend, // git_refdb_backend
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string name // const char *
            );

        public delegate int ref_unlock_callback(
            GitRefDbBackend2 backend, // git_refdb_backend
            IntPtr payload,
            [MarshalAs(UnmanagedType.Bool)] bool force,
            [MarshalAs(UnmanagedType.Bool)] bool update_reflog,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string refName, // const char *
            IntPtr who, // const git_signature *
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = UniqueId.UniqueIdentifier, MarshalTypeRef = typeof(LaxUtf8NoCleanupMarshaler))] string message// const char *
            );
    }
}

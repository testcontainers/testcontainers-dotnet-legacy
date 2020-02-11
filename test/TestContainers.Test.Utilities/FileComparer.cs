using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace TestContainers.Test.Utilities
{
    public class FileComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo x, FileInfo y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.Name == y.Name &&
                   x.Length == y.Length &&
                   ComputeMd5(x).SequenceEqual(ComputeMd5(y));
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must
        // also be equal. Because equality as defined here is a simple value equality, not
        // reference identity, it is possible that two or more objects will produce the same
        // hash code.
        public int GetHashCode(FileInfo obj)
        {
            string s = $"{obj.Name}{obj.Length}{ComputeMd5(obj)}";
            return s.GetHashCode();
        }

        private static byte[] ComputeMd5(FileInfo fileInfo)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = fileInfo.OpenRead())
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}

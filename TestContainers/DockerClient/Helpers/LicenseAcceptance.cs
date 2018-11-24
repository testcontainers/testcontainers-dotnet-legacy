using System;
using System.Collections.Generic;

namespace TestContainers
{
    public class LicenseAcceptance
    {
        private static readonly ISet<string> AcceptedImageLicenses = new HashSet<string>();

        public static void AcceptLicense(string imageName)
        {
            AcceptedImageLicenses.Add(imageName);
        }

        public static void AssertLicenseAccepted(string imageName)
        {
            if (AcceptedImageLicenses.Contains(imageName))
            {
                return;
            }

            throw new InvalidOperationException($"The image {imageName} requires you to accept a license agreement. " +
                                                $"Please call {typeof(LicenseAcceptance).FullName}.{nameof(AcceptLicense)}(\"{imageName}\") before creating the container.");
        }
    }
}
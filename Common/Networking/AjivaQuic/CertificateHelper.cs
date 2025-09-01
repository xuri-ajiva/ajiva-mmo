using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Common.Networking.AjivaQuic;

internal static class CertificateHelper
{
    internal static X509Certificate2 GenerateManualCertificate()
    {
        X509Certificate2? cert = null;
        var store = new X509Store("KestrelWebTransportCertificates", StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        if (store.Certificates.Count > 0)
        {
            cert = store.Certificates[^1];

            // rotate key after it expires
            if (!cert.HasPrivateKey || DateTime.Parse(cert.GetExpirationDateString(), null) < DateTimeOffset.UtcNow)
            {
                cert = null;
            }
        }

        if (cert == null)
        {
            // generate a new cert
            var now = DateTimeOffset.UtcNow;
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost");
            using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest req = new("CN=localhost", ec, HashAlgorithmName.SHA256);
            // Adds purpose
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1") // serverAuth
            }, false));
            // Adds usage
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            // Adds subject alternate names
            req.CertificateExtensions.Add(sanBuilder.Build());
            // Sign
            using var
                crt = req.CreateSelfSigned(now,
                    now.AddDays(14)); // 14 days is the max duration of a certificate for this
            // Save
            cert = X509CertificateLoader.LoadPkcs12(crt.Export(X509ContentType.Pkcs12), null,
                X509KeyStorageFlags.Exportable);
            store.Add(cert);
        }

        store.Close();
        Debug.Assert(cert.HasPrivateKey);
        return cert;
    }
}

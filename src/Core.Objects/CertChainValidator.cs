using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public static class CertChainValidator
{
    public static bool ValidateCertChain(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // If the cert is a valid, done!            // Some shared mail service providers return their root cert to your domain requests, allow this.
        if (sslPolicyErrors is SslPolicyErrors.None or SslPolicyErrors.RemoteCertificateNameMismatch)
            return true;

        // If there are errors in the certificate chain, look at each error to determine the cause.
        return (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && AnalyseChain(certificate, chain);
    }

    static bool AnalyseChain(X509Certificate certificate, X509Chain chain)
    {
        if (chain != null && chain.ChainStatus != null)
        {
            X509ChainStatus status;

            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                status = chain.ChainStatus[i];

                if (certificate.Subject == certificate.Issuer && status.Status == X509ChainStatusFlags.UntrustedRoot)
                    // Self-signed certificates with an untrusted root are valid. 
                    return true;
                else if (status.Status != X509ChainStatusFlags.NoError)
                    // If there are any other errors in the certificate chain, the certificate is invalid,
                    // so the method returns false.
                    return false;
            }
        }

        // When processing reaches this line, the only errors in the certificate chain are 
        // untrusted root errors for self-signed certificates. These certificates are valid
        // for default Exchange server installations, so return true.
        return true;
    }
}
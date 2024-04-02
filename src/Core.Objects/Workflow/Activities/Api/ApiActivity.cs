using Core.Objects.Dtos.Workflow;
using Core.Objects.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Api
{
    public abstract class ApiActivity : Activity
    {
        public static bool IgnoreSSLErrors { get; set; } = false;
        public string AuthType { get; set; } = "bearer";
        public string AuthToken { get; set; }
        public string BaseUrl { get; set; }

        public override Task ExecuteInternal(IWorkflowContext context)
        {
            SafelyRemoveCertChainHandler();
            AddCertChainHandler();

            BaseUrl ??= context.Variables["Api"] as string;
            AuthToken ??= context.Variables["AuthToken"] as string;
            return base.ExecuteInternal(context);
        }

        protected HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate),
                ServerCertificateCustomValidationCallback = CertChainValidator.ValidateCertChain
            }).WithBaseUri(BaseUrl);

            httpClient.Timeout = TimeSpan.FromSeconds(200.0);
            
            if (AuthToken != null)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthType, AuthToken);

            return httpClient;
        }

        void AddCertChainHandler()
        {
            if (IgnoreSSLErrors)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            else
                ServicePointManager.ServerCertificateValidationCallback += ValidateCertChain;
        }

        void SafelyRemoveCertChainHandler()
        {
            try
            {
                if (IgnoreSSLErrors)
                    ServicePointManager.ServerCertificateValidationCallback -= ValidateCertChain;
                else
                    ServicePointManager.ServerCertificateValidationCallback -= (sender, cert, chain, sslPolicyErrors) => true;
            }
            catch { }
        }

        bool ValidateCertChain(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

    /// <summary>
    /// Base type for all Http interactions with workflow
    /// </summary>
    /// <typeparam name="T">result type expected</typeparam>
    public abstract class ApiActivity<T> : ApiActivity
    {
        [JsonIgnore]
        protected internal int BatchSize => 1000;

        public string Query { get; set; }

        [ApiIgnore]
        [IgnoreWhenFlowComplete]
        [JsonIgnore]
        public T Result { get; set; }
    }
}
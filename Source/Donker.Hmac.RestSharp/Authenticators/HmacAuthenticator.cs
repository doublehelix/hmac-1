﻿using System;
using System.Globalization;
using Donker.Hmac.Configuration;
using Donker.Hmac.RestSharp.Helpers;
using Donker.Hmac.RestSharp.Signing;
using Donker.Hmac.Signing;
using RestSharp;
using RestSharp.Authenticators;

namespace Donker.Hmac.RestSharp.Authenticators
{
    /// <summary>
    /// RestSharp authenticator that signs a request message using HMAC.
    /// </summary>
    public class HmacAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Gets the configuration to use for signing.
        /// </summary>
        protected IHmacConfiguration Configuration { get; }
        /// <summary>
        /// Gets the signer used for creating the signature.
        /// </summary>
        protected IRestSharpHmacSigner Signer { get; }
        /// <summary>
        /// Gets the culture to use when converting a datetime to a string for the value of the HTTP Date header.
        /// </summary>
        protected CultureInfo DateHeaderCulture { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HmacAuthenticator"/> class using the specified configuration and signer.
        /// </summary>
        /// <param name="configuration">The configuration to use for signing.</param>
        /// <param name="signer">The signer used for creating the signature.</param>
        /// <exception cref="ArgumentNullException">The configuration or signer is null.</exception>
        public HmacAuthenticator(IHmacConfiguration configuration, IRestSharpHmacSigner signer)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "The configuration cannot be null.");
            if (signer == null)
                throw new ArgumentNullException(nameof(signer), "The signer cannot be null.");

            Configuration = configuration;
            Signer = signer;
            DateHeaderCulture = new CultureInfo(HmacConstants.DateHeaderCulture);
        }

        /// <summary>
        /// Signs a request using HMAC.
        /// </summary>
        /// <param name="client">The client executing the request.</param>
        /// <param name="request">The request that is being executed and should be signed.</param>
        /// <remarks>
        /// The following RestSharp parameters MUST be added BEFORE executing the request and may only be present once:
        /// - The custom username header;
        /// - The body (if there is one);
        /// - The Content-Type header (only if there is a body, RestSharp should do this automatically if a body and handler is set).
        /// 
        /// The following parameters MUST NOT be added before executing the request because they are added by this authenticator:
        /// - The Date header;
        /// - The Content-MD5 header (if a body is present);
        /// - The Authorization header.
        /// 
        /// Additional note 1:
        /// The Content-Type is extracted from the body parameter (from the <see cref="Parameter.Name"/> property), NOT from a header parameter.
        /// 
        /// Additional note 2:
        /// Keep in mind that when signing additional canonicalized headers, some will possibly not be available for signing, which may cause validation to fail.
        /// This is because RestSharps itself adds some headers after authentication and immediately before sending the request (the 'User-Agent' header for example).
        /// </remarks>
        /// <exception cref="ArgumentNullException">The client or request is null.</exception>
        /// <exception cref="HmacConfigurationException">One or more of the configuration parameters are invalid.</exception>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "The client cannot be null.");
            if (request == null)
                throw new ArgumentNullException(nameof(request), "The request cannot be null.");
            if (string.IsNullOrEmpty(Configuration.AuthorizationScheme))
                throw new HmacConfigurationException("The authorization scheme cannot be null or empty.");

            string body = GetBody(client, request);
            SetDate(request);
            SetContentMd5(request, body);
            string signature = CreateSignature(client, request);
            AddAuthorizationHeader(request, signature);
        }

        /// <summary>
        /// Retrieves the request body as a string.
        /// </summary>
        /// <param name="client">The client in which to search for the body.</param>
        /// <param name="request">The request in which to search for the body.</param>
        /// <returns>The request body as a <see cref="string"/> if found; otherwise <c>null</c>.</returns>
        protected virtual string GetBody(IRestClient client, IRestRequest request)
        {
            string body = null;
            Parameter bodyParameter = request.Parameters.GetBodyParameter(client.DefaultParameters);
            if (bodyParameter?.Value != null)
                body = bodyParameter.Value.ToString();
            return body;
        }

        /// <summary>
        /// Sets the HTTP Date header for the request.
        /// </summary>
        /// <param name="request">The request in which to set the date.</param>
        protected virtual void SetDate(IRestRequest request)
        {
            string date = DateTime.UtcNow.ToString(HmacConstants.DateHeaderFormat, DateHeaderCulture);
            request.AddParameter(HmacConstants.DateHeaderName, date, ParameterType.HttpHeader);
        }

        /// <summary>
        /// Creates an MD5 content hash from the body and sets it in the HTTP Content-MD5 header for the request.
        /// </summary>
        /// <param name="request">The request in which to set the hash.</param>
        /// <param name="body">The body to hash.</param>
        protected virtual void SetContentMd5(IRestRequest request, string body)
        {
            if (string.IsNullOrEmpty(body))
                return;
            
            string contentMd5 = Signer.CreateBase64Md5Hash(body);
            request.AddParameter(HmacConstants.ContentMd5HeaderName, contentMd5, ParameterType.HttpHeader);
        }

        /// <summary>
        /// Creates a signature for the request.
        /// </summary>
        /// <param name="client">The client containing parameters to use for signing.</param>
        /// <param name="request">The request containing parameters to use for signing.</param>
        /// <returns>The signature as a <see cref="string"/>.</returns>
        protected virtual string CreateSignature(IRestClient client, IRestRequest request)
        {
            HmacSignatureData signatureData = Signer.GetSignatureDataFromRestRequest(client, request);
            return Signer.CreateSignature(signatureData);
        }

        /// <summary>
        /// Adds the HTTP Authorization header with the signature to the request.
        /// </summary>
        /// <param name="request">The request in which to set the authorization.</param>
        /// <param name="signature">The signature to add to the header.</param>
        protected virtual void AddAuthorizationHeader(IRestRequest request, string signature) => Signer.AddAuthorizationHeader(request, signature);
    }
}
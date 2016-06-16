﻿using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Web;

namespace Donker.Hmac.Signing
{
    /// <summary>
    /// Interface for classes that sign HTTP requests using HMAC.
    /// </summary>
    public interface IHmacSigner
    {
        /// <summary>
        /// Gets all required signature data, if found, from an HTTP request message.
        /// </summary>
        /// <param name="request">The request message to get the data from.</param>
        /// <returns>The extracted data as an <see cref="HmacSignatureData"/> object.</returns>
        HmacSignatureData GetSignatureDataFromHttpRequest(HttpRequestMessage request);
        /// <summary>
        /// Gets all required signature data, if found, from an HTTP request.
        /// </summary>
        /// <param name="request">The request to get the data from.</param>
        /// <returns>The extracted data as an <see cref="HmacSignatureData"/> object.</returns>
        HmacSignatureData GetSignatureDataFromHttpRequest(HttpRequestBase request);
        /// <summary>
        /// Creates a HMAC signature from the supplied data.
        /// </summary>
        /// <param name="signatureData">The data to create a signature from.</param>
        /// <returns>The signature as a <see cref="string"/>.</returns>
        string CreateSignature(HmacSignatureData signatureData);
        /// <summary>
        /// Computes an MD5 hash from a stream.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a <see cref="byte"/> array.</returns>
        byte[] CreateMd5Hash(Stream content);
        /// <summary>
        /// Computes an MD5 hash from a byte array.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a <see cref="byte"/> array.</returns>
        byte[] CreateMd5Hash(byte[] content);
        /// <summary>
        /// Computes an MD5 hash from a string.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a <see cref="byte"/> array.</returns>
        byte[] CreateMd5Hash(string content);
        /// <summary>
        /// Computes an MD5 hash from a stream and returns it as a base64 converted string.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a base64 <see cref="string"/>.</returns>
        string CreateBase64Md5Hash(Stream content);
        /// <summary>
        /// Computes an MD5 hash from a byte array and returns it as a base64 converted string.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a base64 <see cref="string"/>.</returns>
        string CreateBase64Md5Hash(byte[] content);
        /// <summary>
        /// Computes an MD5 hash from a string and returns it as a base64 converted string.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The hash as a base64 <see cref="string"/>.</returns>
        string CreateBase64Md5Hash(string content);
        /// <summary>
        /// Creates a string from a header <see cref="NameValueCollection"/> where the headers are canonicalized.
        /// </summary>
        /// <param name="headers">The collection of headers to canonicalize.</param>
        /// <returns>The canonicalized headers as a single <see cref="string"/>.</returns>
        string CreateCanonicalizedHeadersString(NameValueCollection headers);
        /// <summary>
        /// Adds the HTTP Authorization header with the signature to the request.
        /// </summary>
        /// <param name="request">The request in which to set the authorization.</param>
        /// <param name="signature">The signature to add to the header.</param>
        void AddAuthorizationHeader(HttpRequestMessage request, string signature);
        /// <summary>
        /// Adds the HTTP Authorization header with the signature to the request.
        /// </summary>
        /// <param name="request">The request in which to set the authorization.</param>
        /// <param name="signature">The signature to add to the header.</param>
        void AddAuthorizationHeader(HttpRequestBase request, string signature);
    }
}
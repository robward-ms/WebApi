// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="ODataBatchReader"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchReaderExtensions
    {
        /// <summary>
        /// Reads a ChangeSet request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch Id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpRequest"/> in the ChangeSet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public static async Task<IList<HttpContext>> ReadChangeSetRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.ChangesetStart)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.ChangesetStart.ToString());
            }

            Guid changeSetId = Guid.NewGuid();
            List<HttpContext> contexts = new List<HttpContext>();
            while (reader.Read() && reader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (reader.State == ODataBatchReaderState.Operation)
                {
                    contexts.Add(await ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken));
                }
            }
            return contexts;
        }

        /// <summary>
        /// Reads an Operation request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing the operation.</returns>
        public static Task<HttpContext> ReadOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, null, cancellationToken, bufferContentStream);
        }

        /// <summary>
        /// Reads an Operation request in a ChangeSet.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing a ChangeSet operation</returns>
        public static Task<HttpContext> ReadChangeSetOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, Guid changeSetId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken, bufferContentStream);
        }

        private static async Task<HttpContext> ReadOperationInternalAsync(
            ODataBatchReader reader, HttpContext originalContext, Guid batchId, Guid? changeSetId, CancellationToken cancellationToken, bool bufferContentStream = true)
        {
            ODataBatchOperationRequestMessage batchRequest = reader.CreateOperationRequestMessage();

            HttpContext context = CreateHttpContext(originalContext);
            HttpRequest request = context.Request;

            request.Method = batchRequest.Method;
            request.CopyAbsoluteUrl(batchRequest.Url);

            if (bufferContentStream)
            {
                using (Stream stream = batchRequest.GetStream())
                {
                    MemoryStream bufferedStream = new MemoryStream();
                    // Passing in the default buffer size of 81920 so that we can also pass in a cancellation token
                    await stream.CopyToAsync(bufferedStream, bufferSize: 81920, cancellationToken: cancellationToken);
                    bufferedStream.Position = 0;
                    request.Body = bufferedStream;
                }
            }
            else
            {
                request.Body = batchRequest.GetStream();
            }

            foreach (var header in batchRequest.Headers)
            {
                // Copy headers from batch, overwriting any existing
                // headers.
                string headerName = header.Key;
                string headerValue = header.Value;
                request.Headers[headerName] = headerValue;
            }

            request.SetODataBatchId(batchId);
            request.SetODataContentId(batchRequest.ContentId);

            if (changeSetId != null && changeSetId.HasValue)
            {
                request.SetODataChangeSetId(changeSetId.Value);
            }

            return context;
        }

        private static HttpContext CreateHttpContext(HttpContext originalContext)
        {
            // Clone the features so that a new set is used for each context.
            // The features themselves will be reused but not the collection. We
            // store the request container as a feature of the request and we don't want
            // the features add to one context/request to be visible on another.
            IFeatureCollection features = new FeatureCollection();
            foreach (KeyValuePair<Type, object> kvp in originalContext.Features)
            {
                // Don't include the OData features. They may already
                // be present. This will get-recreated later.
                if (kvp.Key == typeof(IODataBatchFeature) ||
                    kvp.Key == typeof(IODataFeature))
                {
                    continue;
                }

                features[kvp.Key] = kvp.Value;
            }

            // Create a context from the factory or use the default context.
            HttpContext context = null;
            IHttpContextFactory httpContextFactory = originalContext.RequestServices.GetRequiredService<IHttpContextFactory>();
            if (httpContextFactory != null)
            {
                context = httpContextFactory.Create(features);
            }
            else
            {
                context = new DefaultHttpContext(features);
            }

            // Clone the context.
            //context.User = originalContext.User;
            //context.Items = originalContext.Items;
            //context.RequestServices = originalContext.RequestServices;
            //context.RequestAborted = originalContext.RequestAborted;
            //context.TraceIdentifier = originalContext.TraceIdentifier;

            // Clone parts of the request.
            //context.Request.Cookies = originalContext.Request.Cookies;
            //foreach (KeyValuePair<string, StringValues> header in originalContext.Request.Headers)
            //{
            //    context.Request.Headers.Add(header);
            //}

            return context;
        }
    }
}
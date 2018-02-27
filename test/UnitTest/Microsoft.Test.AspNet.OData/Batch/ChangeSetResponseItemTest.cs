// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;
#else
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Batch
{
    public class ChangeSetResponseItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            var contexts = CreateResponseContext(0);
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(contexts);

            Assert.Same(contexts, GetResponseContext(responseItem));
        }

        [Fact]
        public void Constructor_NullResponses_Throws()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ChangeSetResponseItem(null),
#if NETCORE
                "contexts");
#else
                "responses");
#endif
        }

        [Fact]
        public async Task WriteResponseAsync_NullWriter_Throws()
        {
            var contexts = CreateResponseContext(1);
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(contexts);

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => WriteResponseAsync(responseItem, null),
                "writer");
        }

        [Fact]
        public async Task WriteResponseAsync_WritesChangeSet()
        {
            var contexts = CreateResponseContext(2);
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(contexts);

            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            await WriteResponseAsync(responseItem, batchWriter);

            batchWriter.WriteEndBatch();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("No Content", responseString);
        }

        [Fact]
        public void IsResponseSuccessful_TestResponse()
        {
            // Arrange
            var contexts = CreateResponseContext(6);
            ChangeSetResponseItem successResponseItem = new ChangeSetResponseItem(contexts.Take(3));
            ChangeSetResponseItem errorResponseItem = new ChangeSetResponseItem(contexts.Skip(3).Take(3));

            // Act & Assert
            Assert.True(successResponseItem.IsResponseSuccessful());
            Assert.False(errorResponseItem.IsResponseSuccessful());
        }

#if NETFX // Only requests in AspNet as disposable.
        [Fact]
        public void Dispose_DisposesAllHttpResponseMessages()
        {
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(new MockHttpResponseMessage[]
            {
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage()
            });

            responseItem.Dispose();

            Assert.Equal(3, responseItem.Responses.Count());
            foreach (var response in responseItem.Responses)
            {
                Assert.True(((MockHttpResponseMessage)response).IsDisposed);
            }
        }
#endif

#if NETCORE
        private IEnumerable<HttpContext> CreateResponseContext(int itemCount)
        {
            HttpStatusCode[] statusCodes = {
                HttpStatusCode.Accepted,
                HttpStatusCode.NoContent,
                HttpStatusCode.OK,
                HttpStatusCode.Created,
                HttpStatusCode.BadGateway,
                HttpStatusCode.Ambiguous,
            };

            List<HttpContext> contexts = new List<HttpContext>();
            for (int i = 0; i < itemCount; i++)
            {
                DefaultHttpContext context = new DefaultHttpContext();
                context.Response.StatusCode = (int)statusCodes[i];
                contexts.Add(context);
            }

            return contexts;
        }
#else
        private IEnumerable<HttpResponseMessage> CreateResponseContext(int itemCount)
        {
            HttpStatusCode[] statusCodes = {
                HttpStatusCode.Accepted,
                HttpStatusCode.NoContent,
                HttpStatusCode.OK,
                HttpStatusCode.Created,
                HttpStatusCode.BadGateway,
                HttpStatusCode.Ambiguous,
            };

            List<HttpResponseMessage> contexts = new List<HttpResponseMessage>();
            for (int i = 0; i < itemCount; i++)
            {
                HttpResponseMessage request = new HttpResponseMessage(statusCodes[i]);
                contexts.Add(request);
            }

            return contexts;
        }
#endif

#if NETCORE
        private IEnumerable<HttpContext> GetResponseContext(ChangeSetResponseItem batchResponse)
        {
            return batchResponse.Contexts;
        }
#else
        private IEnumerable<HttpResponseMessage> GetResponseContext(ChangeSetResponseItem batchResponse)
        {
            return batchResponse.Responses;
        }
#endif

        private Task WriteResponseAsync(ChangeSetResponseItem batchResponse, ODataBatchWriter writer)
        {
#if NETCORE
            return batchResponse.WriteResponseAsync(writer);
#else
            return batchResponse.WriteResponseAsync(writer, CancellationToken.None);
#endif
        }
    }
}
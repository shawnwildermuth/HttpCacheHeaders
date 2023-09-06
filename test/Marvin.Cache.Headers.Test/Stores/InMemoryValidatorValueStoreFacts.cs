﻿// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/HttpCacheHeaders

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Marvin.Cache.Headers.Interfaces;
using Marvin.Cache.Headers.Stores;
using Moq;
using Xunit;

namespace Marvin.Cache.Headers.Test.Stores
{
    public class InMemoryValidatorValueStoreFacts
    {
        [Fact]
        public void Ctor_ThrowsArgumentNullExcpeiton_WhenStoreKeySerializerIsNull()
        {
            IStoreKeySerializer storeKeySerializer = null;
            Assert.Throws<ArgumentNullException>(() =>new InMemoryValidatorValueStore(storeKeySerializer));
        }
        
        [Fact]
        public async Task GetAsync_Returns_Stored_ValidatorValue()
        {
            // arrange
            var referenceTime = DateTimeOffset.UtcNow;
            var requestKey = new StoreKey
            {
                { "resourcePath", "/v1/gemeenten/11057" },
                { "queryString", string.Empty },
                { "requestHeaderValues", string.Join("-", new List<string> {"text/plain", "gzip"})}
            };
            
            var requestKeyJson =JsonSerializer.Serialize(requestKey);
            var storeKeySerializer =new Mock<IStoreKeySerializer>();
storeKeySerializer.Setup(x =>x.SerializeStoreKey(requestKey)).Returns(requestKeyJson);
storeKeySerializer.Setup(x => x.DeserializeStoreKey(requestKeyJson)).Returns(requestKey);
            
var target = new InMemoryValidatorValueStore(storeKeySerializer.Object);
            
await target.SetAsync(requestKey, new ValidatorValue(new ETag(ETagType.Strong, "test"), referenceTime));

            // act
            var result = await target.GetAsync(requestKey);

            // assert
            Assert.NotNull(result);
            Assert.Equal(ETagType.Strong, result.ETag.ETagType);
            Assert.Equal("test", result.ETag.Value);
            Assert.Equal(result.LastModified, referenceTime);
        }

        [Fact]
        public async Task GetAsync_DoesNotReturn_Unknown_ValidatorValue()
        {
            // arrange
            var referenceTime = DateTimeOffset.UtcNow;
            var requestKey = new StoreKey
            {
                { "resourcePath", "/v1/gemeenten/11057" },
                { "queryString", string.Empty },
                { "requestHeaderValues", string.Join("-", new List<string> {"text/plain", "gzip"})}
            };
            var requestKey2 = new StoreKey
            {
                { "resourcePath", "/v1/gemeenten/1" },
                { "queryString", string.Empty },
                { "requestHeaderValues", string.Join("-", new List<string> {"text/plain", "gzip"})}
            };
            
            var storeKeySerializer = new Mock<IStoreKeySerializer>();
            var requestKeyJson =JsonSerializer.Serialize(requestKey);
            var requestKey2Json = JsonSerializer.Serialize(requestKey2);
            storeKeySerializer.Setup(x =>x.SerializeStoreKey(requestKey)).Returns(requestKeyJson);
            storeKeySerializer.Setup(x => x.SerializeStoreKey(requestKey2)).Returns(requestKey2Json);

            var target = new InMemoryValidatorValueStore(storeKeySerializer.Object);
            await target.SetAsync(requestKey, new ValidatorValue(new ETag(ETagType.Strong, "test"), referenceTime));

            // act
            var result = await target.GetAsync(requestKey2);

            // assert
            Assert.Null(result);
        }
    }
}

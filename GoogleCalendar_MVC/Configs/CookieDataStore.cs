using Google.Apis.Json;
using Google.Apis.Util.Store;
using GoogleCalendar_MVC.Utilities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Configs
{
    /// <summary>
    /// File data store that implements <see cref="IDataStore"/>. This store creates a different file for each 
    /// combination of type and key. This file data store stores a JSON format of the specified object.
    /// </summary>
    public class CookieDataStore : IDataStore
    {
        private static readonly Task CompletedTask = Task.FromResult(0);
        private readonly IHttpContextAccessor _httpContext;

        public CookieDataStore(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            var stringEncrypted = new MyStringEncrypt().Encrypt(serialized, StaticDetails.Secret);

            var options = new CookieOptions();
            options.Expires = DateTime.UtcNow.AddDays(365);
            options.HttpOnly = true;
            options.Secure = true;
            _httpContext.HttpContext.Response.Cookies.Append(GetKeyStore(key, typeof(T)), stringEncrypted, options);

            return CompletedTask;
        }


        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            if (IsExistsCredential(key, typeof(T)))
            {
                _httpContext.HttpContext.Response.Cookies.Delete(GetKeyStore(key, typeof(T)));
            }

            return CompletedTask;
        }


        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            T credential = default(T);
            if (IsExistsCredential(key, typeof(T)))
            {
                string valueEncrypted = _httpContext.HttpContext.Request.Cookies[GetKeyStore(key, typeof(T))];
                string valueDecrypted = new MyStringEncrypt().Decrypt(valueEncrypted, StaticDetails.Secret);
                try
                {
                    credential = NewtonsoftJsonSerializer.Instance.Deserialize<T>(valueDecrypted);
                }
                catch (Exception)
                {
                    DeleteAsync<T>(key).Wait();
                }
            }

            tcs.SetResult(credential);
            return tcs.Task;
        }


        public Task ClearAsync()
        {
            foreach (var key in _httpContext.HttpContext.Request.Cookies.Keys)
            {
                if (key.Contains(StaticDetails.cookieGoogleCredentials))
                {
                    _httpContext.HttpContext.Response.Cookies.Delete(key);
                }
            }

            return CompletedTask;
        }

        private bool IsExistsCredential(string key, Type t)
        {
            if (_httpContext.HttpContext.Request.Cookies.ContainsKey(GetKeyStore(key, t)))
                return _httpContext.HttpContext.Request.Cookies[GetKeyStore(key, t)] != "";

            return false;
        }

        private string GetKeyStore(string key, Type t)
        {
            return key + t.Name;
        }
    }
}

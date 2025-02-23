using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TiketManagementV2.Model
{
    public class ApiServices
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public ApiServices()
        {
            _baseUrl = Properties.Settings.Default.host;
            _httpClient = new HttpClient();
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        private void EnsureValidResponse(HttpResponseMessage response)
        {
            if ((int)response.StatusCode != 422 &&
                response.StatusCode != System.Net.HttpStatusCode.Unauthorized &&
                !response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        #region GET Methods
        public async Task<dynamic> GetAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request: {ex.Message}");
                throw;
            }
        }
        public async Task<dynamic> GetLocationAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetWithHeaderAsync(string endpoint, Dictionary<string, string> headers)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request with headers: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetWithBodyAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{endpoint}");
                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request with body: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetWithHeaderAndBodyAsync<TRequest>(
            string endpoint,
            Dictionary<string, string> headers,
            TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GET request with headers and body: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region POST Methods
        public async Task<dynamic> PostAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", null);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PostWithHeaderAsync(string endpoint, Dictionary<string, string> headers)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request with headers: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PostWithBodyAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
                EnsureValidResponse(response);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(responseContent, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request with body: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PostWithHeaderAndBodyAsync<TRequest>(
            string endpoint,
            Dictionary<string, string> headers,
            TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request with headers and body: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region PUT Methods
        public async Task<dynamic> PutAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", null);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PutWithHeaderAsync(string endpoint, Dictionary<string, string> headers)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request with headers: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PutWithBodyAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", content);
                EnsureValidResponse(response);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(responseContent, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request with body: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> PutWithHeaderAndBodyAsync<TRequest>(
            string endpoint,
            Dictionary<string, string> headers,
            TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request with headers and body: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region DELETE Methods
        public async Task<dynamic> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DELETE request: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> DeleteWithHeaderAsync(string endpoint, Dictionary<string, string> headers)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DELETE request with headers: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> DeleteWithBodyAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{endpoint}");
                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DELETE request with body: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> DeleteWithHeaderAndBodyAsync<TRequest>(
            string endpoint,
            Dictionary<string, string> headers,
            TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{endpoint}");
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var json = JsonConvert.SerializeObject(data, _serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DELETE request with headers and body: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Upload Methods

        public async Task<dynamic> UploadSingleImageWithPutAsync(
            string endpoint,
            Dictionary<string, string> headers,
            byte[] imageBytes,
            object requestBody = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/{endpoint}");

                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var formData = new MultipartFormDataContent();

                // Add form fields from requestBody
                if (requestBody != null)
                {
                    var json = JsonConvert.SerializeObject(requestBody);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    foreach (var item in dict)
                    {
                        formData.Add(new StringContent(item.Value?.ToString() ?? ""), item.Key);
                    }
                }

                // Add image file
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(imageContent, "image", "image.jpg"); // Changed to "image" to match multer field name

                request.Content = formData;

                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PUT request for uploading a single image: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> UploadMultipleImagesWithPostAsync(
            string endpoint,
            Dictionary<string, string> headers,
            List<Tuple<string, byte[]>> imageFiles,
            object requestBody = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");

                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var formData = new MultipartFormDataContent();

                // Add form fields from requestBody
                if (requestBody != null)
                {
                    var json = JsonConvert.SerializeObject(requestBody);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    foreach (var item in dict)
                    {
                        formData.Add(new StringContent(item.Value?.ToString() ?? ""), item.Key);
                    }
                }

                // Add image files
                foreach (var imageFile in imageFiles)
                {
                    var fileName = imageFile.Item1;
                    var fileBytes = imageFile.Item2;
                    var imageContent = new ByteArrayContent(fileBytes);
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    formData.Add(imageContent, "preview", fileName); // Changed to "images" to match multer field name
                }

                request.Content = formData;
                var response = await _httpClient.SendAsync(request);
                EnsureValidResponse(response);

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in POST request for uploading multiple images: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
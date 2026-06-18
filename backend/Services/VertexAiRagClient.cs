using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace GoogleCloud.VertexAI
{
    public class VertexAiRagClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public VertexAiRagClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<string> CreateRagCorpusAsync(string projectId, string location, string corpusDisplayName)
        {
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/ragCorpora";

            var payload = new { display_name = corpusDisplayName };

            return await ExecutePostRequestAsync(endpoint, payload);
        }

        public async Task<string> ListRagCorporaAsync(string projectId, string location)
        {
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/ragCorpora";

            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetOperationAsync(string location, string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentException("operationName es obligatorio.", nameof(operationName));
            }

            var normalizedOperationName = operationName.Trim();
            if (normalizedOperationName.StartsWith("/"))
            {
                normalizedOperationName = normalizedOperationName.TrimStart('/');
            }

            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/{normalizedOperationName}";
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteRagCorpusAsync(string location, string ragCorpusName)
        {
            if (string.IsNullOrWhiteSpace(ragCorpusName))
            {
                throw new ArgumentException("ragCorpusName es obligatorio.", nameof(ragCorpusName));
            }

            var normalizedCorpusName = ragCorpusName.Trim().TrimStart('/');
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/{normalizedCorpusName}";
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UploadRagFileDirectlyAsync(
            string projectId,
            string location,
            string ragCorpusId,
            string filePath,
            string? displayName = null)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("No se encontró el archivo.", filePath);

            var normalizedCorpusId = NormalizeCorpusId(ragCorpusId);
            if (string.IsNullOrWhiteSpace(normalizedCorpusId))
            {
                throw new ArgumentException("ragCorpusId es obligatorio.", nameof(ragCorpusId));
            }

            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/ragCorpora/{normalizedCorpusId}/ragFiles:upload";
            var accessToken = await GetAccessTokenAsync();

            using var formContent = new MultipartFormDataContent();

            var metadata = new
            {
                rag_file = new
                {
                    display_name = displayName ?? Path.GetFileName(filePath)
                }
            };

            var jsonMetadata = JsonSerializer.Serialize(metadata, _jsonOptions);
            var metadataContent = new StringContent(jsonMetadata, Encoding.UTF8, "application/json");
            formContent.Add(metadataContent, "metadata");

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            formContent.Add(fileContent, "file", Path.GetFileName(filePath));

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = formContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UploadFileToGcsAsync(
            string bucketName,
            string objectName,
            string filePath,
            string? contentType = null)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("bucketName es obligatorio.", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentException("objectName es obligatorio.", nameof(objectName));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("No se encontró el archivo.", filePath);
            }

            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopedCredential = credential.CreateScoped(new[] { "https://www.googleapis.com/auth/devstorage.read_write" });
            var storageClient = await new StorageClientBuilder
            {
                Credential = scopedCredential
            }.BuildAsync();

            await using var fileStream = File.OpenRead(filePath);
            await storageClient.UploadObjectAsync(bucketName, objectName, contentType ?? "application/octet-stream", fileStream);

            return $"gs://{bucketName}/{objectName}";
        }

        public async Task<List<string>> ListGcsUrisByPrefixAsync(string bucketName, string prefix)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("bucketName es obligatorio.", nameof(bucketName));
            }

            var normalizedPrefix = prefix?.Trim().TrimStart('/') ?? string.Empty;

            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopedCredential = credential.CreateScoped(new[] { "https://www.googleapis.com/auth/devstorage.read_only" });
            var storageClient = await new StorageClientBuilder
            {
                Credential = scopedCredential
            }.BuildAsync();

            var uris = new List<string>();
            await foreach (var obj in storageClient.ListObjectsAsync(bucketName, normalizedPrefix))
            {
                if (!string.IsNullOrWhiteSpace(obj.Name) && !obj.Name.EndsWith('/'))
                {
                    uris.Add($"gs://{bucketName}/{obj.Name}");
                }
            }

            return uris;
        }

        public async Task<bool> DeleteGcsObjectByUriAsync(string gcsUri, int maxAttempts = 3)
        {
            if (!TryParseGcsUri(gcsUri, out var bucketName, out var objectName))
            {
                throw new ArgumentException("gcsUri no tiene un formato valido.", nameof(gcsUri));
            }

            if (maxAttempts < 1)
            {
                maxAttempts = 1;
            }

            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopedCredential = credential.CreateScoped(new[] { "https://www.googleapis.com/auth/devstorage.read_write" });
            var storageClient = await new StorageClientBuilder
            {
                Credential = scopedCredential
            }.BuildAsync();

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await storageClient.DeleteObjectAsync(bucketName, objectName);
                }
                catch (GoogleApiException ex) when ((int)ex.HttpStatusCode == 404)
                {
                    return true;
                }

                var stillExists = await GcsObjectExistsAsync(storageClient, bucketName, objectName);
                if (!stillExists)
                {
                    return true;
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(400);
                }
            }

            return false;
        }

        public async Task<string> ListRagFilesAsync(string projectId, string location, string ragCorpusId)
        {
            var normalizedCorpusId = NormalizeCorpusId(ragCorpusId);
            if (string.IsNullOrWhiteSpace(normalizedCorpusId))
            {
                throw new ArgumentException("ragCorpusId es obligatorio.", nameof(ragCorpusId));
            }

            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/ragCorpora/{normalizedCorpusId}/ragFiles";
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task DeleteRagFileAsync(string location, string ragFileName)
        {
            if (string.IsNullOrWhiteSpace(ragFileName))
            {
                throw new ArgumentException("ragFileName es obligatorio.", nameof(ragFileName));
            }

            var normalizedRagFileName = ragFileName.Trim().TrimStart('/');
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/{normalizedRagFileName}";
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }
        }

        public async Task<Dictionary<string, bool>> DeleteRagFilesByGcsUrisAsync(
            string projectId,
            string location,
            string ragCorpusId,
            IEnumerable<string> gcsUris)
        {
            var targets = new HashSet<string>(gcsUris.Where(uri => !string.IsNullOrWhiteSpace(uri)), StringComparer.OrdinalIgnoreCase);
            var result = targets.ToDictionary(uri => uri, _ => false, StringComparer.OrdinalIgnoreCase);

            if (targets.Count == 0)
            {
                return result;
            }

            var raw = await ListRagFilesAsync(projectId, location, ragCorpusId);
            using var listDoc = JsonDocument.Parse(raw);

            if (!listDoc.RootElement.TryGetProperty("ragFiles", out var ragFilesElement)
                || ragFilesElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var ragFile in ragFilesElement.EnumerateArray())
            {
                if (!ragFile.TryGetProperty("name", out var nameProperty))
                {
                    continue;
                }

                var ragFileName = nameProperty.GetString();
                if (string.IsNullOrWhiteSpace(ragFileName))
                {
                    continue;
                }

                var uriCandidates = ExtractPrimaryGcsUris(ragFile);
                var matchedUris = uriCandidates.Where(uri => result.ContainsKey(uri) && !result[uri]).ToList();
                if (matchedUris.Count == 0)
                {
                    continue;
                }

                await DeleteRagFileAsync(location, ragFileName);
                foreach (var uri in matchedUris)
                {
                    result[uri] = true;
                }
            }

            return result;
        }

        private static string NormalizeCorpusId(string ragCorpusId)
        {
            if (string.IsNullOrWhiteSpace(ragCorpusId))
            {
                return string.Empty;
            }

            var value = ragCorpusId.Trim().Trim('/');
            var marker = "/ragCorpora/";
            var markerIndex = value.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (markerIndex >= 0)
            {
                return value[(markerIndex + marker.Length)..];
            }

            return value;
        }

        private static bool TryParseGcsUri(string gcsUri, out string bucketName, out string objectName)
        {
            bucketName = string.Empty;
            objectName = string.Empty;

            if (string.IsNullOrWhiteSpace(gcsUri))
            {
                return false;
            }

            gcsUri = gcsUri.Trim();

            const string prefix = "gs://";
            if (!gcsUri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var path = gcsUri[prefix.Length..];
            var separatorIndex = path.IndexOf('/');
            if (separatorIndex <= 0 || separatorIndex == path.Length - 1)
            {
                return false;
            }

            bucketName = path[..separatorIndex];
            objectName = path[(separatorIndex + 1)..];
            return true;
        }

        private static async Task<bool> GcsObjectExistsAsync(StorageClient storageClient, string bucketName, string objectName)
        {
            try
            {
                await storageClient.GetObjectAsync(bucketName, objectName);
                return true;
            }
            catch (GoogleApiException ex) when ((int)ex.HttpStatusCode == 404)
            {
                return false;
            }
        }

        private static HashSet<string> ExtractPrimaryGcsUris(JsonElement ragFile)
        {
            var uris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            TryAddStringPropertyAsGcsUri(ragFile, "sourceUri", uris);
            TryAddStringPropertyAsGcsUri(ragFile, "source_uri", uris);
            TryAddStringPropertyAsGcsUri(ragFile, "gcsUri", uris);
            TryAddStringPropertyAsGcsUri(ragFile, "gcs_uri", uris);
            TryAddStringPropertyAsGcsUri(ragFile, "uri", uris);

            if (ragFile.TryGetProperty("gcsSource", out var gcsSource)
                || ragFile.TryGetProperty("gcs_source", out gcsSource))
            {
                TryAddStringPropertyAsGcsUri(gcsSource, "uri", uris);
                TryAddStringPropertyAsGcsUri(gcsSource, "sourceUri", uris);
                TryAddStringPropertyAsGcsUri(gcsSource, "source_uri", uris);

                if (gcsSource.TryGetProperty("uris", out var urisElement)
                    && urisElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in urisElement.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.String)
                        {
                            continue;
                        }

                        var value = item.GetString();
                        if (!string.IsNullOrWhiteSpace(value)
                            && value.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
                        {
                            uris.Add(value);
                        }
                    }
                }
            }

            return uris;
        }

        private static void TryAddStringPropertyAsGcsUri(JsonElement element, string propertyName, HashSet<string> uris)
        {
            if (!element.TryGetProperty(propertyName, out var prop)
                || prop.ValueKind != JsonValueKind.String)
            {
                return;
            }

            var value = prop.GetString();
            if (!string.IsNullOrWhiteSpace(value)
                && value.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
            {
                uris.Add(value);
            }
        }

        public async Task<string> ImportRagFilesAsync(
            string projectId,
            string location,
            string ragCorpusId,
            IEnumerable<string> gcsUris,
            int chunkSize = 500,
            int chunkOverlap = 50,
            int maxEmbeddingRequestsPerMin = 1000)
        {
            var normalizedCorpusId = NormalizeCorpusId(ragCorpusId);
            if (string.IsNullOrWhiteSpace(normalizedCorpusId))
            {
                throw new ArgumentException("ragCorpusId es obligatorio.", nameof(ragCorpusId));
            }

            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/ragCorpora/{normalizedCorpusId}/ragFiles:import";

            var payload = new
            {
                import_rag_files_config = new
                {
                    gcs_source = new { uris = gcsUris },
                    rag_file_transformation_config = new
                    {
                        rag_file_chunking_config = new
                        {
                            fixed_length_chunking = new
                            {
                                chunk_size = chunkSize,
                                chunk_overlap = chunkOverlap
                            }
                        }
                    },
                    max_embedding_requests_per_min = maxEmbeddingRequestsPerMin
                }
            };

            return await ExecutePostRequestAsync(endpoint, payload);
        }

        public async Task<string> RetrieveContextsAsync(
            string projectId,
            string location,
            string ragCorpusResource,
            string queryText,
            double? vectorDistanceThreshold = null,
            int similarityTopK = 5)
        {
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}:retrieveContexts";

            var payloads = new object[]
            {
                new
                {
                    vertex_rag_store = new
                    {
                        rag_resources = new[] { new { rag_corpus = ragCorpusResource } },
                        vector_distance_threshold = vectorDistanceThreshold
                    },
                    query = new
                    {
                        text = queryText,
                        similarity_top_k = similarityTopK
                    }
                },
                new
                {
                    vertex_rag_store = new
                    {
                        rag_resources = new[] { new { rag_corpus = ragCorpusResource } },
                        similarity_top_k = similarityTopK,
                        vector_distance_threshold = vectorDistanceThreshold
                    },
                    query = new
                    {
                        text = queryText
                    }
                },
                new
                {
                    vertex_rag_store = new
                    {
                        rag_resources = new[] { new { rag_corpus = ragCorpusResource } },
                        vector_distance_threshold = vectorDistanceThreshold
                    },
                    query = new
                    {
                        text = queryText
                    }
                }
            };

            HttpRequestException? lastError = null;
            foreach (var payload in payloads)
            {
                try
                {
                    return await ExecutePostRequestAsync(endpoint, payload);
                }
                catch (HttpRequestException ex)
                {
                    lastError = ex;
                    if (!ex.Message.Contains("Unknown name", StringComparison.OrdinalIgnoreCase))
                    {
                        throw;
                    }
                }
            }

            throw lastError ?? new HttpRequestException("No se pudo ejecutar retrieveContexts con los payloads compatibles.");
        }

        public async Task<string> GenerateContentWithRagAsync(
            string projectId,
            string location,
            string modelId,
            string ragCorpusResource,
            string inputPrompt,
            string generationMethod = "generateContent",
            double? vectorDistanceThreshold = null,
            int similarityTopK = 5)
        {
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{modelId}:{generationMethod}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "USER",
                        parts = new[] { new { text = inputPrompt } }
                    }
                },
                tools = new[]
                {
                    new
                    {
                        retrieval = new
                        {
                            disable_attribution = false,
                            vertex_rag_store = new
                            {
                                rag_resources = new[] { new { rag_corpus = ragCorpusResource } },
                                similarity_top_k = similarityTopK,
                                vector_distance_threshold = vectorDistanceThreshold
                            }
                        }
                    }
                }
            };

            return await ExecutePostRequestAsync(endpoint, payload);
        }

        public async Task<string> GenerateContentAsync(
            string projectId,
            string location,
            string modelId,
            string systemInstruction,
            IEnumerable<object> contents,
            string generationMethod = "generateContent",
            double temperature = 0.4,
            int maxOutputTokens = 1000)
        {
            var endpoint = $"https://aiplatform.googleapis.com/v1beta1/projects/{projectId}/locations/global/publishers/google/models/{modelId}:{generationMethod}";

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = systemInstruction }
                    }
                },
                contents,
                tools = new[]{
                    new { url_context = new { } }
                },
                generation_config = new
                {
                    temperature,
                    max_output_tokens = maxOutputTokens
                }
            };

            return await ExecutePostRequestAsync(endpoint, payload);
        }

        public async Task<string> GenerateContentAsync(
            string projectId,
            string location,
            string modelId,
            string systemInstruction,
            string userPrompt,
            string generationMethod = "generateContent",
            double temperature = 0.4,
            int maxOutputTokens = 1000)
        {
            var endpoint = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{modelId}:{generationMethod}";

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = systemInstruction }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = userPrompt }
                        }
                    }
                },
                generation_config = new
                {
                    temperature,
                    max_output_tokens = maxOutputTokens
                }
            };

            return await ExecutePostRequestAsync(endpoint, payload);
        }

        private async Task<string> ExecutePostRequestAsync(string endpoint, object payload)
        {
            var accessToken = await GetAccessTokenAsync();
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);

            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error de Google API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopedCredential = credential.CreateScoped(new[] { "https://www.googleapis.com/auth/cloud-platform" });
            return await ((ITokenAccess)scopedCredential).GetAccessTokenForRequestAsync();
        }
    }
}
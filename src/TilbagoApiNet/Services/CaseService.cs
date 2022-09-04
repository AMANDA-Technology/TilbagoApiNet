/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TilbagoApiNet.Abstractions.Models;
using TilbagoApiNet.Abstractions.Views;
using TilbagoApiNet.Interfaces;

namespace TilbagoApiNet.Services
{
    /// <summary>
    /// Tilbago case service endpoint
    /// </summary>
    public class CaseService : ICaseService
    {
        /// <summary>
        /// Tilbago connection handler
        /// </summary>
        private readonly IConnectionHandler _tilbagoConnectionHandler ;

        /// <summary>
        /// Inject connection handler at construction
        /// </summary>
        /// <param name="tilbagoConnectionHandler"></param>
        public CaseService(IConnectionHandler tilbagoConnectionHandler)
        {
            _tilbagoConnectionHandler = tilbagoConnectionHandler;
        }

        /// <summary>
        /// Add a new case on tilbago
        /// </summary>
        /// <param name="tilbagoCase"></param>
        /// <returns>Case ID</returns>
        public async Task<string?> CreateAsync(Case tilbagoCase)
        {
            // PUT /case
            var response = await _tilbagoConnectionHandler.Client.PutAsync("/case",
                new StringContent(JsonSerializer.Serialize(tilbagoCase), Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStreamAsync();

            // 200 OK -> case id
            if (response.IsSuccessStatusCode)
            {
                return (await JsonSerializer.DeserializeAsync<CaseCreateResult>(responseContent))?.CaseId;
            }

            // 401 Unauthorized = Invalid api_key
            // 402 Payment Required = Insufficient account balance
            // default = Unexpected error
            throw new((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
        }

        /// <summary>
        /// Add an attachment to a case on tilbago
        /// </summary>
        /// <returns>Attachment ID</returns>
        public async Task<string?> AddAttachmentAsync(string caseId, string fileName, byte[] fileContent)
        {
            // PUT /case/{caseId}/attachment
            throw new NotImplementedException();
            /*var response =
                await _tilbagoConnectionHandler.Client.PutAsync($"/case/{caseId}/status",
                    new ByteArrayContent(fileContent)); // TODO: Set Content-Disposition
            var responseContent = await response.Content.ReadAsStringAsync();

            // 200 OK -> attachmentId
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AddAttachmentResult>(responseContent)?.AttachmentId;
            }

            // 401 Unauthorized = Invalid api_key
            // default = Unexpected error
            throw new(JsonConvert.DeserializeObject<ErrorModel>(responseContent)?.Message);*/
        }

        /// <summary>
        /// Get the status of a case on tilbago
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public async Task<CaseStatus?> GetStatusAsync(string caseId)
        {
            // GET /case/{caseId}/status
            var response = await _tilbagoConnectionHandler.Client.GetAsync($"/case/{caseId}/status");
            var responseContent = await response.Content.ReadAsStreamAsync();

            // 200 OK -> caseStatus
            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<CaseStatus>(responseContent);
            }

            // 401 Unauthorized = Invalid api_key
            // default = Unexpected error
            throw new((await JsonSerializer.DeserializeAsync<ErrorModel>(responseContent))?.Message);
        }
    }
}

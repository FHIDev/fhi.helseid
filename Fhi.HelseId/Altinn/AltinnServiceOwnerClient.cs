using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fhi.HelseId.Altinn
{
    public class AltinnServiceOwnerClient : IAltinnServiceOwnerClient
    {
        private readonly HttpClient httpClient;

        public AltinnServiceOwnerClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        private async Task<HttpResponseMessage> GetWithForceEIAuthentication(string endpoint, CancellationToken cancellationToken = default, params string[] queryParams)
        {
            // Build and send a request, guaranteeing that the ForceEIAuthentication is sent as the first query parameter

            const string firstQueryParameter = "?ForceEIAuthentication"; // Force Enterprise Identified Authentication, meaning we authenticate using an Enterprise Certificate

            var baseUri = new UriBuilder(httpClient.BaseAddress + endpoint);
            // If the query length is 1 we assume the query string is only ? (or garbage) and can be replaced.
            // Otherwise we keep whatever is beyond the ?.
            baseUri.Query = baseUri.Query?.Length > 1 ? firstQueryParameter + "&" + baseUri.Query.Substring(1) : firstQueryParameter;

            if (queryParams != null && queryParams.Length > 0)
            {
                baseUri.Query += "&" + string.Join('&', queryParams);
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = baseUri.Uri
            };

            return await httpClient.SendAsync(request, cancellationToken);
        }

        public async IAsyncEnumerable<Organization> GetOrganizations(IEnumerable<string> orgNumbers, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var orgNo in orgNumbers)
            {
                var org = await GetOrganization(orgNo, cancellationToken);
                if (org != null)
                {
                    yield return org;
                }
            }
        }

        public async IAsyncEnumerable<Organization> GetOrganizationsOfTypes([EnumeratorCancellation] CancellationToken cancellationToken = default, params string[] types)
        {
            int skip = 0, take = 1000, taken = 1000;

            while (taken > 0)
            {
                var orgs = await GetOrganizationsOfTypes(types, skip, take, cancellationToken);
                taken = orgs.Length;
                skip += taken;
                foreach (var org in orgs)
                {
                    yield return org;
                }
            }
        }

        private async Task<Organization[]> GetOrganizationsOfTypes(string[] types, int skip, int take, CancellationToken cancellationToken = default)
        {
            var filter = string.Join(" or ", types.Select(type => $"Type eq '{type}'"));
            using var response = await GetWithForceEIAuthentication("organizations", cancellationToken, $"$filter={filter}", $"$skip={skip}", $"$top={take}");
            return await DeserializeIfSuccessful<Organization[]>(response);
        }

        public async IAsyncEnumerable<Organization> GetOrganizationsNotOfTypes([EnumeratorCancellation] CancellationToken cancellationToken = default, params string[] types)
        {
            int skip = 0, take = 1000, taken = 1000;

            while (taken > 0)
            {
                var orgs = await GetOrganizationsNotOfTypes(types, skip, take, cancellationToken);
                taken = orgs.Length;
                skip += taken;
                foreach (var org in orgs)
                {
                    yield return org;
                }
            }
        }

        private async Task<Organization[]> GetOrganizationsNotOfTypes(string[] types, int skip, int take, CancellationToken cancellationToken = default)
        {
            var filter = string.Join(" and ", types.Select(type => $"Type ne '{type}'"));
            using var response = await GetWithForceEIAuthentication("organizations", cancellationToken, $"$filter={filter}", $"$skip={skip}", $"$top={take}");
            return await DeserializeIfSuccessful<Organization[]>(response);
        }

        public async Task<Organization?> GetOrganization(string orgNo, CancellationToken cancellation = default)
        {
            using var response = await GetWithForceEIAuthentication($"organizations/{orgNo}", cancellation);
            if (response.StatusCode == HttpStatusCode.BadRequest && response.ReasonPhrase == $"Invalid organization number: {orgNo}")
            {
                return null;
            }

            return await DeserializeIfSuccessful<Organization>(response);
        }
        public async IAsyncEnumerable<Reportee> GetReportees(string subject, string serviceCode, int serviceEditionCode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int skip = 0, take = 1000, taken = 1000;

            while (taken > 0)
            {
                var reportees = await GetReportees(subject, serviceCode, serviceEditionCode, skip, take, cancellationToken);
                if (reportees == null)
                {
                    yield break;
                }

                taken = reportees.Length;
                skip += taken;
                foreach (var reportee in reportees)
                {
                    yield return reportee;
                }
            }
        }

        public async Task<Reportee[]?> GetReportees(string subject, string serviceCode, int serviceEditionCode, int skip, int take, CancellationToken cancellationToken = default)
        {
            using var response = await GetWithForceEIAuthentication($"reportees", cancellationToken, $"subject={subject}", $"serviceCode={serviceCode}", $"serviceEdition={serviceEditionCode}", $"$skip={skip}", $"$top={take}");

            if (response.StatusCode == HttpStatusCode.BadRequest && response.ReasonPhrase == $"Invalid social security number: {subject}")
            {
                return null;
            }

            var reportees = await DeserializeIfSuccessful<Reportee[]>(response);
            return reportees;
        }

        public async IAsyncEnumerable<Right> GetRights(string subject, string reportee, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int skip = 0, take = 50, taken = 50;

            while (taken > 0)
            {
                var rights = await GetRights(subject, reportee, skip, take, cancellationToken);
                taken = rights.Rights.Length;
                skip += taken;
                foreach (var right in rights.Rights)
                {
                    yield return right;
                }
            }
        }

        public async Task<RightsResponse> GetRights(string subject, string reportee, int skip, int take, CancellationToken cancellationToken = default)
        {
            using var response = await GetWithForceEIAuthentication("authorization/rights", cancellationToken, $"subject={subject}", $"reportee={reportee}", $"$skip={skip}", $"$top={take}");
            var rights = await DeserializeIfSuccessful<RightsResponse>(response);
            return rights;
        }

        private static async Task<T> DeserializeIfSuccessful<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStreamAsync();
            var instance = await JsonSerializer.DeserializeAsync<T>(content);
            return instance;
        }

        public async Task<bool> HasDelegation(string subject, string reportee, string serviceCode, int serviceEditionCode, CancellationToken cancellationToken = default)
        {
            var rights = GetRights(subject, reportee, cancellationToken);

            // Don't care about the Altinn rights read/write/archiveread/archivedelete - as long as there is one, we consider it granted
            return await rights.AnyAsync(r => r.ServiceCode == serviceCode && r.ServiceEditionCode == serviceEditionCode); 
        }
    }
}

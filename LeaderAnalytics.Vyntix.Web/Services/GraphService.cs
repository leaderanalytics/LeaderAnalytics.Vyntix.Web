using System.Text.Json.Nodes;

namespace LeaderAnalytics.Vyntix.Web.Services;

// user by email query bug:
// https://github.com/microsoftgraph/microsoft-graph-docs/issues/11094

public class GraphService : IGraphService
{
    private GraphClientCredentials graphClientCredentials;
    private GraphServiceClient client;
    private const string _endpoint = "https://graph.microsoft.com/beta";

    private string USER_FIELDS = $"id, userprincipalname, mail, displayname, mailnickname, accountenabled, identities, othermails, creationtype, {UserAttributes.BillingID}, {UserAttributes.IsBanned}, {UserAttributes.IsCorporateAdmin}, {UserAttributes.IsOptIn}, {UserAttributes.PaymentProviderCustomerID}, {UserAttributes.SuspendedUntil}";

    // Deprecated Azure AD endpoint ------------------------------------------------------------------------------
    private const string _Azure_AD_Endpoint = "https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/users?api-version=1.6";
    private HttpClient AzureADGraphClient;
    // Deprecated Azure AD endpoint ------------------------------------------------------------------------------



    public GraphService(Func<string, GraphClientCredentials> credentialsFactory, Func<HttpClient> httpClientFactory)
    {
        graphClientCredentials = credentialsFactory(Domain.Constants.MS_GRAPH_CREDENTIALS);
        this.client = CreateGraphServiceClient(graphClientCredentials);  // MS Graph endpoint
        this.AzureADGraphClient = httpClientFactory();                   // Azure AD endpoint
    }

    public async Task<List<User>> FindUser(string id)
    {
        var page = client.Users.Request()
             .Select(USER_FIELDS);

        if (!string.IsNullOrEmpty(id))
            page = page.Filter($"id eq '{id}'");

        List<User> users = null;
        try
        {
            users = (await page.GetAsync())?.ToList();
        }
        catch (Microsoft.Graph.ServiceException)
        {

        }
        return users;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return (await client.Users.Request().Select(x => new
        {
            x.Id,
            // x.ODataType, Parsing OData Select and Expand failed: Term '@odata.type' is not valid in a $select or $expand expression.
            x.AdditionalData,
            x.TransitiveMemberOf,
            x.ScopedRoleMemberOf,
            x.RegisteredDevices,
            x.OwnedObjects,
            x.OwnedDevices,
            x.Oauth2PermissionGrants,
            x.MemberOf,
            x.Manager,
            x.LicenseDetails,
            x.DirectReports,
            //x.Skills,
            //x.Schools,
            //x.Responsibilities,
            //x.PreferredName,
            //x.Interests,
            //x.HireDate,
            //x.Birthday,
            //x.AboutMe,

            //x.DeviceEnrollmentLimit,
            //x.Activities,
            x.Settings,
            //x.DeviceManagementTroubleshootingEvents,
            //x.ManagedAppRegistrations,
            //x.ManagedDevices,
            x.Extensions,
            //x.FollowedSites,
            //x.Photos,
            x.Photo,
            //x.People,
            //x.InferenceClassification,
            x.UserType,
            //x.UsageLocation,
            x.LastPasswordChangeDateTime,

            x.JobTitle,
            x.IsResourceAccount,
            x.ImAddresses,
            x.Identities,
            x.GivenName,
            x.FaxNumber,
            x.ExternalUserStateChangeDateTime,
            x.ExternalUserState,
            x.EmployeeId,
            x.DisplayName,
            x.Department,
            x.CreationType,
            x.CreatedDateTime,
            x.Country,
            x.ConsentProvidedForMinor,
            x.CompanyName,
            x.City,
            x.BusinessPhones,
            x.AssignedPlans,
            x.AssignedLicenses,
            x.AgeGroup,
            x.AccountEnabled,
            x.LegalAgeGroupClassification,
            x.LicenseAssignmentStates,
            x.MailNickname,
            x.Surname,
            x.StreetAddress,
            x.State,
            x.SignInSessionsValidFromDateTime,
            x.ProxyAddresses,
            x.ProvisionedPlans,
            x.PreferredLanguage,
            x.PostalCode,
            x.PasswordProfile,
            x.PasswordPolicies,
            x.UserPrincipalName,
            x.OtherMails,
            x.OnPremisesSyncEnabled,
            x.OnPremisesSecurityIdentifier,
            x.OnPremisesSamAccountName,
            x.OnPremisesProvisioningErrors,
            x.OnPremisesLastSyncDateTime,
            x.OnPremisesImmutableId,
            x.OnPremisesExtensionAttributes,
            x.OnPremisesDomainName,
            x.OnPremisesDistinguishedName,
            x.OfficeLocation,
            x.MobilePhone,
            x.OnPremisesUserPrincipalName
        }

        ).GetAsync()).ToList();
    }

    public async Task<bool> VerifyUser(string userID)
    {
        User user;
        try
        {
            user = await client.Users[userID].Request().GetAsync();
        }
        catch (Microsoft.Graph.ServiceException)
        {
            return false;
        }
        return user != null;
    }

    public async Task<UserRecord> GetUserRecordByID(string id)
    {
        UserRecord record = null;
        User user = (await FindUser(id))?.FirstOrDefault();

        if (user != null)
            record = new UserRecord(user);

        return record;
    }

    public async Task UpdateUser(UserRecord user)
    {
        if (!user.IsLocalAccount)
            throw new Exception("CreationType must be LocalAccount");

        User graphUser = user.GraphUser();
        await UpdateUser(graphUser);
    }

    public async Task UpdateUser(User user)
    {
        var result = await client.Users[user.Id]
        .Request()
        .UpdateAsync(user);
    }

    public async Task UpdateExtensionProperties(UserRecord userRecord)
    {
        User user = userRecord.GraphUser();
        var result = await client.Users[user.Id]
        .Request()
        .UpdateAsync(user);
    }



    public async Task<User> CreateUser(UserRecord userRecord)
    {
        User user = userRecord?.GraphUser() ?? throw new ArgumentNullException("userRecord");
        return await CreateUser(user);
    }

    public async Task<User> CreateUser(User user)
    {
        var result = await client.Users.Request().AddAsync(user);
        return result;
    }

    public async Task DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new Exception("id is required.");

        await client.Users[id].Request().DeleteAsync();
    }

    public async Task<UserRecord> GetUserByEmailAddress(string email)
    {
        IGraphServiceUsersCollectionPage users = await client.Users.Request()
            .Select(USER_FIELDS)
            .Filter($"identities/any(ids:ids/issuerassignedid eq '{email}' and ids/issuer eq 'x')")
            .GetAsync();

        if (users.Any())
            return new UserRecord(users[0]);

        return null;
    }

    public async Task<List<UserRecord>> GetDelegateUsers(string ownerID)
    {
        List<User> users = await GetAllUsers();
        List<UserRecord> records = new List<UserRecord>(users.Count);

        foreach (User user in users.Where(x => x.AdditionalData != null))
        {
            if (user.AdditionalData.TryGetValue(UserAttributes.BillingID, out object userOwner) && userOwner.ToString() == ownerID)
                records.Add(new UserRecord(user));
        }

        return records;
    }

    public async Task SetAdminFlag(UserRecord userRecord, bool isAdmin)
    {
        if (userRecord is null)
            throw new ArgumentNullException(nameof(userRecord));

        User graphUser = userRecord.GraphUser();

        if (graphUser is null)
            throw new Exception("Graph user is null.");


        if (userRecord.IsLocalAccount)
        {
            userRecord.IsCorporateAdmin = isAdmin;
            await UpdateUser(userRecord);
        }
        else
        {

            HttpClient azureADClient = await CreateAzureADGraphClient();
            string api_version = "api-version=1.6";
            string resource_path = "users/" + graphUser.Id;
            string requestContent = "{" + $"\"{UserAttributes.IsCorporateAdmin}\"" + ":" + (isAdmin ? "true" : "false") + "}";

            string apiUrl = $"https://graph.windows.net/LeaderAnalytics.onmicrosoft.com/{resource_path}?{api_version}";

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(apiUrl),
                Content = new StringContent(requestContent, Encoding.UTF8, "application/json")
            };

            // Send the update request
            HttpResponseMessage response = await azureADClient.SendAsync(request);
            if (response.StatusCode.ToString() != "NoContent")
                throw new Exception($"Attempt to set Admin Flag on user {graphUser.Id} failed.  The status code is: {response.StatusCode.ToString()}");
        }
    }

    private GraphServiceClient CreateGraphServiceClient(GraphClientCredentials credentials)
    {
        var ccab = ConfidentialClientApplicationBuilder
            .Create(credentials.ClientID)
            .WithClientSecret(credentials.ClientSecret)
            .WithTenantId(credentials.TenantID)
            .Build();

        GraphServiceClient client = new GraphServiceClient(new ClientCredentialProvider(ccab));
        return client;
    }


    /// <summary>
    /// Creates an HttpClient for calling the deprecated Azure AD Graph endpoint
    /// </summary>
    /// <returns></returns>
    private async Task<HttpClient> CreateAzureADGraphClient()
    {
        string authContent = $"grant_type=client_credentials&client_id={graphClientCredentials.ClientID}&client_secret={graphClientCredentials.ClientSecret}&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default";
        HttpRequestMessage authRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://login.microsoftonline.com/{graphClientCredentials.TenantID}/oauth2/token"),
            Content = new StringContent(authContent, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        HttpResponseMessage authResponse = await AzureADGraphClient.SendAsync(authRequest);
        string json = await authResponse.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(json);
        string token = (string)node["access_token"];
        AzureADGraphClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return AzureADGraphClient;
    }
}

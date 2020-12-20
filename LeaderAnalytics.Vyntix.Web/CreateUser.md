Why a one hour job to create a user in code is taking me over a week.  I still have nothing to show for my effort.

About a week ago I wrote [this post](https://old.reddit.com/r/dotnet/comments/jud07a/why_a_two_hour_job_in_azure_took_two_days_and_i/)  that documents my effort to create custom attributes for users in Azure Active Directory.  In the time since that was written I was able to implement custom attributes using a work-around method. My original goal of being able to re-create attributes in code remains undone to this day.

This time around my goal is to create Active Directory users in code.  From my prior experience I knew a 20 min task like writing `INSERT INTO USERS...`  would evolve into a multi-day project - I just didn't know how many days.  Documenting this task proved to be more difficult than the last one.  Last time I was able to find documentation and demonstrate it is wrong.  This time around part of the problem was that I could not find documentation.  Its much harder to prove a negative so when I say I spent a long time searching you just have to take my word for it.

Business objective:  I am building a SaaS site that targets corporate users.  The workflow is that a company admin will purchase a subscription than delegate access to users in the company.  To do that, the admin will use a simple page to input email addresses of users who will be able to log into the site.  For each user, I create a user record in Azure Active Directory B2C.  Yes, I am aware an alternative workflow is that users create their own accounts and the admin approves via an email.  I have chosen to not use that workflow for reasons too long to explain here.

My starting point was [this document](https://docs.microsoft.com/en-us/graph/api/user-post-users?view=graph-rest-1.0&tabs=csharp) which describes how to create a user using MS Graph.  Example 2 specifically describes how to create a user for a B2C tenant.  I am constantly expecting to be mislead by documentation that applies to AD but not B2C.  This is page is a great example of how tricky it is.  Note that you can also reach the referenced page from [Manage Azure AD B2C user accounts with Microsoft Graph](https://docs.microsoft.com/en-us/azure/active-directory-b2c/manage-user-accounts-graph-api) by clicking on "Create a user".

The [example code](https://docs.microsoft.com/en-us/graph/api/user-post-users?view=graph-rest-1.0&tabs=csharp#request-1) is very simple and is only a couple lines. The only problem is that it does not work.  When you try to log in as the user you create you get this error message: "We can't seem to find your account".



I posted [this question](https://stackoverflow.com/questions/64959347/how-do-i-create-a-user-in-code-add-it-azure-ad-b2c-then-log-in-as-that-user-us) on stackoverflow.  The person who responded basically re-posted my code and told me I needed to set source property - which does not exist on the [User object](https://docs.microsoft.com/en-us/graph/api/resources/user?view=graph-rest-1.0).



In the course of researching this issue I found [Operations on users | Graph API reference](https://docs.microsoft.com/en-us/previous-versions/azure/ad/graph/api/users-operations#create-a-user-local-or-social-account--) that describes required properties that must be set when creating a user.  Fortunately, this page documents the `signInNames` property that must be set when creating a user.  Unfortunately, there is no such property called `signInNames` on the [User object](https://docs.microsoft.com/en-us/graph/api/resources/user?view=graph-rest-1.0).  This documentation appears to be obsolete.


[Overview of user accounts in Azure Active Directory B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-overview)

Consumer account - A consumer account is used by a user of the applications you've registered with Azure AD B2C. Consumer accounts can be created by:
The user going through a sign-up user flow in an Azure AD B2C application
Using Microsoft Graph API
Using the Azure portal




[Use the Azure portal to create and delete consumer users in Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/manage-users-portal)

Although consumer accounts in an Azure AD B2C directory are most commonly created when users sign up to use one of your applications, you can create them programmatically and by using the Azure portal. 



[Azure AD B2C Preview: Use the Graph API](https://github.com/uglide/azure-content/blob/master/articles/active-directory-b2c/active-directory-b2c-devquickstarts-graph-dotnet.md)

Create consumer user accounts
When you create user accounts in your B2C tenant, you can send an HTTP POST request to the /users endpoint:



https://stackoverflow.com/questions/39195452/creating-test-users-for-azure-ad-b2c?rq=1



https://docs.microsoft.com/en-us/previous-versions/azure/ad/graph/api/users-operations#CreateUser

https://docs.microsoft.com/en-us/previous-versions/azure/ad/graph/api/users-operations#CreateLocalAccountUser


https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview
> If a local account is based on an email address, then the email address is stored in the signInNames property.
> Only the otherMails and signInNames properties are exposed through the Microsoft Graph API.
Where is the signInNames property?

> The email address isn't guaranteed to be verified in any of these cases. Even if email address verification is enabled, addresses aren't verified if they come from a social identity provider and they haven't been changed.
So why can't I set it.


[This post](https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/91)
"I'm sorry about the confusion caused by the lack of documentation.", "The User entity for the Microsoft Graph service API still does not have the signInNames property."

[How to access Email of an User on B2C using Graph API](https://stackoverflow.com/questions/58345786/how-to-access-email-of-an-user-on-b2c-using-graph-api)
"signInNames is NOT available in Microsoft Graph API."



[Trouble facing while using Azure AD B2C for authentication](https://stackoverflow.com/questions/45756152/trouble-facing-while-using-azure-ad-b2c-for-authentication)

> The User type always need to be "Member" and have the Source "Azure Active Directory"    

Where is the Source property?



https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-get-started#register-identity-experience-framework-applications

https://stackoverflow.com/questions/39439830/create-a-new-user-in-azure-active-directory-b2c-with-graph-api-using-http-pos
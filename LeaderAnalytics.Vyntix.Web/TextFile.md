I needed to add a couple columns to my Azure AD user table.  I wanted to be able to create and re-create the columns on the table from code as opposed to typing in column names in the portal.  I wanted to write the code in c#, not issue API calls via MS Graph explorer.  I did a little research and figured it should be pretty simple to do - perhaps an hour or so.  Two days later I was upset and frustrated and had nothing to show for my work.  Worse yet, I had again experienced what has started to become a fairly common occurance - I go into a job thinking it is a simple fix, only to find out it is in fact a major effort.  

I usually walk away from these situations with a great deal of self-disparagement.  After all I am a developer.  I've been doing this a long time.  It's my job to understand the material.  Why do I keep getting body slammed with hours of work for jobs that I think should take just a few minutes? This time, as soon as I began to feel like I was chasing my tail, I opened an instance of notepad and began pasting all the links I was using along with a brief note about why I was using that document.  I just kept pushing forward trying to find something that worked and writing down all the things I tried.  

Bear in mind as you read this that what I am documenting is my process of discovery - not my final solution.  My goal here is to understand why it took me so long to reach a conclusion.  This what I discovered.

 
My starting point was [this page](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-profile-attributes), which is the arguably one of the early starting points for learning about user attributes.
Near the end of the page I read this:

> Azure AD B2C extends the set of attributes stored on each user account. Extension attributes [extend the schema](https://docs.microsoft.com/en-us/graph/extensibility-overview#schema-extensions) of the user objects in the directory.

Click on the link in the sentence above and note where it leads.  Also from the original page cited, I read a little further and I saw this section:

Next steps

Learn more about extension attributes:

[Schema extensions](https://docs.microsoft.com/en-us/graph/extensibility-overview#schema-extensions)

Again, note where the link leads.  I needed to "learn more" and I wanted an example so I used that link.  Then I clicked [Add custom data to groups using schema extensions](https://docs.microsoft.com/en-us/graph/extensibility-schema-groups) which provides an example using groups.
From there I clicked [Create schemaExtension](https://docs.microsoft.com/en-us/graph/api/schemaextension-post-schemaextensions?view=graph-rest-1.0&tabs=http) in the "See also" section which took me to this great page that looks like it has all the code I need to get the job done. 

I will stop here for a quick recap.  Those of you who know Azure AD know that I was wasting my time from the moment I clicked on the very first link in the original document I cited.  Azure AD and Azure AD B2C are not the same product.  They share some overlapping functionality but at the end of the day they are different.  So the question now is: Is the functionality documented in the links I followed applicable to AD, AD B2C, or both?  The document I started out with is clearly applicable to B2C.  If you were just learning Azure AD and you were just following the links as they are presented, what would you think?


Using the code found on the [Create schmeaExtension](https://docs.microsoft.com/en-us/graph/api/schemaextension-post-schemaextensions?view=graph-rest-1.0&tabs=csharp)  I thought I was pretty much out of the woods and I should be wrapping this thing up shortly.  Not so.  It turns out that verifying a domain is a necessary prerequisite to creating a schema extension (there is another option that also does not work.  See my SO ticket).  Fortunately, the Create schemaExtension page has links to all the steps that need to be done in advance of actually creating the extension.  Oh wait..... there are no links to anything of the sort.  Even the [schemaExtension resource type](https://docs.microsoft.com/en-us/graph/api/resources/schemaextension?view=graph-rest-1.0) overview page is conspicuously absent of any mention of domains.  An argument might be made that Microsoft cannot be expected to include a link to every dependency for a given concept.  Fair enough I will accept that.  In this case however I had nothing to search for except "domain" and it took me a couple of hours to find [the link I needed](https://docs.microsoft.com/en-us/graph/api/resources/domain?view=graph-rest-1.0) even though it was located in the same directory tree.  Problem solved, right?  Not even close.  It turns out the [Verify domain](https://docs.microsoft.com/en-us/graph/api/domain-verify?view=graph-rest-1.0&tabs=http) documentation is very thorough and tells you everything you need to know to verify a domain except how to actually verify the domain.  For that you are on your own.  After a couple of hours of banging my head on the wall, I wrote this [StackOverflow issue](https://stackoverflow.com/questions/64806571/how-to-verify-a-domain-so-a-schema-extension-can-be-added-to-ms-graph) which is unanswered as of this writing and will most certainly remain that way forever.

So at this point I thought I had reached the end of my rope.  I was pretty upset at having invested so much time and having absolutely nothing to show for it.  I must have it in my head overnight because the next day I remembered something I had done while setting up DNS for my sites.  I remembered setting up a TXT verification record that was identified by an "@" sign.  Would it work for verifying MS Graph?  I tried it and behold it worked! Bear in mind this was a total guess on my part.  Anyone who has not had any prior experience with DNS could not possibly be expected to know this.

I was pretty excited at this point. I thought for sure I would have something to show for the time and effort that I had invested.  Here is my code. Its not rocket science and conceptually is it the same code as presented in the example page [cited above](https://docs.microsoft.com/en-us/graph/api/schemaextension-post-schemaextensions?view=graph-rest-1.0&tabs=http).


	var schemaExtension = new SchemaExtension
	{
		Id = "domain_subscriberInfo",
		Description = "Billing and configuration properties for users",
		TargetTypes = new List<string> { "User" },
		Properties = new List<ExtensionSchemaProperty>
		{
			new ExtensionSchemaProperty
			{
				Name = "BillingID",
				Type = "String"
			}
		}
	};

	try
	{
		var response = await graphServiceClient.SchemaExtensions
			.Request()
			.AddAsync(schemaExtension);
	}
	catch (Exception ex)
	{
		string nowWhat = ex.ToString();
	}

This is where I made my last stand.  We all know this thing is not going to work.  The question is why.  Here is the error message:
 
 
	Status Code: InternalServerError
	Microsoft.Graph.ServiceException: Code: Service_InternalServerError
	Message: Encountered an internal server error.
	Inner error:
		AdditionalData:
		date: 2020-11-14T20:39:04
		request-id: 8712ad4e-
		client-request-id: 8712ad4e-
	ClientRequestId: 8712ad4e-

	   at Microsoft.Graph.HttpProvider.SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
	   at Microsoft.Graph.BaseRequest.SendRequestAsync(Object serializableObject, CancellationToken cancellationToken, HttpCompletionOption completionOption)
	   at Microsoft.Graph.BaseRequest.SendAsync[T](Object serializableObject, CancellationToken cancellationToken, HttpCompletionOption completionOption)
	   at CreateUserSchemaExtension() in C:\GraphTests.cs:line 137


A quick google search takes me [here](https://stackoverflow.com/questions/59395423/graph-api-adding-schema-extension-using-net-core-3-1) where I read:

>  I don't think Microsoft Graph API Create schemaExtension is supported for Azure AD B2C currently.


And there it is.  Now you have the complete picture.  This is why a one hour job takes two days and you still walk away with nothing.  

As you might expect the links cited above are not the only research I did into this problem.  They are just the ones that are most obviously misleading.  Links like [this one](https://docs.microsoft.com/en-us/answers/questions/6777/user-attributes-and-extensionproperties-how-are-th.html) that say "...Note that extension properties are the same thing as schema extensions...." come up often in google searches.

Same with [this one](https://download.microsoft.com/download/F/C/A/FCA7C6E3-7153-4FB1-9825-0B1BB26F14E0/An-overview-of-AAD-B2C.docx) that says "...Custom attributes use Azure AD Graph API Directory schema extensions under the hood. For more information, see article Azure AD Graph API Directory Schema Extensions."

[This post](https://old.reddit.com/r/dotnet/comments/hdichp/we_need_a_giant_map_of_everything_having_to_do/) makes the same point I am trying to make.  More pages does not equal better documentation.  It has to make sense.  

Those citing [this](https://docs.microsoft.com/en-us/azure/active-directory-b2c/manage-user-accounts-graph-api) as a solution - your hindsight is fantastic. And it's not the solution I want.  I was led to believe the solution I want exists and is attainable. As stated above this post is not about the solution to the problem - it's about the discovery process.

Those who think I'm just pissed off - yeah you bet I am. I am going to take time to fight back when I find things like this.  I am not going to walk away and blame myself.  I spent most of a Saturday documenting this.  I'll do it again if I have to.  Everything I posted here is straight up fact and I stand by it.  

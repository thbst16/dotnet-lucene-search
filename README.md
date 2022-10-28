# dotnet-lucene-search

Dotnet Lucene Search is a [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)-based web application that exposes full text search capability built on top of the [Lucene.Net](https://lucenenet.apache.org/) search engine library. The application was built to fill the void in tutorials focused on command line interaction with Lucene and Blazor web apps focused on database persistence without full text search capabilities.

The repository has 5 sub-projects, each representing a stage of the application buildout, correlating with the steps used to build out the Azure Cognitive search app in [Microsoft's online tutorial](https://learn.microsoft.com/en-us/azure/search/tutorial-csharp-create-first-app).

* <b>Basic Search.</b> Creation of the basic search web application using Lucene.Net with source data seeded using the Bogus synthetic test data library.
* <b>Results Paging.</b>
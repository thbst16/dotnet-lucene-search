# dotnet-lucene-search
[![Build Status](https://beckshome.visualstudio.com/dotnet-lucene-search/_apis/build/status/thbst16.dotnet-lucene-search?branchName=main)](https://beckshome.visualstudio.com/dotnet-lucene-search/_build/latest?definitionId=14&branchName=main)
![Docker Image Version (latest by date)](https://img.shields.io/docker/v/thbst16/dotnet-lucene-search?logo=docker)
![Uptime Robot ratio (7 days)](https://img.shields.io/uptimerobot/ratio/7/m792951447-6396ddf7ae6c22d19364ee62?logo=http)

[Dotnet Lucene Search](https://dotnet-lucene-search.azurewebsites.net/) is a [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)-based web application that exposes full text search capability built on top of the [Lucene.Net](https://lucenenet.apache.org/) search engine library. The application was built to fill the void in tutorials focused on command line interaction with Lucene and Blazor web apps focused on database persistence without full text search capabilities.

The repository has 5 sub-projects, each representing a stage of the application buildout, correlating with the steps used to build out the Azure Cognitive search app in [Microsoft's online tutorial](https://learn.microsoft.com/en-us/azure/search/tutorial-csharp-create-first-app).

* <b>[Basic Search.](https://beckshome.com/2022/10/lucene-blazor-part-1-basic-search)</b> Creation of the basic search web application using Lucene.Net with waffle text source data seeded using the Bogus synthetic test data library.
* <b>[Results Paging.](https://beckshome.com/2022/11/lucene-blazor-part-2-results-paging)</b> Order of magnitude increase in generated records and introduction of paging function to deal with paginated results.
* <b>[AutoComplete.](https://beckshome.com/2022/11/lucene-blazor-part-3-auto-complete)</b> Uses the Lucene.Net.Suggest library in conjunction with Chris Saintly's [Blazored.Typeahead control](https://github.com/Blazored/Typeahead) to provide auto-complete functionality over the indexed waffle text data. 
* <b>[Faceting.](https://beckshome.com/2022/11/lucene-blazor-part-4-faceting)</b> Includes dedicated indexing for 2 facets (scholars and universities) and the ability to drill down on these 2 facets from the user interface.
* <b>Highlighting.</b> Coming soon...

# Screens

### Basic Search Page
![Basic Search](https://s3.amazonaws.com/s3.beckshome.com/20221029-dotnet-lucene-search-basic.jpeg)

### Results Pagination
![Results Paging](https://s3.amazonaws.com/s3.beckshome.com/20221104-dotnet-lucene-search-pagination.jpeg)

### Auto Complete
![Auto Complete](https://s3.amazonaws.com/s3.beckshome.com/20221111-dotnet-lucene-auto-complete.jpeg)

### Faceting
![Faceting](https://s3.amazonaws.com/s3.beckshome.com/20221120-dotnet-lucene-faceting.jpeg)

# Features

* Web-based full-text search on top of Lucene.Net search engine library
* Autogeneration of waffle text records using Bogus data generator
* Pagination of large result sets
* Autocomplete lookup across the entire indexed dataset
* Faceted search qualification across multiple facets

# Motivation and Credits

Some starter guides for using Lucene.Net and ideas for this project are attributable to the sources below.

* [Getting Started with Blazored Typeahead](https://chrissainty.com/getting-started-with-blazored-typeahead/)
* [How to Implement Lucene.NET](https://code-maze.com/how-to-implement-lucene-dotnet/)
* [Lucene.Net Simple Facets Example](https://lucenenet.apache.org/docs/4.8.0-beta00008/api/Lucene.Net.Demo/Lucene.Net.Demo.Facet.SimpleFacetsExample.html)
* [Microsoft Azure Cognitive Search Tutorials](https://learn.microsoft.com/en-us/azure/search/tutorial-csharp-create-first-app)
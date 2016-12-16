# OP10.NewContentAtTopHandler
OP10 New Content At Top Handler allows you to add new content and media to the top of the tree on creation

## Installation
Download the Umbraco Package here: [OP10 New Content At Top Handler](https://our.umbraco.org/projects/backoffice-extensions/op10-new-content-at-top-handler/)

Installation via NuGet:
```
PM> Install-Package OP10.NewContentAtTopHandler
```

## Configuration

Add a new AppSetting to the web.config file: <add key="OP10.NewContentAtTopHandler" value="" />

As value specify the ContentType aliases (semicolon separated) of the Content and/or Medias under which the inserted content needs to be added at the top.

Example: <add key="OP10.NewContentAtTopHandler" value="Homepage;Textpage;Folder" />

### Special aliases

- Content Root: #ContentRoot
- Media Root: #MediaRoot


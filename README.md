Overview
---------------
Combres (previously hosted in [CodePlex](http://combres.codeplex.com/)) helps your ASP.NET and ASP.NET MVC applications perform faster and rank better with [YSlow](http://developer.yahoo.com/yslow/) and [PageSpeed](https://developers.google.com/speed/pagespeed/). 

Features highlights:  

* All in one solution supporting JS/CSS [combination](http://developer.yahoo.com/performance/rules.html#num_http), [minification](http://developer.yahoo.com/performance/rules.html#minify), [comression](http://developer.yahoo.com/performance/rules.html#gzip) and caching (by adding proper [Expires/Cache-Control](http://developer.yahoo.com/performance/rules.html#expires) headers, [ETag](http://developer.yahoo.com/performance/rules.html#etags) and server-side caching)
* Easy to use, simply download it via Nuget, declare JS and CSS resource groups in an XML file and use them in your pages, Combres will take care of the rest
* Integrated with ASP.NET routing engine and work with ASP.NET WebForm 3.5/4.0/4.5, ASP.NET MVC 2/3/4 and Azure web applications
* Detect changes in Combres config file, managed JS/CSS files and support auto-versioning, so you don't have to manually rebundle JS/CSS resources after making changes to them
* Extensible architecture with many extension points
* And many more: CDN, HTTPS, debug mode, external JS/CSS (dynamically requested from other servers), Less CSS etc.
* Proven solution with many thousands of downloads in [NuGet](https://www.nuget.org/packages/combres), [CodePlex](http://combres.codeplex.com/) and [The Code Project](http://www.codeproject.com/Articles/69484/Combres-2-0-A-Library-for-ASP-NET-Website-Optimiza)

Check out [this Code Project article](http://www.codeproject.com/Articles/69484/Combres-2-0-A-Library-for-ASP-NET-Website-Optimiza) for a thorough introduction.



Usage
---------------
Install from [NuGet](https://www.nuget.org/packages/combres)

```
PM> Install-Package combres
PM> Install-Package combres.log4net (optional)
PM> Install-Package combres.mvc (optional)
```

Optional steps for ASP.NET 3.5 users, those using ASP.NET 4 or above can ignore:  
* Delete the generated file `AppStart_Combres.cs`
* Open `global.asax` code-behind file:
  * Import `Combres` namespace
  * Add this line to the first line of either `RegisterRoutes()` or `Application_Start()`: `RouteTable.Routes.AddCombresRoute("Combres")`
    
Edit `App_Data/Combres.xml` to declare your JS and CSS resources

Use resource groups in your pages as follows:

```csharp
<%= WebExtensions.CombresLink("siteCss") %>  
<%= WebExtensions.CombresLink("siteJs") %>
```

ASP.NET MVC developers can import `Combres.Mvc` namespace and declare CSS/JS like below:

```csharp
@using Combres.Mvc;
...
@Url.CombresLink("siteCss")
@Url.CombresLink("siteJs")
```

That should be it. Start your browser to observe Combres in action. For more advanced usages, refer to [this page](http://www.codeproject.com/Articles/69484/Combres-2-0-A-Library-for-ASP-NET-Website-Optimiza) .



Change Log
---------------
2.2.2.15
* Fix issues [7675](http://combres.codeplex.com/workitem/7675), [7656](http://combres.codeplex.com/workitem/7656), [7673](http://combres.codeplex.com/workitem/7673), [7660](http://combres.codeplex.com/workitem/7660), [7654](http://combres.codeplex.com/workitem/7654)
* Built with dotLess 1.3.1.0, Fasterflect 2.1.2, AjaxMin 4.48.4489.28432 and Log4Net 1.2
* Move Log4Net logger implementation to separate Combres.Log4Net package
	+ `NuGet Combres.Log4Net`
	+ Add this attribute the combres element: `logProvider="Combres.Log4Net.Log4NetLogger, Combres.Log4Net"`

2.2.2.6
* Built with latest versions of Log4Net, YUI Compressor and MS Ajax Minifier
* Add MSAjaxCssMinifier to minify CSS resources
* Add line break to JS resources when combining

2.2.2.2
* Add support for [sslHost](http://combres.codeplex.com/discussions/235498)
* Fixed [missing content-type issue](http://combres.codeplex.com/discussions/245217)

2.2.2.0

* Support host prefix

2.2.1.8  

* Fix issues [7649](http://combres.codeplex.com/workitem/7649), [7650](http://combres.codeplex.com/workitem/7650), [245786](http://combres.codeplex.com/discussions/245786) and [245217](http://combres.codeplex.com/discussions/245217)

2.2.1.5
* Support AS.NET MVC 3 and Razor
* Fix [7647](http://combres.codeplex.com/workitem/7647)
* Add `EnableClientUrls()` to `WebExtensions` and `MvcExensions` for generating JS variables providing access to Combres resource sets
* `MvcExtensions` returns `MvcHtmlString` instead of `string`

2.1
* Enable Dot Less for combined contents (instead of individual files) via `DotLessCssCombineFilter`
* Support Azure
* Enable custom server-side caching via `<combres cacheProvider="my.custom.cache" .../>`
* Support local closure compilation via `LocalClosureJSMinifier`
* Support cache-vary extensibility to allow developers to control Combres caching behavior
* Fix issue [6547](http://combres.codeplex.com/workitem/6547)

2.0
* Implement [6103](http://combres.codeplex.com/workitem/6103), [5633](http://combres.codeplex.com/workitem/5633), [5572](http://combres.codeplex.com/workitem/5572), [6088](http://combres.codeplex.com/workitem/6088), [6056](http://combres.codeplex.com/workitem/6056), [5605](http://combres.codeplex.com/workitem/5605), [5941](http://combres.codeplex.com/workitem/5941), [78751](http://combres.codeplex.com/Thread/View.aspx?ThreadId=78751), [4349](http://combres.codeplex.com/workitem/4349)
* Support per-resource minification scheme
* Fix bug in etag handling which may cause client to receive expired content 
* Add overload of `CombresLink()` which accepts `htmlAttributes` object
* Combres now maintains the order of resources in multiple-minifiers resource sets

1.2
* Change detection and auto-versioning
* Fix bug when auto-versioning generates different version across application processes
* Fix [69854](http://combres.codeplex.com/Thread/View.aspx?ThreadId=69854), [5489](http://combres.codeplex.com/workitem/5489)
* Add filters: `DotLessCssFilter`, `HandleCssVariablesFilter`
* Fix `ChangeMonitor#IsSamePath()` as per [this discussion thread](http://combres.codeplex.com/Thread/View.aspx?ThreadId=79884)
* Update `SimpleOjectBinder` to allow case-insensitive binding
* Add `forwardCookie` attribute to `<resource>` element whose mode is `Dynamic`.
* Handle both gzip & deflate for remote resources

1.1
* Support YUI, MS Ajax and Google Closure minifiers
* Support turn-off minification
* Fix [5460](http://combres.codeplex.com/workitem/5460)
* Use default minifier if minifierRef, jsMinifierRef, cssMinifierRef is not specified
* Support `auto` mode for `debugEnabled` (to use `debug` setting from `web.config`)


Author
---------------
Email: [buunguyen@gmail.com](mailto:buunguyen@gmail.com)  
Blog: (www.buunguyen.net)  
Twitter: [@buunguyen](https://twitter.com/buunguyen)  
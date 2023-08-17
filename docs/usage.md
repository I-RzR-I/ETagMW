# USING

You need to add a specific piece of code written below for using this middleware.
Add this piece to your `Startup.cs`.

The way how to use this extension(middleware). All the types are represented below. After adding one of the extension methods, you can continue to use the app and in every response header from the server, you will see the ETag variable.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            
            app.UseETag();
            
            ...
        }
```

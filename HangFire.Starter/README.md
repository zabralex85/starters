# HangFire.Starter 
Simple task scheduler on C# with Web UI Dashboard

Here i implemented two sample jobs which registed by config file.
In project i have used libs:

Autofac<br/>
Autofac.Configuration<br/>
Hangfire<br/>
Hangfire.Core<br/>
Hangfire.MemoryStorage<br/>
Hangfire.SqlServer<br/>
Microsoft.Owin<br/>
Microsoft.Extensions.Configuration.Json<br/>
Newtonsoft.Json<br/>
Topshelf<br/>

After start you could go to dashboard at http://localhost:12346/hangfire/recurring<br/>
Also in project "HangFire.Starter.Server" i have used configuration helper ApplicationSettings.settings which contains:

EndpointHost: localhost<br/>
EndpointPort: 12346<br/>
ServerMode: InMemory (InMemory / SqlServer for hangfire storage)

<h3>Warning:</h3>
<code>In Jobs projects and in server project there a postbuild events for copyng dlls and configs.</code>

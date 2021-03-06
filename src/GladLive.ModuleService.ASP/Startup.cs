﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GladLive.Module.System.Server;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Reflection;
using GladNet.Serializer;
using GladNet.Serializer.Protobuf;

namespace GladLive.ModuleService.ASP
{
	public class Startup : GladLiveASPAppBase
	{
		public Startup(IHostingEnvironment env) 
			: base(env)
		{
			//just pass to base
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			//Configure GladNet
			//Hacky but we need it
			services.AddSingleton<ISerializerStrategy, ProtobufnetSerializerStrategy>();
			services.AddSingleton<IDeserializerStrategy, ProtobufnetDeserializerStrategy>();
			services.AddSingleton<ISerializerRegistry, ProtobufnetRegistry>();

			services.AddGladNet(new ProtobufnetSerializerStrategy(), new ProtobufnetDeserializerStrategy(), new ProtobufnetRegistry());

			base.ConfigureServices(services);
		}

		//This changed in RTM. Fluently build and setup the web hosting
		public static void Main(string[] args)
		{
			//Check the first argument; it is likely the URL if asked for
			new WebHostBuilder()
			.UseKestrel(options =>
			{
				X509Certificate2 cert = new X509Certificate2(@"Certs/TLSCert.pfx");

				//TODO: Handle cert changes
				options.UseHttps(new HttpsConnectionFilterOptions() { SslProtocols = System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12, ServerCertificate = cert });
			})
			.UseUrls(args.Any() ? args.First() : @"https://localhost:44300")
			.UseContentRoot(Directory.GetCurrentDirectory())
			.UseStartup<Startup>()
			.Build()
			.Run();
		}
	}
}

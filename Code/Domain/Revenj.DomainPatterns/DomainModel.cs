﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Revenj.Extensibility;

namespace Revenj.DomainPatterns
{
	public class DomainModel : IDomainModel
	{
		private readonly List<Assembly> DomainAssemblies;
		private readonly ConcurrentDictionary<string, Type> Cache = new ConcurrentDictionary<string, Type>(1, 127);

		public delegate DomainModel Factory(IEnumerable<Assembly> assemblies);

		public DomainModel(
			IEnumerable<Assembly> assemblies,
			IObjectFactory objectFactory)
		{
			Contract.Requires(assemblies != null);
			Contract.Requires(objectFactory != null);

			DomainAssemblies = assemblies.ToList();

			try
			{
				foreach (var asm in assemblies)
					foreach (var type in asm.GetTypes())
						if (typeof(ISystemAspect).IsAssignableFrom(type))
						{
							var aspect = (ISystemAspect)Activator.CreateInstance(type);
							aspect.Initialize(objectFactory);
						}
			}
			catch (ReflectionTypeLoadException ex)
			{
				var first = (ex.LoaderExceptions ?? new Exception[0]).Take(5).ToList();
				throw new ApplicationException(string.Format(@"Can't load types: 
{0}
", string.Join(Environment.NewLine, first.Select(it => it.Message))), ex);
			}
		}

		public Type Find(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;
			Type found;
			if (!Cache.TryGetValue(name, out found))
			{
				found =
					(from asm in DomainAssemblies
					 let asmType = asm.GetType(name)
					 where asmType != null
					 select asmType).FirstOrDefault();
				if (found != null)
					Cache.TryAdd(name, found);
			}
			return found;
		}
	}
}

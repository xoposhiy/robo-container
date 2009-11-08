﻿namespace RoboContainer
{
	public delegate object EnrichPluggableDelegate(object pluggable, Container container);
	public delegate TPluggable EnrichPluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IPluggableConfigurator
	{
		IPluggableConfigurator SetScope(InstanceLifetime lifetime);
		IPluggableConfigurator Ignore();
		IPluggableConfigurator EnrichWith(EnrichPluggableDelegate enrichPluggable);
		IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>();
	}

	public interface IPluggableConfigurator<TPluggable>
	{
		IPluggableConfigurator<TPluggable> SetScope(InstanceLifetime lifetime);
		IPluggableConfigurator<TPluggable> Ignore();
		IPluggableConfigurator<TPluggable> EnrichWith(EnrichPluggableDelegate<TPluggable> enrichPluggable);
	}
}
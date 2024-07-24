using System;


namespace THEBADDEST.SimpleDependencyInjection
{


	public class Binding
	{

		public DependencyContainer                   dependencyContainer;
		public DependencyContainer.DependencyFactory factory;
		public Type                                  interfaceType;
		public Type                                  implementationType;
		public bool                                  single;

		public void Bind()
		{
			dependencyContainer.Bind(interfaceType, implementationType, factory, single);
		}

	}

	public static class DCExtensionMethods
	{

		static DependencyContainer container;

		static DCExtensionMethods()
		{
			container = DependencyContainer.Create();
		}

		public static DependencyContainer BindFactory<TInterface, TImplementation>(this DependencyContainer dc, DependencyContainer.DependencyFactory factory) where TInterface : class where TImplementation : class, TInterface, new()
		{
			dc.Bind<TInterface, TImplementation>(factory, false);
			return dc;
		}

		public static Binding Bind<TInterface>(this DependencyContainer dc)
		{
			Binding binding = new Binding {dependencyContainer = dc, interfaceType = typeof(TInterface)};
			return binding;
		}

		public static Binding To<TImplementation>(this Binding binding, DependencyContainer.DependencyFactory factory = null)
		{
			binding.implementationType = typeof(TImplementation);
			binding.factory            = factory;
			return binding;
		}

		public static void AsSingle(this Binding binding, bool single = true)
		{
			binding.single = single;
			binding.Bind();
		}

		public static DependencyContainer GetStaticContainer()
		{
			return container;
		}

	}


}
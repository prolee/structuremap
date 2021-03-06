using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class PluginFamilyTester
    {
        public class TheGateway : IGateway
        {
            #region IGateway Members

            public string WhoAmI
            {
                get { throw new NotImplementedException(); }
            }

            public void DoSomething()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class DataTableProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void If_PluginFamily_only_has_one_instance_make_that_the_default()
        {
            var family = new PluginFamily(typeof (IGateway));
            string theInstanceKey = "the default";
            ConfiguredInstance instance = new ConfiguredInstance(typeof (TheGateway)).Named(theInstanceKey);
            family.AddInstance(instance);

            family.GetDefaultInstance().ShouldBeTheSameAs(instance);
        }

        [Test]
        public void SetScopeToHttpContext()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            family.Lifecycle.ShouldBeOfType<TransientLifecycle>();

            family.SetScopeTo(InstanceScope.HttpContext);
            family.Lifecycle.ShouldBeOfType<HttpContextLifecycle>();
        }


        [Test]
        public void SetScopeToHybrid()
        {
            var family = new PluginFamily(typeof (IServiceProvider));


            family.SetScopeTo(InstanceScope.Hybrid);
            family.Lifecycle.ShouldBeOfType<HybridLifecycle>();
        }

        [Test]
        public void SetScopeToSingleton()
        {
            var family = new PluginFamily(typeof (IServiceProvider));

            family.SetScopeTo(InstanceScope.Singleton);
            family.Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        [Test]
        public void SetScopeToThreadLocal()
        {
            var family = new PluginFamily(typeof (IServiceProvider));

            family.SetScopeTo(InstanceScope.ThreadLocal);
            family.Lifecycle.ShouldBeOfType<ThreadLocalStorageLifecycle>();
        }

        [Test]
        public void add_instance_fills_and_is_idempotent()
        {
            var instance = new ConstructorInstance(GetType());
            var family = new PluginFamily(GetType());

            family.AddInstance(instance);
            family.SetDefault(instance);
            family.AddInstance(instance);
            family.SetDefault(instance);

            family.Instances.Single().ShouldBeTheSameAs(instance);
        }

        [Test]
        public void add_type_by_name()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            family.AddType(typeof (DataTableProvider), "table");

            family.Instances.Single()
                  .ShouldBeOfType<ConstructorInstance>()
                  .PluggedType.ShouldEqual(typeof (DataTableProvider));
        }

        [Test]
        public void add_type_does_not_add_if_the_concrete_type_can_not_be_cast_to_plugintype()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            family.AddType(GetType());

            family.Instances.Any().ShouldBeFalse();
        }

        [Test]
        public void add_type_works_if_the_concrete_type_can_be_cast_to_plugintype()
        {
            var family = new PluginFamily(typeof (IServiceProvider));

            family.AddType(typeof (DataTableProvider));
            family.Instances.Single()
                  .ShouldBeOfType<ConstructorInstance>()
                  .PluggedType.ShouldEqual(typeof (DataTableProvider));


            family.AddType(typeof (DataTable));
            family.Instances.Count().ShouldEqual(1);
        }

        [Test]
        public void adding_an_instance_sets_itself_as_the_parent()
        {
            var family = new PluginFamily(typeof (IGateway));
            var instance = new ConfiguredInstance(typeof (TheGateway));

            family.AddInstance(instance);
            instance.Parent.ShouldBeTheSameAs(family);
        }

        [Test]
        public void remove_all_clears_the_defaul_and_removes_all_plugins_instances()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            var instance = new SmartInstance<DataSet>();

            family.SetDefault(instance);

            family.AddInstance(new NullInstance());
            family.AddType(typeof (DataSet));

            family.RemoveAll();

            family.GetDefaultInstance().ShouldBeNull();

            family.Instances.Count().ShouldEqual(0);
        }

        [Test]
        public void remove_all_clears_the_default()
        {
            var instance = new ConstructorInstance(GetType());
            var family = new PluginFamily(GetType());

            family.SetDefault(instance);
            family.SetDefault(new ConstructorInstance(GetType()));
            family.SetDefault(new ConstructorInstance(GetType()));

            family.RemoveAll();

            family.Instances.Any().ShouldBeFalse();
            family.GetDefaultInstance().ShouldNotBeTheSameAs(instance);
        }

        [Test]
        public void removing_the_default_will_change_the_default()
        {
            var instance = new ConstructorInstance(GetType());
            var family = new PluginFamily(GetType());

            family.SetDefault(instance);

            family.RemoveInstance(instance);

            family.Instances.Any().ShouldBeFalse();
            family.GetDefaultInstance().ShouldNotBeTheSameAs(instance);
        }

        [Test]
        public void set_default()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            var instance = new SmartInstance<DataSet>();

            family.SetDefault(instance);

            family.GetDefaultInstance().ShouldBeTheSameAs(instance);
        }

        [Test]
        public void set_the_scope_to_session()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            family.SetScopeTo(InstanceScope.HttpSession);

            family.Lifecycle.ShouldBeOfType<HttpSessionLifecycle>();
        }

        [Test]
        public void set_the_scope_to_session_hybrid()
        {
            var family = new PluginFamily(typeof (IServiceProvider));
            family.SetScopeTo(InstanceScope.HybridHttpSession);

            family.Lifecycle.ShouldBeOfType<HybridSessionLifecycle>();
        }
    }


    /// <summary>
    ///     Specifying the default instance is "Default" and marking the PluginFamily
    ///     as an injected Singleton
    /// </summary>
    //[PluginFamily("Default", IsSingleton = true)]
    public interface ISingletonRepository
    {
    }

    //[Pluggable("Default")]
    public class SingletonRepositoryWithAttribute : ISingletonRepository
    {
        private readonly Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get { return _id; }
        }
    }

    public class SingletonRepositoryWithoutPluginAttribute : ISingletonRepository
    {
    }

    public class RandomClass
    {
    }

    //[PluginFamily(IsSingleton = false)]
    public interface IDevice
    {
    }

    public class GenericType<T>
    {
    }
}
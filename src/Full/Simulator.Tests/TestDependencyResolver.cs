//using System;
//using System.Collections.Concurrent;
//using System.ComponentModel.Composition;
//using System.Linq;
//using System.Reflection;
//using Akka.Actor;
//using Akka.DI.Core;
//using Akka.TestKit.VsTest;

///// <summary>
///// http://usemam.blogspot.co.il/2015/09/unit-testing-Simulator-actors-and-mef.html
///// </summary>
//namespace Simulator.Tests
//{
//    public class TestDependencyResolver : IDependencyResolver
//    {
//        private readonly ActorSystem _system;

//        private readonly TestKit _test;

//        private readonly ConcurrentDictionary<string, Type> _typeCache;

//        public TestDependencyResolver(ActorSystem system, TestKit test)
//        {
//            this._test = test;
//            this._typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
//            this._system = system;
//            this._system.AddDependencyResolver(this);
//        }

//        public Props Create<TActor>() where TActor : ActorBase
//        {
//            return this._system.GetExtension<DIExt>().Props(typeof(TActor));
//        }

//        public Func<ActorBase> CreateActorFactory(Type actorType)
//        {
//            return () => this.InitializeActorWithImports(actorType);
//        }

//        public Type GetType(string actorName)
//        {
//            this._typeCache.TryAdd(actorName, actorName.GetTypeValue());

//            return this._typeCache[actorName];
//        }

//        public void Release(ActorBase actor)
//        {
//            // do nothing
//        }

//        private ActorBase InitializeActorWithImports(Type actorType)
//        {
//            PropertyInfo[] mockProperties =
//                this._test.GetType()
//                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
//                    .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Mock<>))
//                    .ToArray();
//            PropertyInfo[] importProperties =
//                actorType.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
//                    .Where(p => p.GetCustomAttribute<ImportAttribute>() != null)
//                    .ToArray();
//            object actor = Activator.CreateInstance(actorType);
//            foreach (PropertyInfo importProperty in importProperties)
//            {
//                PropertyInfo property = importProperty;
//                PropertyInfo mockProperty = mockProperties.SingleOrDefault(
//                    p => p.PropertyType.GetGenericArguments().Single() == property.PropertyType);
//                if (null == mockProperty)
//                {
//                    throw new Exception(string.Format("Can't find mock for import [{0}]", importProperty.Name));
//                }

//                object importValue = ((Mock)mockProperty.GetValue(this._test)).Object;
//                importProperty.SetValue(actor, importValue);
//            }

//            return (ActorBase)actor;
//        }

//        public Props Create(Type actorType)
//        {
//            return this._system.GetExtension<DIExt>().Props(actorType);
//        }
//    }
//}
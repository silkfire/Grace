﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Grace.DependencyInjection
{
    /// <summary>
    /// Enumeration for constructor selection method
    /// </summary>
    public enum ConstructorSelectionMethod
    {
        /// <summary>
        /// Matches the best constructor based on which exports are registered
        /// </summary>
        BestMatch,

        /// <summary>
        /// Use the constructor with the most parameters
        /// </summary>
        MostParameters,

        /// <summary>
        /// Use the constructor with the least parameters
        /// </summary>
        LeastParameters,

        /// <summary>
        /// Not implemented but avaliable for extension purposes
        /// </summary>
        Other
    }

    /// <summary>
    /// Classes that implement this can be used to create enumerables
    /// </summary>
    public interface IEnumerableCreator
    {
        /// <summary>
        /// Construct enumerable
        /// </summary>
        /// <typeparam name="T">Type to enumerate</typeparam>
        /// <param name="scope">export locator scope</param>
        /// <param name="array">array to wrap as enumerable</param>
        /// <returns>enumerable</returns>
        IEnumerable<T> CreateEnumerable<T>(IExportLocatorScope scope, T[] array);
    }
    
    /// <summary>
    /// Configure the how expressions are created
    /// </summary>
    public class ExportCompilationBehavior
    {
        private Func<Type, bool> _keyedTypeSelector = DefaultKeyedTypeSelector;

        /// <summary>
        /// Default implementation for selecting types that should be located by key.
        /// Note: string, primitive and datetime are located by key
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool DefaultKeyedTypeSelector(Type arg)
        {
            if (arg.GetTypeInfo().IsAssignableFrom(typeof(Delegate).GetTypeInfo()))
            {
                return false;
            }

            return arg == typeof(string) || arg.GetTypeInfo().IsPrimitive || arg == typeof(DateTime);
        }

        /// <summary>
        /// Max object graph depth, this is what's used to detect a recursive loop
        /// </summary>
        /// <returns></returns>
        public int MaxObjectGraphDepth { get; set; } = 100;

        /// <summary>
        /// Allow IInjectionScope to be injected, false by default because you normally wnt IExportLocatorScope
        /// </summary>
        public bool AllowInjectionScopeLocation { get; set; } = false;

        /// <summary>
        /// Constructor selection algorithm 
        /// </summary>
        public ConstructorSelectionMethod ConstructorSelection { get; set; } = ConstructorSelectionMethod.BestMatch;

        /// <summary>
        /// customize enumerable creation
        /// </summary>
        public IEnumerableCreator CustomEnumerableCreator { get; set; }

        /// <summary>
        /// Allows you to override the default behavior for what is located by key and what's not 
        /// </summary>
        public Func<Type, bool> KeyedTypeSelector
        {
            get { return _keyedTypeSelector; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(KeyedTypeSelector), "Keyed must not be null");
                }

                _keyedTypeSelector = value;
            }
        }

        /// <summary>
        /// By default ExportInstance and ExportFactory must return a value. 
        /// </summary>
        /// <returns></returns>
        public bool AllowInstanceAndFactoryToReturnNull { get; set; } = false;
    }
}

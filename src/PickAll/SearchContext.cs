﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using PickAll.Searchers;
using PickAll.PostProcessors;

namespace PickAll
{
    /// <summary>
    /// Manages <see cref="Searcher"> and <see cref="IPostProcessor"> instances to gather and
    /// elaborate results.
    /// </summary>
    public sealed class SearchContext
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(
            Configuration.Default.WithDefaultLoader());
        private IEnumerable<object> _services = new object[] {};
        private static bool IsSearcher(Type type) => type.IsSubclassOf(typeof(Searcher)); 
        private static bool IsPostProcessor(Type type) => typeof(IPostProcessor).IsAssignableFrom(type);

#if DEBUG
        public IEnumerable<object> Services
        {
            get { return _services; }
        }
#endif
        /// <summary>
        /// Registers an instance of <see cref="Searcher"> or <see cref="IPostProcessor">.
        /// </summary>
        /// <param name="service">A service instance to register.</param>
        /// <returns>A <see cref="SearchContext"> with the given service added.</returns>
        public SearchContext With(object service)
        {
            var type = service.GetType();
            if (IsSearcher(type)) {
                _services = _services.CloneWith(service);
            }
            else if (IsPostProcessor(type)) {
                _services = _services.CloneWith(service);
            }
            else {
                throw new NotSupportedException(
                    "T must inherit from Searcher or implements IPostProcessor");
            }
            return this;
        }

        /// <summary>
        /// Registers an instance of <see cref="Searcher"> or <see cref="IPostProcessor">
        /// using service name.
        /// </summary>
        /// <param name="serviceName">Name of the service to add (case sensitive).</param>
        /// <param name="args">Optional arguments for service constructor.</param>
        /// <returns>A <see cref="SearchContext"> with the given service added.</returns>
        public SearchContext With(string serviceName, params object[] args)
        {
            if (serviceName == null) {
                throw new ArgumentNullException($"{nameof(serviceName)} cannot be null");
            }
            if (serviceName.Trim() == string.Empty) {
                throw new ArgumentException($"{nameof(serviceName)} cannot be empty or contain only space");
            }
            var type = Type.GetType($"PickAll.Searchers.{serviceName}", false);
            if (type == null) {
                type = Type.GetType($"PickAll.PostProcessors.{serviceName}", false);
                if (type == null) {
                    throw new NotSupportedException($"{serviceName} service not found");
                }
            }
            if (IsSearcher(type)) {
                var searcher = (Searcher)Activator.CreateInstance(type, args);
                searcher.Context = _context;
                _services = _services.CloneWith(searcher);
            }
            else if (IsPostProcessor(type)) {
                _services = _services.CloneWith(Activator.CreateInstance(type, args));
            } else {
                throw new NotSupportedException(
                    $"${nameof(serviceName)} must inherit from Searcher or implements IPostProcessor");
            }
            return this; 
        }

        /// <summary>
        /// Unregisters first instance of <see cref="Searcher"> or <see cref="IPostProcessor">
        /// using type.
        /// </summary>
        /// <typeparam name="T">A type that inherits from <see cref="SearchContext"> or
        /// implements <see cref="IPostProcessor">.</typeparam>
        /// <returns>A <see cref="SearchContext"> instance with the given service removed.</returns>
        public SearchContext Without<T>()
        {
            var type = typeof(T);
            if (IsSearcher(type)) {
                _services = _services.CloneWithout<T>();
            }
            else if (IsPostProcessor(type)) {
                _services = _services.CloneWithout<T>();
            }
            else {
                throw new NotSupportedException(
                    "T must inherit from Searcher or implements IPostProcessor");
            }
            return this;
        }        

        /// <summary>
        /// Executes a search asynchronously, invoking all <see cref="Searcher">
        /// and <see cref="IPostProcessor"> services.
        /// </summary>
        /// <param name="query">A query string for sercher services.</param>
        /// <returns>A colection of <see cref="ResultInfo">.</returns>
        public async Task<IEnumerable<ResultInfo>> SearchAsync(string query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query),
                $"{nameof(query)} cannot be null");
            if (query.Trim() == string.Empty) throw new ArgumentException(nameof(query),
                $"{nameof(query)} cannot be empty or contains only white spaces");

            var results = new List<ResultInfo>();
            foreach (var service in _services) {
                if (service.GetType().IsSubclassOf(typeof(Searcher))) {
                    results.AddRange(await ((Searcher)service).SearchAsync(query));
                } else if (typeof(IPostProcessor).IsAssignableFrom(service.GetType())) {
                    var current = await ((IPostProcessor)service).ProcessAsync(results);
                    results = new List<ResultInfo>();
                    results.AddRange(current);
                }
            }
            return results;
        }

        /// <summary>
        /// Builds a <see cref="SearchContext"> instance, registering default services.
        /// </summary>
        /// <returns>A <see cref="SearchContext"> instance.</returns>
        public static SearchContext Default()
        {
            var @default = new SearchContext();
            @default._services = new object[]
                {
                    SetUpService(new Google(), @default._context),
                    SetUpService(new DuckDuckGo(), @default._context),
                    SetUpService(new Uniqueness()),
                    SetUpService(new Order())
                };
            return @default;
        }

        private static object SetUpService(object service, IBrowsingContext context = null)
        {
            var configured = service;
            if (context != null) {
                ((Searcher)service).Context = context;
            }
            return configured;
        }
    }
}
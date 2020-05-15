using Microsoft.Extensions.Configuration;
using Penguin.DependencyInjection.Extensions;
using Penguin.Persistence.Abstractions;
using Penguin.Persistence.EntityFramework;
using System;
using System.Diagnostics;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Database
{
    /// <summary>
    /// A factory for generating an instance of a Dynamic Context, required for calling database commands
    /// from the powershell command line
    /// </summary>
    public class ContextFactory : DynamicContextFactory
    {
        /// <summary>
        /// Creates a new instance of the Dynamic Context
        /// </summary>
        /// <returns></returns>
        public override DynamicContext Create()
        {
            if (!DependencyEngine.IsRegistered<IConfiguration>())
            {
                string stack = string.Empty;

                StackTrace stackTrace = new StackTrace();

                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    if (stackTrace.GetFrame(i)?.GetMethod()?.ReflectedType?.FullName is string stacklevel)
                    {
                        //Check if we're running a migration from powershell
                        //We hardcore the connection string for now
                        if (stacklevel.Contains("Design.ToolingFacade+BaseRunner", StringComparison.Ordinal))
                        {
                            return new DynamicContext(new PersistenceConnectionInfo("Data Source=.;Initial Catalog=Framework;Integrated Security=True;MultipleActiveResultSets=True"));
                        }

                        stack += stacklevel + System.Environment.NewLine;
                    }
                }

                throw new Exception(stack);
            }

            PersistenceConnectionInfo toLoad = new DependencyEngine().GetService<PersistenceConnectionInfo>();

            return new DynamicContext(toLoad);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;
using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Contains the tasks and the context to execute them.
    /// </summary>
    public sealed class TaskExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskExecutor"/> class.
        /// </summary>
        /// <param name="template">The deployment template.</param>
        /// <param name="tasks">The tasks to execute.</param>
        /// <param name="context">The task execution context.</param>
        public TaskExecutor(DeploymentTemplate template, IList<ITask> tasks, TaskContext context)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Template = template;
            Tasks = tasks;
            Context = context;
        }

        /// <summary>
        /// Gets the deployment template.
        /// </summary>
        public DeploymentTemplate Template { get; private set; }

        /// <summary>
        /// Gets the tasks to execute.
        /// </summary>
        public IList<ITask> Tasks { get; private set; }

        /// <summary>
        /// Gets the task execution context.
        /// </summary>
        public TaskContext Context { get; private set; }

        /// <summary>
        /// Executes the tasks.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <returns>The async <see cref="Task" /> with the deployment log.</returns>
        public async Task<DeploymentLog> Execute(RuntimeContext runtimeContext)
        {
            // Check the roles.
            if (runtimeContext.AccountDetails.Roles == null || runtimeContext.AccountDetails.Roles.Count == 0)
            {
                throw new InvalidOperationException("The account details does not contain any roles.");
            }

            if (!runtimeContext.AccountDetails.Roles.Contains("primary administrator", StringComparer.OrdinalIgnoreCase))
            {
                var requiredRoles = Tasks
                    .Select(resource => resource.GetRequiredRole())
                    .Where(role => !string.IsNullOrEmpty(role))
                    .Distinct();

                var missingRole = requiredRoles
                    .Where(role => !runtimeContext.AccountDetails.Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    .Any();

                if (missingRole)
                {
                    throw new SecurityException("The template requires you to be a member of the following roles: " + string.Join(", ", requiredRoles));
                }
            }

            // Execute each individual task.
            foreach (var task in Tasks)
            {
                try
                {
                    await task.Execute(runtimeContext, Context);

                    if (Context.Log.Status == DeploymentLogStatus.Failed)
                    {
                        return Context.Log;
                    }
                }
                catch (Exception)
                {
                    Context.Log.Status = DeploymentLogStatus.Failed;
                    throw;
                }
            }

            Context.Log.Status = DeploymentLogStatus.Success;
            return Context.Log;
        }
    }
}

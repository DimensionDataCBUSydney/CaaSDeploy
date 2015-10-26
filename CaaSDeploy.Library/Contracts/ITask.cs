﻿using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tasks;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Abstraction for tasks performed as part of a deployment.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task" />.</returns>
        Task Execute(CaasAccountDetails accountDetails, TaskContext context);
    }
}
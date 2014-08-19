using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Security;
using Orchard.Core.Title.Models;
using Orchard.Tasks.Scheduling;
using Orchard.Logging;
using System.Linq;
using NGM.Forum.Services;

namespace NGM.Forum.ScheduledTasks {
    public class PurgeLastReadRepositoryTask : IScheduledTaskHandler
    {
          private const string TaskType = "NGM.Forum.PurgeLastReadRepositoryTask";
          private readonly IScheduledTaskManager _taskManager;
          private readonly IThreadLastReadService _threadLastReadService;

          public ILogger Logger { get; set; }

          public PurgeLastReadRepositoryTask(
              IScheduledTaskManager taskManager,
              IThreadLastReadService threadLastReadService
          )
          {
            _taskManager = taskManager;
            _threadLastReadService = threadLastReadService;
            Logger = NullLogger.Instance;
            try
            {
                //set the first run a bit in the future so the system doesn't run the task immediately on startup
                DateTime firstDate = DateTime.UtcNow.AddHours(1);
                //for testing
                //DateTime firstDate = DateTime.UtcNow.AddMinutes(2);
                ScheduleNextTask(firstDate);
            }
            catch(Exception e)
            {
               this.Logger.Error(e,e.Message);
            }
          }

          public void Process(ScheduledTaskContext context)
          {
             if (context.Task.TaskType == TaskType)
             {
               try
               {
                   _threadLastReadService.PurgeLastReadRepository();
               }
               catch (Exception e)
               {
                 this.Logger.Error(e, e.Message);
               }
               finally
               {
                    //re-run the purge every day);
                    DateTime nextTaskDate = DateTime.UtcNow.AddDays(1);
                    //for testing
                    //DateTime nextTaskDate = DateTime.UtcNow.AddMinutes(1);
                    this.ScheduleNextTask(nextTaskDate);

               }         
             }
          }
          private void ScheduleNextTask(DateTime date)
          {
             if (date > DateTime.UtcNow )
             {
                var tasks = this._taskManager.GetTasks(TaskType);
                 //only start a new one if it doesn't already exist
                if (tasks == null || tasks.Count() == 0)
                  this._taskManager.CreateTask(TaskType, date, null);
              }
          }
    
    }
}
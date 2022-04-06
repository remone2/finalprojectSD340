﻿using finalprojectSD340.Models;
using finalprojectSD340.Data;

namespace finalprojectSD340.HelperClasses
{
    public class TaskHelper : Helper
    {
        private readonly ApplicationDbContext _db;

        public TaskHelper(ApplicationDbContext db)
        {
            _db = db;
        }

        public override string Add(int projectId)
        {
            return "";
        }

        public override string Delete(int id)
        {
            Models.Task? taskToDelete = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToDelete != null)
            {
                try
                {
                    _db.Tasks.Remove(taskToDelete);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return "Task does not exist in database.";
            }

            return "Task successfully deleted.";
        }

        public override string UpdatePriority(int id, Priority newPriority)
        {
            Models.Task? taskToUpdate = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate != null)
            {
                try
                {
                    taskToUpdate.Priority = newPriority;
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return "Task does not exist in database.";
            }

            return "Priority updated.";
        }

        public override string UpdateDeadline(int id, DateTime newDeadline)
        {
            Models.Task? taskToUpdate = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate != null)
            {
                try
                {
                    taskToUpdate.Deadline = newDeadline;
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return "Task does not exist in database.";
            }
            return "Deadline updated.";
        }

        public string Assign(string devIdToAssign, int taskId)
        {
            Models.Task? taskToAssign = _db.Tasks.FirstOrDefault(t => t.Id == taskId);
            ApplicationUser? dev = _db.Users.FirstOrDefault(u => u.Id == devIdToAssign);

            if (taskToAssign == null)
                return "Task does not exist in the database.";

            if (dev == null)
                return "Developer does not exist in the database.";

            try
            {
                taskToAssign.DeveloperId = dev.Id;
                taskToAssign.Developer = dev;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Developer has been assigned.";
        }
    }
}

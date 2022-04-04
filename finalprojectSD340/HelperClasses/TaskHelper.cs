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

        public override string Add()
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
                return "Task does not exist in database";
            }

            return "Task Successfully Deleted!";
        }

        public override string Update(int id)
        {
            return "";
        }

        public string Assign(int devIdToAssign)
        {
            return "";
        }
    }
}

using JonyBalls3.Models;
using System.Collections.Generic;
using System.Linq;

namespace JonyBalls3.Services
{
    public class ProjectService
    {
        private static List<SimpleProject> _projects = new();
        private static int _nextId = 1;
        
        public List<SimpleProject> GetAllProjects()
        {
            return _projects.OrderByDescending(p => p.CreatedDate).ToList();
        }
        
        public SimpleProject? GetProjectById(int id)
        {
            return _projects.FirstOrDefault(p => p.Id == id);
        }
        
        public void AddProject(SimpleProject project)
        {
            project.Id = _nextId++;
            _projects.Add(project);
        }
        
        public void UpdateProject(SimpleProject project)
        {
            var existing = GetProjectById(project.Id);
            if (existing != null)
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
                existing.Area = project.Area;
                existing.Budget = project.Budget;
                existing.Status = project.Status;
            }
        }
        
        public void DeleteProject(int id)
        {
            _projects.RemoveAll(p => p.Id == id);
        }
        
        public void InitializeSampleData()
        {
            if (_projects.Count == 0)
            {
                AddProject(new SimpleProject 
                { 
                    Name = "Ремонт кухни", 
                    Description = "Полный ремонт кухни 15м²",
                    Area = 15,
                    Budget = 150000,
                    Status = "Активный"
                });
                
                AddProject(new SimpleProject 
                { 
                    Name = "Ванная комната", 
                    Description = "Замена сантехники и плитки",
                    Area = 8,
                    Budget = 80000,
                    Status = "Планирование"
                });
            }
        }
    }
}

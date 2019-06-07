internal class BuildProjects
{
  private BuildProjects()
  {
  }

  public ICollection<SolutionProject> All { get; private set; }
  public ICollection<SolutionProject> NoneTests { get; private set; }
  public ICollection<SolutionProject> OnlyTests { get; private set; }

  public static BuildProjects Instance(ICakeContext context, string solution)
  {
    var allProjects = context.ParseSolution($"{solution}.sln").Projects.ToList();

    return new BuildProjects
    {
      All = allProjects,
      NoneTests = allProjects.Where(project => !project.Name.EndsWith("Tests")).ToList(),
      OnlyTests = allProjects.Where(project => project.Name.EndsWith("Tests")).ToList()
    };
  }
}
